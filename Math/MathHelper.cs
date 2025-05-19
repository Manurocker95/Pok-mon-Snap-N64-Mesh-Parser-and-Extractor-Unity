using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public static class MathHelper 
    {
        public static float BitsAsFloat32(long x)
        {
            uint uintValue = (uint)(x & 0xFFFFFFFF);
            byte[] bytes = BitConverter.GetBytes(uintValue);
            return BitConverter.ToSingle(bytes, 0);
        }
    }
}
