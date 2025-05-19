using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class CustomParticleSystem
    {
        public List<EmitterData> Emitters = new List<EmitterData>();
        public List<List<RDP.Texture>> ParticleTextures = new List<List<RDP.Texture>>();
    }
}
