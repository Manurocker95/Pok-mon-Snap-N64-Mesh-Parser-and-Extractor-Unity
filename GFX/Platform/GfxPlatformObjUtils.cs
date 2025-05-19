using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public static class GfxPlatformObjUtils
    {
        public static List<T> ArrayCopy<T>(List<T> a, CopyFunc<T> copyFunc)
        {
            return GfxUtils.ArrayCopy(a, copyFunc); 
        }
        public static GfxBindingsDescriptor GfxBindingsDescriptorCopy(GfxBindingsDescriptor a)
        {
            var bindingLayout = a.BindingLayout;
            var samplerBindings = ArrayCopy(a.SamplerBindings, GfxRenderCacheUtils.GfxSamplerBindingCopy);
            var uniformBufferBindings = ArrayCopy(a.UniformBufferBindings, GfxRenderCacheUtils.GfxBufferBindingCopy);

            return new GfxBindingsDescriptor
            {
                BindingLayout = bindingLayout,
                SamplerBindings = samplerBindings,
                UniformBufferBindings = uniformBufferBindings
            };
        }

        public static bool GfxSamplerDescriptorEquals(GfxSamplerDescriptor a, GfxSamplerDescriptor b)
        {
            return a.WrapS == b.WrapS &&
                   a.WrapT == b.WrapT &&
                   a.WrapQ == b.WrapQ &&
                   a.MinFilter == b.MinFilter &&
                   a.MagFilter == b.MagFilter &&
                   a.MipFilter == b.MipFilter &&
                   a.MinLOD == b.MinLOD &&
                   a.MaxLOD == b.MaxLOD &&
                   a.MaxAnisotropy == b.MaxAnisotropy &&
                   a.CompareMode == b.CompareMode;
        }

        public static bool ArrayEqual<T>(List<T> a, List<T> b, EqualFunc<T> e)
        {
            return GfxUtils.ArrayEqual(a, b, e);
        }

        public static bool GfxChannelBlendStateEquals(GfxChannelBlendState a, GfxChannelBlendState b)
        {
            return a.BlendMode == b.BlendMode &&
                   a.BlendSrcFactor == b.BlendSrcFactor &&
                   a.BlendDstFactor == b.BlendDstFactor;
        }

        public static bool GfxAttachmentStateEquals(GfxAttachmentState a, GfxAttachmentState b)
        {
            if (!GfxChannelBlendStateEquals(a.RgbBlendState, b.RgbBlendState)) return false;
            if (!GfxChannelBlendStateEquals(a.AlphaBlendState, b.AlphaBlendState)) return false;
            if (a.ChannelWriteMask != b.ChannelWriteMask) return false;
            return true;
        }

        public static bool GfxMegaStateDescriptorEquals(GfxMegaStateDescriptor a, GfxMegaStateDescriptor b)
        {
            if (!GfxUtils.ArrayEqual(a.AttachmentsState, b.AttachmentsState, GfxAttachmentStateEquals))
                return false;

            return
                a.DepthCompare == b.DepthCompare &&
                a.DepthWrite == b.DepthWrite &&
                a.StencilCompare == b.StencilCompare &&
                a.StencilWrite == b.StencilWrite &&
                a.StencilPassOp == b.StencilPassOp &&
                a.CullMode == b.CullMode &&
                a.FrontFace == b.FrontFace &&
                a.PolygonOffset == b.PolygonOffset;
        }

        public static bool GfxRenderPipelineDescriptorEquals(GfxRenderPipelineDescriptor a, GfxRenderPipelineDescriptor b)
        {
            if (a.Topology != b.Topology) return false;
            if (a.InputLayout != b.InputLayout) return false;
            if (a.SampleCount != b.SampleCount) return false;
            if (!GfxMegaStateDescriptorEquals(a.MegaStateDescriptor, b.MegaStateDescriptor)) return false;
            if (!GfxProgramEquals(a.Program, b.Program)) return false;
            if (!GfxUtils.ArrayEqual(a.BindingLayouts, b.BindingLayouts, GfxBindingLayoutEquals)) return false;
            if (!GfxUtils.ArrayEqual(a.ColorAttachmentFormats, b.ColorAttachmentFormats, GfxFormatEquals)) return false;
            if (a.DepthStencilAttachmentFormat != b.DepthStencilAttachmentFormat) return false;
            return true;
        }

        public static bool GfxInputLayoutDescriptorEquals(GfxInputLayoutDescriptor a, GfxInputLayoutDescriptor b)
        {
            if (a.IndexBufferFormat != b.IndexBufferFormat) return false;
            if (!ArrayEqual(a.VertexBufferDescriptors, b.VertexBufferDescriptors, GfxInputLayoutBufferDescriptorEquals)) return false;
            if (!ArrayEqual(a.VertexAttributeDescriptors, b.VertexAttributeDescriptors, GfxVertexAttributeDescriptorEquals)) return false;
            return true;
        }

        public static bool GfxInputLayoutBufferDescriptorEquals(GfxInputLayoutBufferDescriptor a, GfxInputLayoutBufferDescriptor b)
        {
            if (a == null) return b == null;
            if (b == null) return false;

            return a.ByteStride == b.ByteStride &&
                   a.Frequency == b.Frequency;
        }

        public static bool GfxVertexAttributeDescriptorEquals(GfxVertexAttributeDescriptor a, GfxVertexAttributeDescriptor b)
        {
            return a.BufferIndex == b.BufferIndex &&
                   a.BufferByteOffset == b.BufferByteOffset &&
                   a.Location == b.Location &&
                   a.Format == b.Format;
        }

        public static bool GfxProgramEquals(GfxProgram a, GfxProgram b)
        {
            return a.ResourceUniqueId == b.ResourceUniqueId;
        }
        public static bool GfxFormatEquals(GfxFormat a, GfxFormat b)
        {
            return a == b;
        }

        public static bool GfxSamplerBindingEquals(GfxSamplerBinding a = null, GfxSamplerBinding b = null)
        {
            if (a == null) return b == null;
            if (b == null) return false;
            return a.GfxSampler == b.GfxSampler && a.GfxTexture == b.GfxTexture;
        }

        public static bool GfxBindingsDescriptorEquals(GfxBindingsDescriptor a, GfxBindingsDescriptor b)
        {
            if (a.SamplerBindings.Count != b.SamplerBindings.Count)
                return false;
            if (!ArrayEqual(a.SamplerBindings, b.SamplerBindings, GfxSamplerBindingEquals))
                return false;
            if (!ArrayEqual(a.UniformBufferBindings, b.UniformBufferBindings, GfxBufferBindingEquals))
                return false;
            if (!GfxBindingLayoutEquals(a.BindingLayout, b.BindingLayout))
                return false;
            return true;
        }
        public static bool GfxBufferBindingEquals(GfxBufferBinding a, GfxBufferBinding b)
        {
            return a.Buffer == b.Buffer && a.WordCount == b.WordCount;
        }
        public static bool GfxBindingLayoutEquals(GfxBindingLayoutDescriptor a, GfxBindingLayoutDescriptor b)
        {
            return a.NumSamplers == b.NumSamplers && a.NumUniformBuffers == b.NumUniformBuffers;
        }

    }
}
