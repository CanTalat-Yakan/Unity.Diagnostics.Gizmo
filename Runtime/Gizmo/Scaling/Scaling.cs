using UnityEngine;

namespace TransformGizmos
{
    public class Scaling : MonoBehaviour
    {
        GameObject m_targetObject;
        Material m_clickedMaterial;
        Material m_transparentMaterial;

        public float m_scaleSpeed = 5;
        float m_gizmoSize = 1;
        float m_scaleSpeedInternal;
        float m_initialScale;
        float m_cameraDistance;
        Vector2 m_moveDirection;
        Vector2 m_initialMousePosition = Vector2.zero;
        Vector2 m_lastMousePosition = Vector2.zero;
        Vector2 m_lastProjectedMousePosition = Vector2.zero;
        bool m_isDragging;
        MeshRenderer[] m_renderers = new MeshRenderer[4];
        MeshRenderer[] m_renderers2 = new MeshRenderer[4];
        GameObject[] m_meshes = new GameObject[3];
        GameObject[] m_meshes2 = new GameObject[3];
        Material[] m_defaultMaterials = new Material[4];
        Material[] m_hoveredMaterials = new Material[4];
        float m_localAxisScale;
        float scale;
        const string GUIZ_TEST_MODE = "unity_GUIZTestMode";
        // this sets the material rendering to always be in front of other objects
        const int FRONT_RENDERING = (int)UnityEngine.Rendering.CompareFunction.Always;
        public static Scaling Instance { get; private set; }

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

        private void OnEnable()
        {
            if (m_targetObject != null)
            {
                //disable collider so that the scaling cubes can be hovered
                if (m_targetObject.GetComponent<Collider>() != null)
                {
                    m_targetObject.GetComponent<Collider>().enabled = false;
                }
            }

        }
        private void OnDisable()
        {
            if (m_targetObject != null)
            {
                if (m_targetObject.GetComponent<Collider>() != null)
                {
                    m_targetObject.GetComponent<Collider>().enabled = true;
                }
            }

        }

        public void Initialization(GameObject targetObject, Material clickedMaterial, Material transparentMaterial)
        {
            m_targetObject = targetObject;
            m_clickedMaterial = clickedMaterial;
            m_transparentMaterial = transparentMaterial;
            m_initialScale = transform.localScale.x;
        }

