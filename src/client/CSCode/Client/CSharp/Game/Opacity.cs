using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace War.Game
{
    public class Opacity : MonoBehaviour
    {
        public double opacity = 1.0f;

        [SerializeField]
        private MeshRenderer[] m_MeshRenderers = null;

        [SerializeField]
        private Color[] m_MeshColors = null;

        public void SetOpacity(double opacity)
        {
            if (this.opacity != opacity)
            {
                var rendererCount = m_MeshRenderers.Length;
                for (int i = 0; i < rendererCount; ++i)
                {
                    var meshRenderer = m_MeshRenderers[i];
                    Color c = m_MeshColors[i];
                    c.a *= (float)opacity;
                    meshRenderer.sharedMaterial.SetColor("_TintColor", c);
                }

                this.gameObject.SetActive(opacity > 0.0001f);
                this.opacity = opacity;
            }
        }
    }

}
