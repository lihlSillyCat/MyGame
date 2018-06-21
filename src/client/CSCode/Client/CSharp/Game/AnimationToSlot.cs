using UnityEngine;
using System;

namespace War.Game
{
    public class AnimationToSlot : MonoBehaviour
    {
        [Serializable]
        public struct AnimationInfor
        {
            [SerializeField]
            public string animationName;

            [SerializeField]
            public int layer;
        }

        public AnimationInfor[] takeWeaponAnimationToSlot;
        public AnimationInfor[] putWeaponAnimationToSlot;
    }
}
