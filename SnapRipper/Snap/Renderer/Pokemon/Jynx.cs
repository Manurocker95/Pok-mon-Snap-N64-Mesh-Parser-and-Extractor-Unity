using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class Jynx : Actor
    {
        private static double BaseOffset = -53.25;

        public Jynx(RenderData renderData, ObjectSpawn spawn, ActorDef def, LevelGlobals globals, bool isEgg = false) : base(renderData, spawn, def, globals, isEgg)
        {
        }

        protected override void StartBlock(LevelGlobals globals)
        {
            switch (this.Def.StateGraph.States[(int)this.CurrState].StartAddress)
            {
                case 0x802C4EF4:
                    this.MotionData.StoredValues[0] = this.Euler.y;
                    this.MotionData.StoredValues[1] = BaseOffset;
                    this.Translation = new Vector3(
                        this.Translation.x,
                        (float)(SnapUtils.GroundHeightAt(globals, this.Translation) + BaseOffset),
                        this.Translation.z
                    );
                    this.UpdatePositions();
                    break;
            }

            base.StartBlock(globals);
        }

        protected override MotionResult CustomMotion(long param, ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            double delta = BaseOffset * viewerInput.DeltaTime / 1000.0;

            if (param == 0)
            {
                if (this.MotionData.StoredValues[1] == 0)
                    return MotionResult.Done;

                this.MotionData.StoredValues[1] -= delta;
                if (this.MotionData.StoredValues[1] > 0)
                    this.MotionData.StoredValues[1] = 0;

                this.Translation = new Vector3(
                    this.Translation.x,
                    (float)(SnapUtils.GroundHeightAt(globals, this.Translation) + this.MotionData.StoredValues[1]),
                    this.Translation.z
                );

                this.Euler = new Vector3(
                    this.Euler.x,
                    (float)(this.Euler.y + MathConstants.Tau * viewerInput.DeltaTime / 1000.0),
                    this.Euler.z
                );

                if (this.Euler.y >= this.MotionData.StoredValues[0] + MathConstants.Tau)
                    this.Euler = new Vector3(this.Euler.x, (float)(this.MotionData.StoredValues[0] + MathConstants.Tau), this.Euler.z);

                return MotionResult.Update;
            }
            else
            {
                if (this.MotionData.StoredValues[1] == BaseOffset)
                    return MotionResult.Done;

                this.MotionData.StoredValues[1] += delta;
                if (this.MotionData.StoredValues[1] < BaseOffset)
                    this.MotionData.StoredValues[1] = BaseOffset;

                this.Translation = new Vector3(
                    this.Translation.x,
                    (float)(SnapUtils.GroundHeightAt(globals, this.Translation) + this.MotionData.StoredValues[1]),
                    this.Translation.z
                );

                this.Euler = new Vector3(
                    this.Euler.x,
                    (float)(this.Euler.y - MathConstants.Tau * viewerInput.DeltaTime / 1000.0),
                    this.Euler.z
                );

                if (this.Euler.y < this.MotionData.StoredValues[0])
                    this.Euler = new Vector3(this.Euler.x, (float)this.MotionData.StoredValues[0], this.Euler.z);

                return MotionResult.Update;
            }
        }
    }

}
