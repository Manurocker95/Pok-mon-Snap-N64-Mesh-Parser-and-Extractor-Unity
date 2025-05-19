using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class GfxDeviceLimits
    {
        public long UniformBufferWordAlignment;
        public long UniformBufferMaxPageWordSize;
        public List<long> SupportedSampleCounts = null;
        public bool OcclusionQueriesRecommended;
        public bool ComputeShadersSupported;
        public bool WireframeSupported;
    }
}
