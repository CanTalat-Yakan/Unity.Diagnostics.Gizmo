using UnityEngine;

namespace TransformGizmos
{
    public class Translation : MonoBehaviour
    {
        GameObject m_targetObject;
        Material m_clickedMaterial;
        Material m_transparentMaterial;

        public float m_translateSpeed = 5;
        float m_gizmoSize = 1;
        float m_translateSpeedInternal;
        float m_initialScale;
        float m_cameraDistance;
        float scale;
        Vector2 m_moveDirection;
        Vector2 m_initialMousePosition = Vector2.zero;
        Vector2 m_lastProjectedMousePosition = Vector2.zero;
        bool m_isDragging;
        MeshRenderer[] m_renderers = new MeshRenderer[3];
        Material[] m_defaultMaterials = new Material[3];
        Material[] m_hoveredMaterials = new Material[3];
        const string GUIZ_TEST_MODE = "unity_GUIZTestMode";
        // this sets the material rendering to always be in front of other objects
        const int FRONT_RENDERING = (int)UnityEngine.Rendering.CompareFunction.Always;
        public static Translation Instance { get; private set; }

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
                //disable collider so that the movement vectors can be hovered
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

        public void StartCode(MeshRenderer renderer, Material defaultMaterial, Material hoveredMaterial, int axis)
        {
            renderer.material.renderQueue = 3001;
            renderer.material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);
            m_renderers[axis] = renderer;
            m_defaultMaterials[axis] = defaultMaterial;
            m_hoveredMaterials[axis] = hoveredMaterial;
        }

        private void Update()
        {
            m_cameraDistance = Vector3.Distance(Camera.main.transform.position, m_targetObject.transform.position);
            scale = m_cameraDistance * m_gizmoSize / 100 * m_initialScale;
            transform.localScale = new Vector3(scale, scale, scale);
            m_translateSpeedInternal = m_translateSpeed / 1000;
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
            }
        }

        public void MouseExitCode(int axis)
        {
            if (!m_isDragging)
            {
                m_renderers[axis].material = m_defaultMaterials[axis];
                m_renderers[axis].material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);
            }
        }

        public (Vector2, Vector2, Vector2) MouseDownCode(int axis)
        {
            for (int i = 0; i < 3; i++)
            {
                m_renderers[i].material = m_transparentMaterial;
                m_renderers[i].material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);
            }

            m_renderers[axis].material = m_clickedMaterial;
            m_renderers[axis].material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);

            m_initialMousePosition = Input.mousePosition;
            m_lastProjectedMousePosition = m_initialMousePosition;

            Vector2 mousePositionCenter = Camera.main.WorldToScreenPoint(transform.position);
            Vector2 positionVector2D = m_initialMousePosition - mousePositionCenter;
            m_moveDirection = positionVector2D;
            m_moveDirection = m_moveDirection.normalized;

            return (m_initialMousePosition, m_lastProjectedMousePosition, m_moveDirection);
        }

        public void MouseUpCode()
        {
            for (int i = 0; i < 3; i++)
            {
                m_renderers[i].material = m_defaultMaterials[i];
                m_renderers[i].material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);
            }

            m_isDragging = false;
        }

        public (float, Vector2) MouseDragCode(Vector2 initialMousePosition, Vector2 moveDirection, Vector2 lastProjectedMousePosition, float totalDist, int axis)
        {
            m_isDragging = true;
            m_renderers[axis].material = m_clickedMaterial;
            m_renderers[axis].material.SetInt(GUIZ_TEST_MODE, FRONT_RENDERING);

            Vector2 moveVector = (Vector2)Input.mousePosition - initialMousePosition;
            Vector2 projectedMoveVector = Vector3.Project(moveVector, moveDirection);
            Vector2 projectedPosition = initialMousePosition + projectedMoveVector;

            float dotProduct = Vector2.Dot(projectedMoveVector, moveDirection);
            float distNow = Vector2.Distance(projectedPosition, initialMousePosition);
            float distBefore = Vector2.Distance(lastProjectedMousePosition, initialMousePosition);
            float moveDist = Vector2.Distance(lastProjectedMousePosition, projectedPosition);
            float dist;

            if ((dotProduct > 0 && distNow > distBefore) || (dotProduct < 0 && distNow < distBefore))
                dist = -moveDist;
            else
                dist = moveDist;

            switch (axis)
            {
                case 0:
                    m_targetObject.transform.Translate(new Vector3(-dist * m_translateSpeedInternal, 0, 0), Space.Self);
                    break;
                case 1:
                    m_targetObject.transform.Translate(new Vector3(0, -dist * m_translateSpeedInternal, 0), Space.Self);
                    break;
                case 2:
                    m_targetObject.transform.Translate(new Vector3(0, 0, -dist * m_translateSpeedInternal), Space.Self);
                    break;
            }

            totalDist += dist;

            (m_lastProjectedMousePosition, _) = TransformationsUtility.HandleMouseOutsideScreen(initialMousePosition, moveDirection);
            return (totalDist, m_lastProjectedMousePosition);
        }
    }
}
