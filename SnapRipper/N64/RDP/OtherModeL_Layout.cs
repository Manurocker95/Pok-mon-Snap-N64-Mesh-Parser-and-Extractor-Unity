using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public enum OtherModeL_Layout
    {
        // non-render-mode fields
        G_MDSFT_ALPHACOMPARE = 0,
        G_MDSFT_ZSRCSEL = 2,
        // cycle-independent render-mode bits
        AA_EN = 3,
        Z_CMP = 4,
        Z_UPD = 5,
        IM_RD = 6,
        CLR_ON_CVG = 7,
        CVG_DST = 8,
        ZMODE = 10,
        CVG_X_ALPHA = 12,
        ALPHA_CVG_SEL = 13,
        FORCE_BL = 14,
        // bit 15 unused, was "TEX_EDGE"
        // cycle-dependent render-mode bits
        B_2 = 16,
        B_1 = 18,
        M_2 = 20,
        M_1 = 22,
        A_2 = 24,
        A_1 = 26,
        P_2 = 28,
        P_1 = 30,
    }
}
