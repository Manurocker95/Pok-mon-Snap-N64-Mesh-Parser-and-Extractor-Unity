using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public static class EggUtils
    {
        public static void EggInputSetup(GfxRenderCache cache, RenderData data, VP_Float32Array<VP_ArrayBuffer> vertices)
        {
            var device = cache.device;
            var eggBuffer = GfxBufferHelpers.MakeStaticDataBuffer(device, GfxBufferUsage.Vertex, vertices.BufferSource);
            data.DynamicBufferCopies.Add(eggBuffer); // put it here to make sure it gets destroyed later

            List<GfxVertexAttributeDescriptor> vertexAttributeDescriptors = new List<GfxVertexAttributeDescriptor>()
            {
                new GfxVertexAttributeDescriptor { Location = EggProgram.A_Position, BufferIndex = 0, Format = GfxUtils.GetGfxFormatByOrder(GfxFormatOrder.F32_RGBA), BufferByteOffset = 0 * 0x04 },
                new GfxVertexAttributeDescriptor { Location = EggProgram.A_TexCoord, BufferIndex = 0, Format = GfxUtils.GetGfxFormatByOrder(GfxFormatOrder.F32_RG), BufferByteOffset = 4 * 0x04 },
                new GfxVertexAttributeDescriptor { Location = EggProgram.A_Color, BufferIndex = 0, Format = GfxUtils.GetGfxFormatByOrder(GfxFormatOrder.F32_RGBA), BufferByteOffset = 6 * 0x04 },
                new GfxVertexAttributeDescriptor { Location = EggProgram.a_EndPosition, BufferIndex = 1, Format = GfxUtils.GetGfxFormatByOrder(GfxFormatOrder.F32_RGB), BufferByteOffset = 0 * 0x04 },
                new GfxVertexAttributeDescriptor { Location = EggProgram.a_EndColor, BufferIndex = 1, Format = GfxUtils.GetGfxFormatByOrder(GfxFormatOrder.F32_RGB), BufferByteOffset = 3 * 0x04 },
            };

            List<GfxInputLayoutBufferDescriptor> vertexBufferDescriptors = new List<GfxInputLayoutBufferDescriptor>()
            {
                new GfxInputLayoutBufferDescriptor { ByteStride = 10 * 0x04, Frequency = GfxVertexBufferFrequency.PerVertex },
                new GfxInputLayoutBufferDescriptor { ByteStride = 6 * 0x04, Frequency = GfxVertexBufferFrequency.PerVertex },
            };

            data.InputLayout = cache.CreateInputLayout(new GfxInputLayoutDescriptor()
            {
                IndexBufferFormat = GfxUtils.GetGfxFormatByOrder(GfxFormatOrder.U32_R),
                VertexBufferDescriptors = vertexBufferDescriptors,
                VertexAttributeDescriptors = vertexAttributeDescriptors,
            });

            data.VertexBufferDescriptors = new List<GfxVertexBufferDescriptor>
            {
                new GfxVertexBufferDescriptor { Buffer = data.VertexBuffer, ByteOffset = 0 },
                new GfxVertexBufferDescriptor { Buffer = eggBuffer, ByteOffset = 0 },
            };
            data.IndexBufferDescriptor = new GfxIndexBufferDescriptor { Buffer = data.IndexBuffer, ByteOffset = 0 };
        }

        public static VP_Float32Array BuildEggData(CRGDataMap dataMap, long id)
        {
            try
            {
                long start = 0;
                long count = 0;

                if (id == 18)
                {
                    start = 0x8018A6F0;
                    count = 0x154;
                }
                else if (id == 20)
                {
                    start = 0x8017C090;
                    count = 0x148;
                }
                else
                {
                    return null;
                }

                var data = new VP_Float32Array(count * 6);
                var view = dataMap.GetView(start);
                var dummyVertex = new StagingVertex();
                long j = 0;

                for (long i = 0; i < count; i++)
                {
                    dummyVertex.SetFromView(view, i << 4);
                    data[j++] = dummyVertex.x;
                    data[j++] = dummyVertex.y;
                    data[j++] = dummyVertex.z;
                    data[j++] = dummyVertex.c0;
                    data[j++] = dummyVertex.c1;
                    data[j++] = dummyVertex.c2;
                }

                return data;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error parsing EGG DATA");
                Debug.LogError(e.Message + " - " + e.StackTrace);
                return new VP_Float32Array();
            }

        }
    }
}
