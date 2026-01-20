using System.Collections.Generic;
using UnityEngine;

namespace TransformGizmos
{
    public class Rotation : MonoBehaviour
    {
        GameObject m_targetObject;
        Material m_clickedMaterial;
        GameObject m_objectWithMeshes;
        GameObject m_degreesText;
        GameObject m_rotationAppendix;

        public float m_rotateSpeed = 5;
        float m_gizmoSize = 1;
        float m_maxDist; // the higher the slower the rotation
        readonly int m_resolution = 500;
        float m_initialScale;
        float m_initialDegreesTextScale;
        float m_cameraDistance;
        Mesh m_mesh;
        Mesh m_mesh2;
        Vector3[] m_vertices;
        Vector2 m_tangent;
        Quaternion m_rotation;
        Vector2 m_initialMousePosition = Vector2.zero;
        Vector2 m_lastProjectedMousePosition = Vector2.zero;
        MeshRenderer[] m_renderers = new MeshRenderer[3];
        MeshRenderer[] m_renderers2 = new MeshRenderer[3];
        Material[] m_defaultMaterials = new Material[3];
        Material[] m_hoveredMaterials = new Material[3];
        MeshRenderer m_degreesTextRenderer;
        bool m_isDragging;
        float scale;
        float gizmoEpsilon = 2;
        Material m_gizmoTransparentMaterial;
        const string GUIZ_TEST_MODE = "unity_GUIZTestMode";
        // this sets the material rendering to always be in front of other objects
        const int FRONT_RENDERING = (int)UnityEngine.Rendering.CompareFunction.Always;
        public static Rotation Instance { get; private set; }

        void Awake()
        {
            // If there is an instance, and it's not me, delete myself.
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
                return;
            }
            Destroy(gameObject);
        }

