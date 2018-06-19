using UnityEngine;
using System.Collections;

namespace War.Game
{
    public class ShellEntity : MonoBehaviour
    {
        public float speed;
        public float delay;

        [SerializeField]
        protected Vector3 m_Direction = Vector3.zero;
        protected Vector3 m_CurrentVelocity = Vector3.zero;

        public delegate void OnCollider();
        public OnCollider colliderCallback = null;

        private static float sGravity = 9.8f;
        private bool m_SimulateFlag = false;

        private Vector3 m_OldPosition = Vector3.zero;
        private Vector3 m_NewPosition = Vector3.zero;
        private Vector3 m_RayDirection = Vector3.one;

        // Use this for initialization
        private void Awake()
        {
            m_SimulateFlag = false;
        }

        private void OnDisable()
        {
            colliderCallback = null;
        }

        public void SetDirection(float x, float y, float z)
        {
            m_Direction.Set(x, y, z);
            //mDirection.Normalize();
        }

        public void StartSimulate(float x, float y, float z)
        {
            m_SimulateFlag = true;
            m_CurrentVelocity = m_Direction * speed;

            m_OldPosition.Set(x, y, z);
            m_OldPosition = SimulatePosition(m_OldPosition, delay);

            transform.position = m_OldPosition;
            SimulateVelocity(delay);
        }

        private void SimulateVelocity(float time)
        {
            m_CurrentVelocity.y -= sGravity * time;
        }

        private Vector3 SimulatePosition(Vector3 pos, float time)
        {
            pos += m_CurrentVelocity * time;
            pos.y -= 0.5f * sGravity * time * time;
            return pos;
        }

        // Update is called once per frame
        private void Update()
        {
            if (!m_SimulateFlag)
            {
                return;
            }
            m_NewPosition = SimulatePosition(m_OldPosition, Time.deltaTime);
            m_RayDirection = m_NewPosition - m_OldPosition;
            float distance = m_RayDirection.magnitude;
            m_RayDirection.Normalize();
            RaycastHit hitInfo;
            if (Physics.Raycast(m_OldPosition, m_RayDirection, out hitInfo, distance, LayerConfig.BulletMask))
            {
                m_NewPosition = hitInfo.point;
                transform.position = m_NewPosition;
                if(colliderCallback != null)
                {
                    colliderCallback();
                }
                m_SimulateFlag = false;
                return;
            }
            SimulateVelocity(Time.deltaTime);
            m_OldPosition = m_NewPosition;
            transform.position = m_OldPosition;
        }
    }
}

