using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class WaitCommand : ParticleCommand
    {
        public long Frames;
        public long TexIndex;

        public WaitCommand()
        {
            Kind = CommandKind.Wait;
        }
    }
}
