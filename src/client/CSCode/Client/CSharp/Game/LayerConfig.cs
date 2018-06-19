using UnityEngine;
using System.Collections;

namespace War.Game
{
    public static class LayerConfig
    {
        public static readonly int BulletMask = LayerMask.GetMask("Default", "PlayerBody", "Water", "BulletCollider","Melee");
        public static readonly int GroundMask = LayerMask.GetMask("Default");
        public static readonly int WalkMask = LayerMask.GetMask("Default", "Tank");
        public static readonly int ParachuteMask = LayerMask.GetMask("Default", "Water", "Tank");
        public static readonly int CameraMask = LayerMask.GetMask("Default");
        public static readonly int BoxGroundMask = LayerMask.GetMask("Default", "Water");
        public static readonly int PlayerMask = LayerMask.GetMask("PlayerBody");

        public static readonly int Water = LayerMask.NameToLayer("Water");
        public static readonly int Player = LayerMask.NameToLayer("Player");
        public static readonly int Tank = LayerMask.NameToLayer("Tank");
    }
}