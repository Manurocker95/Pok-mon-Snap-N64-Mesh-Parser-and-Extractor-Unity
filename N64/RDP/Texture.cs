using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.RDP
{
    [System.Serializable]
    public class Texture
    {
        public string name;
        public string format = "rgba8";
        public TileState tile = new TileState();

        public long dramAddr;
        public long dramPalAddr;
        public long width;
        public long height;
        public byte[] pixels;

        public Texture(TileState tile, long dramAddr, long dramPalAddr, long width, long height, byte[] pixels)
        {
            this.tile.Copy(tile);
            this.dramAddr = dramAddr;
            this.dramPalAddr = dramPalAddr;
            this.width = width;
            this.height = height;
            this.pixels = pixels;

            long nameAddr = tile.cacheKey != 0 ? (long)tile.cacheKey : dramAddr;
            this.name = nameAddr.ToString("X8");
        }
    }

}
