using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.BanjoKazooie
{
    public class StagingVertex : BKVertex
    {
        public long OutputIndex = -1;

        public void SetFromView(VP_DataView<VP_ArrayBuffer> view, long byteOffset)
        {
            OutputIndex = -1;
            RSP.LoadVertexFromView(this, view, byteOffset);
        }
    }
}