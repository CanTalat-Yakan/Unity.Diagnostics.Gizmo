using System;
using System.Collections.Generic;
using ImGuizmoNET;
using ImGuiNET;
using UnityEngine;
using Vector2N = System.Numerics.Vector2;
using Vector4N = System.Numerics.Vector4;

namespace UnityEssentials
{
    public enum GizmoTransformOperation
    {
        Translate,
        Rotate,
        Scale
    }

    public enum GizmoTransformMode
    {
        Local,
        World
    }

    /// <summary>
    /// Runtime diagnostics gizmo API.
    /// Explicit call model only: call methods from your own Update flows.
    /// </summary>
    public static class GizmoImGui
    {
        public static bool Enabled { get; set; } = true;

        private static readonly Color s_axisX = new(1f, 0.26f, 0.26f, 0.95f);
        private static readonly Color s_axisY = new(0.35f, 1f, 0.35f, 0.95f);
        private static readonly Color s_axisZ = new(0.35f, 0.6f, 1f, 0.95f);

        private static int s_lastImGuizmoFrame = -1;

        /// <summary>
        /// Draws and applies a native ImGuizmo transform gizmo for the given target.
        /// Returns true if the transform was modified this frame.
        /// </summary>
        public static bool DrawTransformGizmo(
            Transform target,
            GizmoTransformOperation operation,
            GizmoTransformMode mode,
            Vector3 translationSnap,
            float rotationSnapDegrees = 0f,
            float scaleSnap = 0f)
        {
            if (!Enabled || target == null)
                return false;

            using var scope = ImGuiScope.TryEnter();
            if (!scope.Active)
                return false;

            var cam = ImGuiUtilities.ResolveCamera();
            if (cam == null)
                return false;

            PrepareImGuizmoFrame(cam);

            var view = ToFloat16(cam.worldToCameraMatrix);
            var projection = ToFloat16(cam.projectionMatrix);
            var matrix = ToFloat16(Matrix4x4.TRS(target.position, target.rotation, target.lossyScale));
            var deltaMatrix = new float[16];
            var snap = BuildSnap(operation, translationSnap, rotationSnapDegrees, scaleSnap);

            var changed = snap != null
                ? ImGuizmo.Manipulate(
                    ref view[0],
                    ref projection[0],
                    ToNativeOperation(operation),
                    ToNativeMode(mode),
                    ref matrix[0],
                    ref deltaMatrix[0],
                    ref snap[0])
                : ImGuizmo.Manipulate(
                    ref view[0],
                    ref projection[0],
                    ToNativeOperation(operation),
                    ToNativeMode(mode),
                    ref matrix[0],
                    ref deltaMatrix[0]);

            if (!changed)
                return false;

            ApplyWorldMatrixToTransform(target, FromFloat16(matrix));
            return true;
        }

        public static void DrawLine(Vector3 start, Vector3 end, Color color, float thickness = 1f, Camera camera = null)
        {
            if (!TryGetDrawContext(camera, out var drawList, out var cam, out var screenH))
                return;

            _ = screenH;
            if (!ImGuiUtilities.TryWorldToImGuiScreen(cam, start, out var a) || !ImGuiUtilities.TryWorldToImGuiScreen(cam, end, out var b))
                return;

            drawList.AddLine(a, b, ToImU32(color), Mathf.Max(1f, thickness));
        }

        public static void DrawRay(Vector3 origin, Vector3 direction, float length, Color color, float thickness = 1f, Camera camera = null)
        {
            DrawLine(origin, origin + direction.normalized * Mathf.Max(0f, length), color, thickness, camera);
        }

