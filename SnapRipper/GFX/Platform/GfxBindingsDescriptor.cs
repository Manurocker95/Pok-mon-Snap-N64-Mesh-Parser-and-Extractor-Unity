using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class GfxBindingsDescriptor
    {
        public GfxBindingLayoutDescriptor BindingLayout;
        public List<GfxBufferBinding> UniformBufferBindings;
        public List<GfxSamplerBinding> SamplerBindings;
    }
}
