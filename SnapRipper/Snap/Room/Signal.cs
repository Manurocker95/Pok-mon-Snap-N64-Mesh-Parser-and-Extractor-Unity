using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class Signal
    {
        public long Value;
        public long Target;
        public InteractionType Condition;
        public long ConditionParam;
    }
}
