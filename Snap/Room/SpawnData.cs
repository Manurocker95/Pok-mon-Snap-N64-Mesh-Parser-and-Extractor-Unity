using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class SpawnData 
    {
        public long ID;
        public long Behavior;
        public Vector3 Scale;
        public Direction Yaw;
    }
}
