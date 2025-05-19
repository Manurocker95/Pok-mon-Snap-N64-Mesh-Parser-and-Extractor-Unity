using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public enum ParticleFlags : long
    {
        Gravity = 0x0001,
        Drag = 0x0002,
        Orbit = 0x0004,

        SharedPalette = 0x0010, // seems unnecessary
        MirrorS = 0x0020,
        MirrorT = 0x0040,
        TexAsLerp = 0x0080,
        UseRawTex = 0x0100,
        CustomAlphaMask = 0x0200,
        DitherAlpha = 0x0400,
        NoUpdate = 0x0800,

        PosIndex = 0x7000,
        StorePosition = 0x8000,
    }
}
