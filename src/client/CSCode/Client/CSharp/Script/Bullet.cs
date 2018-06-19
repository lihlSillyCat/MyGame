using UnityEngine;
using XLua;
using War.Base;
using War.Game;
using War.Scene;

namespace War.Script
{
    [LuaCallCSharp]
    public class Bullet : War.Script.BaseObject
    {
        protected Vector3 m_Position;

        protected Vector3 m_Velocity;
        private ObjectToMove m_BulletEntity;
        private Vector3 m_TotalDis = Vector3.zero;
        [CSharpCallLua]
        public delegate void OnBulletHitEventHandler(RaycastHit hit);
        public OnBulletHitEventHandler onBulletHit;
        private float m_DisFactor = 1.0f;
        private static string m_empty = "";

        public Bullet()
        {

        }

        public void InitAssetPath(string path)
        {
            float defaultView = 45.0f;
            if (Camera.main != null)
            {
                defaultView = Camera.main.fieldOfView;
            }
            m_DisFactor = Mathf.Tan(defaultView / 200.0f * Mathf.Deg2Rad);
            GameObjectPool.GetAsync(ref path, ref m_empty, (bulletObj) =>
            {
                m_BulletEntity = (bulletObj as GameObject).GetComponent<ObjectToMove>();
                if (m_IsDestroyed)
                {
                    GameObjectPool.Release(bulletObj);
                    m_BulletEntity = null;
                }
            }, (int)RES_TYPE.RES_TYPE_DYNAMIC, 0);
        }



        public void SetPosition(float x, float y, float z)
        {
            m_Position = new Vector3(x, y, z);
        }

        public void SetVelocity(float x, float y, float z)
        {
            m_Velocity = new Vector3(x, y, z);
        }

        public override void FixedUpdate(float fixedDeltaTime)
        {
            var position = StreamerManager.GetTilePosition(m_Position.x, m_Position.y, m_Position.z);

            m_Velocity += Physics.gravity * Time.fixedDeltaTime;
            var deltaPos = m_Velocity * Time.fixedDeltaTime;

            m_Position += deltaPos;
            m_TotalDis += deltaPos;
            if(m_BulletEntity != null)
            {
                m_BulletEntity.SetPosition(m_Position);
                m_BulletEntity.gameObject.transform.localScale = Vector3.one * m_TotalDis.magnitude * m_DisFactor;
            }

            RaycastHit hit;
            if (Physics.Raycast(position, deltaPos.normalized, out hit, deltaPos.magnitude, LayerConfig.BulletMask))
            {
                if (onBulletHit != null)
                {
                    onBulletHit(hit);
                }
            }
        }

        public override void Dispose()
        {
            onBulletHit = null;
            m_TotalDis = Vector3.zero;
            if (m_BulletEntity != null)
            {
                GameObjectPool.Release(m_BulletEntity.gameObject);
                m_BulletEntity = null;
            }
            base.Dispose();
        }
    }
}