        public static void DrawPolyline(IReadOnlyList<Vector3> points, Color color, float thickness = 1f, bool closed = false, Camera camera = null)
        {
            if (points == null || points.Count < 2)
                return;

            if (!TryGetDrawContext(camera, out var drawList, out var cam, out var screenH))
                return;

            for (var i = 0; i < points.Count - 1; i++)
            {
                if (ImGuiUtilities.TryWorldToImGuiScreen(cam, points[i], out var a) && ImGuiUtilities.TryWorldToImGuiScreen(cam, points[i + 1], out var b))
                    drawList.AddLine(a, b, ToImU32(color), Mathf.Max(1f, thickness));
            }

            if (closed)
            {
                if (ImGuiUtilities.TryWorldToImGuiScreen(cam, points[points.Count - 1], out var last) && ImGuiUtilities.TryWorldToImGuiScreen(cam, points[0], out var first))
                    drawList.AddLine(last, first, ToImU32(color), Mathf.Max(1f, thickness));
            }
        }

        public static void DrawWireBox(Bounds bounds, Color color, float thickness = 1f, Camera camera = null)
        {
            DrawWireOrientedBox(bounds.center, bounds.size, Quaternion.identity, color, thickness, camera);
        }

        public static void DrawWireOrientedBox(
            Vector3 center,
            Vector3 size,
            Quaternion rotation,
            Color color,
            float thickness = 1f,
            Camera camera = null)
        {
            var half = size * 0.5f;
            var c0 = center + rotation * new Vector3(-half.x, -half.y, -half.z);
            var c1 = center + rotation * new Vector3(half.x, -half.y, -half.z);
            var c2 = center + rotation * new Vector3(half.x, -half.y, half.z);
            var c3 = center + rotation * new Vector3(-half.x, -half.y, half.z);

            var c4 = center + rotation * new Vector3(-half.x, half.y, -half.z);
            var c5 = center + rotation * new Vector3(half.x, half.y, -half.z);
            var c6 = center + rotation * new Vector3(half.x, half.y, half.z);
            var c7 = center + rotation * new Vector3(-half.x, half.y, half.z);

            DrawLine(c0, c1, color, thickness, camera);
            DrawLine(c1, c2, color, thickness, camera);
            DrawLine(c2, c3, color, thickness, camera);
            DrawLine(c3, c0, color, thickness, camera);

            DrawLine(c4, c5, color, thickness, camera);
            DrawLine(c5, c6, color, thickness, camera);
            DrawLine(c6, c7, color, thickness, camera);
            DrawLine(c7, c4, color, thickness, camera);

            DrawLine(c0, c4, color, thickness, camera);
            DrawLine(c1, c5, color, thickness, camera);
            DrawLine(c2, c6, color, thickness, camera);
            DrawLine(c3, c7, color, thickness, camera);
        }

        public static void DrawWireSphere(Vector3 center, float radius, Color color, float thickness = 1f, int segments = 32, Camera camera = null)
        {
            DrawCircle(center, Vector3.up, radius, color, thickness, segments, camera);
            DrawCircle(center, Vector3.right, radius, color, thickness, segments, camera);
            DrawCircle(center, Vector3.forward, radius, color, thickness, segments, camera);
        }

        public static void DrawCircle(
            Vector3 center,
            Vector3 normal,
            float radius,
            Color color,
            float thickness = 1f,
            int segments = 32,
            Camera camera = null)
        {
            if (radius <= 0f || segments < 3)
                return;

            var n = normal.sqrMagnitude > 0.0001f ? normal.normalized : Vector3.up;
            var tangent = Vector3.Cross(n, Mathf.Abs(Vector3.Dot(n, Vector3.up)) > 0.98f ? Vector3.right : Vector3.up).normalized;
            var bitangent = Vector3.Cross(n, tangent).normalized;

            var step = Mathf.PI * 2f / segments;
            Vector3 prev = center + tangent * radius;
            for (var i = 1; i <= segments; i++)
            {
                var a = step * i;
                var p = center + (tangent * Mathf.Cos(a) + bitangent * Mathf.Sin(a)) * radius;
                DrawLine(prev, p, color, thickness, camera);
                prev = p;
            }
        }

