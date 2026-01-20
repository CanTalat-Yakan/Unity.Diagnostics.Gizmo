using UnityEngine;

namespace TransformGizmos
{
    public class TranslationX : MonoBehaviour, IGizmoTransforms
    {
        [SerializeField] Material m_defaultMaterial;
        [SerializeField] Material m_hoveredMaterial;

        float m_totalDist;
        Vector2 m_moveDirection;
        Vector2 m_initialMousePosition = Vector2.zero;
        Vector2 m_lastProjectedMousePosition = Vector2.zero;

        void Start()
        {
            Translation.Instance.StartCode(GetComponent<MeshRenderer>(), m_defaultMaterial, m_hoveredMaterial, axis: 0);
        }

        public void OnMouseEnter()
        {
            Translation.Instance.MouseEnterCode(axis: 0);
        }

        public void OnMouseExit()
        {
            Translation.Instance.MouseExitCode(axis: 0);
        }

        public void OnMouseDown()
        {
            m_totalDist = 0;
            (m_initialMousePosition, m_lastProjectedMousePosition, m_moveDirection) = Translation.Instance.MouseDownCode(axis: 0);
        }

        public void OnMouseUp()
        {
            Translation.Instance.MouseUpCode();
        }

        public void OnMouseDrag()
        {
            (m_totalDist, m_lastProjectedMousePosition) = Translation.Instance.MouseDragCode(m_initialMousePosition, m_moveDirection, m_lastProjectedMousePosition, m_totalDist, axis: 0);
        }
    }
}
