using UnityEngine;
using System.Collections;
namespace War.Game
{
    [SharedBetweenAnimators]
    public class RootMotionInJumpState : StateMachineBehaviour
    {
        private Rigidbody m_Rigidbody;
        private CharacterEntity m_CharacterEntity;
        private float m_Speed;
        private float cameraTargetYOffset;
        private float m_TimePassed;
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_TimePassed = 0.0f;
            m_Rigidbody = animator.GetComponent<Rigidbody>();
            m_CharacterEntity = animator.GetComponent<CharacterEntity>();
            cameraTargetYOffset = m_CharacterEntity.cameraTargetYOffset;
            m_Speed = -cameraTargetYOffset / stateInfo.length * 0.75f;
            animator.applyRootMotion = true;
            m_Rigidbody.isKinematic = true;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!m_Rigidbody.isKinematic)
            {
                m_Rigidbody.isKinematic = true;
            }
            m_TimePassed += Time.deltaTime;
            float curveValue = animator.GetFloat("Curve");
            //m_CharacterEntity.cameraTargetYOffset = Mathf.Lerp(cameraTargetYOffset, 0, m_TimePassed  * m_Speed);
            m_CharacterEntity.cameraTargetYOffset = Mathf.Lerp(cameraTargetYOffset, 0, curveValue);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.applyRootMotion = false;
            m_Rigidbody.isKinematic = false;
            m_CharacterEntity.EndSpecialJump();
            m_CharacterEntity.cameraTargetYOffset = 0;
            m_CharacterEntity.isInSpecialJumpState = false;
        }
    }
}
