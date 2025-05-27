using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public static class GfxMegaStateDescriptorHelpers
    {
        public static readonly GfxMegaStateDescriptor DefaultMegaState = new GfxMegaStateDescriptor
        {
            AttachmentsState = new List<GfxAttachmentState> {
            new GfxAttachmentState {
                ChannelWriteMask = GfxChannelWriteMask.RGB,
                RgbBlendState = DefaultBlendState,
                AlphaBlendState = DefaultBlendState,
            }
        },
            DepthCompare = ReversedDepthHelpers.ReverseDepthForCompareMode(GfxCompareMode.LessEqual),
            DepthWrite = true,
            StencilCompare = GfxCompareMode.Always,
            StencilWrite = false,
            StencilPassOp = GfxStencilOp.Keep,
            CullMode = GfxCullMode.None,
            FrontFace = GfxFrontFaceMode.CCW,
            PolygonOffset = false,
            Wireframe = false
        };

        public static readonly GfxChannelBlendState DefaultBlendState = new GfxChannelBlendState
        {
            BlendSrcFactor = GfxBlendFactor.One,
            BlendDstFactor = GfxBlendFactor.Zero,
            BlendMode = GfxBlendMode.Add
        };

        public static GfxMegaStateDescriptor SetAttachmentStateSimple(GfxMegaStateDescriptor dst, AttachmentStateSimple simple)
        {
            if (dst.AttachmentsState == null)
            {
                dst.AttachmentsState = new List<GfxAttachmentState>();
                CopyAttachmentsState(dst.AttachmentsState, DefaultMegaState.AttachmentsState);
            }

            if (dst.AttachmentsState.Count == 0)
            {
                dst.AttachmentsState.Add(new GfxAttachmentState());
            }

            CopyAttachmentStateFromSimple(dst.AttachmentsState[0], simple);
            return dst;
        }

        public static GfxMegaStateDescriptor MakeMegaState(GfxMegaStateDescriptor other = null, GfxMegaStateDescriptor src = null)
        {
            if (src == null)
                src = DefaultMegaState;

            var dst = CopyMegaState(src);

            if (other != null)
                SetMegaStateFlags(dst, other);

            return dst;
        }
        public static void SetMegaStateFlags(GfxMegaStateDescriptor dst, GfxMegaStateDescriptor src)
        {

            if (src.AttachmentsState != null)
                CopyAttachmentsState(dst.AttachmentsState, src.AttachmentsState);

            dst.DepthCompare = FallbackUndefined(src.DepthCompare, dst.DepthCompare);
            dst.DepthWrite = FallbackUndefined(src.DepthWrite, dst.DepthWrite);
            dst.StencilCompare = FallbackUndefined(src.StencilCompare, dst.StencilCompare);
            dst.StencilWrite = FallbackUndefined(src.StencilWrite, dst.StencilWrite);
            dst.StencilPassOp = FallbackUndefined(src.StencilPassOp, dst.StencilPassOp);
            dst.CullMode = FallbackUndefined(src.CullMode, dst.CullMode);
            dst.FrontFace = FallbackUndefined(src.FrontFace, dst.FrontFace);
            dst.PolygonOffset = FallbackUndefined(src.PolygonOffset, dst.PolygonOffset);
            dst.Wireframe = FallbackUndefined(src.Wireframe, dst.Wireframe);
        }

        public static void SetMegaStateFlags(GfxMegaStateDescriptor dst, VP_Partial<GfxMegaStateDescriptor> src)
        {

            if (src.Value.AttachmentsState != null)
                CopyAttachmentsState(dst.AttachmentsState, src.Value.AttachmentsState);

            dst.DepthCompare = FallbackUndefined(src.Value.DepthCompare, dst.DepthCompare);
            dst.DepthWrite = FallbackUndefined(src.Value.DepthWrite, dst.DepthWrite);
            dst.StencilCompare = FallbackUndefined(src.Value.StencilCompare, dst.StencilCompare);
            dst.StencilWrite = FallbackUndefined(src.Value.StencilWrite, dst.StencilWrite);
            dst.StencilPassOp = FallbackUndefined(src.Value.StencilPassOp, dst.StencilPassOp);
            dst.CullMode = FallbackUndefined(src.Value.CullMode, dst.CullMode);
            dst.FrontFace = FallbackUndefined(src.Value.FrontFace, dst.FrontFace);
            dst.PolygonOffset = FallbackUndefined(src.Value.PolygonOffset, dst.PolygonOffset);
            dst.Wireframe = FallbackUndefined(src.Value.Wireframe, dst.Wireframe);
        }

        public static T FallbackUndefined<T>(T v, T fallback)
        {
            return GfxPlatformUtils.FallbackUndefined(v, fallback);
        }

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
            if (src == null || dst == null) return;

            if (src.ChannelWriteMask != default)
                dst.ChannelWriteMask = src.ChannelWriteMask;

            if (src.BlendMode != GfxBlendMode.None && dst.RgbBlendState != null && dst.AlphaBlendState != null)
            {
                dst.RgbBlendState.BlendMode = src.BlendMode;
                dst.AlphaBlendState.BlendMode = src.BlendMode;
            }

            if (src.BlendSrcFactor != GfxBlendFactor.None && dst.RgbBlendState != null && dst.AlphaBlendState != null)
            {
                dst.RgbBlendState.BlendSrcFactor = src.BlendSrcFactor;
                dst.AlphaBlendState.BlendSrcFactor = src.BlendSrcFactor;
            }

            if (src.BlendDstFactor != GfxBlendFactor.None && dst.RgbBlendState != null && dst.AlphaBlendState != null)
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
            if (dst == null || src == null)
                return;

            dst.BlendDstFactor = src.BlendDstFactor;
            dst.BlendSrcFactor = src.BlendSrcFactor;
            dst.BlendMode = src.BlendMode;
        }

        public static readonly GfxMegaStateDescriptor FullscreenMegaState = MakeMegaState(
            new GfxMegaStateDescriptor
            {
                DepthCompare = GfxCompareMode.Always,
                DepthWrite = false
            },
            DefaultMegaState
        );
    }
}
