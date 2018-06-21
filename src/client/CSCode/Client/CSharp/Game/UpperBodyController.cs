using UnityEngine;
using System.Collections;

namespace War.Game
{
    public class UpperBodyController : MonoBehaviour
    {
        [SerializeField]
        protected Transform m_FollowTransform;
        [SerializeField]
        protected Animator m_Animator;
        
        protected bool m_IsRenderVisible;
        protected bool m_IsNeedFollow;

        private void OnEnable()
        {
            m_IsRenderVisible = true;
        }

        public void OnRenderVisible(bool visible)
        {
            m_IsRenderVisible = visible;
            UpdateEnable();
        }

        public void SetNeedFollow(bool isNeed)
        {
            m_IsNeedFollow = isNeed;
            UpdateEnable();
        }

        private void UpdateEnable()
        {
            enabled = m_IsRenderVisible && m_IsNeedFollow;
        }

        void LateUpdate()
        {
            if(m_Animator.GetBool("UpperBody") == true)
            {
                transform.rotation = m_FollowTransform.rotation * transform.localRotation;
            }
        }
    }
}
