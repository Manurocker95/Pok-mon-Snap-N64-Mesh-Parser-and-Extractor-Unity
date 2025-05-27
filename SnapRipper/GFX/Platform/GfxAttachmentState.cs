using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class GfxAttachmentState
    {
        public GfxChannelWriteMask ChannelWriteMask;
        public GfxChannelBlendState RgbBlendState = null;
        public GfxChannelBlendState AlphaBlendState = null;
    }
}
