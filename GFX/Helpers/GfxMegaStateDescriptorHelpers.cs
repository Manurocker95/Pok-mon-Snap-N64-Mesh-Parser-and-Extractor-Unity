using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public static class GfxMegaStateDescriptorHelpers
    {
        public static GfxMegaStateDescriptor CopyMegaState(GfxMegaStateDescriptor src)
        {
            var dst = new GfxMegaStateDescriptor
            {
                DepthCompare = src.DepthCompare,
                DepthWrite = src.DepthWrite,
                StencilCompare = src.StencilCompare,
                StencilWrite = src.StencilWrite,
                StencilPassOp = src.StencilPassOp,
                CullMode = src.CullMode,
                FrontFace = src.FrontFace,
                PolygonOffset = src.PolygonOffset,
                Wireframe = src.Wireframe,
                AttachmentsState = new List<GfxAttachmentState>()
            };

            CopyAttachmentsState(dst.AttachmentsState, src.AttachmentsState);
            return dst;
        }

        public static void CopyAttachmentsState(List<GfxAttachmentState> dst, List<GfxAttachmentState> src)
        {
            if (dst.Count != src.Count)
            {
                dst.Clear();
                for (int i = 0; i < src.Count; i++)
                    dst.Add(null);
            }

            for (int i = 0; i < src.Count; i++)
                dst[i] = CopyAttachmentState(dst[i], src[i]);
        }

        public static void CopyAttachmentStateFromSimple(GfxAttachmentState dst, AttachmentStateSimple src)
        {
            if (src == null) return;

            if (src.ChannelWriteMask != default)
                dst.ChannelWriteMask = src.ChannelWriteMask;

            if (Enum.IsDefined(typeof(GfxBlendMode), src.BlendMode))
            {
                dst.RgbBlendState.BlendMode = src.BlendMode;
                dst.AlphaBlendState.BlendMode = src.BlendMode;
            }

            if (Enum.IsDefined(typeof(GfxBlendFactor), src.BlendSrcFactor))
            {
                dst.RgbBlendState.BlendSrcFactor = src.BlendSrcFactor;
                dst.AlphaBlendState.BlendSrcFactor = src.BlendSrcFactor;
            }

            if (Enum.IsDefined(typeof(GfxBlendFactor), src.BlendDstFactor))
            {
                dst.RgbBlendState.BlendDstFactor = src.BlendDstFactor;
                dst.AlphaBlendState.BlendDstFactor = src.BlendDstFactor;
            }
        }

        public static GfxAttachmentState CopyAttachmentState(GfxAttachmentState dst, GfxAttachmentState src)
        {
            if (dst == null)
            {
                dst = new GfxAttachmentState
                {
                    RgbBlendState = new GfxChannelBlendState(),
                    AlphaBlendState = new GfxChannelBlendState(),
                    ChannelWriteMask = 0
                };
            }

            CopyChannelBlendState(dst.RgbBlendState, src.RgbBlendState);
            CopyChannelBlendState(dst.AlphaBlendState, src.AlphaBlendState);
            dst.ChannelWriteMask = src.ChannelWriteMask;

            return dst;
        }

        public static void CopyChannelBlendState(GfxChannelBlendState dst, GfxChannelBlendState src)
        {
            dst.BlendDstFactor = src.BlendDstFactor;
            dst.BlendSrcFactor = src.BlendSrcFactor;
            dst.BlendMode = src.BlendMode;
        }

    }
}
