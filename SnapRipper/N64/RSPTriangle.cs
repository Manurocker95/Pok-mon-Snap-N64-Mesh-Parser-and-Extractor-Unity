using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class RSPTriangle 
    {
        public List<RSPVertex> Vertices;
        public List<long> Indices;

        public RSPTriangle(List<RSPVertex> vertices, List<long> indices)
        {
            Vertices = vertices;
            Indices = indices;
        }
    }
}
