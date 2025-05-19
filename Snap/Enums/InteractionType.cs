using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{

    public enum InteractionType
    {
        PokefluteA = 0x05,
        PokefluteB = 0x06,
        PokefluteC = 0x07,
        PesterAlmost = 0x08,
        PesterHit = 0x09,
        PesterLanded = 0x0A,
        AppleAlmost = 0x0C,
        AppleHit = 0x0D,
        AppleLanded = 0x0E,
        FindApple = 0x0F,
        NearPlayer = 0x10,
        CheckCollision = 0x11,
        PhotoTaken = 0x12,
        EnterRoom = 0x13,
        GravelerLanded = 0x14,
        AppleRemoved = 0x15,
        TargetRemoved = 0x16,
        PhotoFocus = 0x17,
        PhotoSubject = 0x18,
        Collided = 0x1A,
        EndMarker = 0x3A,

        Basic,
        Random,
        Flag,
        NotFlag,
        Behavior,
        NonzeroBehavior,
        NoTarget,
        HasTarget,
        HasApple,
        OverSurface,
        Unknown
    }
}
