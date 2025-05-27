using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class Poliwag : Actor
    {
        private double startAngle = 0;
        private double amplitude = 0;
        private double endHeight = 0;
        private int pathIndex = 0;

        public Poliwag(RenderData renderData, ObjectSpawn spawn, ActorDef def, LevelGlobals globals, bool isEgg = false) : base(renderData, spawn, def, globals, isEgg)
        {
        }

        protected override void StartBlock(LevelGlobals globals)
        {
            var state = this.Def.StateGraph.States[(int)this.CurrState];

            switch (state.StartAddress)
            {
                case 0x802DCBB8:
                    this.CurrBlock = this.Spawn.Behaviour - 4;
                    break;

                case 0x802DC5A8:
                    this.MotionData.StoredValues[1] = this.MotionData.StoredValues[0] + 1;
                    break;

                case 0x802DC05C:
                    this.MotionData.StoredValues[1] = this.MotionData.StoredValues[0] + 2;
                    break;

                case 0x802DC2F4:
                    this.MotionData.StoredValues[1] = this.MotionData.StoredValues[0] + 3;
                    break;

                case 0x802DC6BC:
                    this.MotionData.StoredValues[1] = this.MotionData.Path.Length - 1;
                    break;

                case 0x802DCC6C:
                    if (this.CurrBlock == 1)
                    {
                        AnimationUtils.GetPathPoint(ref this.Translation, this.MotionData.Path, (float)this.MotionData.Path.Times[this.pathIndex]);
                        this.Translation = new Vector3(
                            this.Translation.x,
                            (float)SnapUtils.GroundHeightAt(globals, this.Translation),
                            this.Translation.z
                        );
                        this.Euler = new Vector3(
                            this.Euler.x,
                            (float)(UnityEngine.Random.value * MathConstants.Tau),
                            this.Euler.z
                        );
                    }
                    break;
            }

            base.StartBlock(globals);
        }

        protected override MotionResult CustomMotion(long param, ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            if (param == 1)
            {
                this.MotionData.StoredValues[0] = this.MotionData.StoredValues[1];

                if (this.Def.StateGraph.States[(int)this.CurrState].StartAddress == 0x802DC60C)
                    this.MotionData.CurrBlock++; // skip the face player block

                return MotionResult.Done;
            }
            else
            {
                return base.CustomMotion(param, viewerInput, globals);
            }
        }

        protected override MotionResult AuxStep(ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            if (this.CurrAux < 0x80000000 && (this.CurrAux & SnapUtils.FakeAuxFlag) != 0)
            {
                this.MotionData.StateFlags |= (long)EndCondition.Pause;
                var refState = this.Def.StateGraph.States[(int)this.CurrAux & 0xFF];
                bool done = this.Renderers[this.HeadAnimationIndex].Animator.LoopCount >= 1;

                switch (this.CurrAnimation)
                {
                    default:
                        this.SetAnimation((int)refState.Blocks[0].Animation);
                        break;

                    case var anim when anim == refState.Blocks[0].Animation:
                        if (done)
                            this.SetAnimation((int)refState.Blocks[1].Animation);
                        break;

                    case var anim when anim == refState.Blocks[1].Animation:
                        if (done)
                            this.SetAnimation((int)refState.Blocks[2].Animation);
                        break;

                    case var anim when anim == refState.Blocks[2].Animation:
                        if (done)
                        {
                            this.MotionData.StateFlags &= ~(long)EndCondition.Pause;
                            return MotionResult.Done;
                        }
                        break;
                }
            }
            else if (this.CurrAux == 0x802DC820)
            {
                if (this.MotionData.AuxStart < 0)
                {
                    this.MotionData.AuxStart = viewerInput.Time;
                    GfxPlatformUtils.Assert(this.MotionData.Start >= 0, "aux before path");
                    Vector3 actorScratch = Vector3.zero;
                    AnimationUtils.GetPathPoint(ref actorScratch, this.MotionData.Path, 1);
                    this.endHeight = SnapUtils.GroundHeightAt(globals, actorScratch) - 330;
                    this.amplitude = this.Translation.y + 200 - this.endHeight;
                    this.startAngle = System.Math.Asin((this.Translation.y - this.endHeight) / this.amplitude);
                }

                double arcDuration = this.MotionData.Path.Duration * (1.0 - (float)this.MotionData.Path.Times[(long)this.MotionData.StoredValues[0]]);
                double frac = (viewerInput.Time - this.MotionData.AuxStart) / 1000.0 * 3 / arcDuration;

                if (frac > 1)
                    return MotionResult.Done;

                float oldHeight = this.Translation.y;
                this.Translation = new Vector3(
                    this.Translation.x,
                    (float)(this.endHeight + this.amplitude * System.Math.Sin(MathHelper.Lerp(this.startAngle, System.Math.PI, frac))),
                    this.Translation.z
                );

                if (oldHeight > 0 && this.Translation.y <= 0)
                    globals.CreateSplash(SplashType.Water, this.Translation, null);

                return MotionResult.Update;
            }

            return MotionResult.None;
        }

        protected override bool EndBlock(long address, LevelGlobals globals)
        {
            if (address == 0x802DC6BC)
            {
                globals.FishTracker = 1;
            }
            else if (address == 0x802DCC6C && this.CurrBlock == 1)
            {
                this.pathIndex++;
                if (this.pathIndex < this.MotionData.Path.Length)
                {
                    this.CurrBlock = 0;
                    this.StartBlock(globals);
                    return true;
                }
            }

            return false;
        }
    }

}
