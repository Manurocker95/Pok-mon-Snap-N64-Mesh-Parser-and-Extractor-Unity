using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public readonly struct GfxrRenderTargetID
    {
        public readonly int Value;

        public GfxrRenderTargetID(int value)
        {
            Value = value;
        }

        public static implicit operator int(GfxrRenderTargetID id) => id.Value;
        public static explicit operator GfxrRenderTargetID(int value) => new GfxrRenderTargetID(value);

        public override string ToString() => $"RenderTargetID({Value})";
    }
}
