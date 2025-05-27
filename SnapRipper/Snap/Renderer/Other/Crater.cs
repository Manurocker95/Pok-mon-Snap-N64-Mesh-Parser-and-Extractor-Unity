using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class Crater : Actor
    {
        public Crater(RenderData renderData, ObjectSpawn spawn, ActorDef def, LevelGlobals globals, bool isEgg = false) : base(renderData, spawn, def, globals, isEgg)
        {
        }

        protected override void StartBlock(LevelGlobals globals)
        {
            var state = this.Def.StateGraph.States[(int)this.CurrState];

            if (state.StartAddress == 0x802DE95C)
            {
                int edgeIndex = 0;

                if (this.Target != null &&
                    SnapUtils.FindGroundPlane(globals.Level.Collision, this.Target.Translation.x, this.Target.Translation.z).Type == 0xFF4C19)
                {
                    edgeIndex = 1;
                }

                this.FollowEdge(state.Blocks[0].Edges[edgeIndex], globals);
                return;
            }

            base.StartBlock(globals);
        }
    }

}
