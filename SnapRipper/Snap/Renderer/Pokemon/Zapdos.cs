using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class Zapdos : Actor
    {
        private Actor egg = null;

        public Zapdos(RenderData renderData, ObjectSpawn spawn, ActorDef def, LevelGlobals globals, bool isEgg = false) : base(renderData, spawn, def, globals, isEgg)
        {
        }

        protected override MotionResult CustomMotion(long param, ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            double r = 100 - (viewerInput.Time - this.MotionData.Start) / 10.0;

            if (r <= 0)
            {
                this.Translation = this.MotionData.StartPos;
                return MotionResult.Done;
            }

            double fromPlayer = SnapUtils.YawTowards(this.MotionData.StartPos, globals.Translation);

            this.Translation = new Vector3(
                (float)(this.MotionData.StartPos.x + r * System.Math.Sin(fromPlayer)),
                this.Translation.y,
                (float)(this.MotionData.StartPos.z + r * System.Math.Cos(fromPlayer))
            );

            return MotionResult.Update;
        }

        protected override MotionResult AuxStep(ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            if (this.CurrAux == 0x802EB6D0)
            {
                if (this.egg == null)
                {
                    this.egg = GfxPlatformUtils.AssertExists(globals.AllActors.FirstOrDefault(a => a.Def.ID == 602));
                    this.MotionData.StateFlags |= (long)EndCondition.Pause;
                }

                if (!this.egg.Visible)
                {
                    this.MotionData.StateFlags &= ~(long)EndCondition.Pause;
                    return MotionResult.Done;
                }
            }

            return MotionResult.None;
        }
    }
}
