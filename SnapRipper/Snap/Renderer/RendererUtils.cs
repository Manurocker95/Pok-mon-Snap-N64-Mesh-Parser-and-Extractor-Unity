using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public static class RendererUtils
    {
        public static void BuildTransform(ref Matrix4x4 dst, Vector3 pos, Vector3 euler, Vector3 scale, bool _useModelMatrixSRT = false)
        {
            if (_useModelMatrixSRT)
            {
                MathHelper.ComputeModelMatrixSRT(ref dst, scale[0], scale[1], scale[2], euler[0], euler[1], euler[2], pos[0], pos[1], pos[2]);
                return;
            }

            Quaternion rotation = Quaternion.Euler(euler * Mathf.Rad2Deg); // Unity usa grados para Quaternion.Euler
            dst = Matrix4x4.TRS(pos, rotation, scale);
        }
    }
}
