using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class RandomCircle : Motion
    {
        public override MotionKind Kind => MotionKind.projectile;

        public float Radius;
        public float MaxTurn;
    }
}
