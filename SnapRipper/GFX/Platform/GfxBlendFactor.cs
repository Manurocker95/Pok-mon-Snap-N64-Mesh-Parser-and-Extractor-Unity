using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public enum GfxBlendFactor : long
    {
        None = -1,
        Zero = 0x0000,
        One = 0x0001,
        Src = 0x0300,                  // SRC_COLOR
        OneMinusSrc = 0x0301,         // ONE_MINUS_SRC_COLOR
        SrcAlpha = 0x0302,            // SRC_ALPHA
        OneMinusSrcAlpha = 0x0303,    // ONE_MINUS_SRC_ALPHA
        DstAlpha = 0x0304,            // DST_ALPHA
        OneMinusDstAlpha = 0x0305,    // ONE_MINUS_DST_ALPHA
        Dst = 0x0306,                 // DST_COLOR
        OneMinusDst = 0x0307,         // ONE_MINUS_DST_COLOR
        ConstantColor = 0x8001,       // CONSTANT_COLOR
        OneMinusConstantColor = 0x8002 // ONE_MINUS_CONSTANT_COLOR
    }
}
