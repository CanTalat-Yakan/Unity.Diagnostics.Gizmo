using UnityEngine;

namespace TransformGizmos
{
    public class ScalingAll : MonoBehaviour, IGizmoTransforms
    {
        [SerializeField] Material m_defaultMaterial;
        [SerializeField] Material m_hoveredMaterial;

        float m_totalDist;
        Vector2 m_moveDirection;
        Vector2 m_initialMousePosition = Vector2.zero;
        Vector2 m_lastProjectedMousePosition = Vector2.zero;

        void Start()
        {
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            Scaling.Instance.StartCode(renderer, renderer, m_defaultMaterial, m_hoveredMaterial, gameObject, gameObject, axis: 3);
        }

        public void OnMouseEnter()
        {
            Scaling.Instance.MouseEnterCode(axis: 3);
        }

        public void OnMouseExit()
        {
            Scaling.Instance.MouseExitCode(axis: 3);
        }

        public void OnMouseDown()
        {
            m_totalDist = 0;
            (m_initialMousePosition, m_lastProjectedMousePosition, m_moveDirection) = Scaling.Instance.MouseDownCode(axis: 3);
        }

        public void OnMouseUp()
        {
            Scaling.Instance.MouseUpCode();
        }

        public void OnMouseDrag()
        {
            (m_totalDist, m_lastProjectedMousePosition) = Scaling.Instance.MouseDragCode(m_initialMousePosition, m_moveDirection, m_lastProjectedMousePosition, m_totalDist, axis: 3);
        }
    }
}
