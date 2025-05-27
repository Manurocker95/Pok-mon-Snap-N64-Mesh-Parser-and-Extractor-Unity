using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class GraphImpl
    {
        // Optional metadata for identifying the graph system.
        public string Species => "GfxrGraph";

        // Used for determining scheduling.
        public List<GfxrRenderTargetDescription> RenderTargetDescriptions { get; } = new();
        public List<int> ResolveTextureRenderTargetIDs { get; } = new();

        // The list of render/computation passes.
        public List<PassImpl> Passes { get; } = new();

        // Debugging information.
        public List<string> RenderTargetDebugNames { get; } = new();
        public List<GfxrDebugThumbnailDesc> DebugThumbnails { get; } = new();
    }

}
