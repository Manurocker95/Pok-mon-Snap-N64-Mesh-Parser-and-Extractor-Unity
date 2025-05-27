using System;
using System.Collections.Generic;

namespace VirtualPhenix.Nintendo64
{
    public static class GfxUtils
    {
        public static int GfxRenderInstCompareSortKey(GfxRenderInst a, GfxRenderInst b)
        {
            return (int)(a.SortKey - b.SortKey);
        }
        public static GfxCullMode TranslateCullMode(long m)
        {
            bool cullFront = (m & (int)BanjoKazooie.RSP_Geometry.G_CULL_FRONT) != 0;
            bool cullBack = (m & (int)BanjoKazooie.RSP_Geometry.G_CULL_BACK) != 0;

            if (cullFront && cullBack)
                throw new Exception("whoops");
            else if (cullFront)
                return GfxCullMode.Front;
            else if (cullBack)
                return GfxCullMode.Back;
            else
                return GfxCullMode.None;
        }

        public static List<T> ArrayCopy<T>(List<T> a, CopyFunc<T> copyFunc)
        {
            var b = new List<T>(a.Count);
            for (int i = 0; i < a.Count; i++)
                b.Add(copyFunc(a[i]));
            return b;
        }


        public static bool ArrayEqual<T>(List<T> a, List<T> b, EqualFunc<T> e)
        {
            if (a.Count != b.Count)
                return false;
            for (long i = 0; i < a.Count; i++)
                if (!e(a[(int)i], b[(int)i]))
                    return false;
            return true;
        }

        public static GfxBindingLayoutSamplerDescriptor DefaultBindingLayout()
        {
            var layout = new GfxBindingLayoutSamplerDescriptor()  
            {
                FormatKind = GfxSamplerFormatKind.Float,
                Dimension = GfxTextureDimension.n2D
            };

            return layout;
        }
      
        public static bool IsFormatSamplerKindCompatible(GfxSamplerFormatKind samplerKind, GfxSamplerFormatKind textureKind)
        {
            if (textureKind == samplerKind)
                return true;
            else if (samplerKind == GfxSamplerFormatKind.UnfilterableFloat && (textureKind == GfxSamplerFormatKind.Depth || textureKind == GfxSamplerFormatKind.Float))
                return true;

            return false;
        }

        public static uint MakeFormat(FormatTypeFlags type, FormatCompFlags comp, FormatFlags flags)
        {
            return ((uint)type << 16) | ((uint)comp << 8) | (uint)flags;
        }

        public static FormatCompFlags GetFormatCompFlags(GfxFormat fmt)
        {
            return fmt.GetFormatCompFlags();
        }

        public static FormatTypeFlags GetFormatTypeFlags(GfxFormat fmt)
        {
            return fmt.GetFormatTypeFlags();
        }

        public static FormatFlags GetFormatFlags(GfxFormat fmt)
        {
            return fmt.GetFormatFlags();
        }

        public static int GetFormatTypeFlagsByteSize(FormatTypeFlags typeFlags)
        {
            switch (typeFlags)
            {
                case FormatTypeFlags.F32:
                case FormatTypeFlags.U32:
                case FormatTypeFlags.S32:
                    return 4;

                case FormatTypeFlags.U16:
                case FormatTypeFlags.S16:
                case FormatTypeFlags.F16:
                    return 2;

                case FormatTypeFlags.U8:
                case FormatTypeFlags.S8:
                    return 1;

                default:
                    throw new System.Exception($"Unsupported FormatTypeFlags: {typeFlags}");
            }
        }

