using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class Pidgey : Actor
    {
        public Pidgey(RenderData renderData, ObjectSpawn spawn, ActorDef def, LevelGlobals globals, bool isEgg = false) : base(renderData, spawn, def, globals, isEgg)
        {
        }

        protected override MotionResult AuxStep(ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            if (this.CurrAux == 0x802C8BC4)
            {
                if (this.MotionData.AuxStart < 0)
                    this.MotionData.AuxStart = viewerInput.Time;

                if (viewerInput.Time > this.MotionData.AuxStart + (0x80 * 1000 / 30))
                {
                    if (this.MotionData.StoredValues[0] == 0)
                        globals.SendGlobalSignal(this, 0x1D);

                    return MotionResult.Done;
                }
            }

            return MotionResult.None;
        }
    }

}
