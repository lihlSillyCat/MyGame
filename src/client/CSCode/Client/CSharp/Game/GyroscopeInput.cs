using UnityEngine;
using System.Collections;

namespace War.Game
{
    public class GyroscopeInput : MonoBehaviour
    {
        private bool m_IsEnable = false;
        public delegate void GetGyroscopeInput(float x, float y, float z);
        public GetGyroscopeInput onGetGyroscopeInput;
        // Update is called once per frame
        void Update()
        {
            if (m_IsEnable)
            {
                if (onGetGyroscopeInput != null)
                {
                    Vector3 rotationRate = Input.gyro.rotationRate * Time.deltaTime;
                    onGetGyroscopeInput(rotationRate.x, rotationRate.y, rotationRate.z);
                }
            }
        }

        private void OnEnable()
        {
            Input.gyro.enabled = true;
            m_IsEnable = true;
        }

        private void OnDisable()
        {
            Input.gyro.enabled = false;
            m_IsEnable = false;
        }

        private Quaternion GyroToUnity(Quaternion q)
        {
            return new Quaternion(q.x, q.y, -q.z, -q.w);
        }
    }
}
