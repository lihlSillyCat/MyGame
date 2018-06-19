using UnityEngine;
using System.Collections;

namespace War.Game
{
    public class ChuteEntity : MonoBehaviour
    {
        public SkinnedMeshRenderer m_SkinnedMeshRenderer;
        // Use this for initialization
        public void SetVisible(bool enabled)
        {
            if(m_SkinnedMeshRenderer != null)
            {
                m_SkinnedMeshRenderer.enabled = enabled;
            } 
        }
    }
}
