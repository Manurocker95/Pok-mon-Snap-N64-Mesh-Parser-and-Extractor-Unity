using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class ModelRenderer : Target
    {
        public bool Visible = true;
        public long ID;
        // Run logic but don't render
        public bool Hidden = false;
        public bool AnimationPaused = false;

        public Matrix4x4 ModelMatrix = Matrix4x4.identity;
        public List<NodeRenderer> Renderers = new List<NodeRenderer>();
        public AdjustableAnimationController AnimationController = new AdjustableAnimationController(30);
        public AdjustableAnimationController MaterialController;
        public int CurrAnimation = -1;
        public int HeadAnimationIndex = -1;

        public bool IsSkybox;
        RenderData renderData;
        public List<GFXNode> Nodes;
        public List<AnimationData> Animations;

        public RSPSharedOutput SharedOutput => renderData.SharedOutput;

        public ModelRenderer(RenderData renderData, List<GFXNode> nodes, List<AnimationData> animations, bool isSkybox = false, bool isEgg = false, long id = -1)
        {
            ID = id;
            this.renderData = renderData;
            Nodes = nodes;
            Animations = animations;
            IsSkybox = isSkybox;

            for (int i = 0; i < nodes.Count; i++)
            {
                var parent = nodes[i].Parent;
                if (parent == -1)
                {
                    Renderers.Add(new NodeRenderer(renderData, nodes[i], ModelMatrix, isSkybox));
                }
                else
                {
                    var nodeRenderer = new NodeRenderer(renderData, nodes[i], Renderers[(int)parent].ModelMatrix, isSkybox, isEgg && i == 1);
                    Renderers.Add(nodeRenderer);
                    Renderers[(int)parent].Children.Add(Renderers[i]);
                }
            }
        }

        public void DecomposeModelMatrix(out Vector3 pos, out Quaternion rot, out Vector3 scale)
        {
            DecomposeMatrix(ModelMatrix, out pos, out rot, out scale);
        }

        public void DecomposeMatrix(Matrix4x4 m, out Vector3 pos, out Quaternion rot, out Vector3 scale)
        {
            pos = m.GetColumn(3);

            scale = new Vector3(
                m.GetColumn(0).magnitude,
                m.GetColumn(1).magnitude,
                m.GetColumn(2).magnitude
            );

            Matrix4x4 rotMatrix = m;
            rotMatrix.SetColumn(0, m.GetColumn(0).normalized);
            rotMatrix.SetColumn(1, m.GetColumn(1).normalized);
            rotMatrix.SetColumn(2, m.GetColumn(2).normalized);

            rot = Quaternion.LookRotation(
                rotMatrix.GetColumn(2),
                rotMatrix.GetColumn(1)
            );
        }

        public virtual List<GfxBindingLayoutDescriptor> BindingLayouts()
        {
            return new List<GfxBindingLayoutDescriptor>() { new GfxBindingLayoutDescriptor() { NumUniformBuffers = 2, NumSamplers = 1 } };
        }

        public void SetBackfaceCullingEnabled(bool value)
        {
            foreach (var renderer in Renderers)
            {
                foreach (var drawCall in renderer.DrawCalls)
                {
                    drawCall.setBackfaceCullingEnabled(value);
                }
            }
        }

        public void SetVertexColorsEnabled(bool value)
        {
            foreach (var renderer in Renderers)
            {
                foreach (var drawCall in renderer.DrawCalls)
                {
                    drawCall.setVertexColorsEnabled(value);
                }
            }
        }

        public void SetTexturesEnabled(bool value)
        {
            foreach (var renderer in Renderers)
            {
                foreach (var drawCall in renderer.DrawCalls)
                {
                    drawCall.setTexturesEnabled(value);
                }
            }
        }

        public void SetMonochromeVertexColorsEnabled(bool value)
        {
            foreach (var renderer in Renderers)
            {
                foreach (var drawCall in renderer.DrawCalls)
                {
                    drawCall.setMonochromeVertexColorsEnabled(value);
                }
            }
        }

        public void SetAlphaVisualizerEnabled(bool value)
        {
            foreach (var renderer in Renderers)
            {
                foreach (var drawCall in renderer.DrawCalls)
                {
                    drawCall.setAlphaVisualizerEnabled(value);
                }
            }
        }

        public void ForceLoop()
        {
            foreach (var renderer in Renderers)
            {
                renderer.Animator.ForceLoop = true;
                foreach (var material in renderer.Materials)
                {
                    material.ForceLoop();
                }
            }
        }

        public virtual void SetAnimation(int index)
        {
            if (Animations.Count <= index)
                return;

            CurrAnimation = index;
            AnimationController.Init(Animations[index].FPS);
            var newAnim = Animations[index];
            HeadAnimationIndex = newAnim.Tracks.FindIndex(t => t != null);

            for (int i = 0; i < Renderers.Count; i++)
            {
                if (newAnim.Tracks.Count <= i)
                {
                    Debug.LogError("Model Renderer: "+this.ID + " has no tracks");
                    Debug.LogError(newAnim.Tracks.Count);
                    continue;
                }

                Renderers[i].Animator.SetTrack(newAnim.Tracks[i]);
                Renderers[i].SetTransfromFromNode();

                if (newAnim.MaterialTracks.Count == 0 || newAnim.MaterialTracks[i].Count == 0)
                {
                    foreach (var mat in Renderers[i].Materials)
                        mat.SetTrack(null);
                }
                else
                {
                    for (int j = 0; j < Renderers[i].Materials.Count; j++)
                        Renderers[i].Materials[j].SetTrack(newAnim.MaterialTracks[i][j]);
                }

                // Force matrix update
                Renderers[i].Animate(0);
                Renderers[i].ModelMatrix = Renderers[i].Parent * Renderers[i].Transform;
            }
        }

        protected virtual void Animate(LevelGlobals globals)
        {
            if (AnimationPaused)
                return;

            float time = AnimationController.GetTimeInFrames();
            float matTime = MaterialController != null ? MaterialController.GetTimeInFrames() : time;

            for (int i = 0; i < Renderers.Count; i++)
            {
                Renderers[i].Animate(time, matTime);

                if (Renderers[i].Animator.LastFunction >= 0 && i > 0)
                {
                    var arg = Renderers[i].Animator.LastFunction;
                    var category = arg >> 8;
                    var index = (arg & 0xFF) - 1;

                    if (index >= 0)
                        globals.Particles.CreateEmitter(category == 3, (int)index, Renderers[i].ModelMatrix);

                    Renderers[i].Animator.LastFunction = -1;
                }
            }
        }

        protected virtual void Motion(ViewerRenderInput viewerInput, LevelGlobals globals) { }

        public virtual void PreviewPrepareToRender( LevelGlobals globals, ViewerRenderInput viewerInput = null)
        {
            AnimationController.SetTimeFromViewerInput(viewerInput);
            if (MaterialController != null)
                MaterialController.SetTimeFromViewerInput(viewerInput);

            Motion(viewerInput, globals);
            Animate(globals);
        }

        public virtual void PrepareToRender(GfxDevice device, GfxRenderInstManager renderInstManager, ViewerRenderInput viewerInput, LevelGlobals globals, bool _updateBuffers = false)
        {
            if (!Visible)
                return;

            AnimationController.SetTimeFromViewerInput(viewerInput);
            if (MaterialController != null)
                MaterialController.SetTimeFromViewerInput(viewerInput);

            Motion(viewerInput, globals);
            Animate(globals);

            if (Hidden || !_updateBuffers)
                return;

            var template = renderInstManager.PushTemplate();
            template.SetBindingLayouts(BindingLayouts());
            template.SetVertexInput(this.renderData.InputLayout, renderData.VertexBufferDescriptors, renderData.IndexBufferDescriptor);

            long offs = template.AllocateUniformBuffer((int)F3DEX_Program.Ub_SceneParams, 16 + 2 * 4);
            var mappedF32 = template.MapUniformBufferF32(F3DEX_Program.Ub_SceneParams);
            offs += GfxBufferHelpers.FillMatrix4x4(mappedF32, offs, viewerInput.Camera.projectionMatrix);

            Vector3 modelTranslation = ModelMatrix.GetColumn(3);
            Vector3 lookat = viewerInput.Camera.cameraToWorldMatrix.MultiplyPoint(modelTranslation);

            Matrix4x4 lookAtMatrix = Matrix4x4.LookAt(Vector3.zero, lookat, Vector3.up);
            offs += GfxBufferHelpers.FillVec4(mappedF32, offs, lookAtMatrix.m00, lookAtMatrix.m10, lookAtMatrix.m20);
            offs += GfxBufferHelpers.FillVec4(mappedF32, offs, lookAtMatrix.m01, lookAtMatrix.m11, lookAtMatrix.m21);

            renderInstManager.SetCurrentList(IsSkybox ? globals.RenderInstListSky : globals.RenderInstListMain);
            Renderers[0].PrepareToRender(device, renderInstManager, viewerInput);

            renderInstManager.PopTemplate();
        }

    }
}
