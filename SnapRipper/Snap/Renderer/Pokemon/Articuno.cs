using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class Articuno : Actor
    {
        public Articuno(RenderData renderData, ObjectSpawn spawn, ActorDef def, LevelGlobals globals, bool isEgg = false) : base(renderData, spawn, def, globals, isEgg)
        {
        }

        protected override void StartBlock(LevelGlobals globals)
        {
            if (this.Def.StateGraph.States[(int)this.CurrState].StartAddress == 0x802C46F0)
            {
                if (this.CurrBlock == 0)
                    this.Translation = GfxPlatformUtils.AssertExists(this.Target).Translation;
            }

            base.StartBlock(globals);
        }
    }

}
