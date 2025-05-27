using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class GfxRenderAttachmentView
    {
        public long Level;
        public long Z;

        public GfxRenderAttachmentView() { }
        public GfxRenderAttachmentView(long level, long z) { Level = level; Z = z; }
    }
}
