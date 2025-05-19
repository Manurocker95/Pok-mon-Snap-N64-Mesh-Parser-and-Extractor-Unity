using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class CollisionTree 
    {
        public Vector3 Line;
        public CollisionTree PosSubtree;
        public GroundPlane PosPlane;
        public CollisionTree NegSubtree;
        public GroundPlane NegPlane;
    }
}
