using UnityEngine;

namespace War.Game
{
    [SharedBetweenAnimators]
    public class FistStateExitTrigger : StateMachineBehaviour
    {
        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var characterEntity = animator.GetComponent<CharacterEntity>();
            characterEntity.OnFistStateExit();
        }
    }
}