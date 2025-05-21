using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class StateGraph 
    {
        public List<State> states = new List<State>();
        public List<AnimationData> animations = new List<AnimationData>();
    }
}
