using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class LinearPoint : Motion
    {
        public override MotionKind Kind => MotionKind.linear;

        public long Duration;
        public Vector3 Velocity;
        public float TurnSpeed;
        public bool MatchTarget;
    }
}
