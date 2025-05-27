using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class AnimatorData
    {
        public EntryKind Kind;
        public long Flags;
        public long Increment;
        public VP_Float32Array<VP_ArrayBuffer> Data;
    }


}
