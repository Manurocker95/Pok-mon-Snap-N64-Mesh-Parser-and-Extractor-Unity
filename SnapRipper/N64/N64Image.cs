using System;
using System.Drawing.Imaging;

namespace VirtualPhenix.Nintendo64
{
    public static class N64Image
    {
        public static long GetTLUTSize(ImageSize siz)
        {
            switch (siz)
            {
                case ImageSize.SIZE_4B:
                    return 0x010;
                case ImageSize.SIZE_8B:
                    return 0x0100;
                case ImageSize.SIZE_16B:
                    return 0x01000;
                default:
                    return 0x10000;
            }

        }

        public static byte Expand3To8(long n)
        {
            return (byte)(((n << (8 - 3)) | (n << (8 - 6)) | (n >> (9 - 8))) & 0xFF);
        }

        public static byte Expand4To8(long n)
        {
            return (byte)(((n << (8 - 4)) | (n >> (8 - 8))) & 0xFF);
        }

        public static byte Expand5To8(long n)
        {
            return (byte)(((n << (8 - 5)) | (n >> (10 - 8))) & 0xFF);
        }

        public static void R5G5B5A1(byte[] dst, long dstOffs, ushort p)
        {
            dst[dstOffs + 0] = Expand5To8((p & 0xF800) >> 11);
            dst[dstOffs + 1] = Expand5To8((p & 0x07C0) >> 6);
            dst[dstOffs + 2] = Expand5To8((p & 0x003E) >> 1);
            dst[dstOffs + 3] = (byte)((p & 0x0001) != 0 ? 0xFF : 0x00);
        }

        public static void CopyTLUTColor(byte[] dst, long dstOffs, byte[] colorTable, long i)
        {
            Array.Copy(colorTable, i * 4, dst, dstOffs, 4);
        }

        public static string GetImageFormatName(ImageFormat fmt)
        {
            return fmt.ToString();
        }

        public static string GetImageSizeName(ImageSize siz)
        {
            return GetBitsPerPixel(siz).ToString();
        }

        public static long GetBitsPerPixel(ImageSize siz)
        {
            switch (siz)
            {
                case ImageSize.SIZE_4B:
                    return 4;
                case ImageSize.SIZE_8B:
                    return 8;
                case ImageSize.SIZE_16B:
                    return 16;
                default:
                    return 32;
            }
        }
    }


}