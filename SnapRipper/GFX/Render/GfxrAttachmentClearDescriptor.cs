using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class GfxrAttachmentClearDescriptor
    {
        public object ClearColor { get; set; } = "load"; // GfxColor o "load"

        public object ClearDepth { get; set; } = "load"; // float o "load"

        public object ClearStencil { get; set; } = "load"; // int o "load"

        public bool IsClearColorLoad => ClearColor is string s && s == "load";
        public bool IsClearDepthLoad => ClearDepth is string s && s == "load";
        public bool IsClearStencilLoad => ClearStencil is string s && s == "load";

        public GfxColor GetClearColor() => ClearColor as GfxColor;
        public float? GetClearDepth() => ClearDepth is float f ? f : null;
        public int? GetClearStencil() => ClearStencil is int i ? i : null;
    }

}
