using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class Splash : ModelRenderer
    {
        private static readonly Vector3 ScaleScratch = Vector3.zero;

        public Splash(RenderData renderData, List<GFXNode> nodes, List<AnimationData> animations, SplashType type, Vector3 baseScale)
            : base(renderData, nodes, animations)
        {
            this.Type = type;
            this.BaseScale = baseScale;
            this.Visible = false;
        }

        public SplashType Type { get; }
        private Vector3 BaseScale { get; }

        public bool TryStart(Vector3 pos, Vector3 scale, LevelGlobals globals)
        {
            if (this.Visible)
                return false;

            this.Visible = true;
            Vector3 scaled = Vector3.Scale(this.BaseScale, scale);
            this.ModelMatrix = Matrix4x4.Scale(scaled);
            this.ModelMatrix[12] = pos.x;
            this.ModelMatrix[13] = (float)SnapUtils.GroundHeightAt(globals, pos);
            this.ModelMatrix[14] = pos.z;

            this.SetAnimation(0);
            this.Renderers[this.HeadAnimationIndex].Animator.LoopCount = 0;
            return true;
        }

        protected override void Motion(ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            if (this.Renderers[this.HeadAnimationIndex].Animator.LoopCount >= 1)
                this.Visible = false;
        }
    }

}
