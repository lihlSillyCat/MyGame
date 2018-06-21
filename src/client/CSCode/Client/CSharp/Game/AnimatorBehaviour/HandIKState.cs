using UnityEngine;
using System.Collections;

namespace War.Game
{
    [SharedBetweenAnimators]
    public class HandIKState : StateMachineBehaviour
    {
        public enum ToggleType
        {
            Off = 0,
            On  = 1,
        }

        public enum TriggerType
        {
            Enter = 0,
            Exit = 1,
            Both = 2,
        }

        public TriggerType triggerType = TriggerType.Enter;
        public ToggleType leftHandEnterToggle = ToggleType.Off;
        public ToggleType rightHandEnterToggle = ToggleType.Off;

        public ToggleType leftHandExitToggle = ToggleType.Off;
        public ToggleType rightHandExitToggle = ToggleType.Off;

        public bool upperBody = false;
        public bool restoreIKStateExit = false;

        private bool m_LeftHandIKStateEnter = false;
        private bool m_RightHandIKStateEnter = false;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var characterEntity = animator.GetComponent<CharacterEntity>();
            if (characterEntity != null)
            {
                m_LeftHandIKStateEnter = characterEntity.leftHandIKToggle;
                m_RightHandIKStateEnter = characterEntity.rightHandIKToggle;
                if (upperBody == true)
                {
                    characterEntity.upperBodyIkEnter++;
                    characterEntity.upperBodyLeftHandPosWeight = leftHandEnterToggle == ToggleType.Off ? 0.0f : 1.0f;
                    characterEntity.upperBodyLeftHandRotWeight = leftHandEnterToggle == ToggleType.Off ? 0.0f : 1.0f;
                    characterEntity.upperBodyRightHandPosWeight = rightHandEnterToggle == ToggleType.Off ? 0.0f : 1.0f;
                    characterEntity.upperBodyRightHandPosWeight = rightHandEnterToggle == ToggleType.Off ? 0.0f : 1.0f;
                    return;
                }
                if (triggerType == TriggerType.Enter || triggerType == TriggerType.Both)
                {
                    bool leftHandIKToggle = leftHandEnterToggle == ToggleType.Off ? false : true;
                    bool rightHandIKToggle = rightHandEnterToggle == ToggleType.Off ? false : true;
                    characterEntity.OnLeftHandIK(leftHandIKToggle);
                    characterEntity.OnRightHandIK(rightHandIKToggle);
                }
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var characterEntity = animator.GetComponent<CharacterEntity>();
            if(upperBody == true)
            {
                characterEntity.upperBodyIkEnter--;
                return;
            }
            if (restoreIKStateExit == true)
            {
                if (m_LeftHandIKStateEnter == true)
                {
                    characterEntity.OnLeftHandIK(m_LeftHandIKStateEnter);
                }
                if (m_RightHandIKStateEnter == true)
                {
                    characterEntity.OnRightHandIK(m_RightHandIKStateEnter);
                }
                return;
            }
            if (triggerType == TriggerType.Exit || triggerType == TriggerType.Both)
            {
                bool leftHandIKToggle = leftHandExitToggle == ToggleType.Off ? false : true;
                bool rightHandIKToggle = rightHandExitToggle == ToggleType.Off ? false : true;
                characterEntity.OnLeftHandIK(leftHandIKToggle);
                characterEntity.OnRightHandIK(rightHandIKToggle);
            }
        }
    }
}
