using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    [System.Serializable]
    public class ViewerRenderInput
    {
        public Camera Camera;
        public float Time;
        public float DeltaTime;
        public int BackbufferWidth;
        public int BackbufferHeight;
        public GfxTexture OnscreenTexture;
        public Vector3 LinearVelocity;

        public bool ContainsSphereInFrustum(Vector3 center, float radius)
        {
            return ViewUtils.ContainsSphereInFrustum(Camera, center, radius);
        }
    }

}
