using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class StateGraph 
    {
        public List<State> States = new List<State>();
        public List<AnimationData> Animations = new List<AnimationData>();
    }
}
