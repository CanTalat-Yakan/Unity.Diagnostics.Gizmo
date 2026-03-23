using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityEssentials
{
    /// <summary>
    /// Minimal runtime example for explicit GizmoImGui usage.
    /// 1/2/3 switch transform operation while dragging axes.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GizmoImGuiExample : MonoBehaviour
    {
        [Header("Target")]
        public Transform Target;

        [Header("Transform Gizmo")]
        public bool EnableTransformGizmo = true;
        public GizmoTransformOperation Operation = GizmoTransformOperation.Translate;
        public GizmoTransformMode Mode = GizmoTransformMode.Local;

        [Header("Snap")]
        public Vector3 TranslationSnap = Vector3.zero;
        public float RotationSnapDegrees;
        public float ScaleSnap;

        [Header("Visual Helpers")]
        public bool DrawHelpers = true;
        public float AxisLength = 1.75f;
        public float SphereRadius = 0.6f;
        public Vector3 BoxSize = new(1.2f, 0.8f, 1.6f);
        public float GridSpacing = 1f;
        public int GridHalfLines = 5;

        private void Update()
        {
            if (Target == null)
                Target = transform;

            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.digit1Key.wasPressedThisFrame)
                    Operation = GizmoTransformOperation.Translate;
                else if (keyboard.digit2Key.wasPressedThisFrame)
                    Operation = GizmoTransformOperation.Rotate;
                else if (keyboard.digit3Key.wasPressedThisFrame)
                    Operation = GizmoTransformOperation.Scale;
            }

            if (DrawHelpers)
                DrawHelperShapes();

            if (!EnableTransformGizmo)
                return;

            GizmoImGui.DrawTransformGizmo(Target, Operation, Mode, TranslationSnap, RotationSnapDegrees, ScaleSnap);
        }

        private void DrawHelperShapes()
        {
            var p = -Vector3.one;
            var r = Quaternion.identity;

            GizmoImGui.DrawAxisTriad(p, r, AxisLength, 2.5f);
            GizmoImGui.DrawWireOrientedBox(p, BoxSize, r, new Color(1f, 0.85f, 0.1f, 0.9f), 1.75f);
            GizmoImGui.DrawWireSphere(p, SphereRadius, new Color(0.2f, 0.95f, 1f, 0.8f), 1.5f, 28);

            var up = r * Vector3.up;
            var down = r * Vector3.down;
            GizmoImGui.DrawCapsule(p + down * 0.75f, p + up * 0.75f, 0.25f, new Color(1f, 0.4f, 0.2f, 0.85f), 1.5f, 18);

            var ground = new Vector3(p.x, 0f, p.z);
            GizmoImGui.DrawGrid(ground, Vector3.up, Vector3.right, GridHalfLines, GridSpacing, new Color(0.9f, 0.9f, 0.9f, 0.4f), 1f);

            GizmoImGui.DrawLine(p, p + r * Vector3.forward * 2.4f, new Color(0.4f, 1f, 0.4f, 0.95f), 2f);
            GizmoImGui.DrawRay(p, r * Vector3.right, 1.6f, new Color(1f, 0.35f, 0.35f, 0.95f), 2f);
        }
    }
}
