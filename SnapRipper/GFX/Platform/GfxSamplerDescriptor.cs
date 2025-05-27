using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class GfxSamplerDescriptor
    {
        public GfxWrapMode WrapS;
        public GfxWrapMode WrapT;
        public GfxWrapMode? WrapQ;

        public GfxTexFilterMode MinFilter;
        public GfxTexFilterMode MagFilter;
        public GfxMipFilterMode MipFilter;

        public long MinLOD = 0;
        public long MaxLOD = 0;
        public long MaxAnisotropy = 0;

        public GfxCompareMode CompareMode = 0;
    }

}
