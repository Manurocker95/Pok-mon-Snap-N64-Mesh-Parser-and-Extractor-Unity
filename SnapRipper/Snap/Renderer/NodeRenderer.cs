using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class NodeRenderer
    {
        private bool visible = true;
        public Matrix4x4 ModelMatrix = Matrix4x4.identity;
        public Matrix4x4 Transform = Matrix4x4.identity;

        public List<NodeRenderer> Children = new List<NodeRenderer>();
        public List<DrawCallInstance> DrawCalls = new List<DrawCallInstance>();

        public Vector3 Translation = Vector3.zero;
        public Vector3 Euler = Vector3.zero;
        public Vector3 Scale = Vector3.one;

        public CRGAnimator Animator = new CRGAnimator();
        public List<CRGMaterial> Materials = new List<CRGMaterial>();

        private GFXNode node;
        public Matrix4x4 Parent;
        public bool IsSkybox;

        public NodeRenderer(RenderData renderData, GFXNode node, Matrix4x4 parent, bool isSkybox = false, bool isEgg = false)
        {
            this.node = node;
            this.Parent = parent;
            this.IsSkybox = isSkybox;
            
            this.SetTransfromFromNode();
            Multiply(ref this.ModelMatrix, this.Parent, this.Transform);

            var drawMatrices = new List<Matrix4x4> { this.ModelMatrix, parent };

            for (int i = 0; i < node.Materials.Count; i++)
                this.Materials.Add(new CRGMaterial(node.Materials[i], renderData.Textures));

            if (node.Model != null && node.Model.RSPOutput != null)
            {
                for (int i = 0; i < node.Model.RSPOutput.DrawCalls.Count; i++)
                {
                    DrawCallInstance dc;
                    if (isEgg)
                        dc = new EggDrawCall(renderData, node.Model.RSPOutput.DrawCalls[i], drawMatrices, this.node.Billboard, this.Materials);
                    else
                        dc = new DrawCallInstance(renderData, node.Model.RSPOutput.DrawCalls[i], drawMatrices, this.node.Billboard, this.Materials);
                    this.DrawCalls.Add(dc);
                }
            }
        }

        public void SetTransfromFromNode()
        {
            this.Translation = node.Translation;
            this.Euler = node.Euler;
            this.Scale = node.Scale;
            RendererUtils.BuildTransform(ref Transform, this.Translation, this.Euler, this.Scale);
        }

        public void Animate(double time, double matTime = -1)
        {
            if (matTime < 0)
                matTime = time;

            if (this.Animator.Update(time))
            {
                for (int i = (int)ModelField.Pitch; i <= (int)ModelField.ScaleZ; i++)
                {
                    if (this.Animator.Interpolators[i].op == AObjOP.NOP)
                        continue;

                    double value = this.Animator.Compute(i, time);
                    switch ((ModelField)i)
                    {
                        case ModelField.Pitch:
                            this.Euler.x = (float)value;
                            break;
                        case ModelField.Yaw:
                            this.Euler.y = (float)value;
                            break;
                        case ModelField.Roll:
                            this.Euler.z = (float)value;
                            break;
                        case ModelField.Path:
                            {
                                var path = VP_BYMLUtils.AssertExists(this.Animator.Interpolators[i].path);
                                AnimationUtils.GetPathPoint(ref this.Translation, path, (float)MathHelper.Clamp(value, 0.0, 1.0), true);
                                break;
                            }
                        case ModelField.X:
                            this.Translation.x = (float)value;
                            break;
                        case ModelField.Y:
                            this.Translation.y = (float)value;
                            break;
                        case ModelField.Z:
                            this.Translation.z = (float)value;
                            break;
                        case ModelField.ScaleX:
                            this.Scale.x = (float)value;
                            break;
                        case ModelField.ScaleY:
                            this.Scale.y = (float)value;
                            break;
                        case ModelField.ScaleZ:
                            this.Scale.z = (float)value;
                            break;
                    }
                }

                RendererUtils.BuildTransform(ref this.Transform, this.Translation, this.Euler, this.Scale);
            }

            for (int i = 0; i < this.Materials.Count; i++)
                this.Materials[i].Update(matTime);
        }
        public void Multiply(ref Matrix4x4 dst, Matrix4x4 a, Matrix4x4 b)
        {
            dst = a * b;
        }

        public void PrepareToRender(GfxDevice device, GfxRenderInstManager renderInstManager, ViewerRenderInput viewerInput)
        {
            if (!this.visible || (this.Animator.StateFlags & 2) != 0)
                return;

            Multiply(ref this.ModelMatrix, this.Parent, this.Transform);

            // Hide flag just skips this node's draw calls, doesn't affect matrix or children
            if ((this.Animator.StateFlags & 1) == 0)
            {
                for (int i = 0; i < this.DrawCalls.Count; i++)
                    this.DrawCalls[i].PrepareToRender(device, renderInstManager, viewerInput, this.IsSkybox);
            }

            for (int i = 0; i < this.Children.Count; i++)
                this.Children[i].PrepareToRender(device, renderInstManager, viewerInput);
        }

    }

}
