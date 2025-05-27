using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class TextureOverride
    {
        public GfxTexture GfxTexture = null;
        public GfxSampler GfxSampler = null;
        public long Width;
        public long Height;
        public bool FlipY;
        public string LateBinding = null;
    }

}
