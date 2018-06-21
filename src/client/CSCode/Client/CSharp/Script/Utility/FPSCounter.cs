using System;
using UnityEngine;
using UnityEngine.UI;

namespace Utility
{
    public class FPSCounter : MonoBehaviour
    {
        const float fpsMeasurePeriod = 0.5f;
        private int m_FpsAccumulator = 0;
        private float m_FpsNextPeriod = 0;

        public uint currentFps
        {
            private set;
            get;
        }

        private uint m_nMinFps = 0;                  // 最小fps  本次上报期间内
        private uint m_nMaxFps = 0;                  // 最大fps  本次上报期间内
        private uint m_nFpsCount = 0;                // 统计的fps次数
        private uint m_nTotalFps = 0;                // 总共fps 数值

        private void Start()
        {
            m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
        }


        private void Update()
        {
            // measure average frames per second
            m_FpsAccumulator++;
            if (Time.realtimeSinceStartup > m_FpsNextPeriod)
            {
                currentFps = (uint)(m_FpsAccumulator / fpsMeasurePeriod);
                m_FpsAccumulator = 0;
                m_FpsNextPeriod += fpsMeasurePeriod;

                m_nTotalFps += currentFps;
                m_nFpsCount += 1;

                // 最小fps不统计0 值
                if (m_nMinFps == 0 || currentFps < m_nMinFps)
                {
                    m_nMinFps = currentFps;
                }

                // 最大fps ,不统计最大为0值
                if (m_nMaxFps == 0 || currentFps > m_nMaxFps)
                {
                    m_nMaxFps = currentFps;
                }

            }
        }

        public void Clean()
        {
            m_nMinFps = 0;
            m_nMaxFps = 0;
            m_nFpsCount = 0;
            m_nTotalFps = 0;
        }

        public uint GetMinFps()
        {
            return m_nMinFps;
        }

        public uint GetMaxFps()
        {
            return m_nMaxFps;
        }

        public uint GetFpsCount()
        {
            return m_nFpsCount;
        }

        public uint GetTotalFps()
        {
            return m_nTotalFps;
        }
    }
}
