using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class ParticleManager
    {
        public List<Emitter> EmitterPool = Enumerable.Range(0, 20).Select(_ => new Emitter()).ToList();
        public List<Particle> ParticlePool = Enumerable.Range(0, 400).Select(_ => new Particle()).ToList();
        public List<Vector3> RefPositions = Enumerable.Range(0, 8).Select(_ => Vector3.zero).ToList();

        private VP_Partial<GfxMegaStateDescriptor> MegaStateFlags;
        private SpriteData SpriteData;
        private List<List<TextureData>> CommonData = new();
        private List<List<TextureData>> LevelData = new();

        private CustomParticleSystem Level;
        private CustomParticleSystem Common;

        public ParticleManager(GfxDevice device, GfxRenderCache cache, CustomParticleSystem level, CustomParticleSystem common)
        {
            this.Level = level;
            this.Common = common;

            // build shared particle sprite buffers
            var vertexBuffer = GfxBufferHelpers.MakeStaticDataBuffer(device, GfxBufferUsage.Vertex, VP_ArrayBufferUtils.CreateFloatArray(new float[] { -1, 1, 0, 1, 1, 0, -1, -1, 0, 1, -1, 0 }).BufferSource);

            var indexBuffer = GfxBufferHelpers.MakeStaticDataBuffer(device, GfxBufferUsage.Index, VP_ArrayBufferUtils.CreateUint16Array(new ushort[] { 0, 2, 3, 0, 1, 3 }).BufferSource);

            var vertexAttributeDescriptors = new List<GfxVertexAttributeDescriptor>()
            {
                new GfxVertexAttributeDescriptor {
                    Location = ParticleProgram.A_Position,
                    BufferIndex = 0,
                    Format = GfxUtils.GetGfxFormatByOrder(GfxFormatOrder.F32_RGB),
                    BufferByteOffset = 0
                }
            };

            var inputLayout = cache.CreateInputLayout(new GfxInputLayoutDescriptor
            {
                IndexBufferFormat = GfxUtils.GetGfxFormatByOrder(GfxFormatOrder.U16_R),
                VertexBufferDescriptors = new List<GfxInputLayoutBufferDescriptor>()
                {
                    new GfxInputLayoutBufferDescriptor() { ByteStride = 12, Frequency = GfxVertexBufferFrequency.PerVertex }
                },
                VertexAttributeDescriptors = vertexAttributeDescriptors
            });

            var vertexBufferDescriptors = new List<GfxVertexBufferDescriptor>
            {
                new GfxVertexBufferDescriptor { Buffer = vertexBuffer, ByteOffset = 0 }
            };

            var indexBufferDescriptor = new GfxIndexBufferDescriptor { Buffer = indexBuffer, ByteOffset = 0 };

            this.SpriteData = new SpriteData
            {
                VertexBuffer = vertexBuffer,
                IndexBuffer = indexBuffer,
                InputLayout = inputLayout,
                VertexBufferDescriptors = vertexBufferDescriptors,
                IndexBufferDescriptor = indexBufferDescriptor
            };

            // create gfx data for all the textures
            foreach (var particle in level.ParticleTextures)
            {
                var data = new List<TextureData>();
                foreach (var tex in particle)
                {
                    var sampler = RDP.RDPUtils.TranslateSampler(device, cache, tex);
                    var texture = RDP.RDPUtils.TranslateToGfxTexture(device, tex);
                    data.Add(new TextureData { Sampler = sampler, Texture = texture });
                }
                this.LevelData.Add(data);
            }

            foreach (var particle in common.ParticleTextures)
            {
                var data = new List<TextureData>();
                foreach (var tex in particle)
                {
                    var sampler = RDP.RDPUtils.TranslateSampler(device, cache, tex);
                    var texture = RDP.RDPUtils.TranslateToGfxTexture(device, tex);
                    data.Add(new TextureData { Sampler = sampler, Texture = texture });
                }
                this.CommonData.Add(data);
            }

            this.MegaStateFlags = new VP_Partial<GfxMegaStateDescriptor>(new GfxMegaStateDescriptor
            {
                DepthCompare = GfxCompareMode.Greater,
                DepthWrite = false,
                CullMode = GfxCullMode.None
            });

            GfxMegaStateDescriptorHelpers.SetAttachmentStateSimple(this.MegaStateFlags.Value, new AttachmentStateSimple
            {
                BlendMode = GfxBlendMode.Add,
                BlendSrcFactor = GfxBlendFactor.SrcAlpha,
                BlendDstFactor = GfxBlendFactor.OneMinusSrcAlpha
            });
        }

        public void SetTexturesEnabled(bool v)
        {
            foreach (var particle in this.ParticlePool)
                particle.SetTexturesEnabled(v);
        }

        public void SetAlphaVisualizerEnabled(bool v)
        {
            foreach (var particle in this.ParticlePool)
                particle.SetAlphaVisualizerEnabled(v);
        }

        public Emitter CreateEmitter(bool common, int index, Matrix4x4? mat)
        {
            var system = common ? Common : Level;
            if (system.Emitters == null || index >= system.Emitters.Count)
                return null;

            for (int i = 0; i < EmitterPool.Count; i++)
            {
                if (EmitterPool[i].Timer >= 0)
                    continue;
            
                EmitterPool[i].Activate(system.Emitters[index], mat);
                return EmitterPool[i];
            }
            return null;
        }

        public Particle CreateParticle(bool common, int index, Vector3 pos, Vector3? vel = null)
        {
            for (int i = 0; i < ParticlePool.Count; i++)
            {
                if (ParticlePool[i].Timer >= 0)
                    continue;
                var system = common ? Common : Level;
                var textures = common ? CommonData : LevelData;
                var data = system.Emitters[index];
                if (vel == null)
                    vel = data.Velocity;
                ParticlePool[i].Activate(data, textures[(int)data.ParticleIndex], pos, vel);
                return ParticlePool[i];
            }
            return null;
        }
        public List<GfxBindingLayoutDescriptor> BindingLayouts()
        {
           return new List<GfxBindingLayoutDescriptor>() { new GfxBindingLayoutDescriptor() { NumUniformBuffers = 2, NumSamplers = 1 } };
        }

        public void PrepareToRender(GfxDevice device, GfxRenderInstManager renderInstManager, ViewerRenderInput viewerInput, bool _flush = false)
        {
            var dt = viewerInput.DeltaTime * 30.0 / 1000.0;

            for (int i = 0; i < EmitterPool.Count; i++)
            {
                if (EmitterPool[i].Timer < 0)
                    continue;
                EmitterPool[i].Update((float)dt, this);
            }

            for (int i = 0; i < ParticlePool.Count; i++)
            {
                if (ParticlePool[i].Timer < 0)
                    continue;
                ParticlePool[i].Update(dt, this);
            }

            if (!_flush)
                return;

            var template = renderInstManager.PushTemplate();
            template.SetBindingLayouts(BindingLayouts());
            template.SetVertexInput(SpriteData.InputLayout, SpriteData.VertexBufferDescriptors, SpriteData.IndexBufferDescriptor);
            template.SetMegaStateFlags(MegaStateFlags);

            template.SortKey = GfxRenderInstUtils.MakeSortKey(GfxRendererLayer.TRANSLUCENT);

            long offs = template.AllocateUniformBuffer((int)ParticleProgram.UbSceneParams, 16);
            var mappedF32 = template.MapUniformBufferF32(ParticleProgram.UbSceneParams);
            GfxBufferHelpers.FillMatrix4x4(mappedF32, offs, viewerInput.Camera.projectionMatrix);

            for (int i = 0; i < ParticlePool.Count; i++)
            {
                if (ParticlePool[i].Timer < 0)
                    continue;
                ParticlePool[i].PrepareToRender(device, renderInstManager, viewerInput);
            }

            renderInstManager.PopTemplate();
        }

        public void Destroy(GfxDevice device)
        {
            device.DestroyBuffer(SpriteData.IndexBuffer);
            device.DestroyBuffer(SpriteData.VertexBuffer);

            for (int i = 0; i < LevelData.Count; i++)
                for (int j = 0; j < LevelData[i].Count; j++)
                    device.DestroyTexture(LevelData[i][j].Texture);

            for (int i = 0; i < CommonData.Count; i++)
                for (int j = 0; j < CommonData[i].Count; j++)
                    device.DestroyTexture(CommonData[i][j].Texture);
        }
    }

}
