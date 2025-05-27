using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.PokemonSnap;
using VirtualPhenix.Nintendo64;

namespace VirtualPhenix.PokemonSnap64
{
    [System.Serializable]
    public class PKSnap_TrackPath 
    {
        public PathKind Kind;
        public float Length;
        public float Duration;
        public float SegmentRate;

        public float[] Times;
        public float[] Points;
        public float[] Quartics;
    }

}