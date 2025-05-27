using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class FollowPathMotion : Motion
    {
        public override MotionKind Kind => MotionKind.path;

        public ObjParam Speed;
        public PathStart Start;
        public ObjParam End;
        public float MaxTurn;
    }
}
