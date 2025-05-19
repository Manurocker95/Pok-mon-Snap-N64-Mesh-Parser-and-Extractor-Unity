using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public enum FormatFlags : byte
    {
        None = 0b00000000,
        Normalized = 0b00000001,
        sRGB = 0b00000010,
        Depth = 0b00000100,
        Stencil = 0b00001000,
        RenderTarget = 0b00010000,
    }

    public enum FormatCompFlags : byte
    {
        R = 0x01,
        RG = 0x02,
        RGB = 0x03,
        RGBA = 0x04,
    }

    public enum FormatTypeFlags : ushort
    {
        U8 = 0x01,
        U16 = 0x02,
        U32 = 0x03,
        S8 = 0x04,
        S16 = 0x05,
        S32 = 0x06,
        F16 = 0x07,
        F32 = 0x08,

        // Compressed texture formats.
        BC1 = 0x41,
        BC2 = 0x42,
        BC3 = 0x43,
        BC4_UNORM = 0x44,
        BC4_SNORM = 0x45,
        BC5_UNORM = 0x46,
        BC5_SNORM = 0x47,
        BC6H_UNORM = 0x48,
        BC6H_SNORM = 0x49,
        BC7 = 0x4A,

        // Packed texture formats.
        U16_PACKED_5551 = 0x61,
        U16_PACKED_565 = 0x62,

        // Depth/stencil texture formats.
        D24 = 0x81,
        D32F = 0x82,
        D24S8 = 0x83,
        D32FS8 = 0x84,
    }

    public enum GfxFormatOrder : uint
    {
        F16_R = 0,
        F16_RG = 1,
        F16_RGB = 2,
        F16_RGBA = 3,
        F32_R = 4,
        F32_RG = 5,
        F32_RGB = 6,
        F32_RGBA = 7,
        U8_R = 8,
        U8_R_NORM = 9,
        U8_RG = 10,
        U8_RG_NORM = 11,
        U8_RGB = 12,
        U8_RGB_NORM = 13,
        U8_RGB_SRGB = 14,
        U8_RGBA = 15,
        U8_RGBA_NORM = 16,
        U8_RGBA_SRGB = 17,
        U16_R = 18,
        U16_R_NORM = 19,
        U16_RG_NORM = 20,
        U16_RGBA_NORM = 21,
        U16_RGB = 22,
        U32_R = 23,
        U32_RG = 24,
        S8_R = 25,
        S8_R_NORM = 26,
        S8_RG_NORM = 27,
        S8_RGB_NORM = 28,
        S8_RGBA_NORM = 29,
        S16_R = 30,
        S16_RG = 31,
        S16_R_NORM = 32,
        S16_RG_NORM = 33,
        S16_RGB_NORM = 34,
        S16_RGBA = 35,
        S16_RGBA_NORM = 36,
        S32_R = 37,
        U16_RGBA_5551 = 38,
        U16_RGB_565 = 39,
        BC1 = 40,
        BC1_SRGB = 41,
        BC2 = 42,
        BC2_SRGB = 43,
        BC3 = 44,
        BC3_SRGB = 45,
        BC4_UNORM = 46,
        BC4_SNORM = 47,
        BC5_UNORM = 48,
        BC5_SNORM = 49,
        BC6H_UNORM = 50,
        BC6H_SNORM = 51,
        BC7 = 52,
        BC7_SRGB = 53,
        D24 = 54,
        D24_S8 = 55,
        D32F = 56,
        D32F_S8 = 57,
        U8_RGB_RT = 58,
        U8_RGBA_RT = 59,
        U8_RGBA_RT_SRGB = 60,
    }

    public enum GfxFormatParsed : uint
    {
        F16_R = 0x00070100,
        F16_RG = 0x00070200,
        F16_RGB = 0x00070300,
        F16_RGBA = 0x00070400,
        F32_R = 0x00080100,
        F32_RG = 0x00080200,
        F32_RGB = 0x00080300,
        F32_RGBA = 0x00080400,
        U8_R = 0x00010100,
        U8_R_NORM = 0x00010101,
        U8_RG = 0x00010200,
        U8_RG_NORM = 0x00010201,
        U8_RGB = 0x00010300,
        U8_RGB_NORM = 0x00010301,
        U8_RGB_SRGB = 0x00010303,
        U8_RGBA = 0x00010400,
        U8_RGBA_NORM = 0x00010401,
        U8_RGBA_SRGB = 0x00010403,
        U16_R = 0x00020100,
        U16_R_NORM = 0x00020101,
        U16_RG_NORM = 0x00020201,
        U16_RGBA_NORM = 0x00020401,
        U16_RGB = 0x00020300,
        U32_R = 0x00030100,
        U32_RG = 0x00030200,
        S8_R = 0x00040100,
        S8_R_NORM = 0x00040101,
        S8_RG_NORM = 0x00040201,
        S8_RGB_NORM = 0x00040301,
        S8_RGBA_NORM = 0x00040401,
        S16_R = 0x00050100,
        S16_RG = 0x00050200,
        S16_R_NORM = 0x00050101,
        S16_RG_NORM = 0x00050201,
        S16_RGB_NORM = 0x00050301,
        S16_RGBA = 0x00050400,
        S16_RGBA_NORM = 0x00050401,
        S32_R = 0x00060100,
        U16_RGBA_5551 = 0x00610401,
        U16_RGB_565 = 0x00620301,
        BC1 = 0x00410401,
        BC1_SRGB = 0x00410403,
        BC2 = 0x00420401,
        BC2_SRGB = 0x00420403,
        BC3 = 0x00430401,
        BC3_SRGB = 0x00430403,
        BC4_UNORM = 0x00440101,
        BC4_SNORM = 0x00450101,
        BC5_UNORM = 0x00460201,
        BC5_SNORM = 0x00470201,
        BC6H_UNORM = 0x00480301,
        BC6H_SNORM = 0x00490301,
        BC7 = 0x004A0401,
        BC7_SRGB = 0x004A0403,
        D24 = 0x00810104,
        D24_S8 = 0x0083020C,
        D32F = 0x00820104,
        D32F_S8 = 0x0084020C,
        U8_RGB_RT = 0x00010311,
        U8_RGBA_RT = 0x00010411,
        U8_RGBA_RT_SRGB = 0x00010413,
    }

    public class GfxFormat 
    {
        public uint Value;

        public GfxFormat(FormatTypeFlags type, FormatCompFlags comp, FormatFlags flags)
        {
            Value = MakeFormat(type, comp, flags);
        }

        public uint MakeFormat(FormatTypeFlags type, FormatCompFlags comp, FormatFlags flags)
        {
            return GfxUtils.MakeFormat(type, comp, flags);
        }

        public FormatCompFlags GetFormatCompFlags()
        {
            return (FormatCompFlags)((Value >> 8) & 0xFF);
        }

        public FormatTypeFlags GetFormatTypeFlags()
        {
            return (FormatTypeFlags)((Value >> 16) & 0xFF);
        }

        public FormatFlags GetFormatFlags()
        {
            return (FormatFlags)(Value & 0xFF);
        }
    }
}
