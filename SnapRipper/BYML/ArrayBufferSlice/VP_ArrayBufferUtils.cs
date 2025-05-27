using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public static class VP_ArrayBufferUtils 
    {
        public static VP_Float32Array<VP_ArrayBuffer> CreateFloatArray(float [] floatArray)
        {
            VP_ArrayBuffer buffer = new VP_ArrayBuffer(FloatArrayToByteArray(floatArray));
            return new VP_Float32Array<VP_ArrayBuffer> (buffer);
        }

        public static VP_Int16Array<VP_ArrayBuffer> CreateInt16Array(short[] arr)
        {
            VP_ArrayBuffer buffer = new VP_ArrayBuffer(ArrayToByteArray(arr));
            return new VP_Int16Array<VP_ArrayBuffer>(buffer);
        }

        public static VP_Uint16Array<VP_ArrayBuffer> CreateUint16Array(ushort[] arr)
        {
            VP_ArrayBuffer buffer = new VP_ArrayBuffer(ArrayToByteArray(arr));
            return new VP_Uint16Array<VP_ArrayBuffer>(buffer);
        }

        public static byte[] ArrayToByteArray<T>(T[] arr)
        {
            int size = Marshal.SizeOf<T>();
            byte[] bytes = new byte[arr.Length * size];
            Buffer.BlockCopy(arr, 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static byte[] LongArrayToByteArray(long[] longArray)
        {
            byte[] bytes = new byte[longArray.Length * sizeof(long)];
            Buffer.BlockCopy(longArray, 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static byte[] FloatArrayToByteArray(float[] floatArray)
        {
            byte[] byteArray = new byte[floatArray.Length * sizeof(float)];

            System.Buffer.BlockCopy(floatArray, 0, byteArray, 0, byteArray.Length);
            return byteArray;
        }
    }
}
