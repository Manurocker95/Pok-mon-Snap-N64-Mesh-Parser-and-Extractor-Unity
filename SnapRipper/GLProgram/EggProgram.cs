using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.RDP;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class EggProgram : F3DEX_Program
    {
        public static long a_EndPosition = 3;
        public static long a_EndColor = 4;

        public EggProgram(long dp_OtherModeH, long dp_OtherModeL, CombineParams combParams, double blendAlpha = 0.5, List<TileState> tiles = null, long g_MW_NUMLIGHT = 0) : base(dp_OtherModeH, dp_OtherModeL, combParams, blendAlpha, tiles, g_MW_NUMLIGHT)
        {

        }
    }
}
