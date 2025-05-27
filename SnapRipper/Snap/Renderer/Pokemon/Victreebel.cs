using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    class Victreebel : Actor
    {
        public Victreebel(RenderData renderData, ObjectSpawn spawn, ActorDef def, LevelGlobals globals, bool isEgg = false) : base(renderData, spawn, def, globals, isEgg)
        {
        }

        protected override void StartBlock(LevelGlobals globals)
        {
            if (this.Def.StateGraph.States[(int)this.CurrState].StartAddress == 0x802BFEF0)
            {
                this.Translation = new Vector3(
                    this.Translation.x,
                    GfxPlatformUtils.AssertExists(this.Target).Translation.y - 100,
                    this.Translation.z
                );
                this.UpdatePositions();
            }

            base.StartBlock(globals);
        }
    }

}
