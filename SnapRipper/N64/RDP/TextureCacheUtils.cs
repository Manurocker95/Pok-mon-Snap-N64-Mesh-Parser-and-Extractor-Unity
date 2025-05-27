using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.RDP
{
    public class TextureCacheUtils
    {
        public static bool TextureMatch(TileState a, TileState b)
        {
            return a.fmt == b.fmt && a.siz == b.siz && a.line == b.line &&
                   a.palette == b.palette && a.cmt == b.cmt && a.cms == b.cms &&
                   a.maskt == b.maskt && a.masks == b.masks &&
                   a.shiftt == b.shiftt && a.shifts == b.shifts &&
                   a.uls == b.uls && a.ult == b.ult &&
                   a.lrs == b.lrs && a.lrt == b.lrt;
        }

        public static Texture TranslateTileTexture(VP_ArrayBufferSlice[] segmentBuffers, long dramAddr, long dramPalAddr, TileState tile, bool deinterleave = false)
        {
            var tlutColorTable = new byte[256 * 4];
            var idx = (int)dramAddr >> 24;
            var view = segmentBuffers[idx].CreateDefaultDataView();

            if (tile.fmt == (long)ImageFormat.CI)
                TranslateTLUT(tlutColorTable, segmentBuffers, dramPalAddr, (ImageSize)tile.siz);

            long tileW = GetTileWidth(tile);
            long tileH = GetTileHeight(tile);

            byte[] dst = new byte[tileW * tileH * 4];
            long srcIdx = dramAddr & 0x00FFFFFF;

            switch ((tile.fmt << 4) | tile.siz)
            {
                case (long)((((long)ImageFormat.CI) << 4) | (long)ImageSize.SIZE_4B): DecodeTex_CI4(dst, view, srcIdx, tileW, tileH, tlutColorTable, tile.line, deinterleave); break;
                case (long)(((long)ImageFormat.CI << 4) | (long)ImageSize.SIZE_8B): DecodeTex_CI8(dst, view, srcIdx, tileW, tileH, tlutColorTable, tile.line, deinterleave); break;
                case (long)(((long)ImageFormat.IA << 4) | (long)ImageSize.SIZE_4B): DecodeTex_IA4(dst, view, srcIdx, tileW, tileH, tile.line, deinterleave); break;
                case (long)(((long)ImageFormat.IA << 4) | (long)ImageSize.SIZE_8B): DecodeTex_IA8(dst, view, srcIdx, tileW, tileH, tile.line, deinterleave); break;
                case (long)(((long)ImageFormat.IA << 4) | (long)ImageSize.SIZE_16B): DecodeTex_IA16(dst, view, srcIdx, tileW, tileH, tile.line, deinterleave); break;
                case (long)(((long)ImageFormat.I << 4) | (long)ImageSize.SIZE_4B): DecodeTex_I4(dst, view, srcIdx, tileW, tileH, tile.line, deinterleave); break;
                case (long)(((long)ImageFormat.I << 4) | (long)ImageSize.SIZE_8B): DecodeTex_I8(dst, view, srcIdx, tileW, tileH, tile.line, deinterleave); break;
                case (long)(((long)ImageFormat.RGBA << 4) | (long)ImageSize.SIZE_16B): DecodeTex_RGBA16(dst, view, srcIdx, tileW, tileH, tile.line, deinterleave); break;
                case (long)(((long)ImageFormat.RGBA << 4) | (long)ImageSize.SIZE_32B): DecodeTex_RGBA32(dst, view, srcIdx, tileW, tileH); break;
                default:
                    throw new Exception($"Unknown image format {tile.fmt} / {tile.siz}");
            }

            return new Texture(tile, dramAddr, dramPalAddr, tileW, tileH, dst);
        }

        public static long GetTileWidth(TileState tile)
        {
            long coordWidth = ((tile.lrs - tile.uls) >> 2) + 1;
            return tile.masks != 0 ? Math.Min(1 << (int)tile.masks, coordWidth) : coordWidth;
        }

        public static long GetTileHeight(TileState tile)
        {
            long coordHeight = ((tile.lrt - tile.ult) >> 2) + 1;
            return tile.maskt != 0 ? Math.Min(1 << (int)tile.maskt, coordHeight) : coordHeight;
        }

        public static void TranslateTLUT(byte[] dst, VP_ArrayBufferSlice[] segmentBuffers, long dramAddr, ImageSize siz)
        {
            var view = segmentBuffers[dramAddr >> 24].CreateDefaultDataView();
            long srcIdx = dramAddr & 0x00FFFFFF;
            ParseTLUT(dst, view, srcIdx, siz, TextureLUT.G_TT_RGBA16);
        }

        public static long ParseTLUT(byte[] dst, VP_DataView view, long idx, ImageSize siz, TextureLUT lutMode)
        {
            if (lutMode != TextureLUT.G_TT_RGBA16)
                throw new Exception("Only RGBA16 TLUT mode supported");

            long tlutSize = N64Image.GetTLUTSize(siz);
            for (long i = 0; i < tlutSize; i++)
            {
                ushort p = view.GetUint16(idx, false);
                R5G5B5A1(dst, i * 4, p);
                idx += 2;
            }
            return tlutSize * 2;
        }

        public static void DecodeTex_CI4(byte[] dst, VP_DataView view, long src, long width, long height, byte[] tlut, long line, bool deinterleave)
        {
            long dstOffs = 0;
            for (long y = 0; y < height; y++)
            {
                var srcOffs = src + y * line * 8;
                for (long x = 0; x < width; x += 2)
                {
                    byte b = view.GetUint8(srcOffs++);
                    long i0 = (b >> 4) & 0x0F;
                    long i1 = b & 0x0F;
                    Array.Copy(tlut, i0 * 4, dst, dstOffs, 4); dstOffs += 4;
                    if (x + 1 < width) { Array.Copy(tlut, i1 * 4, dst, dstOffs, 4); dstOffs += 4; }
                }
            }
        }

        public static void DecodeTex_CI8(byte[] dst, VP_DataView view, long src, long width, long height, byte[] tlut, long line, bool deinterleave)
        {
            long dstOffs = 0;
            for (long y = 0; y < height; y++)
            {
                var srcOffs = src + y * line * 8;
                for (long x = 0; x < width; x++)
                {
                    byte i = view.GetUint8(srcOffs++);
                    Array.Copy(tlut, i * 4, dst, dstOffs, 4); dstOffs += 4;
                }
            }
        }

        public static void DecodeTex_IA4(byte[] dst, VP_DataView view, long src, long width, long height, long line, bool deinterleave)
        {
            long dstOffs = 0;
            for (long y = 0; y < height; y++)
            {
                var srcOffs = src + y * line * 8;
                for (long x = 0; x < width; x += 2)
                {
                    byte b = view.GetUint8(srcOffs++);
                    for (long i = 0; i < 2 && x + i < width; i++)
                    {
                        long shift = 4 * (1 - i);
                        long v = (b >> (int)shift) & 0x0F;
                        long a = (v & 0x8) != 0 ? 0xFF : 0x00;
                        long intensity = Expand3To8(v & 0x7);
                        dst[dstOffs++] = (byte)intensity;
                        dst[dstOffs++] = (byte)intensity;
                        dst[dstOffs++] = (byte)intensity;
                        dst[dstOffs++] = (byte)a;
                    }
                }
            }
        }

        public static void DecodeTex_IA8(byte[] dst, VP_DataView view, long src, long width, long height, long line, bool deinterleave)
        {
            long dstOffs = 0;
            for (long y = 0; y < height; y++)
            {
                var srcOffs = src + y * line * 8;
                for (long x = 0; x < width; x++)
                {
                    byte b = view.GetUint8(srcOffs++);
                    long i = (b >> 4) & 0x0F;
                    long a = b & 0x0F;
                    long intensity = Expand4To8(i);
                    long alpha = Expand4To8(a);
                    dst[dstOffs++] = (byte)intensity;
                    dst[dstOffs++] = (byte)intensity;
                    dst[dstOffs++] = (byte)intensity;
                    dst[dstOffs++] = (byte)alpha;
                }
            }
        }

        public static void DecodeTex_IA16(byte[] dst, VP_DataView view, long src, long width, long height, long line, bool deinterleave)
        {
            long dstOffs = 0;
            for (long y = 0; y < height; y++)
            {
                var srcOffs = src + y * line * 8;
                for (long x = 0; x < width; x++)
                {
                    byte i = view.GetUint8(srcOffs++);
                    byte a = view.GetUint8(srcOffs++);
                    dst[dstOffs++] = i;
                    dst[dstOffs++] = i;
                    dst[dstOffs++] = i;
                    dst[dstOffs++] = a;
                }
            }
        }

        public static void DecodeTex_I4(byte[] dst, VP_DataView view, long src, long width, long height, long line, bool deinterleave)
        {
            long dstOffs = 0;
            for (long y = 0; y < height; y++)
            {
                var srcOffs = src + y * line * 8;
                for (long x = 0; x < width; x += 2)
                {
                    byte b = view.GetUint8(srcOffs++);
                    long i0 = Expand4To8((b >> 4) & 0x0F);
                    long i1 = Expand4To8(b & 0x0F);
                    dst[dstOffs++] = (byte)i0;
                    dst[dstOffs++] = (byte)i0;
                    dst[dstOffs++] = (byte)i0;
                    dst[dstOffs++] = 0xFF;
                    if (x + 1 < width)
                    {
                        dst[dstOffs++] = (byte)i1;
                        dst[dstOffs++] = (byte)i1;
                        dst[dstOffs++] = (byte)i1;
                        dst[dstOffs++] = 0xFF;
                    }
                }
            }
        }

        public static void DecodeTex_I8(byte[] dst, VP_DataView view, long src, long width, long height, long line, bool deinterleave)
        {
            long dstOffs = 0;
            for (long y = 0; y < height; y++)
            {
                var srcOffs = src + y * line * 8;
                for (long x = 0; x < width; x++)
                {
                    byte i = view.GetUint8(srcOffs++);
                    dst[dstOffs++] = i;
                    dst[dstOffs++] = i;
                    dst[dstOffs++] = i;
                    dst[dstOffs++] = 0xFF;
                }
            }
        }
        public static byte Expand5To8(long n) => (byte)(((n << 3) | (n >> 2)) & 0xFF);
        public static byte Expand4To8(long n) => (byte)(((n << 4) | n) & 0xFF);
        public static byte Expand3To8(long n) => (byte)(((n << 5) | (n << 2) | (n >> 1)) & 0xFF);

        public static void R5G5B5A1(byte[] dst, long dstOffs, ushort p)
        {
            dst[dstOffs + 0] = Expand5To8((p & 0xF800) >> 11);
            dst[dstOffs + 1] = Expand5To8((p & 0x07C0) >> 6);
            dst[dstOffs + 2] = Expand5To8((p & 0x003E) >> 1);
            dst[dstOffs + 3] = (byte)((p & 0x0001) != 0 ? 0xFF : 0x00);
        }

        public static void DecodeTex_RGBA16(byte[] dst, VP_DataView view, long src, long width, long height, long line, bool deinterleave)
        {
            long dstOffs = 0;
            for (long y = 0; y < height; y++)
            {
                var srcOffs = src + y * line * 8;
                for (long x = 0; x < width; x++)
                {
                    var p = view.GetUint16(srcOffs, false);
                    R5G5B5A1(dst, dstOffs, p);
                    dstOffs += 4;
                    srcOffs += 2;
                }
            }
        }

        public static void DecodeTex_RGBA32(byte[] dst, VP_DataView view, long src, long width, long height)
        {
            long dstOffs = 0;
            var srcOffs = src;
            for (long y = 0; y < height; y++)
            {
                for (long x = 0; x < width; x++)
                {
                    dst[dstOffs++] = view.GetUint8(srcOffs++); // R
                    dst[dstOffs++] = view.GetUint8(srcOffs++); // G
                    dst[dstOffs++] = view.GetUint8(srcOffs++); // B
                    dst[dstOffs++] = view.GetUint8(srcOffs++); // A
                }
            }
        }
    }
}
