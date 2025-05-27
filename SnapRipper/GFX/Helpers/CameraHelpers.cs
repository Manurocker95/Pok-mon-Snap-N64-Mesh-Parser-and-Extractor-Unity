using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public static class CameraHelpers
    {
        public static void ComputeViewMatrix(ref Matrix4x4 output, Camera camera)
        {
            output = camera.worldToCameraMatrix;
        }

        public static void ComputeViewMatrixSkybox(ref Matrix4x4 output, Camera camera)
        {
            output = camera.worldToCameraMatrix;

            output.m03 = 0f;
            output.m13 = 0f;
            output.m23 = 0f;
        }
    }
}
