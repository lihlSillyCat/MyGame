using UnityEngine;

namespace War.Base
{
    public class AutoReleaseToPool : MonoBehaviour
    {
        [SerializeField]
        protected float m_Delay;
        private bool m_isInDelayRelease = false;
        private bool m_IsRelease = false;
        void OnEnable()
        {
            m_IsRelease = false;
            if(!m_isInDelayRelease)
            {
                Invoke("AutoRelease", m_Delay);
                m_isInDelayRelease = true;
            }  
        }

        void AutoRelease()
        {
            m_isInDelayRelease = false;
            m_IsRelease = true;
            GameObjectPool.Release(gameObject);
        }

        void OnDestroy()
        {
            if(!m_IsRelease)
            {
                GameObjectPool.ReleaseImmediately(gameObject);
                m_IsRelease = true;
            }
        }
    }
}