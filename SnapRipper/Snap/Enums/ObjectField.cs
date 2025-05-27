using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public enum ObjectField : long
    {
        ObjectFlags = 0x08,
        Tangible = 0x10,

        // Root node
        TranslationX = 0x1C,
        TranslationY = 0x20,
        TranslationZ = 0x24,

        // Root transform
        Pitch = 0x1C,
        Yaw = 0x20,
        Roll = 0x24,
        ScaleX = 0x2C,
        ScaleY = 0x30,
        ScaleZ = 0x34,

        Transform = 0x4C,
        ParentFlags = 0x50,

        Apple = 0x64,
        Target = 0x70,
        Behavior = 0x88,
        StateFlags = 0x8C,
        Timer = 0x90,
        ForwardSpeed = 0x98,
        VerticalSpeed = 0x9C,
        MovingYaw = 0xA0,
        LoopTarget = 0xA4,
        FrameTarget = 0xA8,
        Transitions = 0xAC,

        StoredValues = 0xB0,
        Mystery = 0xC0,
        GroundList = 0xCC,

        Path = 0xE8,
        PathParam = 0xEC,
    }

}
