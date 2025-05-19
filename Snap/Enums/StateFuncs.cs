using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public enum StateFuncs : long
    {
        SetAnimation = 0x35F138,
        ForceAnimation = 0x35F15C,
        SetMotion = 0x35EDF8,
        SetState = 0x35EC58,
        InteractWait = 0x35FBF0,
        Wait = 0x35FC54,
        Random = 0x35ECAC,
        RunAux = 0x35ED90,
        EndAux = 0x35EDC8,
        Cleanup = 0x35FD70,
        EatApple = 0x36010C,

        SpawnActorHere = 0x35FE24,
        SpawnActor = 0x363C48,

        SplashAt = 0x35E174,
        SplashBelow = 0x35E1D4,
        DratiniSplash = 0x35E238,
        SplashOnImpact = 0x35E298,

        // only in cave
        DanceInteract = 0x2C1440,
        DanceInteract2 = 0x2C0140,
    }

}
