using UnityEngine;
using System.Collections;

namespace War.Game
{
    [SharedBetweenAnimators]
    public class ReloadTimeScale : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            float animationTime = stateInfo.length;
            var characterEntity = animator.GetComponent<CharacterEntity>();
            animator.SetFloat("ReloadSpeedMultiplier", animationTime / characterEntity.reloadTime);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetFloat("ReloadSpeedMultiplier", 1.0f);
        }
    }
}
