using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public interface GfxRenderPass : IGfxPass
    {
        // State management
        void SetViewport(long x, long y, long w, long h);
        void SetScissor(long x, long y, long w, long h);
        void SetPipeline(GfxRenderPipeline pipeline);
        void SetBindings(long bindingLayoutIndex, GfxBindings bindings, List<long> dynamicByteOffsets);
        void SetVertexInput(GfxInputLayout inputLayout, List<GfxVertexBufferDescriptor> buffers, GfxIndexBufferDescriptor indexBuffer);
        void SetStencilRef(long value);
        void SetBlendColor(GfxColor color);

        // Draw commands
        void Draw(long vertexCount, long firstVertex);
        void DrawIndexed(long indexCount, long firstIndex);
        void DrawIndexedInstanced(long indexCount, long firstIndex, long instanceCount);

        // Query system
        void BeginOcclusionQuery(long dstOffs);
        void EndOcclusionQuery();

        // Debug
        void BeginDebugGroup(string name);
        void EndDebugGroup();
    }

}
