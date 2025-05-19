using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class ColorCommand : ParticleCommand
    {
        public long Flags;
        public long Frames;
        public Vector4 Color = Vector4.zero;

        public ColorCommand()
        {
            Kind = CommandKind.Color;
        }
    }
}
