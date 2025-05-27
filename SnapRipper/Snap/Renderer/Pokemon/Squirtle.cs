using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class Squirtle : Actor
    {
        private float depth = -95;

        public Squirtle(RenderData renderData, ObjectSpawn spawn, ActorDef def, LevelGlobals globals, bool isEgg = false) : base(renderData, spawn, def, globals, isEgg)
        {
        }

        protected override MotionResult AuxStep(ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            if (this.CurrAux == 0x802CBA90)
            {
                if (this.MotionData.AuxStart < 0)
                    this.MotionData.AuxStart = viewerInput.Time;

                double water = SnapUtils.GroundHeightAt(globals, this.Translation);
                double t = (viewerInput.Time - this.MotionData.AuxStart) / 1000.0;
                this.Translation = new Vector3(
                    this.Translation.x,
                    (float)(water + 10 * System.Math.Sin(t / 1.5 * MathConstants.Tau) + this.depth),
                    this.Translation.z
                );

                this.depth = System.Math.Min(this.depth + 60f * viewerInput.DeltaTime / 1000f, -45f);

                return MotionResult.Update;
            }

            return MotionResult.None;
        }
    }

}
