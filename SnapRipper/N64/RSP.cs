using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public static class RSP
    {
        public static void LoadVertexFromView(RSPVertex dst, VP_DataView<VP_ArrayBuffer> view, long offs)
        {
            dst.x = view.GetInt16(offs + 0x00);
            dst.y = view.GetInt16(offs + 0x02);
            dst.z = view.GetInt16(offs + 0x04);

            dst.tx = (view.GetInt16(offs + 0x08) / 0x20); // Convert from S10.5 fixed-point
            dst.ty = (view.GetInt16(offs + 0x0A) / 0x20);

            dst.c0 = (view.GetUint8(offs + 0x0C) / 0xFF);
            dst.c1 = (view.GetUint8(offs + 0x0D) / 0xFF);
            dst.c2 = (view.GetUint8(offs + 0x0E) / 0xFF);
            dst.a = (view.GetUint8(offs + 0x0F) / 0xFF);
        }

        public static long CalculateTextureScaleForShift(long shift)
        {
            if (shift <= 10)
            {
                return 1 / (1 << (int)shift);
            }
            else
            {
                return 1 << (16 - (int)shift);
            }
        }

        public static void CalculateTextureMatrixFromRSPState(ref Matrix4x4 dst, long texScaleS, long texScaleT, long tileWidth, long tileHeight, long tileShiftS, long tileShiftT)
        {
            var tileScaleS = CalculateTextureScaleForShift(tileShiftS) / tileWidth;
            var tileScaleT = CalculateTextureScaleForShift(tileShiftT) / tileHeight;

            dst[0] = (texScaleS * tileScaleS);
            dst[5] = (texScaleT * tileScaleT);

            dst[12] = (0.5f * tileScaleS);
            dst[13] = (0.5f * tileScaleT);
        }
    }
}