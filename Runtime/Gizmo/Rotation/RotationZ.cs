using UnityEngine;

namespace TransformGizmos
{
    public class RotationZ : MonoBehaviour, IGizmoTransforms
    {
        [SerializeField] Material m_defaultMaterial;
        [SerializeField] Material m_hoveredMaterial;
        [SerializeField] GameObject m_otherHalf;

        float m_totalDist;
        Vector3[] m_vertices;
        Vector2 m_tangent;
        Vector2 m_initialMousePosition = Vector2.zero;
        Vector2 m_lastProjectedMousePosition = Vector2.zero;

        void Start()
        {
            Rotation.Instance.StartCode(GetComponent<MeshRenderer>(), m_otherHalf.GetComponent<MeshRenderer>(), m_defaultMaterial, m_hoveredMaterial, axis: 2);
        }

        public void OnMouseEnter()
        {
            Rotation.Instance.MouseEnterCode(axis: 2);
        }

        public void OnMouseExit()
        {
            Rotation.Instance.MouseExitCode(axis: 2);
        }

        public void OnMouseDown()
        {
            m_totalDist = 0;
            (m_initialMousePosition, m_lastProjectedMousePosition, m_tangent, m_vertices) = Rotation.Instance.MouseDownCode(transform.up, axis: 2);
        }

        public void OnMouseUp()
        {
            Rotation.Instance.MouseUpCode(axis: 2);
        }

        public void OnMouseDrag()
        {
            (m_totalDist, m_lastProjectedMousePosition) = Rotation.Instance.MouseDragCode(m_initialMousePosition, m_tangent, m_lastProjectedMousePosition, m_totalDist, m_vertices, axis: 2);
        }
    }
}
