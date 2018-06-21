using UnityEngine;
using System.Collections;

namespace War.Game
{
    [SharedBetweenAnimators]
    public class JumpToPosState : StateMachineBehaviour
    {
        CharacterEntity m_CharacterEntity;
        private Rigidbody m_Rigidbody;
        private Vector3 m_OriginalPos;
        private float m_OriginalPosY;
        private float cameraTargetYOffset;
        // Use this for initialization
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_CharacterEntity = animator.GetComponent<CharacterEntity>();
            m_CharacterEntity.isInSpecialJumpState = true;
            m_Rigidbody = animator.GetComponent<Rigidbody>();
            m_OriginalPosY = m_CharacterEntity.transform.position.y;
            m_OriginalPos = m_CharacterEntity.transform.position;
            float height = m_CharacterEntity.posForJump.y - m_OriginalPosY;
            cameraTargetYOffset = m_OriginalPosY - m_CharacterEntity.posForJump.y;
            m_Rigidbody.isKinematic = true;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            float curveValue = animator.GetFloat("Curve");
            m_CharacterEntity.transform.position = Vector3.Lerp(m_OriginalPos, m_CharacterEntity.posForJump, curveValue);
            m_CharacterEntity.cameraTargetYOffset = m_OriginalPosY - m_CharacterEntity.transform.position.y;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_CharacterEntity.transform.position = m_CharacterEntity.posForJump;
            m_CharacterEntity.cameraTargetYOffset = cameraTargetYOffset;
            //m_Rigidbody.isKinematic = false;
        }
    }
}
