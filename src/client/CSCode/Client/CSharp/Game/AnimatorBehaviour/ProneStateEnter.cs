using UnityEngine;
using System.Collections;

namespace War.Game
{
    [SharedBetweenAnimators]
    public class ProneStateEnter : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var characterEntity = animator.GetComponent<CharacterEntity>();
            if (characterEntity != null)
            {
                characterEntity.OnProneStateEnter();
            }
        }
    }
}
