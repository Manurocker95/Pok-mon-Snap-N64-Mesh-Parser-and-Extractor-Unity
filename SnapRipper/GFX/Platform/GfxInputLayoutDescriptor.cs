using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{

    public class GfxInputLayoutDescriptor
    {
        public List<GfxInputLayoutBufferDescriptor> VertexBufferDescriptors;
        public List<GfxVertexAttributeDescriptor> VertexAttributeDescriptors;
        public GfxFormat IndexBufferFormat;
    }
}
