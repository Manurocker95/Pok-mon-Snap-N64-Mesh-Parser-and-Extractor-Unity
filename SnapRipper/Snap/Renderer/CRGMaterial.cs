using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class CRGMaterial
    {
        private CRGAnimator Animator = new CRGAnimator(true);
        public double LastTime;

        public MaterialData Data;
        private List<GfxTexture> Textures;

        const int ColorFlagStart = 9;
        const int TileFieldOffset = 5;

        public CRGMaterial(MaterialData data, List<GfxTexture> textures)
        {
            this.Data = data;
            this.Textures = textures;
        }

        public void ForceLoop()
        {
            this.Animator.ForceLoop = true;
        }

        public void Update(double time)
        {
            if (this.Animator.Update(time))
                this.LastTime = time;
        }

        public void SetTrack(AnimationTrack track)
        {
            this.Animator.SetTrack(track);
        }

        public void GetColor(ref Vector4 dst, ColorField field)
        {
            if (((int)this.Data.Flags & (1 << ((int)field + ColorFlagStart))) != 0)
                this.Animator.Colors[(int)field].Compute((float)this.LastTime, ref dst);
        }

        public long GetPrimLOD()
        {
            var lodVal = (long)this.Animator.Compute((int)MaterialField.PrimLOD, this.LastTime);
            if (lodVal < 0)
                return 1;
            return lodVal % 1;
        }

        public double XScale()
        {
            var newScale = this.Animator.Compute((int)MaterialField.XScale, this.LastTime);
            if (newScale == 0)
                return 1;
            return this.Data.xScale / newScale;
        }

        public double YScale()
        {
            var newScale = this.Animator.Compute((int)MaterialField.YScale, this.LastTime);
            if (newScale == 0)
                return 1;
            return this.Data.yScale / newScale;
        }

        public double GetXShift(long index)
        {
            var shifter = this.Animator.Interpolators[(int)MaterialField.T0_XShift + index * TileFieldOffset];
            var scaler = this.Animator.Interpolators[(int)MaterialField.XScale];

            double baseShift = (shifter.op == AObjOP.NOP)
                ? this.Data.Tiles[(int)index].xShift
                : shifter.Compute((float)this.LastTime);

            double scale = (scaler.op == AObjOP.NOP)
                ? this.Data.xScale
                : scaler.Compute((float)this.LastTime);

            return (baseShift * this.Data.Tiles[(int)index].Width + this.Data.Shift) / scale;
        }

        public double GetYShift(long index)
        {
            var shifter = this.Animator.Interpolators[(int)MaterialField.T0_YShift + index * TileFieldOffset];
            var scaler = this.Animator.Interpolators[(int)MaterialField.YScale];

            double baseShift = (shifter.op == AObjOP.NOP)
                ? this.Data.Tiles[(int)index].yShift
                : shifter.Compute((float)this.LastTime);

            double scale = (scaler.op == AObjOP.NOP)
                ? this.Data.yScale
                : scaler.Compute((float)this.LastTime);

            return ((1.0 - baseShift - scale) * this.Data.Tiles[(int)index].Height + this.Data.Shift) / scale;
        }

        public void FillTextureMappings(List<TextureMapping> mappings)
        {
            if (((long)this.Data.Flags & (long)(MaterialFlags.Palette | MaterialFlags.Special | MaterialFlags.Tex1 | MaterialFlags.Tex2)) == 0)
                return;

            float pal = -1f;
            if (((long)this.Data.Flags & (long)MaterialFlags.Palette) != 0)
                pal = this.Animator.Interpolators[(int)MaterialField.PalIndex].Compute((float)this.LastTime);

            for (int i = 0; i < mappings.Count; i++)
            {
                float tex = -1f;
                var texFlag = (i == 0) ? MaterialFlags.Tex1 : MaterialFlags.Tex2;

                if (((long)this.Data.Flags & (long)MaterialFlags.Special) != 0)
                    tex = this.Animator.Compute((long)MaterialField.PrimLOD, this.LastTime) + i;
                else if (((long)this.Data.Flags & (long)texFlag) != 0)
                    tex = this.Animator.Compute(((i == 0) ? (long)MaterialField.TexIndex1 : (long)MaterialField.TexIndex2), this.LastTime);
                else if (pal == -1)
                    continue;

                for (int j = 0; j < this.Data.UsedTextures.Count; j++)
                {
                    if (this.Data.UsedTextures[j].PAL == pal && this.Data.UsedTextures[j].TextureID == tex)
                    {
                        mappings[i].GfxTexture = this.Textures[(int)this.Data.UsedTextures[j].Index];
                        break;
                    }
                }
            }
        }

    }
}
