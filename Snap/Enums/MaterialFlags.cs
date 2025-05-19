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
        Special = 0x0010, // cambia suavemente entre una lista de texturas
        Tile0 = 0x0020, // configura la posición de tile0
        Tile1 = 0x0040, // configura la posición de tile1
        Scale = 0x0080, // emite comando de textura, activa tile0 y escala

        Prim = 0x0200, // configura color primario
        Env = 0x0400,
        Blend = 0x0800,
        Diffuse = 0x1000,
        Ambient = 0x2000
    }

}
