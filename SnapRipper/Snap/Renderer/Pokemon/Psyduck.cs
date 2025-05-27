using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class Psyduck : Actor
    {
        private int pathIndex = 0;
        private double oldOffset = 0;

        public Psyduck(RenderData renderData, ObjectSpawn spawn, ActorDef def, LevelGlobals globals, bool isEgg = false) : base(renderData, spawn, def, globals, isEgg)
        {
        }

        protected override void StartBlock(LevelGlobals globals)
        {
            var state = this.Def.StateGraph.States[(int)this.CurrState];

            if (state.StartAddress == 0x802DB93C)
            {
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
            }

            base.StartBlock(globals);
        }

        protected override MotionResult AuxStep(ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            if (this.CurrAux == 0x802DB630)
            {
                if (this.MotionData.AuxStart < 0)
                    this.MotionData.AuxStart = viewerInput.Time;

                double newOffset = 7 * System.Math.Sin((viewerInput.Time - this.MotionData.AuxStart) / 1000.0 * System.Math.PI * 4 / 3);
                this.Translation = new Vector3(
                    this.Translation.x,
                    this.Translation.y + (float)(newOffset - this.oldOffset),
                    this.Translation.z
                );
                this.oldOffset = newOffset;

                return MotionResult.Update;
            }

            return MotionResult.None;
        }

        protected override bool EndBlock(long address, LevelGlobals globals)
        {
            if (address == 0x802DB78C)
            {
                globals.FishTracker = 2;
            }
            else if (address == 0x802DB93C && this.CurrBlock == 1)
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
