using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class Charmander : Actor
    {
        public Charmander(RenderData renderData, ObjectSpawn spawn, ActorDef def, LevelGlobals globals, bool isEgg = false) : base(renderData, spawn, def, globals, isEgg)
        {
        }

        protected override void StartBlock(LevelGlobals globals)
        {
            var state = this.Def.StateGraph.States[(int)this.CurrState];

            switch (state.StartAddress)
            {
                case 0x802D8BB8: // stored in an array
                    this.GlobalPointer = 0x802E1A1C + 4 * this.Spawn.Behaviour;
                    break;
                case 0x802D9074:
                    if (this.CurrBlock == 1)
                    {
                        if (this.Target is Actor actor && actor.Def.ID == 126)
                        {
                            this.FollowEdge(state.Blocks[0].Edges[0], globals);
                            return;
                        }
                    }
                    break;
            }
        }
    }
}
