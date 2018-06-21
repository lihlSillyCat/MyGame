using UnityEngine;
using System.Collections;

namespace War.Game
{
    public class FollowBoneRotation : MonoBehaviour
    {

        [SerializeField]
        protected Transform m_FollowTransform;

        public void OnRenderVisible(bool visible)
        {
            enabled = visible;
        }

        void LateUpdate()
        {
            //transform.position = m_FollowTransform.position;
            transform.rotation = m_FollowTransform.rotation * transform.localRotation;
        }
    }
}
