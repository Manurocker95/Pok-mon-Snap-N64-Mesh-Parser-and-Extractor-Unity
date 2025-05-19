using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class AnimationMotion : Motion
    {
        public override MotionKind Kind => MotionKind.animation;

        public long Index;
        public bool Force;
    }
}
