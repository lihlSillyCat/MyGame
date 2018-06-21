using UnityEngine;
using System.Collections;

namespace War.Game
{
    [SharedBetweenAnimators]
    public class RigidityState : StateMachineBehaviour
    {
        public enum RigidityType
        {
            rigidity = 0,
            fullRigidity = 1,
        }
        public RigidityType rigidityType = RigidityType.rigidity;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var characterEntity = animator.GetComponent<CharacterEntity>();
            if (characterEntity != null)
            {
                switch (rigidityType)
                {
                    case RigidityType.rigidity:
                        characterEntity.OnEnterRigidity();
                        break;
                    case RigidityType.fullRigidity:
                        characterEntity.OnEnterFullRigidity();
                        break;
                    default:
                        break;

                }
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var characterEntity = animator.GetComponent<CharacterEntity>();
            if (characterEntity != null)
            {
                switch (rigidityType)
                {
                    case RigidityType.rigidity:
                        characterEntity.OnLeaveRigidity();
                        break;
                    case RigidityType.fullRigidity:
                        characterEntity.OnLeaveFullRigidity();
                        break;
                    default:
                        break;

                }
            }
        }
    }
}