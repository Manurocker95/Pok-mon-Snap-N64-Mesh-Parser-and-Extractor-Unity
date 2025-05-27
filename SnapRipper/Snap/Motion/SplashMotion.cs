using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class SplashMotion : Motion
    {
        public override MotionKind Kind => MotionKind.splash;
        public bool OnImpact;
        public long Index;
        public Vector3 Scale;
    }
}
