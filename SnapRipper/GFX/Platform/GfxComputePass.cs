using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public interface GfxComputePass : IGfxPass
    {
        // State management
        void SetPipeline(GfxComputePipeline pipeline);
        void SetBindings(long bindingLayoutIndex, object bindings, List<long> dynamicByteOffsets);

        // Dispatch commands
        void Dispatch(long x, long y, long z);

        // Debug
        void BeginDebugGroup(string name);
        void EndDebugGroup();
    }

}
