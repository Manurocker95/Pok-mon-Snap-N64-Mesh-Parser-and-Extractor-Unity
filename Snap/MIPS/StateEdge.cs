using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class StateEdge
    {
        public InteractionType Type;
        public double Param;
        public long Index;
        public long AuxFunc;
    }

}