        public static void DrawCapsule(
            Vector3 start,
            Vector3 end,
            float radius,
            Color color,
            float thickness = 1f,
            int segments = 20,
            Camera camera = null)
        {
            if (radius <= 0f)
                return;

            var axis = end - start;
            var len = axis.magnitude;
            if (len <= 0.0001f)
            {
                DrawWireSphere(start, radius, color, thickness, segments, camera);
                return;
            }

            var dir = axis / len;
            var side = Vector3.Cross(dir, Mathf.Abs(Vector3.Dot(dir, Vector3.up)) > 0.98f ? Vector3.right : Vector3.up).normalized;
            var side2 = Vector3.Cross(dir, side).normalized;

            DrawLine(start + side * radius, end + side * radius, color, thickness, camera);
            DrawLine(start - side * radius, end - side * radius, color, thickness, camera);
            DrawLine(start + side2 * radius, end + side2 * radius, color, thickness, camera);
            DrawLine(start - side2 * radius, end - side2 * radius, color, thickness, camera);

            DrawCircle(start, dir, radius, color, thickness, segments, camera);
            DrawCircle(end, dir, radius, color, thickness, segments, camera);
            DrawCircle(start, side, radius, color, thickness, segments, camera);
            DrawCircle(end, side, radius, color, thickness, segments, camera);
        }

        public static void DrawAxisTriad(
            Vector3 origin,
            Quaternion rotation,
            float axisLength,
            float thickness = 2f,
            Camera camera = null)
        {
            var x = rotation * Vector3.right;
            var y = rotation * Vector3.up;
            var z = rotation * Vector3.forward;

            DrawLine(origin, origin + x * axisLength, s_axisX, thickness, camera);
            DrawLine(origin, origin + y * axisLength, s_axisY, thickness, camera);
            DrawLine(origin, origin + z * axisLength, s_axisZ, thickness, camera);
        }

        public static void DrawGrid(
            Vector3 origin,
            Vector3 normal,
            Vector3 tangent,
            int halfLineCount,
            float spacing,
            Color color,
            float thickness = 1f,
            Camera camera = null)
        {
            if (halfLineCount < 1 || spacing <= 0f)
                return;

            var n = normal.sqrMagnitude > 0.0001f ? normal.normalized : Vector3.up;
            var t = tangent.sqrMagnitude > 0.0001f ? tangent.normalized : Vector3.right;
            t = (t - Vector3.Dot(t, n) * n).normalized;
            var b = Vector3.Cross(n, t).normalized;

            var extent = halfLineCount * spacing;
            for (var i = -halfLineCount; i <= halfLineCount; i++)
            {
                var d = i * spacing;

                var a0 = origin + b * d - t * extent;
                var a1 = origin + b * d + t * extent;
                DrawLine(a0, a1, color, thickness, camera);

                var c0 = origin + t * d - b * extent;
                var c1 = origin + t * d + b * extent;
                DrawLine(c0, c1, color, thickness, camera);
            }
        }

        public static void DrawSelectionOutline(Renderer renderer, Color color, float thickness = 2f, Camera camera = null)
        {
            if (renderer == null)
                return;

            DrawWireBox(renderer.bounds, color, thickness, camera);
        }

        private static void PrepareImGuizmoFrame(Camera cam)
        {
            var ctx = ImGui.GetCurrentContext();
            if (ctx != IntPtr.Zero)
                ImGuizmo.SetImGuiContext(ctx);

            if (s_lastImGuizmoFrame != Time.frameCount)
            {
                ImGuizmo.BeginFrame();
                s_lastImGuizmoFrame = Time.frameCount;
            }

            ImGuizmo.SetDrawlist(ImGui.GetForegroundDrawList());
            ImGuizmo.SetOrthographic(cam.orthographic);

            ImGuiUtilities.GetDisplaySize(out var width, out var height);
            ImGuizmo.SetRect(0f, 0f, width, height);
            ImGuizmo.Enable(true);
        }

