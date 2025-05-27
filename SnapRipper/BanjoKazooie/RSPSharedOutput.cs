using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.BanjoKazooie
{
    public class RSPSharedOutput
    {
        public List<RSPVertex> Vertices = new List<RSPVertex>();
        public List<long> Indices = new List<long>();
        public RDP.TextureCache TextureCache = new RDP.TextureCache();

        public virtual RSPVertex LoadVertex(StagingVertex v)
        {
            if (v.OutputIndex < 0)
            {
                var n = new RSPVertex();
                n.Copy(v);
                this.Vertices.Add(n);
                v.OutputIndex = Vertices.Count - 1;
                return n;
            }

            return null;
        }

        public virtual void SetVertexBufferFromData(VP_DataView<VP_ArrayBuffer> vertexData)
        {
            var scratchVertex = new StagingVertex();
            for (long offs = 0; offs < vertexData.ByteLength; offs += 0x10)
            {
                scratchVertex.SetFromView(vertexData, offs);
                LoadVertex(scratchVertex);
            }
        }
    }

}