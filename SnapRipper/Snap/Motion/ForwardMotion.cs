using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class ForwardMotion : Motion
    {
        public override MotionKind Kind => MotionKind.splash;
        public bool StopIfBlocked;
    }
}