        private static float[] BuildSnap(
            GizmoTransformOperation operation,
            Vector3 translationSnap,
            float rotationSnapDegrees,
            float scaleSnap)
        {
            switch (operation)
            {
                case GizmoTransformOperation.Translate:
                    if (translationSnap.x <= 0f && translationSnap.y <= 0f && translationSnap.z <= 0f)
                        return null;
                    return new[] { translationSnap.x, translationSnap.y, translationSnap.z };

                case GizmoTransformOperation.Rotate:
                    if (rotationSnapDegrees <= 0f)
                        return null;
                    return new[] { rotationSnapDegrees, 0f, 0f };

                case GizmoTransformOperation.Scale:
                    if (scaleSnap <= 0f)
                        return null;
                    return new[] { scaleSnap, 0f, 0f };

                default:
                    return null;
            }
        }

        private static void ApplyWorldMatrixToTransform(Transform target, Matrix4x4 world)
        {
            if (target.parent == null)
            {
                target.position = world.GetColumn(3);
                target.rotation = world.rotation;
                target.localScale = ExtractScale(world);
                return;
            }

            var local = target.parent.worldToLocalMatrix * world;
            target.localPosition = local.GetColumn(3);
            target.localRotation = local.rotation;
            target.localScale = ExtractScale(local);
        }

        private static Vector3 ExtractScale(Matrix4x4 matrix)
        {
            return new Vector3(
                matrix.GetColumn(0).magnitude,
                matrix.GetColumn(1).magnitude,
                matrix.GetColumn(2).magnitude);
        }

        private static OPERATION ToNativeOperation(GizmoTransformOperation operation)
        {
            switch (operation)
            {
                case GizmoTransformOperation.Translate: return OPERATION.TRANSLATE;
                case GizmoTransformOperation.Rotate: return OPERATION.ROTATE;
                case GizmoTransformOperation.Scale: return OPERATION.SCALE;
                default: return OPERATION.TRANSLATE;
            }
        }

        private static MODE ToNativeMode(GizmoTransformMode mode)
        {
            return mode == GizmoTransformMode.Local ? MODE.LOCAL : MODE.WORLD;
        }

        private static uint ToImU32(Color color)
        {
            var v = new Vector4N(color.r, color.g, color.b, color.a);
            return ImGui.ColorConvertFloat4ToU32(v);
        }

        private static bool TryGetDrawContext(Camera camera, out ImDrawListPtr drawList, out Camera cam, out float screenH)
        {
            drawList = default;
            cam = null;
            screenH = 0f;

            if (!Enabled)
                return false;

            using var scope = ImGuiScope.TryEnter();
            if (!scope.Active)
                return false;

            cam = ImGuiUtilities.ResolveCamera(camera);
            if (cam == null)
                return false;

            drawList = ImGui.GetForegroundDrawList();
            screenH = ImGuiUtilities.GetDisplayHeight();

            return true;
        }

        private static float[] ToFloat16(Matrix4x4 matrix)
        {
            return new[]
            {
                matrix.m00, matrix.m10, matrix.m20, matrix.m30,
                matrix.m01, matrix.m11, matrix.m21, matrix.m31,
                matrix.m02, matrix.m12, matrix.m22, matrix.m32,
                matrix.m03, matrix.m13, matrix.m23, matrix.m33,
            };
        }

        private static Matrix4x4 FromFloat16(float[] values)
        {
            var matrix = new Matrix4x4();
            matrix.m00 = values[0];
            matrix.m10 = values[1];
            matrix.m20 = values[2];
            matrix.m30 = values[3];
            matrix.m01 = values[4];
            matrix.m11 = values[5];
            matrix.m21 = values[6];
            matrix.m31 = values[7];
            matrix.m02 = values[8];
            matrix.m12 = values[9];
            matrix.m22 = values[10];
            matrix.m32 = values[11];
            matrix.m03 = values[12];
            matrix.m13 = values[13];
            matrix.m23 = values[14];
            matrix.m33 = values[15];
            return matrix;
        }
    }
}