using UnityEngine;
using System.Collections;
namespace War.Game
{
    [SharedBetweenAnimators]
    public class GunChamberingStateExit : StateMachineBehaviour
    {
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var characterEntity = animator.GetComponent<CharacterEntity>();
            characterEntity.OnChamberingStateExit();
        }
    }
}
