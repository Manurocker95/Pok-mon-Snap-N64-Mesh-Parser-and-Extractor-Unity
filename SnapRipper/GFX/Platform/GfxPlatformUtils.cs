using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public static class GfxPlatformUtils
    {
        public static GfxTextureDescriptor MakeTextureDescriptor2D(GfxFormat pixelFormat, long width, long height, long numLevels)
        {
            var dimension = GfxTextureDimension.n2D;
            var depth = 1;
            var usage = GfxTextureUsage.Sampled;

            return new GfxTextureDescriptor
            {
                Dimension = dimension,
                PixelFormat = pixelFormat,
                Width = width,
                Height = height,
                DepthOrArrayLayers = depth,
                NumLevels = numLevels,
                Usage = usage
            };
        }

        public static void Assert(bool condition, string message = "")
        {
            if (!condition)
            {
                UnityEngine.Debug.LogError(Environment.StackTrace);
                throw new Exception("Assert fail: " + message);
            }
        }

        public static T AssertExists<T>(T value)
        {
            if (value == null)
                throw new Exception("Missing object");
            return value;
        }

        public static List<long> Range(long start, long count)
        {
            var result = new List<long>();
            for (long i = start; i < start + count; i++)
                result.Add(i);
            return result;
        }

        public static string LeftPad(string s, int totalWidth, char ch = '0')
        {
            return s.PadLeft(totalWidth, ch);
        }

        public static List<T> NArray<T>(int n, Func<T> constructor)
        {
            var result = new List<T>(n);
            for (int i = 0; i < n; i++)
                result.Add(constructor());
            return result;
        }

        public static List<T> NArray<T>(int n, Func<long, T> generator)
        {
            var result = new List<T>(n);
            for (int i = 0; i < n; i++)
                result[i] = generator(i);
            return result;
        }

        public static T Nullify<T>(T value) where T : class
        {
            return value == null ? null : value;
        }

        // Assumes multiple is power of two
        public static long Align(long n, long multiple)
        {
            long mask = multiple - 1;
            return (n + mask) & ~mask;
        }

        public static long AlignNonPowerOfTwo(long n, long multiple)
        {
            return ((n + multiple - 1) / multiple) * multiple;
        }
        public static T FallbackUndefined<T>(T v, T fallback)
        {
            return v != null ? v : fallback;
        }
    }
}
