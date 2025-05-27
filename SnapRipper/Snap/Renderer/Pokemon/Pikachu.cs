using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class Pikachu : Actor
    {
        public static long CurrDiglett = 0;
        public static long TargetDiglett = 1;

        public Pikachu(RenderData renderData, ObjectSpawn spawn, ActorDef def, LevelGlobals globals, bool isEgg = false) : base(renderData, spawn, def, globals, isEgg)
        {
        }

        protected override MotionResult AuxStep(ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            switch (this.CurrAux)
            {
                case 0x802CB814:
                    AnimationUtils.GetPathPoint(ref this.MotionData.Destination, GfxPlatformUtils.AssertExists(this.Spawn.Path), 1);
                    Vector3 deltaScratch = this.MotionData.Destination - this.Translation;
                    if (System.Math.Sqrt(deltaScratch.x * deltaScratch.x + deltaScratch.z * deltaScratch.z) < 475)
                    {
                        this.ReceiveSignal(this, 0x23, globals);
                        return MotionResult.Done;
                    }
                    break;

                case 0x802E7CA4:
                    if (!SnapUtils.CanHearSong(this.Translation, globals))
                    {
                        this.MotionData.StateFlags |= (long)EndCondition.Misc;
                        return MotionResult.Done;
                    }
                    break;
            }

            return MotionResult.None;
        }

        protected override void StartBlock(LevelGlobals globals)
        {
            var state = this.Def.StateGraph.States[(int)this.CurrState];

            switch (state.StartAddress)
            {
                case 0x802E8290:
                    if ((CurrDiglett & TargetDiglett) != 0)
                    {
                        this.FollowEdge(state.Blocks[0].Edges[0], globals);
                        return;
                    }
                    TargetDiglett <<= 1;
                    var pathStart = this.MotionData.StoredValues[0];
                    this.MotionData.StoredValues[1] = pathStart + (pathStart < 3 ? 1 : 2);
                    break;

                case 0x802E7D04:
                case 0x802E7E5C:
                    if (this.CurrBlock == 0)
                        this.Target = globals.AllActors.FirstOrDefault(a => a.Def.ID == 145);
                    break;
            }

            base.StartBlock(globals);
        }

        protected override bool EndBlock(long address, LevelGlobals globals)
        {
            var state = this.Def.StateGraph.States[(int)this.CurrState];

            switch (address)
            {
                case 0x802E8330:
                    this.MotionData.StoredValues[0] = this.MotionData.StoredValues[1];
                    break;

                case 0x802E7B3C:
                    var egg = globals.AllActors.FirstOrDefault(a => a.Def.ID == 602);
                    if (egg != null && egg.Visible && Vector3.Distance(egg.Translation, this.Translation) < 600)
                        return this.FollowEdge(state.Blocks[0].Edges[1], globals);
                    break;
            }

            return false;
        }
    }

}
