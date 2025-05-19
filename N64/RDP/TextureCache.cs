using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.RDP
{
    public class TextureCache
    {
        public Dictionary<long, long> textureMap = new Dictionary<long, long>();
        public List<Texture> textures = new List<Texture>(); // Referencia a IDs o índices de texturas

        public long TranslateTileTexture(VP_ArrayBufferSlice[] segmentBuffers, long dramAddr, long dramPalAddr, TileState tile, bool deinterleave = false)
        {
            for (int i = 0; i < textures.Count; i++)
            {
                var t = textures[i];
                if (t.dramAddr == dramAddr && ((ImageFormat)tile.fmt != ImageFormat.CI || t.dramPalAddr == dramPalAddr) && TextureMatch(t.tile, tile))
                    return i;
            }

            var texture = TextureCacheUtils.TranslateTileTexture(segmentBuffers, dramAddr, dramPalAddr, tile, deinterleave);
            long index = textures.Count;
            textures.Add(texture);
            return index;
        }

        private bool TextureMatch(TileState a, TileState b)
        {
            return a.fmt == b.fmt && a.siz == b.siz && a.line == b.line &&
                   a.palette == b.palette && a.cmt == b.cmt && a.cms == b.cms &&
                   a.maskt == b.maskt && a.masks == b.masks &&
                   a.shiftt == b.shiftt && a.shifts == b.shifts &&
                   a.uls == b.uls && a.ult == b.ult &&
                   a.lrs == b.lrs && a.lrt == b.lrt;
        }
    }
}
