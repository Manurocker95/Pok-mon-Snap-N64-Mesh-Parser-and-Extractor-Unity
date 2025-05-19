using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]   
    public class AnimationData
    {
        public long FPS;
        public long Frames;
        public List<AnimationTrack> Tracks = new List<AnimationTrack>();
        public List<List<AnimationTrack>> MaterialTracks = new List<List<AnimationTrack>>();
    }
}
