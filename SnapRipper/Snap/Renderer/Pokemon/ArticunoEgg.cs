using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class ArticunoEgg : Actor
    {
        private double currFPS = 30;

        public ArticunoEgg(RenderData renderData, ObjectSpawn spawn, ActorDef def, LevelGlobals globals, bool isEgg = false) : base(renderData, spawn, def, globals, isEgg)
        {
        }

        protected override void StartBlock(LevelGlobals globals)
        {
            var state = this.Def.StateGraph.States[(int)this.CurrState];
            if (state.StartAddress == 0x802C4B04)
            {
                if (this.CurrBlock == 0)
                    this.currFPS = 30;
            }

            base.StartBlock(globals);
        }

        protected override MotionResult CustomMotion(long param, ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            if (this.currFPS >= 120)
            {
                this.MotionData.StateFlags |= (long)EndCondition.Misc;
                return MotionResult.Done;
            }

            if (!SnapUtils.CanHearSong(this.Translation, globals))
            {
                this.MotionData.StateFlags &= ~(long)EndCondition.Misc;
                return MotionResult.Done;
            }

            this.currFPS += 15 * viewerInput.DeltaTime / 1000.0;
            this.AnimationController.Adjust((float)this.currFPS);

            return MotionResult.None;
        }

        protected override bool EndBlock(long address, LevelGlobals globals)
        {
            if (address == 0x802C4B04)
            {
                if (this.CurrBlock == 0)
                {
                    this.AnimationController.Adjust(30);
                    var articuno = globals.AllActors.FirstOrDefault(a => a.Def.ID == 144);
                    if (articuno != null)
                        this.MotionData.StoredValues[0] = articuno.Translation.y - 250;
                }
            }

            return false;
        }
    }

}
