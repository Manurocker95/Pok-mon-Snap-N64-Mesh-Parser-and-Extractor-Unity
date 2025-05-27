using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.BanjoKazooie
{
    public enum RSP_Geometry : long
    {
        G_ZBUFFER = 1 << 0,
        G_SHADE = 1 << 2,
        G_CULL_FRONT = 1 << 9,
        G_CULL_BACK = 1 << 10,
        G_FOG = 1 << 16,
        G_LIGHTING = 1 << 17,
        G_TEXTURE_GEN = 1 << 18,
        G_TEXTURE_GEN_LINEAR = 1 << 19,
        G_SHADING_SMOOTH = 1 << 21,
        G_CLIPPING = 1 << 23,
    }
}