using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public enum CCMUX
    {
        COMBINED = 0,
        TEXEL0 = 1,
        TEXEL1 = 2,
        PRIMITIVE = 3,
        SHADE = 4,
        ENVIRONMENT = 5,
        ONE = 6,
        ADD_ZERO = 7,
        COMBINED_A = 7, // only for C
        TEXEL0_A = 8,
        TEXEL1_A = 9,
        PRIMITIVE_A = 10,
        SHADE_A = 11,
        ENV_A = 12,
        PRIM_LOD = 14,
        MUL_ZERO = 15
    }

}
