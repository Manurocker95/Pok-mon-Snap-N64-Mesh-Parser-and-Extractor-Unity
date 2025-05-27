using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public readonly struct GfxrResolveTextureID
    {
        public readonly int Value;

        public GfxrResolveTextureID(int value)
        {
            Value = value;
        }

        public static implicit operator int(GfxrResolveTextureID id) => id.Value;
        public static explicit operator GfxrResolveTextureID(int value) => new GfxrResolveTextureID(value);

        public override string ToString() => $"ResolveTextureID({Value})";
    }
}
