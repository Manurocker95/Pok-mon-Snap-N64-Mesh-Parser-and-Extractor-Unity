using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public enum TexCM : long
    {
        WRAP = 0x00, 
        MIRROR = 0x01, 
        CLAMP = 0x02, 
        MIRROR_CLAMP = 0x03,
    }
}
