using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class Vulpix : Actor
    {
        public Vulpix(RenderData renderData, ObjectSpawn spawn, ActorDef def, LevelGlobals globals, bool isEgg = false) : base(renderData, spawn, def, globals, isEgg)
        {
        }

        protected override MotionResult AuxStep(ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            switch (this.CurrAux)
            {
                // runs both of these at the same time, second one is actually for cleanup
                case 0x802D9DFC:
                case 0x802D9E7C:
                    if ((this.MotionData.StateFlags & (long)EndCondition.Misc) != 0)
                        return MotionResult.Done;

                    AnimationUtils.GetPathPoint(ref MotionData.Destination, GfxPlatformUtils.AssertExists(this.Spawn.Path), 1);
                    Vector3 deltaScratch = this.MotionData.Destination - this.Translation;
                    if (System.Math.Sqrt(deltaScratch.x * deltaScratch.x + deltaScratch.z * deltaScratch.z) < 1000)
                    {
                        this.ReceiveSignal(this, 0x2C, globals);
                        return MotionResult.Done;
                    }
                    break;
            }

            return MotionResult.None;
        }
    }
}