        public static long GetGfxFormat(GfxFormatOrder order)
        {
            switch (order)
            {
                case GfxFormatOrder.F16_R: return MakeFormat(FormatTypeFlags.F16, FormatCompFlags.R, FormatFlags.None);
                case GfxFormatOrder.F16_RG: return MakeFormat(FormatTypeFlags.F16, FormatCompFlags.RG, FormatFlags.None);
                case GfxFormatOrder.F16_RGB: return MakeFormat(FormatTypeFlags.F16, FormatCompFlags.RGB, FormatFlags.None);
                case GfxFormatOrder.F16_RGBA: return MakeFormat(FormatTypeFlags.F16, FormatCompFlags.RGBA, FormatFlags.None);
                case GfxFormatOrder.F32_R: return MakeFormat(FormatTypeFlags.F32, FormatCompFlags.R, FormatFlags.None);
                case GfxFormatOrder.F32_RG: return MakeFormat(FormatTypeFlags.F32, FormatCompFlags.RG, FormatFlags.None);
                case GfxFormatOrder.F32_RGB: return MakeFormat(FormatTypeFlags.F32, FormatCompFlags.RGB, FormatFlags.None);
                case GfxFormatOrder.F32_RGBA: return MakeFormat(FormatTypeFlags.F32, FormatCompFlags.RGBA, FormatFlags.None);
                case GfxFormatOrder.U8_R: return MakeFormat(FormatTypeFlags.U8, FormatCompFlags.R, FormatFlags.None);
                case GfxFormatOrder.U8_R_NORM: return MakeFormat(FormatTypeFlags.U8, FormatCompFlags.R, FormatFlags.Normalized);
                case GfxFormatOrder.U8_RG: return MakeFormat(FormatTypeFlags.U8, FormatCompFlags.RG, FormatFlags.None);
                case GfxFormatOrder.U8_RG_NORM: return MakeFormat(FormatTypeFlags.U8, FormatCompFlags.RG, FormatFlags.Normalized);
                case GfxFormatOrder.U8_RGB: return MakeFormat(FormatTypeFlags.U8, FormatCompFlags.RGB, FormatFlags.None);
                case GfxFormatOrder.U8_RGB_NORM: return MakeFormat(FormatTypeFlags.U8, FormatCompFlags.RGB, FormatFlags.Normalized);
                case GfxFormatOrder.U8_RGB_SRGB: return MakeFormat(FormatTypeFlags.U8, FormatCompFlags.RGB, FormatFlags.Normalized | FormatFlags.sRGB);
                case GfxFormatOrder.U8_RGBA: return MakeFormat(FormatTypeFlags.U8, FormatCompFlags.RGBA, FormatFlags.None);
                case GfxFormatOrder.U8_RGBA_NORM: return MakeFormat(FormatTypeFlags.U8, FormatCompFlags.RGBA, FormatFlags.Normalized);
                case GfxFormatOrder.U8_RGBA_SRGB: return MakeFormat(FormatTypeFlags.U8, FormatCompFlags.RGBA, FormatFlags.Normalized | FormatFlags.sRGB);
                case GfxFormatOrder.U16_R: return MakeFormat(FormatTypeFlags.U16, FormatCompFlags.R, FormatFlags.None);
                case GfxFormatOrder.U16_R_NORM: return MakeFormat(FormatTypeFlags.U16, FormatCompFlags.R, FormatFlags.Normalized);
                case GfxFormatOrder.U16_RG_NORM: return MakeFormat(FormatTypeFlags.U16, FormatCompFlags.RG, FormatFlags.Normalized);
                case GfxFormatOrder.U16_RGBA_NORM: return MakeFormat(FormatTypeFlags.U16, FormatCompFlags.RGBA, FormatFlags.Normalized);
                case GfxFormatOrder.U16_RGB: return MakeFormat(FormatTypeFlags.U16, FormatCompFlags.RGB, FormatFlags.None);
                case GfxFormatOrder.U32_R: return MakeFormat(FormatTypeFlags.U32, FormatCompFlags.R, FormatFlags.None);
                case GfxFormatOrder.U32_RG: return MakeFormat(FormatTypeFlags.U32, FormatCompFlags.RG, FormatFlags.None);
                case GfxFormatOrder.S8_R: return MakeFormat(FormatTypeFlags.S8, FormatCompFlags.R, FormatFlags.None);
                case GfxFormatOrder.S8_R_NORM: return MakeFormat(FormatTypeFlags.S8, FormatCompFlags.R, FormatFlags.Normalized);
                case GfxFormatOrder.S8_RG_NORM: return MakeFormat(FormatTypeFlags.S8, FormatCompFlags.RG, FormatFlags.Normalized);
                case GfxFormatOrder.S8_RGB_NORM: return MakeFormat(FormatTypeFlags.S8, FormatCompFlags.RGB, FormatFlags.Normalized);
                case GfxFormatOrder.S8_RGBA_NORM: return MakeFormat(FormatTypeFlags.S8, FormatCompFlags.RGBA, FormatFlags.Normalized);
                case GfxFormatOrder.S16_R: return MakeFormat(FormatTypeFlags.S16, FormatCompFlags.R, FormatFlags.None);
                case GfxFormatOrder.S16_RG: return MakeFormat(FormatTypeFlags.S16, FormatCompFlags.RG, FormatFlags.None);
                case GfxFormatOrder.S16_R_NORM: return MakeFormat(FormatTypeFlags.S16, FormatCompFlags.R, FormatFlags.Normalized);
                case GfxFormatOrder.S16_RG_NORM: return MakeFormat(FormatTypeFlags.S16, FormatCompFlags.RG, FormatFlags.Normalized);
                case GfxFormatOrder.S16_RGB_NORM: return MakeFormat(FormatTypeFlags.S16, FormatCompFlags.RGB, FormatFlags.Normalized);
                case GfxFormatOrder.S16_RGBA: return MakeFormat(FormatTypeFlags.S16, FormatCompFlags.RGBA, FormatFlags.None);
                case GfxFormatOrder.S16_RGBA_NORM: return MakeFormat(FormatTypeFlags.S16, FormatCompFlags.RGBA, FormatFlags.Normalized);
                case GfxFormatOrder.S32_R: return MakeFormat(FormatTypeFlags.S32, FormatCompFlags.R, FormatFlags.None);
                case GfxFormatOrder.U16_RGBA_5551: return MakeFormat(FormatTypeFlags.U16_PACKED_5551, FormatCompFlags.RGBA, FormatFlags.Normalized);
                case GfxFormatOrder.U16_RGB_565: return MakeFormat(FormatTypeFlags.U16_PACKED_565, FormatCompFlags.RGB, FormatFlags.Normalized);
                case GfxFormatOrder.BC1: return MakeFormat(FormatTypeFlags.BC1, FormatCompFlags.RGBA, FormatFlags.Normalized);
                case GfxFormatOrder.BC1_SRGB: return MakeFormat(FormatTypeFlags.BC1, FormatCompFlags.RGBA, FormatFlags.Normalized | FormatFlags.sRGB);
                case GfxFormatOrder.BC2: return MakeFormat(FormatTypeFlags.BC2, FormatCompFlags.RGBA, FormatFlags.Normalized);
                case GfxFormatOrder.BC2_SRGB: return MakeFormat(FormatTypeFlags.BC2, FormatCompFlags.RGBA, FormatFlags.Normalized | FormatFlags.sRGB);
                case GfxFormatOrder.BC3: return MakeFormat(FormatTypeFlags.BC3, FormatCompFlags.RGBA, FormatFlags.Normalized);
                case GfxFormatOrder.BC3_SRGB: return MakeFormat(FormatTypeFlags.BC3, FormatCompFlags.RGBA, FormatFlags.Normalized | FormatFlags.sRGB);
                case GfxFormatOrder.BC4_UNORM: return MakeFormat(FormatTypeFlags.BC4_UNORM, FormatCompFlags.R, FormatFlags.Normalized);
                case GfxFormatOrder.BC4_SNORM: return MakeFormat(FormatTypeFlags.BC4_SNORM, FormatCompFlags.R, FormatFlags.Normalized);
                case GfxFormatOrder.BC5_UNORM: return MakeFormat(FormatTypeFlags.BC5_UNORM, FormatCompFlags.RG, FormatFlags.Normalized);
                case GfxFormatOrder.BC5_SNORM: return MakeFormat(FormatTypeFlags.BC5_SNORM, FormatCompFlags.RG, FormatFlags.Normalized);
                case GfxFormatOrder.BC6H_UNORM: return MakeFormat(FormatTypeFlags.BC6H_UNORM, FormatCompFlags.RGB, FormatFlags.Normalized);
                case GfxFormatOrder.BC6H_SNORM: return MakeFormat(FormatTypeFlags.BC6H_SNORM, FormatCompFlags.RGB, FormatFlags.Normalized);
                case GfxFormatOrder.BC7: return MakeFormat(FormatTypeFlags.BC7, FormatCompFlags.RGBA, FormatFlags.Normalized);
                case GfxFormatOrder.BC7_SRGB: return MakeFormat(FormatTypeFlags.BC7, FormatCompFlags.RGBA, FormatFlags.Normalized | FormatFlags.sRGB);
                case GfxFormatOrder.D24: return MakeFormat(FormatTypeFlags.D24, FormatCompFlags.R, FormatFlags.Depth);
                case GfxFormatOrder.D24_S8: return MakeFormat(FormatTypeFlags.D24S8, FormatCompFlags.RG, FormatFlags.Depth | FormatFlags.Stencil);
                case GfxFormatOrder.D32F: return MakeFormat(FormatTypeFlags.D32F, FormatCompFlags.R, FormatFlags.Depth);
                case GfxFormatOrder.D32F_S8: return MakeFormat(FormatTypeFlags.D32FS8, FormatCompFlags.RG, FormatFlags.Depth | FormatFlags.Stencil);
                case GfxFormatOrder.U8_RGB_RT: return MakeFormat(FormatTypeFlags.U8, FormatCompFlags.RGB, FormatFlags.RenderTarget | FormatFlags.Normalized);
                case GfxFormatOrder.U8_RGBA_RT: return MakeFormat(FormatTypeFlags.U8, FormatCompFlags.RGBA, FormatFlags.RenderTarget | FormatFlags.Normalized);
                case GfxFormatOrder.U8_RGBA_RT_SRGB: return MakeFormat(FormatTypeFlags.U8, FormatCompFlags.RGBA, FormatFlags.RenderTarget | FormatFlags.Normalized | FormatFlags.sRGB);
                default:
                    throw new Exception($"Unknown GfxFormatOrder: {order}");
            }
        }

        public static GfxFormat GetGfxFormatByOrder(GfxFormatOrder order)
        {
            return new GfxFormat(order);
        }
    }
}