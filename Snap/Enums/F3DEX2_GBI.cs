using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public enum F3DEX2_GBI : byte
    {
        G_VTX = 0x01,
        G_MODIFYVTX = 0x02,
        G_CULLDL = 0x03,
        G_BRANCH_Z = 0x04,
        G_TRI1 = 0x05,
        G_TRI2 = 0x06,
        G_QUAD = 0x07,
        G_LINE3D = 0x08,

        G_TEXTURE = 0xD7,
        G_POPMTX = 0xD8,
        G_GEOMETRYMODE = 0xD9,
        G_MTX = 0xDA,
        G_MOVEWORD = 0xDB,
        G_DL = 0xDE,
        G_ENDDL = 0xDF,

        G_SETCIMG = 0xFF,
        G_SETZIMG = 0xFE,
        G_SETTIMG = 0xFD,
        G_SETCOMBINE = 0xFC,
        G_SETENVCOLOR = 0xFB,
        G_SETPRIMCOLOR = 0xFA,
        G_SETBLENDCOLOR = 0xF9,
        G_SETFOGCOLOR = 0xF8,
        G_SETFILLCOLOR = 0xF7,
        G_FILLRECT = 0xF6,
        G_SETTILE = 0xF5,
        G_LOADTILE = 0xF4,
        G_LOADBLOCK = 0xF3,
        G_SETTILESIZE = 0xF2,
        G_LOADTLUT = 0xF0,
        G_RDPSETOTHERMODE = 0xEF,
        G_SETPRIMDEPTH = 0xEE,
        G_SETSCISSOR = 0xED,
        G_SETCONVERT = 0xEC,
        G_SETKEYR = 0xEB,
        G_SETKEYFB = 0xEA,
        G_RDPFULLSYNC = 0xE9,
        G_RDPTILESYNC = 0xE8,
        G_RDPPIPESYNC = 0xE7,
        G_RDPLOADSYNC = 0xE6,
        G_TEXRECTFLIP = 0xE5,
        G_TEXRECT = 0xE4,
        G_SETOTHERMODE_H = 0xE3,
        G_SETOTHERMODE_L = 0xE2,
        G_RDPHALF_1 = 0xE1,
    }
}
