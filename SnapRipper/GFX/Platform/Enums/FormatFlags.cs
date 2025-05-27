using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public enum FormatFlags : long
    {
        None = 0b00000000,
        Normalized = 0b00000001,
        sRGB = 0b00000010,
        Depth = 0b00000100,
        Stencil = 0b00001000,
        RenderTarget = 0b00010000,
    }
}