        public void StartCode(MeshRenderer renderer, MeshRenderer renderer2, Material defaultMaterial, Material hoveredMaterial, GameObject mesh, GameObject mesh2, int axis)
        {
            renderer.material.renderQueue = 3001;
            renderer.material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);
            renderer2.material.renderQueue = 3001;
            renderer2.material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);
            m_renderers[axis] = renderer;
            m_renderers2[axis] = renderer2;
            m_defaultMaterials[axis] = defaultMaterial;
            m_hoveredMaterials[axis] = hoveredMaterial;
            if (axis != 3)
            {
                m_meshes[axis] = mesh;
                m_meshes2[axis] = mesh2;
                m_localAxisScale = m_meshes[axis].transform.localScale.z;
            }
        }

        private void Update()
        {
            if (m_isDragging)
                return;
            m_cameraDistance = Vector3.Distance(Camera.main.transform.position, m_targetObject.transform.position);
            scale = m_cameraDistance * m_gizmoSize / 100 * m_initialScale;
            transform.localScale = new Vector3(scale, scale, scale);
            m_scaleSpeedInternal = m_scaleSpeed / 1000;
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
                m_renderers2[axis].material = m_hoveredMaterials[axis];
                m_renderers2[axis].material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);
            }
        }

        public void MouseExitCode(int axis)
        {
            if (!m_isDragging)
            {
                m_renderers[axis].material = m_defaultMaterials[axis];
                m_renderers[axis].material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);
                m_renderers2[axis].material = m_defaultMaterials[axis];
                m_renderers2[axis].material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);
            }
        }

        public (Vector2, Vector2, Vector2) MouseDownCode(int axis)
        {
            for (int i = 0; i < 4; i++)
            {
                if (axis == 3)
                {
                    m_renderers[i].material = m_clickedMaterial;
                    m_renderers2[i].material = m_clickedMaterial;
                }
                else
                {
                    m_renderers[i].material = m_transparentMaterial;
                    m_renderers2[i].material = m_transparentMaterial;
                }
                m_renderers[i].material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);
                m_renderers2[i].material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);
            }

            m_renderers[axis].material = m_clickedMaterial;
            m_renderers[axis].material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);
            m_renderers2[axis].material = m_clickedMaterial;
            m_renderers2[axis].material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);

            m_initialMousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            m_lastMousePosition = m_initialMousePosition;
            m_lastProjectedMousePosition = m_initialMousePosition;

            Vector2 mousePositionCenter = Camera.main.WorldToScreenPoint(transform.position);
            Vector2 positionVector2D = m_initialMousePosition - mousePositionCenter;
            m_moveDirection = positionVector2D;
            m_moveDirection = m_moveDirection.normalized;

            return (m_initialMousePosition, m_lastProjectedMousePosition, m_moveDirection);
        }

        public void MouseUpCode()
        {
            m_isDragging = false;

            for (int i = 0; i < 4; i++)
            {
                m_renderers[i].material = m_defaultMaterials[i];
                m_renderers[i].material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);
                m_renderers2[i].material = m_defaultMaterials[i];
                m_renderers2[i].material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);
            }

            for (int i = 0; i < 3; i++)
            {
                m_meshes[i].transform.localScale = new Vector3(1, 1, m_localAxisScale);
                m_meshes2[i].transform.localScale = new Vector3(1, 1, 1 / m_localAxisScale);
            }
        }

        public (float, Vector2) MouseDragCode(Vector2 initialMousePosition, Vector2 moveDirection, Vector2 lastProjectedMousePosition, float totalDist, int axis)
        {
            m_isDragging = true;

            Vector2 moveVector;
            Vector2 projectedMoveVector;
            Vector2 projectedPosition;
            float dist;

            if (axis == 3)
            {
                moveVector = (Vector2)Input.mousePosition - m_lastMousePosition;
                float movedist = moveVector.magnitude;
                if (Vector2.Dot(moveVector, new Vector2(1, 0)) >= 0 && Vector2.Dot(moveVector, new Vector2(0, 1)) >= 0)
                {
                    dist = -movedist;
                }
                else
                {
                    dist = movedist;
                }
            }
            else
            {
                moveVector = (Vector2)Input.mousePosition - initialMousePosition;
                projectedMoveVector = Vector3.Project(moveVector, moveDirection);
                projectedPosition = initialMousePosition + projectedMoveVector;

                float dotProduct = Vector2.Dot(projectedMoveVector, moveDirection);
                float distNow = Vector2.Distance(projectedPosition, initialMousePosition);
                float distBefore = Vector2.Distance(lastProjectedMousePosition, initialMousePosition);
                float moveDist = Vector2.Distance(lastProjectedMousePosition, projectedPosition);

                if ((dotProduct > 0 && distNow > distBefore) || (dotProduct < 0 && distNow < distBefore))
                    dist = -moveDist;
                else
                    dist = moveDist;
            }

            switch (axis)
            {
                case 0:
                    m_targetObject.transform.localScale += new Vector3(-dist * m_scaleSpeedInternal, 0, 0);
                    break;
                case 1:
                    m_targetObject.transform.localScale += new Vector3(0, -dist * m_scaleSpeedInternal, 0);
                    break;
                case 2:
                    m_targetObject.transform.localScale += new Vector3(0, 0, -dist * m_scaleSpeedInternal);
                    break;
                case 3:
                    m_targetObject.transform.localScale += new Vector3(-dist * m_scaleSpeedInternal, -dist * m_scaleSpeedInternal, -dist * m_scaleSpeedInternal);
                    break;
            }

            //scale the scale axis itself
            if (axis == 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    m_meshes[i].transform.localScale += new Vector3(0, 0, -dist * m_scaleSpeedInternal);
                    m_meshes2[i].transform.localScale = new Vector3(1, 1, 1 / m_meshes[i].transform.localScale.z);
                }
            }
            else
            {
                m_meshes[axis].transform.localScale += new Vector3(0, 0, -dist * m_scaleSpeedInternal);
                m_meshes2[axis].transform.localScale = new Vector3(1, 1, 1 / m_meshes[axis].transform.localScale.z);
            }

            totalDist += dist;

            (m_lastProjectedMousePosition, m_lastMousePosition) = TransformationsUtility.HandleMouseOutsideScreen(initialMousePosition, moveDirection);

            return (totalDist, m_lastProjectedMousePosition);
        }
    }
}
