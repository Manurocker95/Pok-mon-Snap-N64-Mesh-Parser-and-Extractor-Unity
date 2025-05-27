using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public enum MotionFuncs : long
    {
        PathPoint = 0x01FCA4,
        NodePos = 0x0A5E98,
        FindGround = 0x0E41D8,

        RiseBy = 0x360300,
        RiseTo = 0x36044C,
        FallBy = 0x360590,
        FallTo = 0x3606E8,
        Projectile = 0x360AB8,
        MoveForward = 0x360F1C,
        RandomCircle = 0x361110,
        GetSong = 0x361440,
        FaceTarget = 0x36148C,
        WalkToTarget = 0x361748,
        WalkFromTarget = 0x36194C,
        WalkFromTarget2 = 0x361B20,
        SetTarget = 0x361B50,
        StepToPoint = 0x361B68,
        ApproachPoint = 0x361E58,
        ResetPos = 0x362050,
        Path = 0x3620C8,
        DynamicVerts = 0x362414,

        VolcanoForward = 0x2D6E14,
    }
}
