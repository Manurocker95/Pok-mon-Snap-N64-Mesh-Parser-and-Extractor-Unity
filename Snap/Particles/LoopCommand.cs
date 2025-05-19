using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class LoopCommand : ParticleCommand
    {
        public bool IsEnd;
        public long Count;

        public LoopCommand()
        {
            Kind = CommandKind.Loop;
        }
    }
}
