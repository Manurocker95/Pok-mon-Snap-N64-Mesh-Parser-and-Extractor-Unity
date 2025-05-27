using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public interface IGfxrPassScope
    {
        /// <summary>
        /// Retrieve the resolve texture resource for a given resolve texture ID.
        /// This is guaranteed to be a single-sampled texture suitable for binding to a shader.
        /// </summary>
        /// <param name="id">The resolve texture ID.</param>
        /// <returns>The resolved GfxTexture.</returns>
        GfxTexture GetResolveTextureForID(GfxrResolveTextureID id);

        /// <summary>
        /// Retrieve the underlying texture resource for a given attachment slot.
        /// This is not guaranteed to be single-sampled; use GetResolveTextureForID to resolve it if needed.
        /// </summary>
        /// <param name="slot">The attachment slot.</param>
        /// <returns>The GfxTexture, or null if none exists.</returns>
        GfxTexture GetRenderTargetTexture(GfxrAttachmentSlot slot);
    }

}
