using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class GfxRenderPassAttachment
    {
        public GfxRenderTarget RenderTarget = null;
        public GfxRenderAttachmentView View = null;
        public GfxTexture ResolveTo = null;
        public GfxRenderAttachmentView ResolveView = null;
        public bool Store;
    }

}
