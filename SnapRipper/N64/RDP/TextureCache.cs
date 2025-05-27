using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.RDP
{
    public class TextureCache
    {
        public Dictionary<long, long> textureMap = new Dictionary<long, long>();
        public List<Texture> textures = new List<Texture>(); 

        public long TranslateTileTexture(VP_ArrayBufferSlice[] segmentBuffers, long dramAddr, long dramPalAddr, TileState tile, bool deinterleave = false)
        {
            for (int i = 0; i < textures.Count; i++)
            {
                var t = textures[i];
                if (t.dramAddr == dramAddr && ((ImageFormat)tile.fmt != ImageFormat.CI || t.dramPalAddr == dramPalAddr) && TextureCacheUtils.TextureMatch(t.tile, tile))
                    return i;
            }

            var texture = TextureCacheUtils.TranslateTileTexture(segmentBuffers, dramAddr, dramPalAddr, tile, deinterleave);
            long index = textures.Count;
            textures.Add(texture);
            return index;
        }
    }
}
