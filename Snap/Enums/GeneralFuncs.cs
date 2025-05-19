using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public enum GeneralFuncs : long
    {
        RunProcess = 0x08C28,
        EndProcess = 0x08F2C,

        Signal = 0x0B774,
        SignalAll = 0x0B830,
        Yield = 0x0BCA8,

        AnimateNode = 0x11090,

        ArcTan = 0x19ABC,
        Random = 0x19DB0,
        RandomInt = 0x19E14,
        GetRoom = 0xE2184,
    }
}
