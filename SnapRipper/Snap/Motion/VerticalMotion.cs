using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class VerticalMotion : Motion
    {
        public override MotionKind Kind => MotionKind.vertical;

        public ObjParam Target;
        public bool AsDelta;
        public float StartSpeed;
        public float G;
        public float MinVel;
        public float MaxVel;
        public Direction Direction;
    }
}
