using System;
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
        public List<GfxTexture> Textures = new List<GfxTexture>();
        public List<GfxSampler> Samplers = new List<GfxSampler>();
        public VP_Float32Array<VP_ArrayBuffer> VertexBufferData;
        public GfxBuffer IndexBuffer;
        public List<GfxBuffer> DynamicBufferCopies = new List<GfxBuffer>();

        public RSPSharedOutput SharedOutput;

        public RenderData(GfxDevice device, GfxRenderCache cache, RSPSharedOutput sharedOutput)
        {
            this.SharedOutput = sharedOutput;

            var textures = sharedOutput.TextureCache.textures;
            for (int i = 0; i < textures.Count; i++)
            {
                var tex = textures[i];
                this.Textures.Add(RDP.RDPUtils.TranslateToGfxTexture(device, tex));
                this.Samplers.Add(RDP.RDPUtils.TranslateSampler(device, cache, tex));
            }

            this.VertexBufferData = RDP.RDPUtils.MakeVertexBufferData(sharedOutput.Vertices);
            this.VertexBuffer = GfxBufferHelpers.MakeStaticDataBuffer(device, GfxBufferUsage.Vertex, this.VertexBufferData);

            GfxPlatformUtils.Assert((long)sharedOutput.Vertices.Count <= 0xFFFFFFFF);

            var indexBufferData = new VP_Uint32Array(new VP_ArrayBuffer(GfxBufferHelpers.LongListToByteArray(sharedOutput.Indices)));
            this.IndexBuffer = GfxBufferHelpers.MakeStaticDataBuffer(device, GfxBufferUsage.Index, indexBufferData);

            var vertexAttributeDescriptors = new List<GfxVertexAttributeDescriptor> 
            {
                new GfxVertexAttributeDescriptor { Location = F3DEX_Program.A_Position, BufferIndex = 0, Format = GfxUtils.GetGfxFormatByOrder(GfxFormatOrder.F32_RGBA), BufferByteOffset = 0 * 0x04 },
                new GfxVertexAttributeDescriptor { Location = F3DEX_Program.A_TexCoord, BufferIndex = 0, Format = GfxUtils.GetGfxFormatByOrder(GfxFormatOrder.F32_RG), BufferByteOffset = 4 * 0x04 },
                new GfxVertexAttributeDescriptor { Location = F3DEX_Program.A_Color, BufferIndex = 0, Format = GfxUtils.GetGfxFormatByOrder(GfxFormatOrder.F32_RGBA), BufferByteOffset = 6 * 0x04 }
            };

            var vertexBufferDescriptors = new List<GfxInputLayoutBufferDescriptor> 
            {
                new GfxInputLayoutBufferDescriptor { ByteStride = 10 * 0x04, Frequency = GfxVertexBufferFrequency.PerVertex }
            };

            this.InputLayout = cache.CreateInputLayout(new GfxInputLayoutDescriptor
            {
                IndexBufferFormat = GfxUtils.GetGfxFormatByOrder(GfxFormatOrder.U32_R),
                VertexBufferDescriptors = vertexBufferDescriptors,
                VertexAttributeDescriptors = vertexAttributeDescriptors
            });

            this.VertexBufferDescriptors = new List<GfxVertexBufferDescriptor>
            {
                new GfxVertexBufferDescriptor { Buffer = this.VertexBuffer, ByteOffset = 0 }
            };

            this.IndexBufferDescriptor = new GfxIndexBufferDescriptor
            {
                Buffer = this.IndexBuffer,
                ByteOffset = 0
            };
        }

        public void Destroy(GfxDevice device)
        {
            for (int i = 0; i < this.Textures.Count; i++)
                device.DestroyTexture(this.Textures[i]);

            device.DestroyBuffer(this.IndexBuffer);
            device.DestroyBuffer(this.VertexBuffer);

            for (int i = 0; i < this.DynamicBufferCopies.Count; i++)
                device.DestroyBuffer(this.DynamicBufferCopies[i]);
        }
    }
}
