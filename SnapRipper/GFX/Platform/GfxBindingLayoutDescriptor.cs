using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class GfxBindingLayoutDescriptor
    {
        public long NumUniformBuffers;
        public long NumSamplers;
        public List<GfxBindingLayoutSamplerDescriptor> SamplerEntries;
    }

}
