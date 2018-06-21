using UnityEngine;
using UnityEngine.Events;

namespace War.Game
{
    public class ReloadStateEventTrigger : StateMachineBehaviour
    {
        public float triggerTime = 0.0f;
        private float m_Now;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            int integer = (int)stateInfo.normalizedTime;
            m_Now = stateInfo.normalizedTime - integer;
            var charaterEntity = animator.GetComponent<CharacterEntity>();
            charaterEntity.StartReload();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            float preTime = m_Now;
            int integer = (int)stateInfo.normalizedTime;
            m_Now = stateInfo.normalizedTime - integer;
            if (preTime <= triggerTime && triggerTime < m_Now)
            {
                var charaterEntity = animator.GetComponent<CharacterEntity>();
                charaterEntity.FinishReload();
            }
        }
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var charaterEntity = animator.GetComponent<CharacterEntity>();
            charaterEntity.ExitReload();
        }
    }
}