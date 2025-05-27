using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public enum MoveFlags : long
    {
        Ground = 0x01,
        SnapTurn = 0x02,
        Update = 0x02,
        DuringSong = 0x04,
        Continuous = 0x08,
        ConstHeight = 0x10,
        FacePlayer = 0x20,
        FaceAway = 0x40,
        SmoothTurn = 0x80,
    }
}
