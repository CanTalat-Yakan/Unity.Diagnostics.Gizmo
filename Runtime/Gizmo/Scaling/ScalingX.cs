using UnityEngine;

namespace TransformGizmos
{
    public class ScalingX : MonoBehaviour, IGizmoTransforms
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

            if (transform.childCount > 0)
                Scaling.Instance.StartCode(renderer, transform.GetChild(0).GetComponent<MeshRenderer>(), m_defaultMaterial, m_hoveredMaterial, gameObject, transform.GetChild(0).gameObject, axis: 0);
            else
                Scaling.Instance.StartCode(renderer, transform.parent.GetComponent<MeshRenderer>(), m_defaultMaterial, m_hoveredMaterial, transform.parent.gameObject, gameObject, axis: 0);
        }

        public void OnMouseEnter()
        {
            Scaling.Instance.MouseEnterCode(axis: 0);
        }

        public void OnMouseExit()
        {
            Scaling.Instance.MouseExitCode(axis: 0);
        }

        public void OnMouseDown()
        {
            m_totalDist = 0;
            (m_initialMousePosition, m_lastProjectedMousePosition, m_moveDirection) = Scaling.Instance.MouseDownCode(axis: 0);
        }

        public void OnMouseUp()
        {
            Scaling.Instance.MouseUpCode();

        }

        public void OnMouseDrag()
        {
            (m_totalDist, m_lastProjectedMousePosition) = Scaling.Instance.MouseDragCode(m_initialMousePosition, m_moveDirection, m_lastProjectedMousePosition, m_totalDist, axis: 0);
        }
    }
}
