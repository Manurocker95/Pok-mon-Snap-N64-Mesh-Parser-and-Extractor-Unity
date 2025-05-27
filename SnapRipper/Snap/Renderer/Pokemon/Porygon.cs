using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class Porygon : Actor
    {
        private double startAngle = 0;
        private double amplitude = 0;
        private double endHeight = 0;

        public Porygon(RenderData renderData, ObjectSpawn spawn, ActorDef def, LevelGlobals globals, bool isEgg = false) : base(renderData, spawn, def, globals, isEgg)
        {
        }

        protected override void StartBlock(LevelGlobals globals)
        {
            var state = this.Def.StateGraph.States[(int)this.CurrState];

            if (state.StartAddress == 0x802DD1D4)
            {
                if (this.Target != null && Vector3.Distance(this.Translation, this.Target.Translation) < 1000)
                {
                    this.CurrBlock = this.Spawn.Behaviour - 1;
                }
                else
                {
                    this.ChangeState(0, globals);
                    return;
                }
            }

            base.StartBlock(globals);

            if (state.StartAddress == 0x802DD0E0 && this.Spawn.Behaviour == 1 && this.CurrAnimation != 1)
                this.SetAnimation(1);

            if (state.StartAddress == 0x802DD398 && this.CurrBlock == 2 && this.Spawn.Behaviour == 2)
                globals.SendGlobalSignal(this, 0x2B);
        }

        protected override MotionResult AuxStep(ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            if (this.CurrAux == 0x802DD53C)
            {
                if (this.MotionData.AuxStart < 0)
                {
                    this.MotionData.AuxStart = viewerInput.Time;
                    GfxPlatformUtils.Assert(this.MotionData.Start >= 0, "aux before path");

                    Vector3 actorScratch = Vector3.zero;
                    AnimationUtils.GetPathPoint(ref actorScratch, this.MotionData.Path, 1);
                    this.endHeight = SnapUtils.GroundHeightAt(globals, actorScratch);
                    this.amplitude = this.Translation.y + 50 - this.endHeight;
                    this.startAngle = System.Math.Asin((this.Translation.y - this.endHeight) / this.amplitude);
                }

                double frac = (viewerInput.Time - this.MotionData.AuxStart) / 1000.0 * 3 / this.MotionData.Path.Duration;

                if (frac > 1)
                    return MotionResult.Done;

                this.Translation = new Vector3(
                    this.Translation.x,
                    (float)(this.endHeight + this.amplitude *  System.Math.Sin(MathHelper.Lerp(this.startAngle, System.Math.PI, frac))),
                    this.Translation.z
                );

                return MotionResult.Update;
            }

            return MotionResult.None;
        }
    }

}
