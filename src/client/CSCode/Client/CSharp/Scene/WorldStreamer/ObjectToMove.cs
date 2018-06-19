using UnityEngine;
using System.Collections;

namespace War.Scene
{
    /// <summary>
    /// Object to move by world mover.
    /// </summary>
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    [DisallowMultipleComponent]
    public class ObjectToMove : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField]
        protected Vector3 m_WorldPosition;
#endif
        private void OnEnable()
        {
            if (StreamerManager.Instance != null)
            {
                StreamerManager.Instance.AddObjectToMove(transform);
            }
        }

        private void OnDisable()
        {
            if (StreamerManager.Instance != null)
            {
                StreamerManager.Instance.RemoveObjectToMove(transform);
            }
        }

        public void SetPosition(float x, float y, float z)
        {
            transform.position = StreamerManager.GetTilePosition(x, y, z);
        }

        public void SetPosition(Vector3 pos)
        {
            transform.position = StreamerManager.GetTilePosition(pos);
        }

        public Vector3 GetPosition()
        {
            return StreamerManager.GetRealPosition(transform.position);
        }

        public void SetAngle(float x, float y, float z)
        {
            Vector3 angle = transform.rotation.eulerAngles;
            angle.x += x;
            angle.y += y;
            angle.z += z;
            transform.rotation = Quaternion.Euler(angle);
        }

        public float GetPlayerDistance()
        {
            return StreamerManager.GetPlayerDistance(transform.position);
        }

        public float GetPlayerDistanceSqr()
        {
            return StreamerManager.GetPlayerDistanceSqr(transform.position);
        }


#if UNITY_EDITOR
        void Update()
        {
            m_WorldPosition = GetPosition();
        }
#endif
    }
}