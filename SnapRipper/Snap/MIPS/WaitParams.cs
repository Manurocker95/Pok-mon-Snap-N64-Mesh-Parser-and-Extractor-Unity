using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class WaitParams 
    {
        public bool AllowInteraction;
        public List<StateEdge> Interactions = new List<StateEdge>();
        public float Duration;
        public float DurationRange;
        public long LoopTarget;
        public long EndCondition;
    }
}
