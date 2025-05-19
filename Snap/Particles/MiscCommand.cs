using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class MiscCommand : ParticleCommand
    {
        public long Subtype;
        public List<long> Values = null;
        public Vector3 Vector = Vector3.zero;
        public Vector4 Color = Vector4.zero;

        public MiscCommand()
        {
            Kind = CommandKind.Misc;
        }
    }

}
