using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public enum GfxRendererLayer : long
    {
        BACKGROUND = 0x00,
        ALPHA_TEST = 0x10,
        OPAQUE = 0x20,
        TRANSLUCENT = 0x80,
    }

}
