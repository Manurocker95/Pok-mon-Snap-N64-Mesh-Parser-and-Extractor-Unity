using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class TrackPath 
    {
        public PathKind Kind;
        public float Length;
        public float Duration;
        public float SegmentRate;

        public VP_Float32Array<VP_ArrayBuffer> Times;
        public VP_Float32Array<VP_ArrayBuffer> Points;
        public VP_Float32Array<VP_ArrayBuffer> Quartics;
    }
}
