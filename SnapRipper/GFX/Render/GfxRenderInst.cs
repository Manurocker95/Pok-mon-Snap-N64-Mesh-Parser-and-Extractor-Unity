using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class GfxRenderInst
    {
        public long SortKey = 0;

        public object Debug = null;
        public string DebugMarker = null;

        private GfxRenderPipelineDescriptor _RenderPipelineDescriptor;

        private GfxRenderDynamicUniformBuffer _UniformBuffer;
        private List<GfxBindingsDescriptor> _BindingDescriptors = GfxPlatformUtils.NArray(1, () => new GfxBindingsDescriptor { BindingLayout = null!, SamplerBindings = new List<GfxSamplerBinding>(), UniformBufferBindings = new List<GfxBufferBinding>() });
        private List<long> _DynamicUniformBufferByteOffsets = GfxPlatformUtils.NArray(4, () => 0L);

        private bool _AllowSkippingPipelineIfNotReady = true;
        private List<GfxVertexBufferDescriptor> _VertexBuffers = null;
        private GfxIndexBufferDescriptor _IndexBuffer = null;
        private long _DrawStart = 0;
        private long _DrawCount = 0;
        private long _DrawInstanceCount = 1;

        private long? _StencilRef = null;
        private GfxColor _BlendColor = null;

        public GfxRenderInst()
        {
            this._RenderPipelineDescriptor = new GfxRenderPipelineDescriptor
            {
                BindingLayouts = new List<GfxBindingLayoutDescriptor>(),
                InputLayout = null,
                MegaStateDescriptor = GfxMegaStateDescriptorHelpers.CopyMegaState(GfxMegaStateDescriptorHelpers.DefaultMegaState),
                Program = null!,
                Topology = GfxPrimitiveTopology.Triangles,
                ColorAttachmentFormats = new List<GfxFormat>(),
                DepthStencilAttachmentFormat = null,
                SampleCount = 1,
            };
        }

        public void CopyFrom(GfxRenderInst o)
        {
            GfxMegaStateDescriptorHelpers.SetMegaStateFlags(this._RenderPipelineDescriptor.MegaStateDescriptor, o._RenderPipelineDescriptor.MegaStateDescriptor);
            this._RenderPipelineDescriptor.Program = o._RenderPipelineDescriptor.Program;
            this._RenderPipelineDescriptor.InputLayout = o._RenderPipelineDescriptor.InputLayout;
            this._RenderPipelineDescriptor.Topology = o._RenderPipelineDescriptor.Topology;

            int maxLen = System.Math.Max(this._RenderPipelineDescriptor.ColorAttachmentFormats.Count, o._RenderPipelineDescriptor.ColorAttachmentFormats.Count);
            while (this._RenderPipelineDescriptor.ColorAttachmentFormats.Count < maxLen)
                this._RenderPipelineDescriptor.ColorAttachmentFormats.Add(null!);

            for (int i = 0; i < o._RenderPipelineDescriptor.ColorAttachmentFormats.Count; i++)
                this._RenderPipelineDescriptor.ColorAttachmentFormats[i] = o._RenderPipelineDescriptor.ColorAttachmentFormats[i];

            this._RenderPipelineDescriptor.DepthStencilAttachmentFormat = o._RenderPipelineDescriptor.DepthStencilAttachmentFormat;
            this._RenderPipelineDescriptor.SampleCount = o._RenderPipelineDescriptor.SampleCount;

            this._UniformBuffer = o._UniformBuffer;
            this._DrawCount = o._DrawCount;
            this._DrawStart = o._DrawStart;
            this._DrawInstanceCount = o._DrawInstanceCount;
            this._VertexBuffers = o._VertexBuffers;
            this._IndexBuffer = o._IndexBuffer;
            this._AllowSkippingPipelineIfNotReady = o._AllowSkippingPipelineIfNotReady;
            this.SortKey = o.SortKey;

            for (int i = 0; i < o._BindingDescriptors.Count; i++)
            {
                var tbd = this._BindingDescriptors[i];
                var obd = o._BindingDescriptors[i];

                if (obd.BindingLayout != null)
                    this._SetBindingLayout(i, obd.BindingLayout);

                int minLength = System.Math.Min(tbd.UniformBufferBindings.Count, obd.UniformBufferBindings.Count);
                for (int j = 0; j < minLength; j++)
                    tbd.UniformBufferBindings[j].WordCount = obd.UniformBufferBindings[j].WordCount;

                this.SetSamplerBindingsFromTextureMappings(obd.SamplerBindings);
            }

            for (int i = 0; i < o._DynamicUniformBufferByteOffsets.Count; i++)
                this._DynamicUniformBufferByteOffsets[i] = o._DynamicUniformBufferByteOffsets[i];
        }

        public void Validate()
        {
            for (int i = 0; i < this._BindingDescriptors.Count; i++)
            {
                var bd = this._BindingDescriptors[i];
                for (int j = 0; j < bd.BindingLayout.NumUniformBuffers; j++)
                    GfxPlatformUtils.Assert(bd.UniformBufferBindings[j].WordCount > 0);
            }

            GfxPlatformUtils.Assert(this._DrawCount > 0);
        }

        public void SetPrimitiveTopology(GfxPrimitiveTopology topology)
        {
            this._RenderPipelineDescriptor.Topology = topology;
        }

        public void SetGfxProgram(GfxProgram program)
        {
            this._RenderPipelineDescriptor.Program = program;
        }

        public GfxMegaStateDescriptor SetMegaStateFlags(VP_Partial<GfxMegaStateDescriptor> r)
        {
            GfxMegaStateDescriptorHelpers.SetMegaStateFlags(this._RenderPipelineDescriptor.MegaStateDescriptor, r);
            return this._RenderPipelineDescriptor.MegaStateDescriptor;
        }

        public GfxMegaStateDescriptor GetMegaStateFlags()
        {
            return this._RenderPipelineDescriptor.MegaStateDescriptor;
        }

        public void SetVertexInput(GfxInputLayout inputLayout, List<GfxVertexBufferDescriptor> vertexBuffers, GfxIndexBufferDescriptor indexBuffer)
        {
            this._VertexBuffers = vertexBuffers;
            this._IndexBuffer = indexBuffer;
            this._RenderPipelineDescriptor.InputLayout = inputLayout;
        }

        private void _SetBindingLayout(int i, GfxBindingLayoutDescriptor bindingLayout)
        {
            GfxPlatformUtils.Assert(bindingLayout.NumUniformBuffers <= this._DynamicUniformBufferByteOffsets.Count);
            this._RenderPipelineDescriptor.BindingLayouts[i] = bindingLayout;

            var bindingDescriptor = this._BindingDescriptors[i];
            bindingDescriptor.BindingLayout = bindingLayout;

            for (int j = bindingDescriptor.UniformBufferBindings.Count; j < bindingLayout.NumUniformBuffers; j++)
                bindingDescriptor.UniformBufferBindings.Add(new GfxBufferBinding { Buffer = null!, WordCount = 0 });

            for (int j = bindingDescriptor.SamplerBindings.Count; j < bindingLayout.NumSamplers; j++)
                bindingDescriptor.SamplerBindings.Add(new GfxSamplerBinding { GfxSampler = null, GfxTexture = null, LateBinding = null });
        }

        public void SetBindingLayouts(List<GfxBindingLayoutDescriptor> bindingLayouts)
        {
            GfxPlatformUtils.Assert(bindingLayouts.Count <= this._BindingDescriptors.Count);
            for (int i = 0; i < this._BindingDescriptors.Count; i++)
                this._SetBindingLayout(i, bindingLayouts[i]);
        }

        public void SetDrawCount(long count, long start = 0)
        {
            this._DrawCount = count;
            this._DrawStart = start;
        }

        public long GetDrawCount()
        {
            return this._DrawCount;
        }

        public void SetInstanceCount(long instanceCount)
        {
            this._DrawInstanceCount = instanceCount;
        }

        public void SetUniformBuffer(GfxRenderDynamicUniformBuffer uniformBuffer)
        {
            this._UniformBuffer = uniformBuffer;
        }

        public long AllocateUniformBuffer(int bufferIndex, long wordCount)
        {
            GfxPlatformUtils.Assert(this._BindingDescriptors[0].BindingLayout.NumUniformBuffers <= this._DynamicUniformBufferByteOffsets.Count);
            GfxPlatformUtils.Assert(bufferIndex < this._BindingDescriptors[0].BindingLayout.NumUniformBuffers);

            this._DynamicUniformBufferByteOffsets[bufferIndex] = this._UniformBuffer.AllocateChunk(wordCount) << 2;

            var dst = this._BindingDescriptors[0].UniformBufferBindings[(int)bufferIndex];
            dst.WordCount = wordCount;

            return this._DynamicUniformBufferByteOffsets[bufferIndex] >> 2;
        }

        public VP_Float32Array<VP_ArrayBuffer> AllocateUniformBufferF32(int bufferIndex, long wordCount)
        {
            long wordOffset = this.AllocateUniformBuffer(bufferIndex, wordCount);
            return this._UniformBuffer.MapBufferF32().Subarray<VP_Float32Array<VP_ArrayBuffer>>(wordOffset);
        }

        public void SetUniformBufferOffset(int bufferIndex, long wordOffset, long wordCount)
        {
            this._DynamicUniformBufferByteOffsets[bufferIndex] = wordOffset << 2;

            var dst = this._BindingDescriptors[0].UniformBufferBindings[(int)bufferIndex];
            dst.WordCount = wordCount;
        }

        public VP_Float32Array MapUniformBufferF32(long bufferIndex)
        {
            return this._UniformBuffer.MapBufferF32();
        }

        public GfxRenderDynamicUniformBuffer GetUniformBuffer()
        {
            return this._UniformBuffer;
        }

        public void SetSamplerBindings(int bindingIndex, List<GfxSamplerBinding> m)
        {
            var bindingDescriptor = this._BindingDescriptors[bindingIndex];

            for (int i = 0; i < bindingDescriptor.SamplerBindings.Count; i++)
            {
                var dst = bindingDescriptor.SamplerBindings[i];
                var binding = m[i];

                if (binding == null)
                {
                    dst.GfxTexture = null;
                    dst.GfxSampler = null;
                    dst.LateBinding = null;
                    continue;
                }

                dst.GfxTexture = binding.GfxTexture;
                dst.GfxSampler = binding.GfxSampler;
                dst.LateBinding = binding.LateBinding;
            }
        }

        public void SetSamplerBindingsFromTextureMappings(List<GfxSamplerBinding> m)
        {
            GfxPlatformUtils.Assert(this._BindingDescriptors.Count == 1);
            this.SetSamplerBindings(0, m);
        }

        public bool HasLateSamplerBinding(string name)
        {
            for (int i = 0; i < this._BindingDescriptors.Count; i++)
            {
                var bindingDescriptor = this._BindingDescriptors[i];
                for (int j = 0; j < bindingDescriptor.SamplerBindings.Count; j++)
                {
                    var dst = bindingDescriptor.SamplerBindings[j];
                    if (dst.LateBinding == name)
                        return true;
                }
            }

            return false;
        }

        public void ResolveLateSamplerBinding(string name, GfxSamplerBinding binding)
        {
            for (int i = 0; i < this._BindingDescriptors.Count; i++)
            {
                var bindingDescriptor = this._BindingDescriptors[i];
                for (int j = 0; j < bindingDescriptor.SamplerBindings.Count; j++)
                {
                    var dst = bindingDescriptor.SamplerBindings[j];
                    if (dst.LateBinding == name)
                    {
                        if (binding == null)
                        {
                            dst.GfxTexture = null;
                            dst.GfxSampler = null;
                        }
                        else
                        {
                            GfxPlatformUtils.Assert(binding.LateBinding == null);
                            dst.GfxTexture = binding.GfxTexture;
                            if (binding.GfxSampler != null)
                                dst.GfxSampler = binding.GfxSampler;
                        }

                        dst.LateBinding = null;
                    }
                }
            }
        }

        public void SetAllowSkippingIfPipelineNotReady(bool v)
        {
            this._AllowSkippingPipelineIfNotReady = v;
        }

        private void SetAttachmentFormatsFromRenderPass(GfxDevice device, GfxRenderPass passRenderer)
        {
            var passDescriptor = device.QueryRenderPass(passRenderer);

            long sampleCount = -1;
            for (int i = 0; i < passDescriptor.ColorAttachments.Count; i++)
            {
                var attachment = passDescriptor.ColorAttachments[i];
                var colorAttachmentDescriptor = attachment != null
                    ? device.QueryRenderTarget(attachment.RenderTarget)
                    : null;

                this._RenderPipelineDescriptor.ColorAttachmentFormats[i] = colorAttachmentDescriptor != null
                    ? colorAttachmentDescriptor.PixelFormat
                    : null;

                if (colorAttachmentDescriptor != null)
                {
                    if (sampleCount == -1)
                        sampleCount = colorAttachmentDescriptor.SampleCount;
                    else
                        GfxPlatformUtils.Assert(sampleCount == colorAttachmentDescriptor.SampleCount);
                }
            }

            var depthStencilAttachment = passDescriptor.DepthStencilAttachment;
            var depthStencilAttachmentDescriptor = depthStencilAttachment != null
                ? device.QueryRenderTarget(depthStencilAttachment.RenderTarget)
                : null;

            this._RenderPipelineDescriptor.DepthStencilAttachmentFormat = depthStencilAttachmentDescriptor != null
                ? depthStencilAttachmentDescriptor.PixelFormat
                : null;

            if (depthStencilAttachmentDescriptor != null)
            {
                if (sampleCount == -1)
                    sampleCount = depthStencilAttachmentDescriptor.SampleCount;
                else
                    GfxPlatformUtils.Assert(sampleCount == depthStencilAttachmentDescriptor.SampleCount);
            }

            GfxPlatformUtils.Assert(sampleCount > 0);
            this._RenderPipelineDescriptor.SampleCount = sampleCount;
        }

        public void SetStencilRef(long? value)
        {
            this._StencilRef = value;
        }

        public void SetBlendColor(GfxColor value)
        {
            this._BlendColor = value;
        }

        public void DrawOnPass(GfxRenderCache cache, GfxRenderPass passRenderer)
        {
            var device = cache.device;
            this.SetAttachmentFormatsFromRenderPass(device, passRenderer);

            var gfxPipeline = cache.CreateRenderPipeline(this._RenderPipelineDescriptor);

            bool pipelineReady = device.PipelineQueryReady(gfxPipeline);
            if (!pipelineReady)
            {
                if (this._AllowSkippingPipelineIfNotReady)
                    return;

                device.PipelineForceReady(gfxPipeline);
            }

            if (this.DebugMarker != null)
                passRenderer.BeginDebugGroup(this.DebugMarker);

            passRenderer.SetPipeline(gfxPipeline);
            passRenderer.SetVertexInput(this._RenderPipelineDescriptor.InputLayout, this._VertexBuffers, this._IndexBuffer);

            int uboIndex = 0;
            for (int i = 0; i < this._BindingDescriptors.Count; i++)
            {
                var bindingDescriptor = this._BindingDescriptors[i];

                for (int j = 0; j < bindingDescriptor.UniformBufferBindings.Count; j++)
                    bindingDescriptor.UniformBufferBindings[j].Buffer = GfxPlatformUtils.AssertExists(this._UniformBuffer.GfxBuffer);

                var gfxBindings = cache.CreateBindings(bindingDescriptor);
                var numBuffers = (int)bindingDescriptor.BindingLayout.NumUniformBuffers;

                passRenderer.SetBindings(i, gfxBindings, this._DynamicUniformBufferByteOffsets.Skip(uboIndex).Take(numBuffers).ToList());
                uboIndex += numBuffers;
            }

            if (this._StencilRef != null)
                passRenderer.SetStencilRef(this._StencilRef.Value);

            if (this._BlendColor != null)
                passRenderer.SetBlendColor(this._BlendColor);

            bool indexed = this._IndexBuffer != null;

            if (this._DrawInstanceCount > 1)
            {
                GfxPlatformUtils.Assert(indexed);
                passRenderer.DrawIndexedInstanced(this._DrawCount, this._DrawStart, this._DrawInstanceCount);
            }
            else if (indexed)
            {
                passRenderer.DrawIndexed(this._DrawCount, this._DrawStart);
            }
            else
            {
                passRenderer.Draw(this._DrawCount, this._DrawStart);
            }

            if (this.DebugMarker != null)
                passRenderer.EndDebugGroup();
        }

    }
}
