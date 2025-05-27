using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class TextureMapping : GfxSamplerBinding
    {
        // These are not used when binding to samplers, and are conveniences for custom behavior.
        public long Width = 0;
        public long Height = 0;
        public long LodBias = 0;

        // GL sucks. This is a convenience when building texture matrices.
        public bool FlipY = false;

        public void Reset()
        {
            this.GfxTexture = null;
            this.GfxSampler = null;
            this.LateBinding = null;
            this.Width = 0;
            this.Height = 0;
            this.LodBias = 0;
            this.FlipY = false;
        }

        public bool FillFromTextureOverride(TextureOverride textureOverride)
        {
            this.GfxTexture = textureOverride.GfxTexture;
            if (textureOverride.GfxSampler != null)
                this.GfxSampler = textureOverride.GfxSampler;
            this.Width = textureOverride.Width;
            this.Height = textureOverride.Height;
            this.FlipY = textureOverride.FlipY;
            if (textureOverride.LateBinding != null)
                this.LateBinding = textureOverride.LateBinding;
            return true;
        }

        public void Copy(TextureMapping other)
        {
            this.GfxTexture = other.GfxTexture;
            this.GfxSampler = other.GfxSampler;
            this.LateBinding = other.LateBinding;
            this.Width = other.Width;
            this.Height = other.Height;
            this.LodBias = other.LodBias;
            this.FlipY = other.FlipY;
        }
    }
}
