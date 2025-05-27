using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class TrackEntry
    {
        public EntryKind Kind;
        public long Flags;
        public long Increment;
        public bool Block;
        public VP_Float32Array<VP_ArrayBuffer> Data;
        public TrackPath Path;
        public List<Vector4> Colors = new List<Vector4>();
    }
}
