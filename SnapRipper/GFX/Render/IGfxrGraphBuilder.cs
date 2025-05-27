using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public interface IGfxrGraphBuilder
    {
        void PushPass(Action<IGfxrPass> setupFunc);
        void PushComputePass(Action<IGfxrComputePass> setupFunc);

        GfxrRenderTargetID CreateRenderTargetID(GfxrRenderTargetDescription desc, string debugName);

        GfxrResolveTextureID ResolveRenderTargetPassAttachmentSlot(IGfxrPass pass, GfxrAttachmentSlot attachmentSlot);
        GfxrResolveTextureID ResolveRenderTarget(GfxrRenderTargetID renderTargetID);

        void ResolveRenderTargetToExternalTexture(GfxrRenderTargetID renderTargetID, GfxTexture texture, GfxRenderAttachmentView view = null);

        GfxrRenderTargetDescription GetRenderTargetDescription(GfxrRenderTargetID renderTargetID);

        void PushDebugThumbnail(GfxrRenderTargetID renderTargetID, string debugLabel = null);
    }
}
