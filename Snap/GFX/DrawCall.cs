using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class DrawCall : BanjoKazooie.DrawCall
    {
        public Vector4 DP_PrimColor = new Vector4(1f, 1f, 1f, 1f);
        public Vector4 DP_EnvColor = new Vector4(1f, 1f, 1f, 1f);
        public float DP_PrimLOD = 0f;
        public long materialIndex = -1;
    }
}
