using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.RDP
{
    public class RDPUtils
    {
        public static VP_Float32Array MakeVertexBufferData(List<RSPVertex> v)
        {
            var buf = new VP_Float32Array(10 * v.Count);
            long j = 0;
            for (int i = 0; i < v.Count; i++)
            {
                buf[j++] = (float)v[i].x;
                buf[j++] = (float)v[i].y;
                buf[j++] = (float)v[i].z;
                buf[j++] = (float)v[i].matrixIndex;

                buf[j++] = (float)v[i].tx;
                buf[j++] = (float)v[i].ty;

                buf[j++] = (float)v[i].c0;
                buf[j++] = (float)v[i].c1;
                buf[j++] = (float)v[i].c2;
                buf[j++] = (float)v[i].a;
            }
            return buf;
        }

        public static long GetMaskedCMS(TileState tile)
        {
            long coordWidth = ((tile.lrs - tile.uls) >> 2) + 1;
            if (tile.masks != 0 && (1L << (int)tile.masks) < coordWidth)
                return tile.cms & 1;
            return tile.cms;
        }

        public static long GetMaskedCMT(TileState tile)
        {
            long coordHeight = ((tile.lrt - tile.ult) >> 2) + 1;
            if (tile.maskt != 0 && (1L << (int)tile.maskt) < coordHeight)
                return tile.cmt & 1;
            return tile.cmt;
        }

        public static long TexturePadWidth(ImageSize siz, long line, long width)
        {
            if (line == 0)
                return 0;
            long padTexels = (line << (4 - (int)siz)) - width;
            if (siz == ImageSize.SIZE_4B)
                return padTexels >> 1;
            else
                return padTexels << ((int)siz - 1);
        }

        public static GfxSampler TranslateSampler(GfxDevice device, GfxRenderCache cache, Texture texture)
        {
            return cache.CreateSampler(new GfxSamplerDescriptor
            {
                WrapS = TranslateCM((TexCM)GetMaskedCMS(texture.tile)),
                WrapT = TranslateCM((TexCM)GetMaskedCMT(texture.tile)),
                MinFilter = GfxTexFilterMode.Point,
                MagFilter = GfxTexFilterMode.Point,
                MipFilter = GfxMipFilterMode.Nearest,
                MinLOD = 0,
                MaxLOD = 0
            });
        }

        public static GfxWrapMode TranslateCM(TexCM cm)
        {
            switch (cm)
            {
                case TexCM.WRAP: return GfxWrapMode.Repeat;
                case TexCM.MIRROR: return GfxWrapMode.Mirror;
                case TexCM.CLAMP: return GfxWrapMode.Clamp;
                case TexCM.MIRROR_CLAMP: return GfxWrapMode.Mirror;
                default: throw new Exception("Unknown TexCM: " + cm);
            }
        }

        public static GfxTexture TranslateToGfxTexture(GfxDevice device, Texture texture)
        {
            var gfxTexture = device.CreateTexture(GfxPlatformUtils.MakeTextureDescriptor2D(GfxUtils.GetGfxFormatByOrder(GfxFormatOrder.U8_RGBA_NORM), texture.width, texture.height, 1));
            device.SetResourceName(gfxTexture, texture.name);
            var buffer = new VP_ArrayBuffer(texture.pixels);
            device.UploadTextureData(gfxTexture, 0, new List<VP_ArrayBufferView<VP_ArrayBuffer>> { new VP_Uint8Array(buffer) });
            return gfxTexture;
        }

        public static GfxCompareMode TranslateZMode(ZMode zmode)
        {
            switch (zmode)
            {
                case ZMode.ZMODE_OPA:
                case ZMode.ZMODE_INTER: // TODO: understand this better
                case ZMode.ZMODE_XLU:
                    return GfxCompareMode.Less;

                case ZMode.ZMODE_DEC:
                    return GfxCompareMode.LessEqual;

                default:
                    throw new System.Exception("Unknown Z mode: " + zmode);
            }
        }
        public static GfxBlendFactor TranslateBlendParamB(BlendParam_B paramB, GfxBlendFactor srcParam)
        {
            switch (paramB)
            {
                case BlendParam_B.G_BL_1MA:
                    if (srcParam == GfxBlendFactor.SrcAlpha)
                        return GfxBlendFactor.OneMinusSrcAlpha;
                    if (srcParam == GfxBlendFactor.One)
                        return GfxBlendFactor.Zero;
                    return GfxBlendFactor.One;

                case BlendParam_B.G_BL_A_MEM:
                    return GfxBlendFactor.DstAlpha;

                case BlendParam_B.G_BL_1:
                    return GfxBlendFactor.One;

                case BlendParam_B.G_BL_0:
                    return GfxBlendFactor.Zero;

                default:
                    throw new System.Exception("Unknown Blend Param B: " + paramB);
            }
        }

        public static GfxMegaStateDescriptor TranslateRenderMode(long renderMode)
        {
            var output = new GfxMegaStateDescriptor();

            BlendParam_PM_Color srcColor = (BlendParam_PM_Color)((renderMode >> (int)OtherModeL_Layout.P_2) & 0x03);
            BlendParam_A srcFactor = (BlendParam_A)((renderMode >> (int)OtherModeL_Layout.A_2) & 0x03);
            BlendParam_PM_Color dstColor = (BlendParam_PM_Color)((renderMode >> (int)OtherModeL_Layout.M_2) & 0x03);
            BlendParam_B dstFactor = (BlendParam_B)((renderMode >> (int)OtherModeL_Layout.B_2) & 0x03);

            bool doBlend = ((renderMode & (1 << (int)OtherModeL_Layout.FORCE_BL)) != 0)
                           && (dstColor == BlendParam_PM_Color.G_BL_CLR_MEM);

            if (doBlend)
            {
                VP_BYMLUtils.Assert(srcColor == BlendParam_PM_Color.G_BL_CLR_IN);

                GfxBlendFactor blendSrcFactor;
                if (srcFactor == BlendParam_A.G_BL_0)
                {
                    blendSrcFactor = GfxBlendFactor.Zero;
                }
                else if ((renderMode & (1 << (int)OtherModeL_Layout.ALPHA_CVG_SEL)) != 0 &&
                           (renderMode & (1 << (int)OtherModeL_Layout.CVG_X_ALPHA)) == 0)
                {
                    blendSrcFactor = GfxBlendFactor.One;
                }
                else
                {
                    blendSrcFactor = GfxBlendFactor.SrcAlpha;
                }

                GfxMegaStateDescriptorHelpers.SetAttachmentStateSimple(output, new AttachmentStateSimple
                {
                    BlendSrcFactor = blendSrcFactor,
                    BlendDstFactor = TranslateBlendParamB(dstFactor, blendSrcFactor),
                    BlendMode = GfxBlendMode.Add
                });
            }
            else
            {
                GfxMegaStateDescriptorHelpers.SetAttachmentStateSimple(output, new AttachmentStateSimple
                {
                    BlendSrcFactor = GfxBlendFactor.One,
                    BlendDstFactor = GfxBlendFactor.Zero,
                    BlendMode = GfxBlendMode.Add
                });
            }

            if ((renderMode & (1 << (int)OtherModeL_Layout.Z_CMP)) != 0)
            {
                ZMode zmode = (ZMode)((renderMode >> (int)OtherModeL_Layout.ZMODE) & 0x03);
                output.DepthCompare = ReversedDepthHelpers.ReverseDepthForCompareMode(TranslateZMode(zmode));
            }

            ZMode zmodeCheck = (ZMode)((renderMode >> (int)OtherModeL_Layout.ZMODE) & 0x03);
            if (zmodeCheck == ZMode.ZMODE_DEC)
                output.PolygonOffset = true;

            output.DepthWrite = (renderMode & (1 << (int)OtherModeL_Layout.Z_UPD)) != 0;

            return output;
        }

        public static TextFilt GetTextFiltFromOtherModeH(long modeH)
        {
            return (TextFilt)((modeH >> (int)OtherModeH_Layout.G_MDSFT_TEXTFILT) & 0x03);
        }

        public static OtherModeH_CycleType GetCycleTypeFromOtherModeH(long modeH)
        {
            return (OtherModeH_CycleType)((modeH >> (int)OtherModeH_Layout.G_MDSFT_CYCLETYPE) & 0x03);
        }

        private static long MapAdditive(long x)
        {
            return x >= 8 ? (long)CCMUX.ADD_ZERO : x;
        }

        private static long MapMult(long x)
        {
            return x >= 16 ? (long)CCMUX.MUL_ZERO : x;
        }

        public static CombineParams DecodeCombineParams(long w0, long w1)
        {
            long a0 = MapAdditive((w0 >> 20) & 0x0F);
            long c0 = MapMult((w0 >> 15) & 0x1F);
            long Aa0 = (w0 >> 12) & 0x07;
            long Ac0 = (w0 >> 9) & 0x07;
            long a1 = MapAdditive((w0 >> 5) & 0x0F);
            long c1 = MapMult(w0 & 0x1F);

            long b0 = MapAdditive((w1 >> 28) & 0x0F);
            long b1 = MapAdditive((w1 >> 24) & 0x0F);
            long Aa1 = (w1 >> 21) & 0x07;
            long Ac1 = (w1 >> 18) & 0x07;
            long d0 = (w1 >> 15) & 0x07;
            long Ab0 = (w1 >> 12) & 0x07;
            long Ad0 = (w1 >> 9) & 0x07;
            long d1 = (w1 >> 6) & 0x07;
            long Ab1 = (w1 >> 3) & 0x07;
            long Ad1 = w1 & 0x07;

            //System.Debug.Assert(b0 != (long)CCMUX.ONE && c0 != (long)CCMUX.ONE && b1 != (long)CCMUX.ONE && c1 != (long)CCMUX.ONE);

            return new CombineParams
            {
                c0 = new ColorCombinePass { a = (CCMUX)a0, b = (CCMUX)b0, c = (CCMUX)c0, d = (CCMUX)d0 },
                a0 = new AlphaCombinePass { a = (ACMUX)Aa0, b = (ACMUX)Ab0, c = (ACMUX)Ac0, d = (ACMUX)Ad0 },
                c1 = new ColorCombinePass { a = (CCMUX)a1, b = (CCMUX)b1, c = (CCMUX)c1, d = (CCMUX)d1 },
                a1 = new AlphaCombinePass { a = (ACMUX)Aa1, b = (ACMUX)Ab1, c = (ACMUX)Ac1, d = (ACMUX)Ad1 },
            };
        }

        public static bool CombineParamsUsesT0(CombineParams cp)
        {
            return ColorCombinePassUsesT0(cp.c0) || ColorCombinePassUsesT0(cp.c1) ||
                   AlphaCombinePassUsesT0(cp.a0) || AlphaCombinePassUsesT0(cp.a1);
        }

        public static bool CombineParamsUsesT1(CombineParams cp)
        {
            return ColorCombinePassUsesT1(cp.c0) || ColorCombinePassUsesT1(cp.c1) ||
                   AlphaCombinePassUsesT1(cp.a0) || AlphaCombinePassUsesT1(cp.a1);
        }

        public static bool CombineParamsUseTexelsInSecondCycle(CombineParams comb)
        {
            return comb.a1.a == ACMUX.TEXEL0 || comb.a1.b == ACMUX.TEXEL0 || comb.a1.c == ACMUX.TEXEL0 || comb.a1.d == ACMUX.TEXEL0 ||
                   comb.a1.a == ACMUX.TEXEL1 || comb.a1.b == ACMUX.TEXEL1 || comb.a1.c == ACMUX.TEXEL1 || comb.a1.d == ACMUX.TEXEL1 ||
                   comb.c1.a == CCMUX.TEXEL0 || comb.c1.b == CCMUX.TEXEL0 || comb.c1.c == CCMUX.TEXEL0 || comb.c1.d == CCMUX.TEXEL0 ||
                   comb.c1.a == CCMUX.TEXEL1 || comb.c1.b == CCMUX.TEXEL1 || comb.c1.c == CCMUX.TEXEL1 || comb.c1.d == CCMUX.TEXEL1 ||
                   comb.c1.c == CCMUX.TEXEL0_A || comb.c1.c == CCMUX.TEXEL1_A;
        }

        public static bool CombineParamsUseCombinedInFirstCycle(CombineParams comb)
        {
            return comb.a0.a == ACMUX.ADD_COMBINED || comb.a0.b == ACMUX.ADD_COMBINED || comb.a0.d == ACMUX.ADD_COMBINED ||
                   comb.c0.a == CCMUX.COMBINED || comb.c0.b == CCMUX.COMBINED || comb.c0.c == CCMUX.COMBINED || comb.c0.d == CCMUX.COMBINED ||
                   comb.c0.c == CCMUX.COMBINED_A;
        }

        public static bool CombineParamsUseT1InFirstCycle(CombineParams comb)
        {
            return comb.a0.a == ACMUX.TEXEL1 || comb.a0.b == ACMUX.TEXEL1 || comb.a0.c == ACMUX.TEXEL1 || comb.a0.d == ACMUX.TEXEL1 ||
                   comb.c0.a == CCMUX.TEXEL1 || comb.c0.b == CCMUX.TEXEL1 || comb.c0.c == CCMUX.TEXEL1 || comb.c0.d == CCMUX.TEXEL1 ||
                   comb.c0.c == CCMUX.TEXEL1_A;
        }

        private static bool ColorCombinePassUsesT0(ColorCombinePass ccp)
        {
            return ccp.a == CCMUX.TEXEL0 || ccp.a == CCMUX.TEXEL0_A ||
                   ccp.b == CCMUX.TEXEL0 || ccp.b == CCMUX.TEXEL0_A ||
                   ccp.c == CCMUX.TEXEL0 || ccp.c == CCMUX.TEXEL0_A ||
                   ccp.d == CCMUX.TEXEL0 || ccp.d == CCMUX.TEXEL0_A;
        }

        private static bool AlphaCombinePassUsesT0(AlphaCombinePass acp)
        {
            return acp.a == ACMUX.TEXEL0 || acp.b == ACMUX.TEXEL0 || acp.c == ACMUX.TEXEL0 || acp.d == ACMUX.TEXEL0;
        }

        private static bool ColorCombinePassUsesT1(ColorCombinePass ccp)
        {
            return ccp.a == CCMUX.TEXEL1 || ccp.a == CCMUX.TEXEL1_A ||
                   ccp.b == CCMUX.TEXEL1 || ccp.b == CCMUX.TEXEL1_A ||
                   ccp.c == CCMUX.TEXEL1 || ccp.c == CCMUX.TEXEL1_A ||
                   ccp.d == CCMUX.TEXEL1 || ccp.d == CCMUX.TEXEL1_A;
        }

        private static bool AlphaCombinePassUsesT1(AlphaCombinePass acp)
        {
            return acp.a == ACMUX.TEXEL1 || acp.b == ACMUX.TEXEL1 || acp.c == ACMUX.TEXEL1 || acp.d == ACMUX.TEXEL1;
        }
    }
}
