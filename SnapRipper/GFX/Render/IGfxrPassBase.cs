using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public interface IGfxrPassBase
    {
        /// <summary>
        /// Set the debug name of a given pass. Strongly encouraged.
        /// </summary>
        void SetDebugName(string debugName);

        /// <summary>
        /// Attach the resolve texture ID to the given pass. All resolve textures used within the pass
        /// must be attached beforehand in order for the scheduler to properly allocate our resolve texture.
        /// </summary>
        void AttachResolveTexture(GfxrResolveTextureID resolveTextureID);

        /// <summary>
        /// Set the pass's post callback. This will be executed immediately after the pass is submitted,
        /// allowing additional custom work. Expected to be seldomly used.
        /// </summary>
        void Post(PassPostFunc func);

        /// <summary>
        /// Adds an extra reference to a render target to avoid premature destruction.
        /// </summary>
        void AddExtraRef(GfxrAttachmentSlot renderTargetID);
    }
}
