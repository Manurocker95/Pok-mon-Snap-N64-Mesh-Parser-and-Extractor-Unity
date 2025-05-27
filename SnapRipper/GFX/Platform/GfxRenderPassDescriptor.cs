using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class GfxRenderPassDescriptor
    {
        public List<GfxRenderPassAttachmentColor> ColorAttachments = new List<GfxRenderPassAttachmentColor>();
        public GfxRenderPassAttachmentDepthStencil DepthStencilAttachment = null;

        // Query system
        public GfxQueryPool OcclusionQueryPool = null;
    }

}
