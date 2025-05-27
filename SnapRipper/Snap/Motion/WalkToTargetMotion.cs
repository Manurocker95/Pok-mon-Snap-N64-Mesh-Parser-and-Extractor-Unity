using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class WalkToTargetMotion : Motion
    {
        public override MotionKind Kind => MotionKind.walkToTarget;

        public float Radius;
        public float MaxTurn;
        public bool Away;
    }
}
