using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class Room 
    {
        public GFXNode Node;
        public List<ObjectSpawn> Objects = new List<ObjectSpawn>();
        public AnimationData Animation;
    }
}
