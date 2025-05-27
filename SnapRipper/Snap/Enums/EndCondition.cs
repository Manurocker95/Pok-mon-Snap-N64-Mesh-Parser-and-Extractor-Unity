using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public enum EndCondition : long
    {
        Animation = 0x01,
        Motion = 0x02,
        Timer = 0x04,
        Aux = 0x08,
        Target = 0x10,
        Pause = 0x20,      // used to pause motion, not as a condition
        Misc = 0x1000,

        Dance = 0x010000,  // special dance-related behavior in cave
        Hidden = 0x020000,  // from flags on the parent object
        PauseAnim = 0x040000,
        Collide = 0x080000,  // separate object flags
        AllowBump = 0x200000,
    }
}
