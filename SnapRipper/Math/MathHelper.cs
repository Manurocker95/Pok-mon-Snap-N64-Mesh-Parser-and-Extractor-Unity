using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public static class MathHelper 
    {
        public static void TransformVec3Mat4W0(ref Vector3 dst, Matrix4x4 m, Vector3 v)
        {
            float x = v.x, y = v.y, z = v.z;

            dst.x = m.m00 * x + m.m01 * y + m.m02 * z;
            dst.y = m.m10 * x + m.m11 * y + m.m12 * z;
            dst.z = m.m20 * x + m.m21 * y + m.m22 * z;
        }

        public static void CalcBillboardMatrix(ref Matrix4x4 dst, ref Matrix4x4 m, CalcBillboardFlags flags, Vector3? axisY = null)
        {
            float mx = new Vector3(m.m00, m.m01, m.m02).magnitude;
            float my = new Vector3(m.m10, m.m11, m.m12).magnitude;
            float mz = new Vector3(m.m20, m.m21, m.m22).magnitude;

            // General calculation:
            //
            //   GlobalX = { 1, 0, 0 }, GlobalY = { 0, 1, 0 }, GlobalZ = { 0, 0, 1 }
            //   MatrixX = { m[0], m[1], m[2] }
            //   MatrixY = axisY || { m[4], m[5], m[6] }
            //   MatrixZ = { m[8], m[9], m[10] }
            //
            // Pick InputZ:
            //   UseZPlane: GlobalZ
            //   UseZSphere: { -m[12], -m[13], -m[14] }
            //
            // Pick InputYRoll:
            //   UseRollLocal: MatrixY
            //   UseRollGlobal: GlobalY
            //
            // Calculate:
            //   Z = InputZ
            //   X = InputYRoll ^ Z
            // PriorityZ:
            //   Y = Z ^ X
            // PriorityY:
            //   Y = MatrixY
            //   Z = X ^ Y

            Vector3 x = Vector3.zero, y = Vector3.zero, z = Vector3.zero;

            if (flags == (CalcBillboardFlags.UseRollGlobal | CalcBillboardFlags.PriorityZ | CalcBillboardFlags.UseZPlane))
            {
                // InputZ = { 0, 0, 1 }, InputRollY = { 0, 1, 0 }
                // Z = InputZ         = { 0, 0, 1 }
                // X = InputRollY ^ Z = { 0, 1, 0 } ^ { 0, 0, 1 } = { 1, 0, 0 }
                // Y = Z ^ X          = { 0, 0, 1 } ^ { 1, 0, 0 } = { 0, 1, 0 }

                x = Vector3.right * mx;
                y = Vector3.up * my;
                z = Vector3.forward * mz;
            }
            else if (flags == (CalcBillboardFlags.UseRollGlobal | CalcBillboardFlags.PriorityY | CalcBillboardFlags.UseZPlane))
            {
                // InputZ = { 0, 0, 1 }, InputRollY = { 0, 1, 0 }
                // Z = InputZ         = { 0, 0, 1 }
                // X = InputRollY ^ Z = { 0, 1, 0 } ^ { 0, 0, 1 } = { 1, 0, 0 }
                // Z = X ^ Y          = { 0, -Y[2], Y[1] }

                Vector3 tmpZ = new Vector3(0, -m.m12, m.m11).normalized;
                x = Vector3.right * mx;
                y = new Vector3(m.m10, m.m11, m.m12);
                z = new Vector3(0, tmpZ.y * mz, tmpZ.z * mz);
            }
            else if (flags == (CalcBillboardFlags.UseRollLocal | CalcBillboardFlags.PriorityZ | CalcBillboardFlags.UseZPlane))
            {
                // InputZ = { 0, 0, 1 }, InputRollY = { m[4], m[5], m[6] }
                // Z = InputZ         = { 0, 0, 1 }
                // X = InputRollY ^ Z = { Y[1], -Y[0], 0 }
                // Y = Z ^ X          = { Y[0],  Y[1], 0 }

                Vector3 inputRollY = new Vector3(m.m10, m.m11, 0).normalized;
                x = new Vector3(inputRollY.y * mx, -inputRollY.x * mx, 0);
                y = new Vector3(inputRollY.x * my, inputRollY.y * my, 0);
                z = Vector3.forward * mz;
            }
            else
            {
                // Generic code.

                // Pick InputZ:
                //   UseZPlane: GlobalZ
                //   UseZSphere: { -m[12], -m[13], -m[14] }

                // General case
                z = ((flags & CalcBillboardFlags.UseZSphere) != 0)
                    ? new Vector3(-m.m03, -m.m13, -m.m23).normalized
                    : Vector3.forward;

                // Pick InputYRoll:
                //   UseRollLocal: MatrixY
                //   UseRollGlobal: GlobalY

                Vector3 inputYRoll;
                if ((flags & CalcBillboardFlags.UseRollGlobal) != 0)
                    inputYRoll = Vector3.up;
                else
                    inputYRoll = axisY ?? new Vector3(m.m10, m.m11, m.m12).normalized;

                x = Vector3.Cross(inputYRoll, z).normalized;

                if ((flags & CalcBillboardFlags.PriorityY) != 0)
                {
                    y = axisY ?? new Vector3(m.m10, m.m11, m.m12).normalized;
                    z = Vector3.Cross(x, y).normalized;
                }
                else
                {
                    y = Vector3.Cross(z, x).normalized;
                }

                x *= mx;
                y *= my;
                z *= mz;
            }

            dst = new Matrix4x4();
            dst.SetColumn(0, new Vector4(x.x, x.y, x.z, 9999f));
            dst.SetColumn(1, new Vector4(y.x, y.y, y.z, 9999f));
            dst.SetColumn(2, new Vector4(z.x, z.y, z.z, 9999f));
            dst.SetColumn(3, new Vector4(m.m03, m.m13, m.m23, 9999f));
        }
        public static float RandomRange(float a, float b = float.NaN)
        {
            if (float.IsNaN(b))
                b = -a;
            return Mathf.Lerp(a, b, UnityEngine.Random.value);
        }
        public static float RayTriangleIntersect(Vector3? barycentricOut, Vector3 p, Vector3 dir, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 ab = b - a;
            Vector3 ac = c - a;
            Vector3 n = Vector3.Cross(ab, ac);

            float d = -Vector3.Dot(dir, n);
            if (d <= 0.0f)
                return 0.0f;

            Vector3 ap = p - a;
            float t = Vector3.Dot(ap, n);
            if (t < 0.0f)
                return 0.0f;

            Vector3 e = Vector3.Cross(ap, dir);
            float v = Vector3.Dot(ac, e);
            if (v < 0.0f || v > d)
                return 0.0f;

            float w = -Vector3.Dot(ab, e);
            if (w < 0.0f || v + w > d)
                return 0.0f;

            float denom = 1.0f / d;
            t *= denom;

            if (barycentricOut.HasValue)
            {
                v *= denom;
                w *= denom;
                float u = 1.0f - (v + w);
                barycentricOut = new Vector3(u, v, w);
            }

            return t;
        }
        public static void ReflectVec3(ref Vector3 dst, Vector3 source, Vector3 normal)
        {
            dst = source - 2.0f * Vector3.Dot(source, normal) * normal;
        }
        /// <summary>
        /// Reflects a given vector <paramref name="source"/> around <paramref name="normal"/>.
        /// </summary>
        public static Vector3 ReflectVec3(Vector3 source, Vector3 normal)
        {
            return source - 2.0f * Vector3.Dot(source, normal) * normal;
        }

        /// <summary>
        /// Sets all three components of <paramref name="dst"/> to <paramref name="v"/>.
        /// </summary>
        public static void Vec3SetAll(ref Vector3 dst, float v)
        {
            dst.x = v;
            dst.y = v;
            dst.z = v;
        }

        /// <summary>
        /// Computes a vector from two basis vectors and their scales.
        /// </summary>
        public static Vector3 Vec3FromBasis2(Vector3 pt, Vector3 b0, float s0, Vector3 b1, float s1)
        {
            return pt + b0 * s0 + b1 * s1;
        }

        /// <summary>
        /// Computes a vector from three basis vectors and their scales.
        /// </summary>
        public static Vector3 Vec3FromBasis3(Vector3 pt, Vector3 b0, float s0, Vector3 b1, float s1, Vector3 b2, float s2)
        {
            return pt + b0 * s0 + b1 * s1 + b2 * s2;
        }
        public static float BitsAsFloat32_v2(long x)
        {
            var baseBuffer = new VP_ArrayBuffer(4);
            var asUint32 = new VP_Uint32Array(baseBuffer);
            var asFloat32 = new VP_Float32Array(baseBuffer);

            asUint32[0] = (x >> 0) & 0xFFFFFFFF;

            return (float)asFloat32[0];
        }
        public static uint Float32AsBits(float value)
        {
            return BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);
        }
        public static float BitsAsFloat32(long x)
        {
            uint uintValue = (uint)(x & 0xFFFFFFFF);
            byte[] bytes = BitConverter.GetBytes(uintValue);
            return BitConverter.ToSingle(bytes, 0);
        }

        public static Matrix4x4 MatrixFromSRT(Vector3 scale, Vector3 eulerRadians, Vector3 translation)
        {
            double sx = scale.x, sy = scale.y, sz = scale.z;
            double rx = eulerRadians.x, ry = eulerRadians.y, rz = eulerRadians.z;

            double sinX = Math.Sin(rx), cosX = Math.Cos(rx);
            double sinY = Math.Sin(ry), cosY = Math.Cos(ry);
            double sinZ = Math.Sin(rz), cosZ = Math.Cos(rz);

            Matrix4x4 m = new Matrix4x4();

            m.m00 = (float)(sx * (cosY * cosZ));
            m.m01 = (float)(sx * (sinZ * cosY));
            m.m02 = (float)(sx * (-sinY));
            m.m03 = 0.0f;

            m.m10 = (float)(sy * (sinX * cosZ * sinY - cosX * sinZ));
            m.m11 = (float)(sy * (sinX * sinZ * sinY + cosX * cosZ));
            m.m12 = (float)(sy * (sinX * cosY));
            m.m13 = 0.0f;

            m.m20 = (float)(sz * (cosX * cosZ * sinY + sinX * sinZ));
            m.m21 = (float)(sz * (cosX * sinZ * sinY - sinX * cosZ));
            m.m22 = (float)(sz * (cosY * cosX));
            m.m23 = 0.0f;

            m.m30 = translation.x;
            m.m31 = translation.y;
            m.m32 = translation.z;
            m.m33 = 1.0f;

            return m;
        }

        public static void ComputeModelMatrixSRT(ref Matrix4x4 dst, double scaleX, double scaleY, double scaleZ, double rotationX, double rotationY, double rotationZ, double translationX, double translationY, double translationZ)
        {
            double sinX = Math.Sin(rotationX), cosX = Math.Cos(rotationX);
            double sinY = Math.Sin(rotationY), cosY = Math.Cos(rotationY);
            double sinZ = Math.Sin(rotationZ), cosZ = Math.Cos(rotationZ);

            dst.m00 = (float)(scaleX * (cosY * cosZ));
            dst.m01 = (float)(scaleX * (sinZ * cosY));
            dst.m02 = (float)(scaleX * (-sinY));
            dst.m03 = 0.0f;

            dst.m10 = (float)(scaleY * (sinX * cosZ * sinY - cosX * sinZ));
            dst.m11 = (float)(scaleY * (sinX * sinZ * sinY + cosX * cosZ));
            dst.m12 = (float)(scaleY * (sinX * cosY));
            dst.m13 = 0.0f;

            dst.m20 = (float)(scaleZ * (cosX * cosZ * sinY + sinX * sinZ));
            dst.m21 = (float)(scaleZ * (cosX * sinZ * sinY - sinX * cosZ));
            dst.m22 = (float)(scaleZ * (cosY * cosX));
            dst.m23 = 0.0f;

            dst.m30 = (float)translationX;
            dst.m31 = (float)translationY;
            dst.m32 = (float)translationZ;
            dst.m33 = 1.0f;
        }

        public static void ComputeModelMatrixS(ref Matrix4x4 dst, double scaleX, double? scaleY = null, double? scaleZ = null)
        {
            double y = scaleY ?? scaleX;
            double z = scaleZ ?? scaleX;

            dst.m00 = (float)scaleX;
            dst.m01 = 0.0f;
            dst.m02 = 0.0f;
            dst.m03 = 0.0f;

            dst.m10 = 0.0f;
            dst.m11 = (float)y;
            dst.m12 = 0.0f;
            dst.m13 = 0.0f;

            dst.m20 = 0.0f;
            dst.m21 = 0.0f;
            dst.m22 = (float)z;
            dst.m23 = 0.0f;

            dst.m30 = 0.0f;
            dst.m31 = 0.0f;
            dst.m32 = 0.0f;
            dst.m33 = 1.0f;
        }

        public static void ComputeModelMatrixR(ref Matrix4x4 dst, double rotationX, double rotationY, double rotationZ)
        {
            double sinX = Math.Sin(rotationX), cosX = Math.Cos(rotationX);
            double sinY = Math.Sin(rotationY), cosY = Math.Cos(rotationY);
            double sinZ = Math.Sin(rotationZ), cosZ = Math.Cos(rotationZ);

            dst.m00 = (float)(cosY * cosZ);
            dst.m01 = (float)(cosY * sinZ);
            dst.m02 = (float)(-sinY);
            dst.m03 = 0.0f;

            dst.m10 = (float)(sinX * cosZ * sinY - cosX * sinZ);
            dst.m11 = (float)(sinX * sinZ * sinY + cosX * cosZ);
            dst.m12 = (float)(sinX * cosY);
            dst.m13 = 0.0f;

            dst.m20 = (float)(cosX * cosZ * sinY + sinX * sinZ);
            dst.m21 = (float)(cosX * sinZ * sinY - sinX * cosZ);
            dst.m22 = (float)(cosY * cosX);
            dst.m23 = 0.0f;

            dst.m30 = 0.0f;
            dst.m31 = 0.0f;
            dst.m32 = 0.0f;
            dst.m33 = 1.0f;
        }

        public static void ComputeModelMatrixT(ref Matrix4x4 dst, double translationX, double translationY, double translationZ)
        {
            dst.m00 = 1.0f; dst.m01 = 0.0f; dst.m02 = 0.0f; dst.m03 = 0.0f;
            dst.m10 = 0.0f; dst.m11 = 1.0f; dst.m12 = 0.0f; dst.m13 = 0.0f;
            dst.m20 = 0.0f; dst.m21 = 0.0f; dst.m22 = 1.0f; dst.m23 = 0.0f;
            dst.m30 = (float)translationX;
            dst.m31 = (float)translationY;
            dst.m32 = (float)translationZ;
            dst.m33 = 1.0f;
        }

        public static void ScaleMatrix(ref Matrix4x4 dst, in Matrix4x4 m, double scaleX, double? scaleY = null, double? scaleZ = null)
        {
            double y = scaleY ?? scaleX;
            double z = scaleZ ?? scaleX;

            dst.m00 = (float)(m.m00 * scaleX); dst.m01 = (float)(m.m01 * scaleX); dst.m02 = (float)(m.m02 * scaleX); dst.m03 = (float)(m.m03 * scaleX);
            dst.m10 = (float)(m.m10 * y); dst.m11 = (float)(m.m11 * y); dst.m12 = (float)(m.m12 * y); dst.m13 = (float)(m.m13 * y);
            dst.m20 = (float)(m.m20 * z); dst.m21 = (float)(m.m21 * z); dst.m22 = (float)(m.m22 * z); dst.m23 = (float)(m.m23 * z);
            dst.m30 = m.m30;
            dst.m31 = m.m31;
            dst.m32 = m.m32;
            dst.m33 = m.m33;
        }

        public static void ComputeNormalMatrix(ref Matrix4x4 dst, in Matrix4x4 m, bool? isUniformScale = null)
        {
            bool uniform = isUniformScale ?? MatrixHasUniformScale(m);

            if (!ReferenceEquals(dst, m))
                dst = m;

            dst.m30 = 0f;
            dst.m31 = 0f;
            dst.m32 = 0f;

            if (!uniform)
            {
                dst = dst.inverse.transpose;
            }
        }

        public static void TransformVec3Mat4w1(ref Vector3 dst, in Matrix4x4 m, in Vector3 v)
        {
            dst.x = m.m00 * v.x + m.m10 * v.y + m.m20 * v.z + m.m30;
            dst.y = m.m01 * v.x + m.m11 * v.y + m.m21 * v.z + m.m31;
            dst.z = m.m02 * v.x + m.m12 * v.y + m.m22 * v.z + m.m32;
        }
        public static Matrix4x4 TargetTo(Vector3 eye, Vector3 target, Vector3 up)
        {
            Vector3 zAxis = (eye - target).normalized; // back
            Vector3 xAxis = Vector3.Cross(up, zAxis).normalized; // right
            Vector3 yAxis = Vector3.Cross(zAxis, xAxis); // up

            Matrix4x4 m = new Matrix4x4();
            m[0, 0] = xAxis.x;
            m[1, 0] = xAxis.y;
            m[2, 0] = xAxis.z;
            m[3, 0] = 0;

            m[0, 1] = yAxis.x;
            m[1, 1] = yAxis.y;
            m[2, 1] = yAxis.z;
            m[3, 1] = 0;

            m[0, 2] = zAxis.x;
            m[1, 2] = zAxis.y;
            m[2, 2] = zAxis.z;
            m[3, 2] = 0;

            m[0, 3] = eye.x;
            m[1, 3] = eye.y;
            m[2, 3] = eye.z;
            m[3, 3] = 1;

            return m;
        }
        public static void TargetTo(ref Matrix4x4 m, Vector3 eye, Vector3 target, Vector3 up)
        {
            Vector3 zAxis = (eye - target).normalized; // back
            Vector3 xAxis = Vector3.Cross(up, zAxis).normalized; // right
            Vector3 yAxis = Vector3.Cross(zAxis, xAxis); // up

            m = new Matrix4x4();
            m[0, 0] = xAxis.x;
            m[1, 0] = xAxis.y;
            m[2, 0] = xAxis.z;
            m[3, 0] = 0;

            m[0, 1] = yAxis.x;
            m[1, 1] = yAxis.y;
            m[2, 1] = yAxis.z;
            m[3, 1] = 0;

            m[0, 2] = zAxis.x;
            m[1, 2] = zAxis.y;
            m[2, 2] = zAxis.z;
            m[3, 2] = 0;

            m[0, 3] = eye.x;
            m[1, 3] = eye.y;
            m[2, 3] = eye.z;
            m[3, 3] = 1;

        }

        public static void TransformVec3Mat4w0(ref Vector3? dst, in Matrix4x4 m, in Vector3? v)
        {
            if (!dst.HasValue || !v.HasValue) 
                return;

            Vector3 dst2 = Vector3.zero;

            dst2.x = m.m00 * v.Value.x + m.m10 * v.Value.y + m.m20 * v.Value.z;
            dst2.y = m.m01 * v.Value.x + m.m11 * v.Value.y + m.m21 * v.Value.z;
            dst2.z = m.m02 * v.Value.x + m.m12 * v.Value.y + m.m22 * v.Value.z;

            dst = dst2;
        }

        public static void TransformVec3Mat4w0(ref Vector3 dst, in Matrix4x4 m, in Vector3 v)
        {
            dst.x = m.m00 * v.x + m.m10 * v.y + m.m20 * v.z;
            dst.y = m.m01 * v.x + m.m11 * v.y + m.m21 * v.z;
            dst.z = m.m02 * v.x + m.m12 * v.y + m.m22 * v.z;
        }

        public static bool CompareEpsilon(double a, double b)
        {
            double max = Math.Max(1.0, Math.Max(Math.Abs(a), Math.Abs(b)));
            return Math.Abs(a - b) <= MathConstants.Epsilon * max;
        }

        public static bool MatrixHasUniformScale(Matrix4x4 m)
        {
            double sx = m.m00 * m.m00 + m.m10 * m.m10 + m.m20 * m.m20;
            double sy = m.m01 * m.m01 + m.m11 * m.m11 + m.m21 * m.m21;
            double sz = m.m02 * m.m02 + m.m12 * m.m02 + m.m22 * m.m22;
            return CompareEpsilon(sx, sy) && CompareEpsilon(sx, sz);
        }

        public static void TexEnvMtx(ref Matrix4x4 dst, double scaleS, double scaleT, double transS, double transT)
        {
            dst.m00 = (float)scaleS; dst.m10 = 0f; dst.m20 = 0f; dst.m30 = (float)transS;
            dst.m01 = 0f; dst.m11 = (float)-scaleT; dst.m21 = 0f; dst.m31 = (float)transT;
            dst.m02 = 0f; dst.m12 = 0f; dst.m22 = 0f; dst.m32 = 1f;
            dst.m03 = 9999f; dst.m13 = 9999f; dst.m23 = 9999f; dst.m33 = 9999f;
        }

        public static void ComputeMatrixWithoutScale(ref Matrix4x4 dst, in Matrix4x4 m)
        {
            double mx = 1.0 / Math.Sqrt(m.m00 * m.m00 + m.m10 * m.m10 + m.m20 * m.m20);
            double my = 1.0 / Math.Sqrt(m.m01 * m.m01 + m.m11 * m.m11 + m.m21 * m.m21);
            double mz = 1.0 / Math.Sqrt(m.m02 * m.m02 + m.m12 * m.m12 + m.m22 * m.m22);

            dst.m00 = (float)(m.m00 * mx); dst.m10 = (float)(m.m10 * mx); dst.m20 = (float)(m.m20 * mx);
            dst.m01 = (float)(m.m01 * my); dst.m11 = (float)(m.m11 * my); dst.m21 = (float)(m.m21 * my);
            dst.m02 = (float)(m.m02 * mz); dst.m12 = (float)(m.m12 * mz); dst.m22 = (float)(m.m22 * mz);

            dst.m30 = m.m30;
            dst.m31 = m.m31;
            dst.m32 = m.m32;

            // Preserve homogeneous coordinate row if needed
            dst.m03 = m.m03;
            dst.m13 = m.m13;
            dst.m23 = m.m23;
            dst.m33 = m.m33;
        }

        public static void ComputeMatrixWithoutTranslation(ref Matrix4x4 dst, in Matrix4x4 m)
        {
            dst = m;
            dst.m03 = 0f;
            dst.m13 = 0f;
            dst.m23 = 0f;
        }

        public static double Clamp(double v, double min, double max)
        {
            return Math.Max(min, Math.Min(v, max));
        }

        public static double Saturate(double v)
        {
            return Clamp(v, 0.0, 1.0);
        }

        public static double ClampRange(double v, double lim)
        {
            return Clamp(v, -lim, lim);
        }

        public static double Lerp(double a, double b, double t)
        {
            return a + (b - a) * t;
        }

        public static double InvLerp(double a, double b, double v)
        {
            return (v - a) / (b - a);
        }

        public static double SmoothStep(double t)
        {
            return t * t * (3 - 2 * t);
        }

        public static double LerpAngle(double v0, double v1, double t, double maxAngle = Math.PI * 2)
        {
            double da = (v1 - v0) % maxAngle;
            double dist = (2 * da) % maxAngle - da;
            return v0 + dist * t;
        }

        public static double AngleDist(double v0, double v1, double maxAngle = Math.PI * 2)
        {
            double da = (v1 - v0) % maxAngle;
            return (2 * da) % maxAngle - da;
        }

        public static void ProjectionMatrixForFrustum(ref Matrix4x4 m, double left, double right, double bottom, double top, double near, double far)
        {
            double rl = 1.0 / (right - left);
            double tb = 1.0 / (top - bottom);

            m.m00 = (float)(near * 2 * rl);
            m.m01 = 0;
            m.m02 = 0;
            m.m03 = 0;

            m.m10 = 0;
            m.m11 = (float)(near * 2 * tb);
            m.m12 = 0;
            m.m13 = 0;

            m.m20 = (float)((right + left) * rl);
            m.m21 = (float)((top + bottom) * tb);
            m.m23 = -1;

            m.m30 = 0;
            m.m31 = 0;
            m.m33 = 0;

            if (!double.IsInfinity(far))
            {
                double nf = 1.0 / (near - far);
                m.m22 = (float)((far + near) * nf);
                m.m32 = (float)(2 * far * near * nf);
            }
            else
            {
                m.m22 = -1;
                m.m32 = (float)(-2 * near);
            }
        }

        public static void ProjectionMatrixForCuboid(ref Matrix4x4 m, double left, double right, double bottom, double top, double near, double far)
        {
            double rl = 1.0 / (right - left);
            double tb = 1.0 / (top - bottom);
            double nf = 1.0 / (near - far);

            m.m00 = (float)(2 * rl);
            m.m01 = 0;
            m.m02 = 0;
            m.m03 = 0;

            m.m10 = 0;
            m.m11 = (float)(2 * tb);
            m.m12 = 0;
            m.m13 = 0;

            m.m20 = 0;
            m.m21 = 0;
            m.m22 = (float)(2 * nf);
            m.m23 = 0;

            m.m30 = (float)(-(right + left) * rl);
            m.m31 = (float)(-(top + bottom) * tb);
            m.m32 = (float)((far + near) * nf);
            m.m33 = 1;
        }

        public static void CalcEulerAngleRotationFromSRTMatrix(ref Vector3 dst, Matrix4x4 m)
        {
            if (CompareEpsilon(m.m02, 1.0f))
            {
                dst.x = Mathf.Atan2(-m.m10, -m.m20);
                dst.y = -Mathf.PI / 2;
                dst.z = 0.0f;
            }
            else if (CompareEpsilon(m.m02, -1.0f))
            {
                dst.x = Mathf.Atan2(m.m10, m.m20);
                dst.y = Mathf.PI / 2;
                dst.z = 0.0f;
            }
            else
            {
                dst.x = Mathf.Atan2(m.m12, m.m22);
                dst.y = -Mathf.Asin(m.m02);
                dst.z = Mathf.Atan2(m.m01, m.m00);
            }
        }

        public static bool CompareEpsilon(float a, float b, float epsilon = 1e-6f)
        {
            return Mathf.Abs(a - b) <= epsilon * Mathf.Max(1f, Mathf.Abs(a), Mathf.Abs(b));
        }

        public static void CalcUnitSphericalCoordinates(ref Vector3 dst, float azimuthal, float polar)
        {
            float sinP = Mathf.Sin(polar);
            dst.x = sinP * Mathf.Cos(azimuthal);
            dst.y = Mathf.Cos(polar);
            dst.z = sinP * Mathf.Sin(azimuthal);
        }

        public static List<int> Range(int start, int count)
        {
            List<int> result = new List<int>();
            for (int i = start; i < start + count; i++)
                result.Add(i);
            return result;
        }

        public static void NormToLength(ref Vector3 v, float length)
        {
            float currentLength = v.magnitude;
            if (currentLength > 0f)
            {
                float scale = length / currentLength;
                v.x *= scale;
                v.y *= scale;
                v.z *= scale;
            }
        }


        public static void NormToLength(ref Vector3? v, float length)
        {
            if (!v.HasValue)
                return;

            Vector3 v2 = Vector3.zero;

            float currentLength = v.Value.magnitude;

            if (currentLength > 0f)
            {
                float scale = length / currentLength;
                v2.x *= scale;
                v2.y *= scale;
                v2.z *= scale;
                v = v2;
            }
        }

        public static void NormToLength(ref Vector3 v, double length)
        {
            float currentLength = v.magnitude;
            if (currentLength > 0f)
            {
                double scale = length / currentLength;
                v.x *= (float)scale;
                v.y *= (float)scale;
                v.z *= (float)scale;
            }
        }
        public static void NormToLengthAndAdd(ref Vector3 dst, Vector3 a, float len)
        {
            float aLen = a.magnitude;
            if (aLen > 0f)
            {
                float inv = len / aLen;
                dst.x += a.x * inv;
                dst.y += a.y * inv;
                dst.z += a.z * inv;
            }
        }

        public static void NormToLengthAndAdd(ref Vector3? dst, Vector3 a, float len)
        {
            if (!dst.HasValue)
                return;

            Vector3 dst2 = Vector3.zero;
            float aLen = a.magnitude;
            if (aLen > 0f)
            {
                float inv = len / aLen;
                dst2.x += a.x * inv;
                dst2.y += a.y * inv;
                dst2.z += a.z * inv;

                dst = dst2;
            }
        }

        public static bool IsNearZero(float v, float min)
        {
            return v > -min && v < min;
        }

        public static bool IsNearZeroVec3(Vector3 v, float min)
        {
            return v.x > -min && v.x < min &&
                   v.y > -min && v.y < min &&
                   v.z > -min && v.z < min;
        }

        public static void QuatFromEulerRadians(ref Quaternion dst, float x, float y, float z)
        {
            float sx = Mathf.Sin(0.5f * x), cx = Mathf.Cos(0.5f * x);
            float sy = Mathf.Sin(0.5f * y), cy = Mathf.Cos(0.5f * y);
            float sz = Mathf.Sin(0.5f * z), cz = Mathf.Cos(0.5f * z);

            dst.x = sx * cy * cz - cx * sy * sz;
            dst.y = cx * sy * cz + sx * cy * sz;
            dst.z = cx * cy * sz - sx * sy * cz;
            dst.w = cx * cy * cz + sx * sy * sz;
        }

        public static void GetMatrixAxisX(ref Vector3 dst, Matrix4x4 m)
        {
            dst = new Vector3(m.m00, m.m01, m.m02);
        }

        public static void GetMatrixAxisY(ref Vector3 dst, Matrix4x4 m)
        {
            dst = new Vector3(m.m10, m.m11, m.m12);
        }

        public static void GetMatrixAxisZ(ref Vector3 dst, Matrix4x4 m)
        {
            dst = new Vector3(m.m20, m.m21, m.m22);
        }

        public static void GetMatrixAxis(ref Vector3? dstX, ref Vector3? dstY, ref Vector3? dstZ, Matrix4x4 m)
        {
            if (dstX.HasValue)
                dstX = new Vector3(m.m00, m.m01, m.m02);
            if (dstY.HasValue)
                dstY = new Vector3(m.m10, m.m11, m.m12);
            if (dstZ.HasValue)
                dstZ = new Vector3(m.m20, m.m21, m.m22);
        }

        public static void SetMatrixAxis(ref Matrix4x4 m, Vector3? axisX, Vector3? axisY, Vector3? axisZ)
        {
            if (axisX.HasValue)
            {
                m.m00 = axisX.Value.x;
                m.m01 = axisX.Value.y;
                m.m02 = axisX.Value.z;
            }

            if (axisY.HasValue)
            {
                m.m10 = axisY.Value.x;
                m.m11 = axisY.Value.y;
                m.m12 = axisY.Value.z;
            }

            if (axisZ.HasValue)
            {
                m.m20 = axisZ.Value.x;
                m.m21 = axisZ.Value.y;
                m.m22 = axisZ.Value.z;
            }
        }

        public static Vector3 GetMatrixTranslation(Matrix4x4 m)
        {
            return new Vector3(m.m03, m.m13, m.m23);
        }

        public static void SetMatrixTranslation(ref Matrix4x4 m, Vector3 v)
        {
            m.m03 = v.x;
            m.m13 = v.y;
            m.m23 = v.z;
        }

    }
}
