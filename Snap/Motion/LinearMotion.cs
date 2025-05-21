using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class LinearMotion : Motion
    {
        public override MotionKind Kind => MotionKind.linear;

        public float Duration;
        public Vector3 Velocity;
        public float TurnSpeed;
        public bool MatchTarget;
    }
}
