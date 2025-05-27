using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class GfxRenderPipelineDescriptor
    {
        public List<GfxBindingLayoutDescriptor> BindingLayouts = null;
        public GfxInputLayout InputLayout = null;
        public GfxProgram Program = null;
        public GfxPrimitiveTopology Topology;
        public GfxMegaStateDescriptor MegaStateDescriptor = null;

        // Attachment data
        public List<GfxFormat> ColorAttachmentFormats = null;
        public GfxFormat DepthStencilAttachmentFormat = null;
        public long SampleCount;
    }
}
