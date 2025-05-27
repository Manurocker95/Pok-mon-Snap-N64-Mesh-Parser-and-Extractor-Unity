using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public static class SplineUtils 
    {
        /// <summary>
        /// Evaluate a general 1D cubic spline at time t, given the coefficients cf: [cubic, square, linear, constant].
        /// </summary>
        public static float GetPointCubic(Vector4 cf, float t)
        {
            return ((cf.x * t + cf.y) * t + cf.z) * t + cf.w;
        }

        /// <summary>
        /// Evaluate the derivative of a general 1D cubic spline at time t.
        /// </summary>
        public static float GetDerivativeCubic(Vector4 cf, float t)
        {
            return (3f * cf.x * t + 2f * cf.y) * t + cf.z;
        }

        /// <summary>
        /// Calculate the coefficients for a Cubic Hermite spline with standard parameterization.
        /// </summary>
        public static void GetCoeffHermite(out Vector4 dst, float p0, float p1, float s0, float s1)
        {
            dst = new Vector4
            {
                x = (2f * p0) + (-2f * p1) + (1f * s0) + (1f * s1),  // Cubic
                y = (-3f * p0) + (3f * p1) + (-2f * s0) + (-1f * s1), // Square
                z = (0f * p0) + (0f * p1) + (1f * s0) + (0f * s1),    // Linear
                w = (1f * p0) + (0f * p1) + (0f * s0) + (0f * s1)     // Constant
            };
        }

        /// <summary>
        /// Evaluate a Cubic Hermite spline at time t.
        /// </summary>
        public static float GetPointHermite(float p0, float p1, float s0, float s1, float t)
        {
            GetCoeffHermite(out Vector4 coeff, p0, p1, s0, s1);
            return GetPointCubic(coeff, t);
        }
        public static float GetDerivativeHermite(float p0, float p1, float s0, float s1, float t)
        {
            GetCoeffHermite(out Vector4 coeff, p0, p1, s0, s1);
            return GetDerivativeCubic(coeff, t);
        }

        /// <summary>
        /// Calculate the coefficients for a Cubic Bezier spline.
        /// </summary>
        public static void GetCoeffBezier(out Vector4 dst, float p0, float p1, float p2, float p3)
        {
            dst = new Vector4
            {
                x = -p0 + 3f * p1 - 3f * p2 + p3, // Cubic
                y = 3f * p0 - 6f * p1 + 3f * p2,  // Square
                z = -3f * p0 + 3f * p1,           // Linear
                w = p0                            // Constant
            };
        }

        public static float GetPointBezier(float p0, float p1, float p2, float p3, float t)
        {
            Vector4 coeff;
            GetCoeffBezier(out coeff, p0, p1, p2, p3);
            return GetPointCubic(coeff, t);
        }

        public static float GetDerivativeBezier(float p0, float p1, float p2, float p3, float t)
        {
            Vector4 coeff;
            GetCoeffBezier(out coeff, p0, p1, p2, p3);
            return GetDerivativeCubic(coeff, t);
        }

        public static void GetCoeffBspline(out Vector4 dst, float p0, float p1, float p2, float p3)
        {
            dst = new Vector4
            {
                x = ((-1 * p0) + (3 * p1) + (-3 * p2) + (1 * p3)) / 6f,
                y = ((3 * p0) + (-6 * p1) + (3 * p2) + (0 * p3)) / 6f,
                z = ((-3 * p0) + (0 * p1) + (3 * p2) + (0 * p3)) / 6f,
                w = ((1 * p0) + (4 * p1) + (1 * p2) + (0 * p3)) / 6f
            };
        }

        public static float GetPointBspline(float p0, float p1, float p2, float p3, float t)
        {
            Vector4 coeff;
            GetCoeffBspline(out coeff, p0, p1, p2, p3);
            return GetPointCubic(coeff, t);
        }

        public static float GetDerivativeBspline(float p0, float p1, float p2, float p3, float t)
        {
            Vector4 coeff;
            GetCoeffBspline(out coeff, p0, p1, p2, p3);
            return GetDerivativeCubic(coeff, t);
        }

        public static void GetCoeffCatmullRom(out Vector4 dst, float p0, float p1, float s0, float s1, float s = 0.5f)
        {
            dst = new Vector4
            {
                x = (-1 * s * p0) + ((2 - s) * p1) + ((s - 2) * s0) + (s * s1),
                y = (2 * s * p0) + ((s - 3) * p1) + ((3 - 2 * s) * s0) + (-s * s1),
                z = (-s * p0) + (s * s0),
                w = p1
            };
        }

        public static float GetPointCatmullRom(float p0, float p1, float s0, float s1, float t, float s = 0.5f)
        {
            Vector4 coeff;
            GetCoeffCatmullRom(out coeff, p0, p1, s0, s1, s);
            return GetPointCubic(coeff, t);
        }

    }
}