        public void Initialization(GameObject targetObject, Material clickedMaterial, Material gizmoTransparentMaterial, GameObject objectWithMeshes, GameObject degreesText, GameObject rotationAppendix)
        {
            m_targetObject = targetObject;
            m_clickedMaterial = clickedMaterial;
            m_objectWithMeshes = objectWithMeshes;
            m_degreesText = degreesText;
            m_rotationAppendix = rotationAppendix;
            m_objectWithMeshes.transform.rotation = m_targetObject.transform.rotation;
            m_degreesText.SetActive(false);
            m_mesh = new Mesh();
            m_objectWithMeshes.transform.GetChild(0).GetComponent<MeshFilter>().mesh = m_mesh;
            m_objectWithMeshes.transform.GetChild(0).GetComponent<MeshRenderer>().material = m_clickedMaterial;
            m_mesh2 = new Mesh();
            m_objectWithMeshes.transform.GetChild(1).GetComponent<MeshFilter>().mesh = m_mesh2;
            m_objectWithMeshes.transform.GetChild(1).GetComponent<MeshRenderer>().material = m_clickedMaterial;
            m_initialScale = transform.localScale.x;
            m_initialDegreesTextScale = m_degreesText.transform.localScale.x;
            m_degreesTextRenderer = m_degreesText.GetComponent<MeshRenderer>();
            m_gizmoTransparentMaterial = gizmoTransparentMaterial;
        }
        public void StartCode(MeshRenderer renderer, MeshRenderer renderer2, Material defaultMaterial, Material hoveredMaterial, int axis)
        {
            renderer.material.renderQueue = 3001;
            renderer.material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);
            m_renderers[axis] = renderer;
            renderer2.material.renderQueue = 3001;
            renderer2.material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);
            m_renderers2[axis] = renderer2;
            m_defaultMaterials[axis] = defaultMaterial;
            m_hoveredMaterials[axis] = hoveredMaterial;
        }

        private void Update()
        {
            m_cameraDistance = Vector3.Distance(Camera.main.transform.position, m_targetObject.transform.position);
            scale = m_cameraDistance * m_gizmoSize / 100 * m_initialScale;
            transform.localScale = new Vector3(scale, scale, scale);
            scale = m_cameraDistance * m_gizmoSize / 100 * m_initialDegreesTextScale;
            m_degreesText.transform.localScale = new Vector3(scale, scale, scale);
            m_degreesText.transform.LookAt(Camera.main.transform);
            m_degreesText.transform.rotation = Quaternion.LookRotation(m_degreesText.transform.position - Camera.main.transform.position);
            m_maxDist = 1500 * 5 / m_rotateSpeed; //m_maxDist = 5 and m_rotateSpeed = 1500 are the default values

            //X
            RotateGizmos(0, -transform.right, transform.forward, -transform.up);
            //Y
            RotateGizmos(1, transform.up, transform.right, transform.forward);
            //Z
            RotateGizmos(2, transform.forward, transform.right, -transform.up);
        }

        public void SetGizmoSize(float size)
        {
            m_gizmoSize = size;
        }

        public void MouseEnterCode(int axis)
        {
            if (!m_isDragging)
            {
                m_renderers[axis].material = m_hoveredMaterials[axis];
                m_renderers[axis].material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);
                m_degreesTextRenderer.material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);
            }
        }

        public void MouseExitCode(int axis)
        {
            if (!m_isDragging)
            {
                m_renderers[axis].material = m_defaultMaterials[axis];
                m_renderers[axis].material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);
                m_degreesTextRenderer.material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);
            }
        }

        public (Vector2, Vector2, Vector2, Vector3[]) MouseDownCode(Vector3 upVector, int axis)
        {
            m_isDragging = true;
            m_objectWithMeshes.transform.parent = null;
            m_renderers[axis].material = m_clickedMaterial;
            m_renderers[axis].material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);
            m_renderers2[axis].material = m_clickedMaterial;
            m_renderers2[axis].material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);
            m_degreesText.SetActive(true);
            m_degreesTextRenderer.material.renderQueue = 3003;
            m_degreesTextRenderer.material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);

            m_initialMousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            m_lastProjectedMousePosition = m_initialMousePosition;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Vector3 positionVector = hit.point - transform.position;
                m_rotation = m_targetObject.transform.rotation;
                m_rotation = Quaternion.Inverse(m_rotation);
                positionVector = m_rotation * positionVector;

                float startingAngle = 0;
                switch (axis)
                {
                    case 0:
                        startingAngle = Vector3.SignedAngle(new Vector3(positionVector.x, 1, 0), positionVector, new Vector3(1, 0, 0));
                        break;
                    case 1:
                        startingAngle = Vector3.SignedAngle(new Vector3(1, positionVector.y, 0), positionVector, new Vector3(0, -1, 0));
                        break;
                    case 2:
                        startingAngle = Vector3.SignedAngle(new Vector3(0, 1, positionVector.z), positionVector, new Vector3(0, 0, -1));
                        break;
                }
                if (startingAngle < 0)
                    startingAngle += 360;
                startingAngle *= Mathf.Deg2Rad;

                m_vertices = new Vector3[m_resolution + 1];
                m_vertices[m_resolution] = Vector3.zero;

                float scale = transform.localScale.x;
                float radius = 0.6f * scale / m_initialScale;
                float ratio;
                float angle;
                for (int i = 0; i < m_resolution; i++)
                {
                    ratio = (float)i / m_resolution;
                    angle = startingAngle + 2 * Mathf.PI * ratio;
                    switch (axis)
                    {
                        case 0:
                            m_vertices[i] = new Vector3(0, radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));
                            break;
                        case 1:
                            m_vertices[i] = new Vector3(radius * Mathf.Cos(angle), 0, radius * Mathf.Sin(angle));
                            break;
                        case 2:
                            m_vertices[i] = new Vector3(radius * Mathf.Sin(angle), radius * Mathf.Cos(angle), 0);
                            break;
                    }
                }

                Vector2 mousePositionCenter = Camera.main.WorldToScreenPoint(transform.position);
                Vector2 positionVector2D = m_initialMousePosition - mousePositionCenter;
                m_tangent = new Vector2(-positionVector2D.y, positionVector2D.x);
                m_tangent = m_tangent.normalized;

                //check which side you are looking at the circle from
                Vector3 viewDirection = transform.position - Camera.main.transform.position;
                viewDirection = viewDirection.normalized;
                Vector3 circleNormal = upVector;

                if (Vector3.Dot(viewDirection, circleNormal) < 0)
                    m_tangent = -m_tangent;
            }

            return (m_initialMousePosition, m_lastProjectedMousePosition, m_tangent, m_vertices);
        }

        public void MouseUpCode(int axis)
        {
            m_objectWithMeshes.transform.parent = m_rotationAppendix.transform;
            m_renderers[axis].material = m_defaultMaterials[axis];
            m_renderers[axis].material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);
            m_renderers2[axis].material = m_gizmoTransparentMaterial;
            m_renderers2[axis].material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);
            m_degreesText.SetActive(false);
            m_mesh.Clear();
            m_mesh2.Clear();
            m_objectWithMeshes.transform.rotation = m_targetObject.transform.rotation;
            m_isDragging = false;
        }

        public (float, Vector2) MouseDragCode(Vector2 initialMousePosition, Vector2 tangent, Vector2 lastProjectedMousePosition, float totalDist, Vector3[] vertices, int axis)
        {
            m_renderers[axis].material = m_clickedMaterial;
            m_renderers[axis].material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);
            m_degreesTextRenderer.material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);

            Vector2 moveVector = (Vector2)Input.mousePosition - initialMousePosition;
            Vector2 projectedMoveVector = Vector3.Project(moveVector, tangent);
            Vector2 projectedPosition = initialMousePosition + projectedMoveVector;

            float dotProduct = Vector2.Dot(projectedMoveVector, tangent);
            float distNow = Vector2.Distance(projectedPosition, initialMousePosition);
            float distBefore = Vector2.Distance(lastProjectedMousePosition, initialMousePosition);
            float moveDist = Vector2.Distance(lastProjectedMousePosition, projectedPosition);
            float dist;

            if ((dotProduct > 0 && distNow > distBefore) || (dotProduct < 0 && distNow < distBefore))
                dist = -moveDist;
            else
                dist = moveDist;

            moveDist = dist / m_maxDist * 360;

            switch (axis)
            {
                case 0:
                    m_targetObject.transform.Rotate(-m_targetObject.transform.right, -moveDist, Space.World);
                    break;
                case 1:
                    m_targetObject.transform.Rotate(-m_targetObject.transform.up, moveDist, Space.World);
                    break;
                case 2:
                    m_targetObject.transform.Rotate(-m_targetObject.transform.forward, moveDist, Space.World);
                    break;
            }

            totalDist += dist;

            ComputeAndShowTriangles(totalDist, m_mesh, m_mesh2, vertices);

            (m_lastProjectedMousePosition, _) = TransformationsUtility.HandleMouseOutsideScreen(initialMousePosition, tangent);

            return (totalDist, m_lastProjectedMousePosition);
        }

        void ComputeAndShowTriangles(float totalDist, Mesh mesh, Mesh mesh2, Vector3[] vertices)
        {
            bool clockwise = totalDist <= 0;

            List<int> triangles = new List<int>();
            List<int> triangles2 = new List<int>();

            triangles.Add(m_resolution);
            triangles.Add(0);
            triangles.Add(clockwise ? m_resolution - 1 : 1);
            triangles2.Add(clockwise ? m_resolution - 1 : 1);
            triangles2.Add(0);
            triangles2.Add(m_resolution);

            int degree = (int)(Mathf.Abs(totalDist) % m_maxDist / m_maxDist * 360);
            //m_degreesText.GetComponent<TextMeshPro>().text = degree + "°";
            m_degreesTextRenderer.material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);

            float ratio = (Mathf.Abs(totalDist) % m_maxDist) / m_maxDist;

            for (int i = 2; i <= m_resolution; i++)
            {
                if (!(ratio >= (float)i / m_resolution))
                    break;

                if (i == m_resolution)
                {
                    triangles.Add(m_resolution);
                    triangles.Add(clockwise ? m_resolution - (i - 1) : i - 1);
                    triangles.Add(0);
                    triangles2.Add(0);
                    triangles2.Add(clockwise ? m_resolution - (i - 1) : i - 1);
                    triangles2.Add(m_resolution);
                    break;
                }

                triangles.Add(m_resolution);
                triangles.Add(clockwise ? m_resolution - (i - 1) : i - 1);
                triangles.Add(clockwise ? m_resolution - i : i);
                triangles2.Add(clockwise ? m_resolution - i : i);
                triangles2.Add(clockwise ? m_resolution - (i - 1) : i - 1);
                triangles2.Add(m_resolution);
            }

            int[] trianglesArr = triangles.ToArray();
            int[] trianglesArr2 = triangles2.ToArray();

            mesh.vertices = vertices;
            mesh.triangles = trianglesArr;
            mesh2.vertices = vertices;
            mesh2.triangles = trianglesArr2;

            MeshRenderer meshRenderer = m_objectWithMeshes.transform.GetChild(0).GetComponent<MeshRenderer>();
            MeshRenderer meshRenderer2 = m_objectWithMeshes.transform.GetChild(1).GetComponent<MeshRenderer>();

            m_objectWithMeshes.transform.GetChild(0).GetComponent<MeshFilter>().mesh = mesh;
            m_objectWithMeshes.transform.GetChild(1).GetComponent<MeshFilter>().mesh = mesh2;
            meshRenderer.material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);
            meshRenderer2.material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);
            meshRenderer.material.renderQueue = 3002;
            meshRenderer2.material.renderQueue = 3002;
        }

        private void RotateGizmos(int axis, Vector3 forward, Vector3 down, Vector3 left)
        {
            Vector3 lookVector = Camera.main.transform.forward;
            Vector3 projectedHorizontalLookVector = Vector3.ProjectOnPlane(lookVector, down);
            Vector3 projectedVerticalLookVector = Vector3.ProjectOnPlane(lookVector, left);

            if (Vector2.Dot(projectedHorizontalLookVector, forward) < 0)
                projectedHorizontalLookVector = Vector3.Reflect(projectedHorizontalLookVector, forward);

            if (Vector2.Dot(projectedVerticalLookVector, forward) < 0)
                projectedVerticalLookVector = Vector3.Reflect(projectedVerticalLookVector, forward);

            float horizontalAngle = Vector3.SignedAngle(forward, projectedHorizontalLookVector, down);
            float verticalAngle = Vector3.SignedAngle(forward, projectedVerticalLookVector, -left);
            float angleDiff;

            //if both angles are within the range apply the material of the active half to the other half 
            bool withinRange = verticalAngle < gizmoEpsilon && verticalAngle > -gizmoEpsilon && horizontalAngle < gizmoEpsilon && horizontalAngle > -gizmoEpsilon;
            if (withinRange)
            {
                m_renderers2[axis].material = m_renderers[axis].material;
                m_renderers2[axis].material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);
                return;
            }

            if (!m_isDragging)
            {
                m_renderers2[axis].material = m_gizmoTransparentMaterial;
                m_renderers2[axis].material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);
            }

            if (verticalAngle >= 0 && horizontalAngle >= 0)
            {
                //top right
                angleDiff = verticalAngle - horizontalAngle;
                Quaternion finalRotation;

                if (angleDiff >= 0)
                    //vertical or diagonal
                    finalRotation = Quaternion.Euler(0, Mathf.Min(angleDiff * 4 + 135, 180), 0);
                else
                    //horizontal
                    finalRotation = Quaternion.Euler(0, Mathf.Max(angleDiff * 4 + 135, 90), 0);

                m_renderers[axis].transform.localRotation = finalRotation;
                m_renderers2[axis].transform.localRotation = finalRotation;
            }
            else if (verticalAngle >= 0 && horizontalAngle < 0)
            {
                horizontalAngle = -horizontalAngle;

                //top left
                angleDiff = verticalAngle - horizontalAngle;
                Quaternion finalRotation;

                if (angleDiff >= 0)
                    //vertical or diagonal
                    finalRotation = Quaternion.Euler(0, Mathf.Max(-angleDiff * 4 + 225, 180), 0);
                else
                    //horizontal
                    finalRotation = Quaternion.Euler(0, Mathf.Min(-angleDiff * 4 + 225, 270), 0);

                m_renderers[axis].transform.localRotation = finalRotation;
                m_renderers2[axis].transform.localRotation = finalRotation;
            }
            else if (verticalAngle < 0 && horizontalAngle >= 0)
            {
                verticalAngle = -verticalAngle;

                //bottom right
                angleDiff = verticalAngle - horizontalAngle;
                Quaternion finalRotation;

                if (angleDiff >= 0)
                    //vertical or diagonal
                    finalRotation = Quaternion.Euler(0, Mathf.Max(-angleDiff * 4 + 45, 0), 0);
                else
                    //horizontal
                    finalRotation = Quaternion.Euler(0, Mathf.Min(-angleDiff * 4 + 45, 90), 0);

                m_renderers[axis].transform.localRotation = finalRotation;
                m_renderers2[axis].transform.localRotation = finalRotation;
            }
            else if (verticalAngle < 0 && horizontalAngle < 0)
            {
                verticalAngle = -verticalAngle;
                horizontalAngle = -horizontalAngle;

                //bottom left
                angleDiff = verticalAngle - horizontalAngle;
                Quaternion finalRotation;

                if (angleDiff >= 0)
                    //vertical or diagonal
                    finalRotation = Quaternion.Euler(0, Mathf.Min(angleDiff * 4 + 315, 360), 0);
                else
                    //horizontal
                    finalRotation = Quaternion.Euler(0, Mathf.Max(angleDiff * 4 + 315, 270), 0);

                m_renderers[axis].transform.localRotation = finalRotation;
                m_renderers2[axis].transform.localRotation = finalRotation;
            }
        }
    }
}
