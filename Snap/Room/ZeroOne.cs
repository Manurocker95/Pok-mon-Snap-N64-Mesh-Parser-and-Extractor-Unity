using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class ZeroOne
    {
        public List<AnimationData> Animations = new List<AnimationData>();
        public List<GFXNode> Nodes = new List<GFXNode>();
        public RSPSharedOutput SharedOutput;
    }
}
