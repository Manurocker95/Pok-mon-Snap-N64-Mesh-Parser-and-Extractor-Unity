using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class ObjectSpawn 
    {
        public long ID;
        public long Behaviour;
        public Vector3 Position;
        public Vector3 Euler;
        public Vector3 Scale;
        public TrackPath Path; // opcional
    }
}
