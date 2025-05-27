using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class AnimationTrack
    {
        public List<TrackEntry> Entries = new List<TrackEntry>();
        public long LoopStart;
    }
}
