using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class ApproachPointMotion : Motion
    {
        public override MotionKind Kind => MotionKind.point;
        public ApproachGoal Goal;
        public float MaxTurn;
        public Destination Destination;

    }
}
