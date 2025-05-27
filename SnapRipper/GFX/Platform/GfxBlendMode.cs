using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public enum GfxBlendMode
    {
        None = 0,
        Add = 0x8006,             // FUNC_ADD
        Subtract = 0x800A,        // FUNC_SUBTRACT
        ReverseSubtract = 0x800B  // FUNC_REVERSE_SUBTRACT
    }
}
