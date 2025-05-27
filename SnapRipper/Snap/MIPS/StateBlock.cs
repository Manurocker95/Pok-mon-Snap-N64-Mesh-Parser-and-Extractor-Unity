using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class StateBlock 
    {
        public long Animation;
        public List<Motion> Motion;
        public long AuxAddress;
        public bool Force;
        public List<Signal> Signals = new List<Signal>();
        public WaitParams Wait;

        public long FlagSet;
        public long FlagClear;
        public bool? IgnoreGround;
        public bool? EatApple;
        public float? ForwardSpeed;
        public bool? Tangible;
        public SplashMotion Splash;
        public SpawnData Spawn;

        public List<StateEdge> Edges = new List<StateEdge>();
    }
}
