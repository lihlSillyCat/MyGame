using UnityEngine;
using System.Collections.Generic;

namespace War.Render
{
    public class ShaderCollectionWarmUp : MonoBehaviour
    {
        [SerializeField]
        private ShaderVariantCollection[] m_ShaderVariantCollections = null;

#pragma warning disable 0414
        [SerializeField]
        private string[] m_ExtraShaderNames = null;

        [SerializeField]
        private List<Shader> m_Shaders = null;
#pragma warning restore 0414

        private void Start()
        {
            Warmup(null);
        }

        public void Warmup(System.Action callback)
        {
            foreach (var shaderCollection in m_ShaderVariantCollections)
            {
                shaderCollection.WarmUp();
            }

            if (callback != null)
            {
                callback();
            }
        }
    }
}