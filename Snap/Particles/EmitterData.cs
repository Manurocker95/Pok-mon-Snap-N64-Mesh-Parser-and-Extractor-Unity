using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class EmitterData
    {
        public bool IsCommon;
        public long Index;

        public long ParticleIndex;
        public double Lifetime;
        public double ParticleLifetime;
        public long Flags;
        public double G;
        public double Drag;

        public Vector3 Velocity = Vector3.zero;

        public double Radius;
        public double SprayAngle;
        public double Increment;
        public double Size;

        public List<ParticleCommand> Program = new List<ParticleCommand>();
    }
}
