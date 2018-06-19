using UnityEngine;
using System.Collections;

namespace War.Game
{
    public class Scope : MonoBehaviour
    {
        public new Camera camera;              // 主摄像机
        public float magFactor             // 放大倍数
        {
            get
            {
                return m_MagFactor;
            }
            set
            {
                m_MagFactor = value;
                float cosAlpha = Mathf.Cos(oldfildOfView);
                fieldOfView = Mathf.Acos(cosAlpha * Mathf.Sqrt(m_MagFactor));
            }
        } 
        private float fieldOfView = 0;    // 根据放大倍数计算出的视野
        private float oldfildOfView = 0;  // 本来的视野
        private float m_MagFactor = 1;
        private bool m_IsScope = false;
        // Use this for initialization
        void Start()
        {
            oldfildOfView = camera.fieldOfView;
        }

        public void ScopeSwitch()
        {
            m_IsScope = !m_IsScope;
            if(m_IsScope)
            {
                StartCoroutine(Open());
            }
            else
            {
                Close();
            }
        }

        public IEnumerator Open()
        {
            yield return new WaitForSeconds(0.15f);
            camera.fieldOfView = fieldOfView;
        }

        public void Close()
        {
            camera.fieldOfView = oldfildOfView;
        }
    }
}
