using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public enum FormatTypeFlags : long
    {
        U8 = 0x01,
        U16 = 0x02,
        U32 = 0x03,
        S8 = 0x04,
        S16 = 0x05,
        S32 = 0x06,
        F16 = 0x07,
        F32 = 0x08,

        // Compressed texture formats.
        BC1 = 0x41,
        BC2 = 0x42,
        BC3 = 0x43,
        BC4_UNORM = 0x44,
        BC4_SNORM = 0x45,
        BC5_UNORM = 0x46,
        BC5_SNORM = 0x47,
        BC6H_UNORM = 0x48,
        BC6H_SNORM = 0x49,
        BC7 = 0x4A,

        // Packed texture formats.
        U16_PACKED_5551 = 0x61,
        U16_PACKED_565 = 0x62,

        // Depth/stencil texture formats.
        D24 = 0x81,
        D32F = 0x82,
        D24S8 = 0x83,
        D32FS8 = 0x84,
    }
}
