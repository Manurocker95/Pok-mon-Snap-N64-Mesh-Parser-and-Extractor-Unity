using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public enum CalcBillboardFlags : long
    {
        // The up vector for computing roll should come from the input matrix.
        UseRollLocal = 0 << 0,
        // The up vector for computing roll should be global world up 0, 1, 0.
        UseRollGlobal = 1 << 0,

        // Z, X, Y priority (normal billboard mode)
        PriorityZ = 0 << 1,
        // Z, X, Y, Z priority ("Y billboard" mode)
        PriorityY = 1 << 1,

        // The Z+ vector should be projected onto a plane (Z+ = 0, 0, 1)
        UseZPlane = 0 << 2,
        // The Z+ vector should be projected onto a sphere (Z+ = -Translation), aka "persp" mode
        UseZSphere = 1 << 2,
    }

}
