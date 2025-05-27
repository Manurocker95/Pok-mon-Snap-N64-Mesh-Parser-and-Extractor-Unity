using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class Slowpoke : Actor
    {
        public Slowpoke(RenderData renderData, ObjectSpawn spawn, ActorDef def, LevelGlobals globals, bool isEgg = false) : base(renderData, spawn, def, globals, isEgg)
        {
        }

        protected override MotionResult AuxStep(ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            if (this.CurrAux == 0x802D9A58)
            {
                AnimationUtils.GetPathPoint(ref MotionData.Destination, GfxPlatformUtils.AssertExists(this.MotionData.Path), 1);

                if (Vector3.Distance(this.MotionData.Destination, this.Translation) < 475)
                {
                    if (this.ReceiveSignal(this, 0x1C, globals))
                        return MotionResult.Done;
                }
            }

            return MotionResult.None;
        }

        protected override MotionResult CustomMotion(long param, ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            Vector3 actorScratch = Vector3.zero;
            AnimationUtils.GetPathTangent(ref actorScratch, GfxPlatformUtils.AssertExists(this.MotionData.Path), 1);

            double targetYaw = System.Math.Atan2(actorScratch.x, actorScratch.z) + System.Math.PI;

            if (SnapUtils.StepYawTowards(this.Euler, targetYaw, System.Math.PI / 90, viewerInput.DeltaTime / 1000.0))
                return MotionResult.Done;

            return MotionResult.Update;
        }
    }

}
