using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class ProjectileMotion : Motion
    {
        public override MotionKind Kind => MotionKind.projectile;

        public float ySpeed;
        public Direction Direction;
        public float Yaw;
        public float G;
        public bool MoveForward;
    }
}
