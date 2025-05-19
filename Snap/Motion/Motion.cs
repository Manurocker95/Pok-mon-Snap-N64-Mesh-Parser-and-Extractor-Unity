using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class Motion 
    {
        public long Flags;

        public virtual MotionKind Kind => MotionKind.none;
    }
}
