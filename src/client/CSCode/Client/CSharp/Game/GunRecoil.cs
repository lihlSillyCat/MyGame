using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;
using System;

namespace War.Game
{
    public class GunRecoil : OffsetModifier
    {
        [System.Serializable]
        public class RecoilOffset
        {
            [Tooltip("产生后座力时，在x, y, z方向上的偏移量")]
            public Vector3 offset;
            [Tooltip("如果上一次回复过程没有完成，对此次偏移量的影响")]
            [Range(0f, 1f)]
            public float additivity = 1f;
            [Tooltip("Max additive recoil for automatic fire.")]
            public float maxAdditiveOffsetMag = 0.2f;

            // Linking this to an effector
            [System.Serializable]
            public class EffectorLink
            {
                [Tooltip("后座力影响的身体部位")]
                public FullBodyBipedEffector effector;
                [Tooltip("权重")]
                public float weight;
            }

            [Tooltip("后座力影响几个部位")]
            public EffectorLink[] effectorLinks;

            private Vector3 additiveOffset;
            private Vector3 lastOffset;

            // Start recoil
            public void Start()
            {
                if (additivity <= 0f) return;

                additiveOffset = Vector3.ClampMagnitude(lastOffset * additivity, maxAdditiveOffsetMag);
            }

            // Apply offset to FBBIK effectors
            public void Apply(IKSolverFullBodyBiped solver, Quaternion rotation, float masterWeight, float length, float timeLeft)
            {
                additiveOffset = Vector3.Lerp(Vector3.zero, additiveOffset, timeLeft / length);
                lastOffset = (rotation * (offset * masterWeight)) + (rotation * additiveOffset);

                foreach (EffectorLink e in effectorLinks)
                {
                    solver.GetEffector(e.effector).positionOffset += lastOffset * e.weight;
                }
            }
        }

        [System.Serializable]
        public enum Handedness
        {
            Right,
            Left
        }
        private float m_WeightTarget = 0.0f;
        public Vector3 gunDirection;
        [Tooltip("Which hand is holding the weapon?")]
        public Handedness handedness;
        public float blendTime;
        public RecoilOffset offset;
        private float m_Length = 1.0f;
        private float m_EndTime = 0.0f;
        private float m_MagnitudeMultiplier = 1.0f;
        private float m_Weight = 0.0f;
        private bool m_Initiated = false;

        public void RecoilDist(float d)
        {
            m_WeightTarget = d;
        }

        protected override void OnModifyOffset()
        {
            if (Mathf.Abs(m_WeightTarget) <= Mathf.Epsilon)
            {
                return;
            }

            if (ik != null && !m_Initiated)
            {
                m_Initiated = true;
            }

            blendTime = Mathf.Max(blendTime, 0.0f);
            float blendWeight = 0.0f;
            if (blendTime > 0.0f)
            {
                blendWeight = Mathf.Min(blendWeight + Time.deltaTime * (1f / blendTime), 1f);
            }
            else
            {
                blendWeight = 1.0f;
            }

            m_WeightTarget *= m_MagnitudeMultiplier;
            m_Weight = Mathf.Lerp(m_Weight, m_WeightTarget, blendWeight);

            var lookRotation = Quaternion.LookRotation(gunDirection, ik.references.root.up);
            offset.Apply(ik.solver, lookRotation, m_Weight, m_Length, m_EndTime - Time.time);
        }
    }
}
