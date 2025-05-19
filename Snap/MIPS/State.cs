using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class State 
    {
        public long StartAddress;
        public List<StateBlock> Blocks = new List<StateBlock>();
        public bool DoCleanup;
    }
}
