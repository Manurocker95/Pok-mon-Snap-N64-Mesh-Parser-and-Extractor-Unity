using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class PhysicsCommand : ParticleCommand
    {
        public long Flags;
        public Vector3 Values = Vector3.zero;

        public PhysicsCommand()
        {
            Kind = CommandKind.Physics;
        }
    }
}
