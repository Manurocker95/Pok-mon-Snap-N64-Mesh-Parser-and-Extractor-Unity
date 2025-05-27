using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class ReversedDepthHelpers
    {
        public const bool IsDepthReversed = true;

        public static readonly Matrix4x4 ReverseDepthMatrix = new Matrix4x4
        {
            m00 = 1,
            m01 = 0,
            m02 = 0,
            m03 = 0,
            m10 = 0,
            m11 = 1,
            m12 = 0,
            m13 = 0,
            m20 = 0,
            m21 = 0,
            m22 = -1,
            m23 = 0,
            m30 = 0,
            m31 = 0,
            m32 = 0,
            m33 = 1
        };

        public static void ProjectionMatrixReverseDepth(ref Matrix4x4 matrix, bool isDepthReversed = IsDepthReversed)
        {
            if (isDepthReversed)
                matrix = ReverseDepthMatrix * matrix;
        }

        public static GfxCompareMode ReverseDepthForCompareMode(GfxCompareMode compareMode, bool isDepthReversed = IsDepthReversed)
        {
            if (!isDepthReversed)
                return compareMode;

            switch (compareMode)
            {
                case GfxCompareMode.Less: return GfxCompareMode.Greater;
                case GfxCompareMode.LessEqual: return GfxCompareMode.GreaterEqual;
                case GfxCompareMode.GreaterEqual: return GfxCompareMode.LessEqual;
                case GfxCompareMode.Greater: return GfxCompareMode.Less;
                default: return compareMode;
            }
        }

        public static double ReverseDepthForClearValue(double n, bool isDepthReversed = IsDepthReversed)
        {
            return isDepthReversed ? 1.0 - n : n;
        }

        public static double ReverseDepthForDepthOffset(double n, bool isDepthReversed = IsDepthReversed)
        {
            return isDepthReversed ? -n : n;
        }

        public static bool CompareDepthValues(double a, double b, GfxCompareMode op, bool isDepthReversed = IsDepthReversed)
        {
            op = ReverseDepthForCompareMode(op, isDepthReversed);

            switch (op)
            {
                case GfxCompareMode.Less: return a < b;
                case GfxCompareMode.LessEqual: return a <= b;
                case GfxCompareMode.Greater: return a > b;
                case GfxCompareMode.GreaterEqual: return a >= b;
                default: throw new System.Exception("Unknown depth compare mode.");
            }
        }
    }
}
