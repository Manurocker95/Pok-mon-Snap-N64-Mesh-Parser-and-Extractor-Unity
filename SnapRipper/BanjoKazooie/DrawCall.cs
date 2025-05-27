using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.BanjoKazooie
{
    public class DrawCall
    {
        public int FirstIndex = 0;
        public int IndexCount = 0;

        public long DP_OtherModeL;
        public long DP_OtherModeH;
        public RDP.CombineParams DP_Combine;

        public long SP_GeometryMode;
        public TextureState SP_TextureState = new TextureState();

        public List<int> TextureIndices = new List<int>();
    }
}