using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class GfxMegaStateDescriptor
    {
        public List<GfxAttachmentState> AttachmentsState = new List<GfxAttachmentState>();
        public GfxCompareMode DepthCompare;
        public bool DepthWrite;
        public GfxCompareMode StencilCompare;
        public bool StencilWrite;
        public GfxStencilOp StencilPassOp;
        public GfxCullMode CullMode;
        public GfxFrontFaceMode FrontFace;
        public bool PolygonOffset;
        public bool Wireframe;
    }

}
