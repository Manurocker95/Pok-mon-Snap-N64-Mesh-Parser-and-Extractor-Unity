using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public static class GfxRenderCacheUtils
    {
        public static List<T> ArrayCopy<T>(List<T> a, CopyFunc<T> copyFunc)
        {
            return GfxUtils.ArrayCopy(a, copyFunc);
        }

        public static GfxBufferBinding GfxBufferBindingCopy(GfxBufferBinding a)
        {
            var buffer = a.Buffer;
            var wordCount = a.WordCount;

            return new GfxBufferBinding
            {
                Buffer = buffer,
                WordCount = wordCount
            };
        }

        public static GfxSamplerBinding GfxSamplerBindingNew()
        {
            return new GfxSamplerBinding
            {
                GfxSampler = null,
                GfxTexture = null,
                LateBinding = null
            };
        }

        public static GfxInputLayoutBufferDescriptor GfxInputLayoutBufferDescriptorCopy(GfxInputLayoutBufferDescriptor a)
        {
            if (a == null)
                return null;

            return new GfxInputLayoutBufferDescriptor
            {
                ByteStride = a.ByteStride,
                Frequency = a.Frequency
            };
        }

        public static GfxVertexAttributeDescriptor GfxVertexAttributeDescriptorCopy(GfxVertexAttributeDescriptor a)
        {
            return new GfxVertexAttributeDescriptor
            {
                Location = a.Location,
                Format = a.Format,
                BufferIndex = a.BufferIndex,
                BufferByteOffset = a.BufferByteOffset
            };
        }


        public static GfxInputLayoutDescriptor GfxInputLayoutDescriptorCopy(GfxInputLayoutDescriptor a)
        {
            var vertexAttributeDescriptors = ArrayCopy(a.VertexAttributeDescriptors, GfxVertexAttributeDescriptorCopy);
            var vertexBufferDescriptors = ArrayCopy(a.VertexBufferDescriptors, GfxInputLayoutBufferDescriptorCopy);
            var indexBufferFormat = a.IndexBufferFormat;

            return new GfxInputLayoutDescriptor
            {
                VertexAttributeDescriptors = vertexAttributeDescriptors,
                VertexBufferDescriptors = vertexBufferDescriptors,
                IndexBufferFormat = indexBufferFormat
            };
        }


        public static GfxRenderProgramDescriptor GfxProgramDescriptorCopy(GfxRenderProgramDescriptor a)
        {
            var preprocessedVert = a.PreprocessedVert;
            var preprocessedFrag = a.PreprocessedFrag;

            return new GfxRenderProgramDescriptor
            {
                PreprocessedVert = preprocessedVert,
                PreprocessedFrag = preprocessedFrag
            };
        }

        public static GfxSamplerBinding GfxSamplerBindingCopy(GfxSamplerBinding a)
        {
            var gfxSampler = a.GfxSampler;
            var gfxTexture = a.GfxTexture;
            var lateBinding = a.LateBinding;

            return new GfxSamplerBinding
            {
                GfxSampler = gfxSampler,
                GfxTexture = gfxTexture,
                LateBinding = lateBinding
            };
        }

        public static GfxBindingLayoutDescriptor GfxBindingLayoutDescriptorCopy(GfxBindingLayoutDescriptor a)
        {
            var numSamplers = a.NumSamplers;
            var numUniformBuffers = a.NumUniformBuffers;
            var samplerEntries = a.SamplerEntries != null ? ArrayCopy(a.SamplerEntries, GfxBindingLayoutSamplerDescriptorCopy) : null;

            return new GfxBindingLayoutDescriptor
            {
                NumSamplers = numSamplers,
                NumUniformBuffers = numUniformBuffers,
                SamplerEntries = samplerEntries
            };
        }

        public static GfxBindingLayoutSamplerDescriptor GfxBindingLayoutSamplerDescriptorCopy(GfxBindingLayoutSamplerDescriptor a)
        {
            var dimension = a.Dimension;
            var formatKind = a.FormatKind;
            var comparison = a.Comparison == true;

            return new GfxBindingLayoutSamplerDescriptor
            {
                Dimension = dimension,
                FormatKind = formatKind,
                Comparison = comparison
            };
        }

        public static GfxRenderPipelineDescriptor GfxRenderPipelineDescriptorCopy(GfxRenderPipelineDescriptor a)
        {
            var bindingLayouts = ArrayCopy(a.BindingLayouts, GfxBindingLayoutDescriptorCopy);
            var inputLayout = a.InputLayout;
            var program = a.Program;
            var topology = a.Topology;
            var megaStateDescriptor = GfxMegaStateDescriptorHelpers.CopyMegaState(a.MegaStateDescriptor);
            var colorAttachmentFormats = new List<GfxFormat>(a.ColorAttachmentFormats);
            var depthStencilAttachmentFormat = a.DepthStencilAttachmentFormat;
            var sampleCount = a.SampleCount;

            return new GfxRenderPipelineDescriptor
            {
                BindingLayouts = bindingLayouts,
                InputLayout = inputLayout,
                Program = program,
                Topology = topology,
                MegaStateDescriptor = megaStateDescriptor,
                ColorAttachmentFormats = colorAttachmentFormats,
                DepthStencilAttachmentFormat = depthStencilAttachmentFormat,
                SampleCount = sampleCount
            };
        }


        public static GfxBindingsDescriptor GfxBindingsDescriptorCopy(GfxBindingsDescriptor a)
        {
            var bindingLayout = a.BindingLayout;
            var samplerBindings = ArrayCopy(a.SamplerBindings, GfxSamplerBindingCopy);
            var uniformBufferBindings = ArrayCopy(a.UniformBufferBindings, GfxBufferBindingCopy);

            return new GfxBindingsDescriptor
            {
                BindingLayout = bindingLayout,
                SamplerBindings = samplerBindings,
                UniformBufferBindings = uniformBufferBindings
            };
        }

        public static bool GfxProgramDescriptorEquals(GfxRenderProgramDescriptor a, GfxRenderProgramDescriptor b)
        {
            //Debug.Assert(a.PreprocessedVert != "" && b.PreprocessedVert != "");
            //Debug.Assert(a.PreprocessedFrag != "" && b.PreprocessedFrag != "");

            return a.PreprocessedVert == b.PreprocessedVert &&
                   a.PreprocessedFrag == b.PreprocessedFrag;
        }

        public static long HashCodeNumberUpdate(long hash, long v)
        {
            return N64Utils.HashCodeNumberUpdate(hash, v);
        }

        public static long HashCodeNumberFinish(long hash)
        {
            return N64Utils.HashCodeNumberFinish(hash);
        }

        public static long GfxRenderBindingLayoutHash(long hash, GfxBindingLayoutDescriptor a)
        {
            hash = HashCodeNumberUpdate(hash, a.NumUniformBuffers);
            hash = HashCodeNumberUpdate(hash, a.NumSamplers);
            return hash;
        }

        public static long GfxMegaStateDescriptorHash(long hash, GfxMegaStateDescriptor a)
        {
            for (long i = 0; i < a.AttachmentsState.Count; i++)
                hash = GfxAttachmentStateHash(hash, a.AttachmentsState[(int)i]);

            hash = HashCodeNumberUpdate(hash, (long)a.DepthCompare);
            hash = HashCodeNumberUpdate(hash, a.DepthWrite ? 1 : 0);
            hash = HashCodeNumberUpdate(hash, (long)a.StencilCompare);
            hash = HashCodeNumberUpdate(hash, (long)a.StencilPassOp);
            hash = HashCodeNumberUpdate(hash, a.StencilWrite ? 1 : 0);
            hash = HashCodeNumberUpdate(hash, (long)a.CullMode);
            hash = HashCodeNumberUpdate(hash, a.FrontFace == GfxFrontFaceMode.CW ? 1 : 0);
            hash = HashCodeNumberUpdate(hash, a.PolygonOffset ? 1 : 0);
            hash = HashCodeNumberUpdate(hash, a.Wireframe ? 1 : 0);

            return hash;
        }
        public static long GfxBlendStateHash(long hash, GfxChannelBlendState a)
        {
            hash = HashCodeNumberUpdate(hash, (long)a.BlendMode);
            hash = HashCodeNumberUpdate(hash, (long)a.BlendSrcFactor);
            hash = HashCodeNumberUpdate(hash, (long)a.BlendDstFactor);
            return hash;
        }

        public static long GfxAttachmentStateHash(long hash, GfxAttachmentState a)
        {
            hash = GfxBlendStateHash(hash, a.RgbBlendState);
            hash = GfxBlendStateHash(hash, a.AlphaBlendState);
            hash = HashCodeNumberUpdate(hash, (long)a.ChannelWriteMask);
            return hash;
        }


        public static long GfxRenderPipelineDescriptorHash(GfxRenderPipelineDescriptor a)
        {
            long hash = 0;
            hash =  N64Utils.HashCodeNumberUpdate(hash, a.Program.ResourceUniqueId);

            if (a.InputLayout != null)
                hash = HashCodeNumberUpdate(hash, a.InputLayout.ResourceUniqueId);

            for (long i = 0; i < a.BindingLayouts.Count; i++)
                hash = GfxRenderBindingLayoutHash(hash, a.BindingLayouts[(int)i]);

            hash = GfxMegaStateDescriptorHash(hash, a.MegaStateDescriptor);

            for (long i = 0; i < a.ColorAttachmentFormats.Count; i++)
                hash = HashCodeNumberUpdate(hash, a.ColorAttachmentFormats[(int)i] != null ? (long)a.ColorAttachmentFormats[(int)i].Value : 0);

            hash = HashCodeNumberUpdate(hash, a.DepthStencilAttachmentFormat != null ? (long)a.DepthStencilAttachmentFormat.Value : 0);

            return HashCodeNumberFinish(hash);
        }

        public static long GfxBindingsDescriptorHash(GfxBindingsDescriptor a)
        {
            long hash = 0;
            for (long i = 0; i < a.SamplerBindings.Count; i++)
            {
                var binding = a.SamplerBindings[(int)i];
                if (binding != null && binding.GfxTexture != null)
                    hash = HashCodeNumberUpdate(hash, binding.GfxTexture.ResourceUniqueId);
            }

            for (long i = 0; i < a.UniformBufferBindings.Count; i++)
            {
                var binding = a.UniformBufferBindings[(int)i];
                if (binding != null && binding.Buffer != null)
                {
                    hash = HashCodeNumberUpdate(hash, binding.Buffer.ResourceUniqueId);
                    hash = HashCodeNumberUpdate(hash, binding.WordCount);
                }
            }

            return HashCodeNumberFinish(hash);
        }

    }
}
