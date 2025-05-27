using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class ZapdosEgg : Actor
    {
        public ZapdosEgg(RenderData renderData, ObjectSpawn spawn, ActorDef def, LevelGlobals globals, bool isEgg = false) : base(renderData, spawn, def, globals, isEgg)
        {
        }

        protected override void StartBlock(LevelGlobals globals)
        {
            var state = this.Def.StateGraph.States[(int)this.CurrState];

            if (state.StartAddress == 0x802EC078)
            {
                if (this.CurrBlock == 0)
                {
                    var zapdos = globals.AllActors.FirstOrDefault(a => a.Def.ID == 145);
                    if (zapdos != null)
                        this.MotionData.Destination = zapdos.Translation;
                }
            }

            base.StartBlock(globals);
        }

        protected override MotionResult CustomMotion(long param, ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            if (param == 1)
            {
                double delta = 300 * viewerInput.DeltaTime / 1000.0;

                if (this.Translation.y + delta > this.MotionData.Destination.y - 120)
                {
                    this.Translation = new Vector3(
                        this.Translation.x,
                        (float)(this.MotionData.Destination.y - 120),
                        this.Translation.z
                    );
                    return MotionResult.Done;
                }

                this.Translation = new Vector3(
                    this.Translation.x,
                    this.Translation.y + (float)delta,
                    this.Translation.z
                );

                return MotionResult.Update;
            }
            else
            {
                return base.CustomMotion(param, viewerInput, globals);
            }
        }
    }

}
