using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.RDP
{
    public class RDPUtils
    {
        public static OtherModeH_CycleType GetCycleTypeFromOtherModeH(long modeH)
        {
            return (OtherModeH_CycleType)((modeH >> (int)OtherModeH_Layout.G_MDSFT_CYCLETYPE) & 0x03);
        }
        private static long MapAdditive(long x)
        {
            return x >= 8 ? (long)CCMUX.ADD_ZERO : x;
        }

        private static long MapMult(long x)
        {
            return x >= 16 ? (long)CCMUX.MUL_ZERO : x;
        }

        public static CombineParams DecodeCombineParams(long w0, long w1)
        {
            long a0 = MapAdditive((w0 >> 20) & 0x0F);
            long c0 = MapMult((w0 >> 15) & 0x1F);
            long Aa0 = (w0 >> 12) & 0x07;
            long Ac0 = (w0 >> 9) & 0x07;
            long a1 = MapAdditive((w0 >> 5) & 0x0F);
            long c1 = MapMult(w0 & 0x1F);

            long b0 = MapAdditive((w1 >> 28) & 0x0F);
            long b1 = MapAdditive((w1 >> 24) & 0x0F);
            long Aa1 = (w1 >> 21) & 0x07;
            long Ac1 = (w1 >> 18) & 0x07;
            long d0 = (w1 >> 15) & 0x07;
            long Ab0 = (w1 >> 12) & 0x07;
            long Ad0 = (w1 >> 9) & 0x07;
            long d1 = (w1 >> 6) & 0x07;
            long Ab1 = (w1 >> 3) & 0x07;
            long Ad1 = w1 & 0x07;

            //System.Debug.Assert(b0 != (long)CCMUX.ONE && c0 != (long)CCMUX.ONE && b1 != (long)CCMUX.ONE && c1 != (long)CCMUX.ONE);

            return new CombineParams
            {
                c0 = new ColorCombinePass { a = (CCMUX)a0, b = (CCMUX)b0, c = (CCMUX)c0, d = (CCMUX)d0 },
                a0 = new AlphaCombinePass { a = (ACMUX)Aa0, b = (ACMUX)Ab0, c = (ACMUX)Ac0, d = (ACMUX)Ad0 },
                c1 = new ColorCombinePass { a = (CCMUX)a1, b = (CCMUX)b1, c = (CCMUX)c1, d = (CCMUX)d1 },
                a1 = new AlphaCombinePass { a = (ACMUX)Aa1, b = (ACMUX)Ab1, c = (ACMUX)Ac1, d = (ACMUX)Ad1 },
            };
        }

        public static bool CombineParamsUsesT0(CombineParams cp)
        {
            return ColorCombinePassUsesT0(cp.c0) || ColorCombinePassUsesT0(cp.c1) ||
                   AlphaCombinePassUsesT0(cp.a0) || AlphaCombinePassUsesT0(cp.a1);
        }

        public static bool CombineParamsUsesT1(CombineParams cp)
        {
            return ColorCombinePassUsesT1(cp.c0) || ColorCombinePassUsesT1(cp.c1) ||
                   AlphaCombinePassUsesT1(cp.a0) || AlphaCombinePassUsesT1(cp.a1);
        }

        public static bool CombineParamsUseTexelsInSecondCycle(CombineParams comb)
        {
            return comb.a1.a == ACMUX.TEXEL0 || comb.a1.b == ACMUX.TEXEL0 || comb.a1.c == ACMUX.TEXEL0 || comb.a1.d == ACMUX.TEXEL0 ||
                   comb.a1.a == ACMUX.TEXEL1 || comb.a1.b == ACMUX.TEXEL1 || comb.a1.c == ACMUX.TEXEL1 || comb.a1.d == ACMUX.TEXEL1 ||
                   comb.c1.a == CCMUX.TEXEL0 || comb.c1.b == CCMUX.TEXEL0 || comb.c1.c == CCMUX.TEXEL0 || comb.c1.d == CCMUX.TEXEL0 ||
                   comb.c1.a == CCMUX.TEXEL1 || comb.c1.b == CCMUX.TEXEL1 || comb.c1.c == CCMUX.TEXEL1 || comb.c1.d == CCMUX.TEXEL1 ||
                   comb.c1.c == CCMUX.TEXEL0_A || comb.c1.c == CCMUX.TEXEL1_A;
        }

        public static bool CombineParamsUseCombinedInFirstCycle(CombineParams comb)
        {
            return comb.a0.a == ACMUX.ADD_COMBINED || comb.a0.b == ACMUX.ADD_COMBINED || comb.a0.d == ACMUX.ADD_COMBINED ||
                   comb.c0.a == CCMUX.COMBINED || comb.c0.b == CCMUX.COMBINED || comb.c0.c == CCMUX.COMBINED || comb.c0.d == CCMUX.COMBINED ||
                   comb.c0.c == CCMUX.COMBINED_A;
        }

        public static bool CombineParamsUseT1InFirstCycle(CombineParams comb)
        {
            return comb.a0.a == ACMUX.TEXEL1 || comb.a0.b == ACMUX.TEXEL1 || comb.a0.c == ACMUX.TEXEL1 || comb.a0.d == ACMUX.TEXEL1 ||
                   comb.c0.a == CCMUX.TEXEL1 || comb.c0.b == CCMUX.TEXEL1 || comb.c0.c == CCMUX.TEXEL1 || comb.c0.d == CCMUX.TEXEL1 ||
                   comb.c0.c == CCMUX.TEXEL1_A;
        }

        private static bool ColorCombinePassUsesT0(ColorCombinePass ccp)
        {
            return ccp.a == CCMUX.TEXEL0 || ccp.a == CCMUX.TEXEL0_A ||
                   ccp.b == CCMUX.TEXEL0 || ccp.b == CCMUX.TEXEL0_A ||
                   ccp.c == CCMUX.TEXEL0 || ccp.c == CCMUX.TEXEL0_A ||
                   ccp.d == CCMUX.TEXEL0 || ccp.d == CCMUX.TEXEL0_A;
        }

        private static bool AlphaCombinePassUsesT0(AlphaCombinePass acp)
        {
            return acp.a == ACMUX.TEXEL0 || acp.b == ACMUX.TEXEL0 || acp.c == ACMUX.TEXEL0 || acp.d == ACMUX.TEXEL0;
        }

        private static bool ColorCombinePassUsesT1(ColorCombinePass ccp)
        {
            return ccp.a == CCMUX.TEXEL1 || ccp.a == CCMUX.TEXEL1_A ||
                   ccp.b == CCMUX.TEXEL1 || ccp.b == CCMUX.TEXEL1_A ||
                   ccp.c == CCMUX.TEXEL1 || ccp.c == CCMUX.TEXEL1_A ||
                   ccp.d == CCMUX.TEXEL1 || ccp.d == CCMUX.TEXEL1_A;
        }

        private static bool AlphaCombinePassUsesT1(AlphaCombinePass acp)
        {
            return acp.a == ACMUX.TEXEL1 || acp.b == ACMUX.TEXEL1 || acp.c == ACMUX.TEXEL1 || acp.d == ACMUX.TEXEL1;
        }
    }
}
