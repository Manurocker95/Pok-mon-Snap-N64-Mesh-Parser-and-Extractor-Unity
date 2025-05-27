using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class BasicMotion : Motion
    {
        public override MotionKind Kind => MotionKind.basic;

        public BasicMotionKind Subtype;
        public double Param;
    }
}
