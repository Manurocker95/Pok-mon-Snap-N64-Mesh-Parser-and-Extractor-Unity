using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class GFXNode
    {
        public long Name;
        public Model Model;
        public long Billboard;
        public long Parent;
        public Vector3 Translation;
        public Vector3 Euler;
        public Vector3 Scale;
        public List<MaterialData> Materials = new List<MaterialData>();
    }
}
