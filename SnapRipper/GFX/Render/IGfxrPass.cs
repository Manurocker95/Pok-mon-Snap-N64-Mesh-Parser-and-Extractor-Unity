using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public interface IGfxrPass : IGfxrPassBase
    {
        /// <summary>
        /// Set the viewport for the given render pass in normalized coordinates (0..1).
        /// Not required; defaults to full viewport.
        /// </summary>
        void SetViewport(float x, float y, float width, float height);

        /// <summary>
        /// Attach the given render target ID to the specified attachment slot.
        /// </summary>
        void AttachRenderTargetID(GfxrAttachmentSlot attachmentSlot, GfxrRenderTargetID renderTargetID, GfxRenderAttachmentView view = null);

        /// <summary>
        /// Attach the given texture to the specified attachment slot with an optional view and clear descriptor.
        /// </summary>
        void AttachTexture(GfxrAttachmentSlot attachmentSlot, GfxTexture texture, GfxRenderAttachmentView view = null, GfxrAttachmentClearDescriptor clearDescriptor = null);

        /// <summary>
        /// Attach an occlusion query pool to this rendering pass.
        /// </summary>
        void AttachOcclusionQueryPool(GfxQueryPool queryPool);

        /// <summary>
        /// Set the execution callback for this pass. This will be called with the GfxRenderPass and access to resources.
        /// </summary>
        void Exec(PassExecFunc func);
    }

}
