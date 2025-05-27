using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public enum MaterialFlags : long
    {
        Tex1 = 0x0001,
        Tex2 = 0x0002,
        Palette = 0x0004,
        PrimLOD = 0x0008,
        Special = 0x0010,
        Tile0 = 0x0020,
        Tile1 = 0x0040,
        Scale = 0x0080, 

        Prim = 0x0200,
        Env = 0x0400,
        Blend = 0x0800,
        Diffuse = 0x1000,
        Ambient = 0x2000
    }

}
