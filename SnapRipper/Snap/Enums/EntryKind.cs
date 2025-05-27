using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public enum EntryKind : byte
    {
        Exit = 0x00,
        InitFunc = 0x01,
        Block = 0x02,
        LerpBlock = 0x03,
        Lerp = 0x04,
        SplineVelBlock = 0x05,
        SplineVel = 0x06,
        SplineEnd = 0x07,
        SplineBlock = 0x08,
        Spline = 0x09,
        StepBlock = 0x0A,
        Step = 0x0B,
        Skip = 0x0C,
        Path = 0x0D, // only models
        Loop = 0x0E,
        SetFlags = 0x0F, // model animation only
        Func = 0x10,
        MultiFunc = 0x11,
        ColorStepBlock = 0x12, // material color only
        ColorStep = 0x13,
        ColorLerpBlock = 0x14,
        ColorLerp = 0x15,
        SetColor = 0x16  // choose based on flags, also directly sets update time???
    }
}
