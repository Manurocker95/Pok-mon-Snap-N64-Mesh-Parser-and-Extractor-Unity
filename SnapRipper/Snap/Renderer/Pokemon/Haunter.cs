using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class Haunter : Actor
    {
        public ModelRenderer FullModel;
        private double timer = 2000;

        public Haunter(RenderData renderData, ObjectSpawn spawn, ActorDef def, LevelGlobals globals, bool isEgg = false) : base(renderData, spawn, def, globals, isEgg)
        {
        }

        public override void PrepareToRender(GfxDevice device, GfxRenderInstManager renderInstManager, ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            if (!this.Visible)
                return;
            if (viewerInput.Time > this.timer)
            {
                // every few seconds, flash the full model
                if (this.Hidden)
                    this.timer = viewerInput.Time + 2000 + 2000 * UnityEngine.Random.value;
                else
                    this.timer = viewerInput.Time + 200 + 300 * UnityEngine.Random.value;

                this.Hidden = !this.Hidden;
                this.FullModel.Visible = this.Hidden;
            }

            base.PrepareToRender(device, renderInstManager, viewerInput, globals);

            if (this.Hidden)
            {
                this.FullModel.ModelMatrix = this.ModelMatrix;

                for (int i = 0; i < this.Renderers.Count; i++)
                {
                    this.FullModel.Renderers[i].Transform = this.Renderers[i].Transform;
                }
            }
        }
    }
}
