using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class Weepinbell : Actor
    {
        public Weepinbell(RenderData renderData, ObjectSpawn spawn, ActorDef def, LevelGlobals globals, bool isEgg = false) : base(renderData, spawn, def, globals, isEgg)
        {
        }

        protected override void StartBlock(LevelGlobals globals)
        {
            var state = this.Def.StateGraph.States[(int)this.CurrState];

            switch (state.StartAddress)
            {
                case 0x802BF68C:
                    MotionData.StoredValues[1] = 0.08f;
                    break;

                case 0x802BFA3C:
                    MotionData.Destination = Translation;
                    break;
            }

            base.StartBlock(globals);
        }
    }
}
