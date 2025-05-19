using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.BanjoKazooie
{
    public class RenderData 
    {
        public GfxBuffer VertexBuffer;
        public GfxInputLayout InputLayout;
        public List<GfxVertexBufferDescriptor> VertexBufferDescriptors;
        public GfxIndexBufferDescriptor IndexBufferDescriptor;
        public List<GfxTexture> Textures;
        public List<GfxSampler> Samplers;
        public VP_Float32Array<VP_ArrayBuffer> VertexBufferData;
        public GfxBuffer IndexBuffer;

        public List<GfxBuffer> DynamicBufferCopies;

        public RenderData(GfxRenderCache cache, RSPSharedOutput sharedOutput)
        {

        }
    }
}
