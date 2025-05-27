using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class RSPState
    {
        private RSPOutput output = new RSPOutput();
        
        private bool stateChanged = false;
        private bool minorChange = false;
        private StagingVertex[] vertexCache = Enumerable.Range(0, 64).Select(_ => new StagingVertex()).ToArray();
        private long SP_GeometryMode = 0;
        private TextureState SP_TextureState = new TextureState();

        private long DP_OtherModeL = 0;
        private long DP_OtherModeH = 0;
        private long DP_CombineL = 0;
        private long DP_CombineH = 0;
        private TextureImageState DP_TextureImageState = new TextureImageState();
        private TileState[] DP_TileState = Enumerable.Range(0, 8).Select(_ => new TileState()).ToArray();
        private Dictionary<long, long> DP_TMemTracker = new Dictionary<long, long>();


        private Vector4 DP_PrimColor = new Vector4(0, 0, 0, 0);
        private Vector4 DP_EnvColor = new Vector4(0, 0, 0, 0);
        private float DP_PrimLOD = 0;

        private long SP_MatrixIndex = 0;
        public long DP_Half1 = 0;
        public long materialIndex = -1;

        public RSPSharedOutput sharedOutput;
        public CRGDataMap dataMap;
        private bool preloaded;
        public List<RSPTriangle> Triangles;
        public StagingVertex[] VertexCache => vertexCache;

        public RSPState(RSPSharedOutput sharedOutput, CRGDataMap dataMap, bool preloaded = false)
        {
            Triangles = new List<RSPTriangle>();
            output = new RSPOutput();
            this.sharedOutput = sharedOutput;
            this.dataMap = dataMap;
            this.preloaded = preloaded;
        }

        public RSPOutput Finish()
        {
            return output.DrawCalls.Count == 0 ? null : output;
        }

        public void Clear()
        {
            SP_MatrixIndex = 0;
            output = new RSPOutput();
            stateChanged = true;
            materialIndex = -1;

            foreach (var v in vertexCache)
            {
                v.matrixIndex = 1;
                v.OutputIndex = -1;
            }
        }

        private void SetGeometryMode(long newGeometryMode)
        {
            if (SP_GeometryMode == newGeometryMode)
                return;
            minorChange = true;
            SP_GeometryMode = newGeometryMode;
        }

        public void GSPSetGeometryMode(long mask)
        {
            SetGeometryMode(SP_GeometryMode | mask);
        }

        public void GSPClearGeometryMode(long mask)
        {
            SetGeometryMode(SP_GeometryMode & ~mask);
        }

        public void GSPTexture(bool on, long tile, long level, long s, long t)
        {
            double ss = (double)s / 0x10000;
            double tt = (double)t / 0x10000;

            // This is the texture we're using to rasterize triangles going forward.
            SP_TextureState.Set(on, tile, level, ss, tt);
            stateChanged = true;
        }

        public void GSPVertex(long dramAddr, long n, long v0)
        {
            var range = dataMap.GetRange(dramAddr);
            var view = dataMap.GetView(dramAddr);
            for (long i = 0; i < n; i++)
            {
                vertexCache[v0 + i].SetFromView(view, i * 0x10);
                if (preloaded)
                {
                    long outIndex = (long)((dramAddr - range.Start) >> 4) + i;
                    sharedOutput.Vertices[(int)outIndex].matrixIndex = SP_MatrixIndex;
                    vertexCache[v0 + i].OutputIndex = outIndex;
                }
                else
                {
                    vertexCache[v0 + i].matrixIndex = SP_MatrixIndex;
                    vertexCache[v0 + i].tx *= SP_TextureState.s; // TODO Check this
                    vertexCache[v0 + i].ty *= SP_TextureState.t;

                   // vertexCache[v0 + i].s_tx = vertexCache[v0 + i].tx * SP_TextureState.s;
                   // vertexCache[v0 + i].s_ty = vertexCache[v0 + i].ty * SP_TextureState.t;

                    //vertexCache[v0 + i].s_tx = SP_TextureState.ss;
                    //vertexCache[v0 + i].s_ty = SP_TextureState.tt;
                }
            }
        }

        public void GSPModifyVertex(long w, long n, short upper, short lower)
        {
            var vtx = vertexCache[n];
            if (preloaded)
                Console.WriteLine("modifying preloaded vertex buffer");

            switch (w)
            {
                case 0x14:
                    vtx.tx = (upper / 0x20) + 0.5f;
                    vtx.ty = (lower / 0x20) + 0.5f;
                    break;
                default:
                    Console.WriteLine($"new modifyvtx {w} {upper} {lower}");
                    break;
            }
            vtx.OutputIndex = -1;
        }

        private long TranslateTileTexture(long tileIndex)
        {
            var tile = DP_TileState[tileIndex];
            long dramAddr = DP_TMemTracker[tile.tmem];

            long dramPalAddr;
            if ((ImageFormat)tile.fmt == ImageFormat.CI)
            {
                long textlut = (DP_OtherModeH >> 14) & 0x03;
                // Assert(textlut == TextureLUT.G_TT_RGBA16);

                long palTmem = 0x100 + (tile.palette << 4);
                dramPalAddr = DP_TMemTracker[palTmem];
            }
            else
            {
                dramPalAddr = 0;
            }

            var segments = new List<VP_ArrayBufferSlice>();
            long texAddr = AddSegment(dataMap, segments, dramAddr);
            long palAddr = dramPalAddr == 0 ? 0 : AddSegment(dataMap, segments, dramPalAddr);

            return sharedOutput.TextureCache.TranslateTileTexture(segments.ToArray(), texAddr, palAddr, tile);
        }
        public long AddSegment(CRGDataMap dataMap, List<VP_ArrayBufferSlice> segments, long addr)
        {
            long seg = (addr >> 24) & 0x7F;
            var range = dataMap.GetRange(addr);
            if (seg > segments.Count)
                segments[(int)seg] = range.Data;
            else
                segments.Add(range.Data);

            if (seg == 0)
                return addr - range.Start;

            return addr;
        }

        private void FlushTextures(DrawCall dc)
        {
            if (!SP_TextureState.on)
                return;

            bool lod_en = ((DP_OtherModeH >> 16) & 0x01) != 0;
            if (lod_en)
            {
                throw new NotImplementedException("mip-mapping not supported");
            }
            else
            {
                var cycletype = RDP.RDPUtils.GetCycleTypeFromOtherModeH(DP_OtherModeH);
                //Debug.Assert(cycletype == RDP.RDPUtils.OtherModeH_CycleType.G_CYC_1CYCLE || cycletype == RDP.OtherModeH_CycleType.G_CYC_2CYCLE);

                dc.TextureIndices.Add((int)TranslateTileTexture(SP_TextureState.tile));

                if (SP_TextureState.level == 0 && RDP.RDPUtils.CombineParamsUsesT1(dc.DP_Combine))
                {
                    dc.TextureIndices.Add((int)TranslateTileTexture(SP_TextureState.tile + 1));
                }
            }
        }

        private void FlushDrawCall()
        {
            if (stateChanged || minorChange)
            {
                if (materialIndex == ((DrawCall)output.CurrentDrawCall).materialIndex && stateChanged)
                    materialIndex = -1;
                stateChanged = false;
                minorChange = false;

                var dc = (DrawCall)output.NewDrawCall(sharedOutput.Indices.Count);
                dc.SP_GeometryMode = (long)SP_GeometryMode;
                dc.SP_TextureState.Copy(SP_TextureState);
                dc.DP_Combine = RDP.RDPUtils.DecodeCombineParams(DP_CombineH, DP_CombineL);
                dc.DP_OtherModeH = (long)DP_OtherModeH;
                dc.DP_OtherModeL = (long)DP_OtherModeL;
                dc.DP_PrimColor = DP_PrimColor;
                dc.DP_EnvColor = DP_EnvColor;
                dc.DP_PrimLOD = DP_PrimLOD;
                dc.materialIndex = materialIndex;

                FlushTextures(dc);
            }
        }

        public void GSPTri(long i0, long i1, long i2)
        {
            FlushDrawCall();
            var v1 = sharedOutput.LoadVertex(vertexCache[i0]);
            var v2 = sharedOutput.LoadVertex(vertexCache[i1]);
            var v3 = sharedOutput.LoadVertex(vertexCache[i2]);
            sharedOutput.Indices.Add(vertexCache[i0].OutputIndex);
            sharedOutput.Indices.Add(vertexCache[i1].OutputIndex);
            sharedOutput.Indices.Add(vertexCache[i2].OutputIndex);

            Triangles.Add(new RSPTriangle(new List<RSPVertex>() { v1, v2, v3 }, new List<long>() { vertexCache[i0].OutputIndex, vertexCache[i1].OutputIndex, vertexCache[i2].OutputIndex }));

            output.CurrentDrawCall.IndexCount += 3;
        }

        public void GDPSetTextureImage(long fmt, long siz, long w, long addr)
        {
            DP_TextureImageState.Set(fmt, siz, w, addr);
        }

        public void GDPSetTile(long fmt, long siz, long line, long tmem, long tile, long palette, long cmt, long maskt, long shiftt, long cms, long masks, long shifts)
        {
            DP_TileState[tile].Set(fmt, siz, line, tmem, palette, cmt, maskt, shiftt, cms, masks, shifts);
            stateChanged = true;
        }

        public void GDPLoadTLUT(long tile, long count)
        {
            DP_TMemTracker[DP_TileState[tile].tmem] = (long)DP_TextureImageState.addr;
        }

        public void GDPLoadBlock(long tileIndex, long uls, long ult, long texels, long dxt)
        {
            //UnityEngine.Debug.LogError(uls == 0 && ult == 0);
            var tile = DP_TileState[tileIndex];
            DP_TMemTracker[tile.tmem] = (long)DP_TextureImageState.addr;
            stateChanged = true;
        }

        public void GDPSetTileSize(long tile, long uls, long ult, long lrs, long lrt)
        {
            DP_TileState[tile].SetSize(uls, ult, lrs, lrt);
            stateChanged = true;
        }

        public void GDPSetOtherModeL(long sft, long len, long w1)
        {
            long mask = ((1 << (int)len) - 1) << (int)sft;
            long result = (DP_OtherModeL & ~mask) | (w1 & mask);
            if (result != DP_OtherModeL)
            {
                DP_OtherModeL = result;
                stateChanged = true;
            }
        }

        public void GDPSetOtherModeH(long sft, long len, long w1)
        {
            long mask = ((1 << (int)len) - 1) << (int)sft;
            long result = (DP_OtherModeH & ~mask) | (w1 & mask);
            if (result != DP_OtherModeH)
            {
                DP_OtherModeH = result;
                stateChanged = true;
            }
        }

        public void GDPSetCombine(long w0, long w1)
        {
            if (DP_CombineH != w0 || DP_CombineL != w1)
            {
                DP_CombineH = w0;
                DP_CombineL = w1;
                stateChanged = true;
            }
        }

        public Vector4 GSPSetPrimColor(float lod, float r, float g, float b, float a)
        {
            DP_PrimColor = new Vector4(r / 255f, g / 255f, b / 255f, a / 255f);
            DP_PrimLOD = lod / 255f;
            stateChanged = true;
            return DP_PrimColor;
        }

        public Vector4 GSPSetEnvColor(float r, float g, float b, float a)
        {
            DP_EnvColor = new Vector4(r / 255f, g / 255f, b / 255f, a / 255f);
            stateChanged = true;
            return DP_EnvColor;
        }

        public void GSPResetMatrixStackDepth(long value)
        {
            SP_MatrixIndex = value;
        }
    }

}
