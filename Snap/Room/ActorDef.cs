using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class ActorDef : ObjectDef
    {
        public List<GFXNode> Nodes = new List<GFXNode>();
        public Vector3 Scale;
        public Vector3 Center;
        public float Radius;
        public long Flags;
        public SpawnType Spawn;
        public StateGraph StateGraph;
        public long GlobalPointer;
    }
}
