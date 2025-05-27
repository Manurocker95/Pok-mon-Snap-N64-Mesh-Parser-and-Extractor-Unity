using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class GfxrDebugThumbnailDesc
    {
        public int RenderTargetID { get; }
        public IGfxrPass Pass { get; }
        public GfxrAttachmentSlot AttachmentSlot { get; }
        public string DebugLabel { get; }

        public GfxrDebugThumbnailDesc(int renderTargetID, IGfxrPass pass, GfxrAttachmentSlot attachmentSlot, string debugLabel)
        {
            RenderTargetID = renderTargetID;
            Pass = pass;
            AttachmentSlot = attachmentSlot;
            DebugLabel = debugLabel;
        }
    }

}
