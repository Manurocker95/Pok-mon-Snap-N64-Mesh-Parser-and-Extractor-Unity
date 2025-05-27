using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public delegate void DlRunner(RSPState state, long addr, DlRunner subDLHandler);

    public static class F3DEXUtils
    {
        public static GfxMegaStateDescriptor TranslateBlendMode(long geoMode, long renderMode)
        {
            
            var output = RDP.RDPUtils.TranslateRenderMode(renderMode);
            output.CullMode = GfxUtils.TranslateCullMode(geoMode);
            return output;
        }

        public static void RunDL_F3DEX2(RSPState state, long addr, DlRunner subDLHandler = null)
        {
            if (subDLHandler == null)
                subDLHandler = RunDL_F3DEX2;

            VP_DataView view = state.dataMap.GetView(addr);
            for (long i = 0; i < view.ByteLength; i += 8)
            {
                long w0 = view.GetUint32(i, false);
                long w1 = view.GetUint32(i + 4, false);
                byte cmd = (byte)(w0 >> 24);

                switch (cmd)
                {
                    case (byte)F3DEX2_GBI.G_ENDDL: return;
                    case (byte)F3DEX2_GBI.G_GEOMETRYMODE:
                        state.GSPClearGeometryMode(~(long)(w0 & 0x00FFFFFF));
                        state.GSPSetGeometryMode((long)w1);
                        break;
                    case (byte)F3DEX2_GBI.G_SETTIMG:
                        state.GDPSetTextureImage((long)((w0 >> 21) & 0x07), (long)((w0 >> 19) & 0x03), (long)((w0 & 0x0FFF) + 1), (long)w1);
                        break;
                    case (byte)F3DEX2_GBI.G_SETTILE:
                        state.GDPSetTile((long)((w0 >> 21) & 0x07), (long)((w0 >> 19) & 0x03), (long)((w0 >> 9) & 0x1FF), (long)(w0 & 0x1FF),
                                         (long)((w1 >> 24) & 0x07), (long)((w1 >> 20) & 0x0F), (long)((w1 >> 18) & 0x03), (long)((w1 >> 14) & 0x0F),
                                         (long)((w1 >> 10) & 0x0F), (long)((w1 >> 8) & 0x03), (long)((w1 >> 4) & 0x0F), (long)(w1 & 0x0F));
                        break;
                    case (byte)F3DEX2_GBI.G_LOADTLUT:
                        state.GDPLoadTLUT((long)((w1 >> 24) & 0x07), (long)((w1 >> 14) & 0x3FF));
                        break;
                    case (byte)F3DEX2_GBI.G_LOADBLOCK:
                        state.GDPLoadBlock((long)((w1 >> 24) & 0x07), (long)((w0 >> 12) & 0x0FFF), (long)(w0 & 0x0FFF), (long)((w1 >> 12) & 0x0FFF), (long)(w1 & 0x0FFF));
                        break;
                    case (byte)F3DEX2_GBI.G_VTX:
                        long v0w = (long)((w0 >> 1) & 0xFF);
                        long n = (long)((w0 >> 12) & 0xFF);
                        long v0 = v0w - n;
                        state.GSPVertex((long)w1, n, v0);
                        break;
                    case (byte)F3DEX2_GBI.G_TRI1:
                        state.GSPTri((long)((w0 >> 16) & 0xFF) / 2, (long)((w0 >> 8) & 0xFF) / 2, (long)(w0 & 0xFF) / 2);
                        break;
                    case (byte)F3DEX2_GBI.G_TRI2:
                        state.GSPTri((long)((w0 >> 16) & 0xFF) / 2, (long)((w0 >> 8) & 0xFF) / 2, (long)(w0 & 0xFF) / 2);
                        state.GSPTri((long)((w1 >> 16) & 0xFF) / 2, (long)((w1 >> 8) & 0xFF) / 2, (long)(w1 & 0xFF) / 2);
                        break;
                    case (byte)F3DEX2_GBI.G_DL:
                        long segment = (long)((w1 >> 24) & 0xFF);
                        if (segment == 0x80)
                            RunDL_F3DEX2(state, (long)w1, subDLHandler);
                        else
                            subDLHandler(state, (long)w1, null);
                        if (((w0 >> 16) & 0xFF) != 0)
                            return;
                        break;
                    case (byte)F3DEX2_GBI.G_RDPSETOTHERMODE:
                        state.GDPSetOtherModeH(0, 24, (long)(w0 & 0x00FFFFFF));
                        state.GDPSetOtherModeL(0, 32, (long)w1);
                        break;
                    case (byte)F3DEX2_GBI.G_SETOTHERMODE_H:
                        long lenH = ((long)w0 & 0xFF) + 1;
                        long sftH = 0x20 - (((long)w0 >> 8) & 0xFF) - lenH;
                        state.GDPSetOtherModeH(sftH, lenH, (long)w1);
                        break;
                    case (byte)F3DEX2_GBI.G_SETOTHERMODE_L:
                        long lenL = ((long)w0 & 0xFF) + 1;
                        long sftL = 0x20 - (((long)w0 >> 8) & 0xFF) - lenL;
                        state.GDPSetOtherModeL(sftL, lenL, (long)w1);
                        break;
                    case (byte)F3DEX2_GBI.G_SETCOMBINE:
                        state.GDPSetCombine((long)(w0 & 0x00FFFFFF), (long)w1);
                        break;
                    case (byte)F3DEX2_GBI.G_TEXTURE:
                        long level = (long)((w0 >> 11) & 0x07);
                        long tile = (long)((w0 >> 8) & 0x07);
                        bool on = ((w0 & 0x7F) != 0);
                        long s = (long)((w1 >> 16) & 0xFFFF);
                        long t = (long)(w1 & 0xFFFF);

                        state.GSPTexture(on, tile, level, s, t);
                        break;
                    case (byte)F3DEX2_GBI.G_SETTILESIZE:
                        state.GDPSetTileSize((long)((w1 >> 24) & 0x07), (long)((w0 >> 12) & 0x0FFF), (long)(w0 & 0x0FFF), (long)((w1 >> 12) & 0x0FFF), (long)(w1 & 0x0FFF));
                        break;
                    case (byte)F3DEX2_GBI.G_POPMTX:
                        state.GSPResetMatrixStackDepth(1);
                        break;
                    case (byte)F3DEX2_GBI.G_MTX:
                        if ((w1 >> 24) != 5) throw new Exception("Expected matrix index format 5");
                        state.GSPResetMatrixStackDepth((long)((w1 & 0xFFFFFF) >> 6));
                        break;
                    case (byte)F3DEX2_GBI.G_SETPRIMCOLOR:
                        state.GSPSetPrimColor((long)(w0 & 0xFF), (long)((w1 >> 24) & 0xFF), (long)((w1 >> 16) & 0xFF), (long)((w1 >> 8) & 0xFF), (int)(w1 & 0xFF));
                        break;
                    case (byte)F3DEX2_GBI.G_SETBLENDCOLOR:
                        break; // Not implemented
                    case (byte)F3DEX2_GBI.G_SETENVCOLOR:
                        state.GSPSetEnvColor((long)((w1 >> 24) & 0xFF), (long)((w1 >> 16) & 0xFF), (long)((w1 >> 8) & 0xFF), (long)(w1 & 0xFF));
                        break;
                    case (byte)F3DEX2_GBI.G_BRANCH_Z:
                        RunDL_F3DEX2(state, state.DP_Half1, subDLHandler);
                        return;
                    case (byte)F3DEX2_GBI.G_RDPHALF_1:
                        state.DP_Half1 = (long)w1;
                        break;
                    case (byte)F3DEX2_GBI.G_MODIFYVTX:
                        long w = (long)((w0 >> 16) & 0xFF);
                        long nmod = (long)((w0 >> 1) & 0x7FFF);
                        short upper = view.GetInt16(i + 4, false);
                        short lower = view.GetInt16(i + 6, false);
                        state.GSPModifyVertex(w, nmod, upper, lower);
                        break;
                    case (byte)F3DEX2_GBI.G_MOVEWORD:
                        // assert(((w0 >>> 16) & 0xFF) === 0x0A)
                        break; // TODO
                    case (byte)F3DEX2_GBI.G_CULLDL:
                    case (byte)F3DEX2_GBI.G_RDPFULLSYNC:
                    case (byte)F3DEX2_GBI.G_RDPTILESYNC:
                    case (byte)F3DEX2_GBI.G_RDPPIPESYNC:
                    case (byte)F3DEX2_GBI.G_RDPLOADSYNC:
                        break;
                    default:
                        Console.WriteLine($"Unknown DL opcode: {cmd:X2} at {i:X8}");
                        break;
                }
            }
        }
    }
}
