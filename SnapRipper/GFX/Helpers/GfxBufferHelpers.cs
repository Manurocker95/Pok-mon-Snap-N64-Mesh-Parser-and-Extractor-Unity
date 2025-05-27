using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public static class GfxBufferHelpers
    {
        public static GfxBuffer MakeStaticDataBuffer(GfxDevice device, GfxBufferUsage usage, IArrayBufferLike data, long srcOffset = 0, long? srcLength = null)
        {
            long length = srcLength ?? data.ByteLength;
            long wordCount = GfxPlatformUtils.Align(data.ByteLength, 4) / 4;
            return device.CreateBuffer(wordCount, usage, GfxBufferFrequencyHint.Static, new VP_Uint8Array(data, srcOffset, length));
        }

        public static byte[] LongListToByteArray(List<long> list)
        {
            byte[] bytes = new byte[list.Count * sizeof(long)];
            Buffer.BlockCopy(list.ToArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static int FillVec3v(VP_Float32Array<VP_ArrayBuffer> buffer, long offset, Vector3 v, float v3 = 0f)
        {
            buffer[offset + 0] = v.x;
            buffer[offset + 1] = v.y;
            buffer[offset + 2] = v.z;
            buffer[offset + 3] = v3;
            return 4;
        }

        public static int FillVec4(VP_Float32Array<VP_ArrayBuffer>  buffer, long offset, float v0, float v1 = 0f, float v2 = 0f, float v3 = 0f)
        {
            buffer[offset + 0] = v0;
            buffer[offset + 1] = v1;
            buffer[offset + 2] = v2;
            buffer[offset + 3] = v3;
            return 4;
        }

        public static int FillVec4v(VP_Float32Array<VP_ArrayBuffer> buffer, long offset, Vector4 v)
        {
            buffer[offset + 0] = v.x;
            buffer[offset + 1] = v.y;
            buffer[offset + 2] = v.z;
            buffer[offset + 3] = v.w;
            return 4;
        }

        public static int FillColor(VP_Float32Array<VP_ArrayBuffer> buffer, long offset, Color c, float? a = null)
        {
            buffer[offset + 0] = c.r;
            buffer[offset + 1] = c.g;
            buffer[offset + 2] = c.b;
            buffer[offset + 3] = a ?? c.a;
            return 4;
        }

        // Assumes matrices are row-major and Unity stores them column-major internally.
        public static int FillMatrix4x4(VP_Float32Array<VP_ArrayBuffer> buffer, long offset, Matrix4x4 m)
        {
            buffer[offset + 0] = m.m00;
            buffer[offset + 1] = m.m10;
            buffer[offset + 2] = m.m20;
            buffer[offset + 3] = m.m30;
            buffer[offset + 4] = m.m01;
            buffer[offset + 5] = m.m11;
            buffer[offset + 6] = m.m21;
            buffer[offset + 7] = m.m31;
            buffer[offset + 8] = m.m02;
            buffer[offset + 9] = m.m12;
            buffer[offset + 10] = m.m22;
            buffer[offset + 11] = m.m32;
            buffer[offset + 12] = m.m03;
            buffer[offset + 13] = m.m13;
            buffer[offset + 14] = m.m23;
            buffer[offset + 15] = m.m33;
            return 16;
        }

        public static int FillMatrix4x3(VP_Float32Array<VP_ArrayBuffer> buffer, long offset, Matrix4x4 m)
        {
            buffer[offset + 0] = m.m00;
            buffer[offset + 1] = m.m10;
            buffer[offset + 2] = m.m20;
            buffer[offset + 3] = m.m30;
            buffer[offset + 4] = m.m01;
            buffer[offset + 5] = m.m11;
            buffer[offset + 6] = m.m21;
            buffer[offset + 7] = m.m31;
            buffer[offset + 8] = m.m02;
            buffer[offset + 9] = m.m12;
            buffer[offset + 10] = m.m22;
            buffer[offset + 11] = m.m32;
            return 12;
        }

        // Assumes you have a struct like this:
        // public struct Matrix3x2 { public float m00, m01, m10, m11, m20, m21; }
        public static int FillMatrix3x2(VP_Float32Array<VP_ArrayBuffer> buffer, long offset, Matrix3x2 m)
        {
            buffer[offset + 0] = m.m00;
            buffer[offset + 1] = m.m10;
            buffer[offset + 2] = m.m20;
            buffer[offset + 3] = 0.0f;
            buffer[offset + 4] = m.m01;
            buffer[offset + 5] = m.m11;
            buffer[offset + 6] = m.m21;
            buffer[offset + 7] = 0.0f;
            return 8;
        }

        public static int FillMatrix4x2(VP_Float32Array<VP_ArrayBuffer> buffer, long offset, Matrix4x4 m)
        {
            buffer[offset + 0] = m.m00;
            buffer[offset + 1] = m.m10;
            buffer[offset + 2] = m.m20;
            buffer[offset + 3] = m.m30;
            buffer[offset + 4] = m.m01;
            buffer[offset + 5] = m.m11;
            buffer[offset + 6] = m.m21;
            buffer[offset + 7] = m.m31;
            return 8;
        }
    }
}
