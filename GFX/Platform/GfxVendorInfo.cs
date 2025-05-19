using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class GfxVendorInfo
    {
        public string PlatformString = null;
        public string GlslVersion = null;
        public bool ExplicitBindingLocations;
        public bool SeparateSamplerTextures;
        public GfxViewportOrigin ViewportOrigin;
        public GfxClipSpaceNearZ ClipSpaceNearZ;
    }

}
