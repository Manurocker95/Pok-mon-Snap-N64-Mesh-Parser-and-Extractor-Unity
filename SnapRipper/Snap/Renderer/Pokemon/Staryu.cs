using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class Staryu : Actor
    {
        private double spinSpeed = 0;
        private Actor whirlpool = null;
        private bool relativeToPlayer = true;

        public static int EvolveCount = 0;
        public static double SeparationScale = 1;
        private static long PlayerRadius = 800;

        public Staryu(RenderData renderData, ObjectSpawn spawn, ActorDef def, LevelGlobals globals, bool isEgg = false) : base(renderData, spawn, def, globals, isEgg)
        {
        }

        private static double BaseAngle(double time)
        {
            return MathConstants.Tau * (1 - ((time / 1500) % 1));
        }

        private static void TargetPosition(ref Vector3 dst, Vector3 pos, double time, long bhv)
        {
            double angle = BaseAngle(time) + (bhv == 0 ? 0 : (bhv - 1) * System.Math.PI / 9) * SeparationScale;
            dst = new Vector3(
                (float)(PlayerRadius * System.Math.Sin(angle)),
                -200,
                (float)(PlayerRadius * System.Math.Cos(angle))
            );
            dst += pos;
        }

        protected override MotionResult AuxStep(ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            switch (this.CurrAux)
            {
                case 0x802CD5D8:
                    double refHeight = this.relativeToPlayer ? globals.Translation.y : SnapUtils.GroundHeightAt(globals, this.Translation);
                    double delta = refHeight + this.MotionData.StoredValues[0] - this.Translation.y;
                    double step = 600 * viewerInput.DeltaTime / 1000.0;
                    if (System.Math.Abs(delta) < step)
                        this.MotionData.StateFlags |= (long)EndCondition.Misc;
                    this.Translation = new Vector3(this.Translation.x, (float)(this.Translation.y + MathHelper.ClampRange(delta, step)), this.Translation.z);
                    goto case 0x802CCAB4;

                case 0x802CCAB4:
                    if (this.spinSpeed == 0)
                        this.spinSpeed = MathHelper.RandomRange(1, 2) * System.Math.PI / 3;
                    this.Euler = new Vector3(this.Euler.x, (float)(this.Euler.y + this.spinSpeed * viewerInput.DeltaTime / 1000.0), this.Euler.z);
                    break;
            }

            return MotionResult.Update;
        }

        protected override MotionResult CustomMotion(long param, ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            double dt = viewerInput.DeltaTime / 1000.0;

            switch (param)
            {
                case 1:
                    this.MotionData.Destination = this.Translation - globals.Translation;
                    double approachAngle = System.Math.Atan2(this.MotionData.Destination.x, this.MotionData.Destination.z);
                    double radius = this.MotionData.Destination.magnitude;
                    MathHelper.NormToLength(ref this.MotionData.Destination, PlayerRadius);
                    this.MotionData.Destination += globals.Translation;
                    AnimationUtils.ApproachPoint(ref this.Translation, ref this.Euler, this.MotionData, globals, MIPSUtils.StaryuApproach, (float)dt);
                    this.Euler = new Vector3(this.Euler.x, (float)(approachAngle + System.Math.PI), this.Euler.z);

                    if (System.Math.Abs(radius - PlayerRadius) < 25 &&
                        System.Math.Abs(MathHelper.AngleDist(approachAngle, BaseAngle(viewerInput.Time))) < System.Math.PI / 72)
                    {
                        TargetPosition(ref this.MotionData.Destination, globals.Translation, viewerInput.Time, this.Spawn.Behaviour);
                        return MotionResult.Done;
                    }
                    break;

                case 2:
                case 3:
                case 4:
                    TargetPosition(ref this.MotionData.Destination, globals.Translation, viewerInput.Time, this.Spawn.Behaviour);

                    if (Vector3.Distance(this.Translation, this.MotionData.Destination) > PlayerRadius)
                    {
                        this.ChangeState(3, globals);
                        return MotionResult.Update;
                    }

                    double oldYaw = this.Euler.y;
                    MotionResult result = AnimationUtils.ApproachPoint(ref this.Translation, ref this.Euler, this.MotionData, globals, MIPSUtils.StaryuApproach, (float)dt);

                    if (param == 2)
                        return result;

                    this.Euler = new Vector3(this.Euler.x, (float)oldYaw, this.Euler.z);

                    if (this.whirlpool == null)
                    {
                        this.whirlpool = GfxPlatformUtils.AssertExists(globals.AllActors.FirstOrDefault(a => a.Def.ID == 1033));
                    }

                    if (param == 3)
                    {
                        if (Vector3.Distance(globals.Translation, this.whirlpool.Translation) < 4000)
                            return MotionResult.Done;
                        else
                            return MotionResult.Update;
                    }

                    SeparationScale = MathHelper.Clamp(SeparationScale + 0.9 * dt, 1, 4);

                    double whirlpoolAngle = SnapUtils.YawTowards(this.whirlpool.Translation, globals.Translation);
                    double staryuAngle = SnapUtils.YawTowards(this.Translation, globals.Translation);

                    if (System.Math.Abs(MathHelper.AngleDist(whirlpoolAngle, staryuAngle)) < System.Math.PI / 60)
                    {
                        this.MotionData.Destination = this.whirlpool.Translation;
                        this.relativeToPlayer = false;
                        this.MotionData.StoredValues[0] = 1000;
                        this.MotionData.Destination = new Vector3(
                            this.MotionData.Destination.x,
                            (float)(SnapUtils.GroundHeightAt(globals, this.whirlpool.Translation) + 1000),
                            this.MotionData.Destination.z
                        );
                        return MotionResult.Done;
                    }
                    break;

                default:
                    return base.CustomMotion(param, viewerInput, globals);
            }

            return MotionResult.Update;
        }

        protected override void StartBlock(LevelGlobals globals)
        {
            var state = this.Def.StateGraph.States[(int)this.CurrState];

            switch (state.StartAddress)
            {
                case 0x802CCCFC:
                case 0x802CCD80:
                    this.MotionData.StoredValues[0] = 3000;
                    break;

                case 0x802CCDDC:
                    this.MotionData.StoredValues[0] = -200;
                    break;

                case 0x802CD0B8:
                    if (this.CurrBlock == 0 && UnityEngine.Random.value < 0.5f)
                        this.CurrBlock = 1;
                    break;

                case 0x802CD4F4:
                    if (this.CurrBlock == 0)
                    {
                        this.MotionData.StoredValues[0] = -400;
                    }
                    else if (this.CurrBlock == 1 && EvolveCount <= 2)
                    {
                        globals.SendGlobalSignal(this, state.Blocks[1].Signals[EvolveCount++].Value);
                    }
                    break;
            }

            base.StartBlock(globals);
        }
    }

}
