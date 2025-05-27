using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.PokemonSnap;

namespace VirtualPhenix.Nintendo64
{
    public static class GfxRenderInstUtils
    {
        public const long MaxDepth = 0x10000;
        public const int DepthBits = 16;

        public static long MakeDepthKey(double depth, bool flipDepth, long maxDepth = MaxDepth)
        {
            double normalizedDepth = MathHelper.Clamp(depth, 0, maxDepth) / maxDepth;
            if (flipDepth)
                normalizedDepth = 1.0 - normalizedDepth;
            long depthKey = (long)(normalizedDepth * ((1 << DepthBits) - 1));
            return depthKey & 0xFFFF;
        }

        public static long GetSortKeyLayer(long sortKey)
        {
            return (sortKey >> 24) & 0xFF;
        }

        public static long SetSortKeyProgramKey(long sortKey, long programKey)
        {
            bool isTransparent = ((sortKey >> 31) & 1) != 0;
            if (isTransparent)
                return sortKey;
            else
                return (sortKey & 0xFF0000FF) | ((programKey & 0xFFFF) << 8);
        }

        public static long SetSortKeyBias(long sortKey, long bias)
        {
            bool isTransparent = ((sortKey >> 31) & 1) != 0;
            if (isTransparent)
                return (sortKey & 0xFFFFFF00) | (bias & 0xFF);
            else
                return sortKey;
        }

        public static long MakeSortKeyOpaque(long layer, long programKey)
        {
            return SetSortKeyLayer(SetSortKeyProgramKey(0, programKey), layer);
        }

        public static long SetSortKeyOpaqueDepth(long sortKey, long depthKey)
        {
            Assert(depthKey >= 0);
            return (sortKey & 0xFFFFFF00) | ((depthKey >> 8) & 0xFF);
        }

        public static long MakeSortKeyTranslucent(long layer)
        {
            return SetSortKeyLayer(0, layer);
        }

        public static long SetSortKeyTranslucentDepth(long sortKey, long depthKey)
        {
            Assert(depthKey >= 0);
            return (sortKey & 0xFF0000FF) | (depthKey << 8);
        }

        public static long SetSortKeyLayer(long sortKey, long layer)
        {
            return (sortKey & 0x00FFFFFF) | ((layer & 0xFF) << 24);
        }

        private static void Assert(bool condition)
        {
            if (!condition)
                throw new Exception("Assertion failed.");
        }

        public static long MakeSortKey(GfxRendererLayer layer, long programKey = 0)
        {
            if ((layer & GfxRendererLayer.TRANSLUCENT) != 0)
                return MakeSortKeyTranslucent((long)layer);
            else
                return MakeSortKeyOpaque((long)layer, programKey);
        }

        public static long SetSortKeyDepthKey(long sortKey, long depthKey)
        {
            bool isTranslucent = ((sortKey >> 31) & 1) != 0;
            return isTranslucent ? SetSortKeyTranslucentDepth(sortKey, depthKey)
                                 : SetSortKeyOpaqueDepth(sortKey, depthKey);
        }

        public static long SetSortKeyDepth(long sortKey, double depth, long maxDepth = MaxDepth)
        {
            bool isTranslucent = ((sortKey >> 31) & 1) != 0;
            long depthKey = MakeDepthKey(depth, isTranslucent, maxDepth);
            return isTranslucent ? SetSortKeyTranslucentDepth(sortKey, depthKey)
                                 : SetSortKeyOpaqueDepth(sortKey, depthKey);
        }
    }
}
