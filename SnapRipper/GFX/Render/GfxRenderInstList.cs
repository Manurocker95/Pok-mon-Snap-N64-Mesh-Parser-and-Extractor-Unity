using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public delegate int GfxRenderInstCompareFunc(GfxRenderInst a, GfxRenderInst b);

    public class GfxRenderInstList
    {
        public List<GfxRenderInst> RenderInsts = new List<GfxRenderInst>();

        public GfxRenderInstCompareFunc CompareFunction;
        public GfxRenderInstExecutionOrder ExecutionOrder;

        public GfxRenderInstList(
            GfxRenderInstCompareFunc compareFunction = null,
            GfxRenderInstExecutionOrder executionOrder = GfxRenderInstExecutionOrder.Forwards
        )
        {
            this.CompareFunction = compareFunction ?? GfxUtils.GfxRenderInstCompareSortKey;
            this.ExecutionOrder = executionOrder;
        }

        public void SubmitRenderInst(GfxRenderInst renderInst)
        {
            renderInst.Validate();
            this.RenderInsts.Add(renderInst);
        }

        public bool HasLateSamplerBinding(string name)
        {
            for (int i = 0; i < this.RenderInsts.Count; i++)
            {
                if (this.RenderInsts[i].HasLateSamplerBinding(name))
                    return true;
            }
            return false;
        }

        public void ResolveLateSamplerBinding(string name, GfxSamplerBinding binding)
        {
            for (int i = 0; i < this.RenderInsts.Count; i++)
                this.RenderInsts[i].ResolveLateSamplerBinding(name, binding);
        }

        public void EnsureSorted()
        {
            if (this.CompareFunction != null && this.RenderInsts.Count != 0)
                this.RenderInsts.Sort(new Comparison<GfxRenderInst>(this.CompareFunction));
        }

        private void DrawOnPassRendererNoReset(GfxRenderCache cache, GfxRenderPass passRenderer)
        {
            this.EnsureSorted();

            if (this.ExecutionOrder == GfxRenderInstExecutionOrder.Forwards)
            {
                for (int i = 0; i < this.RenderInsts.Count; i++)
                    this.RenderInsts[i].DrawOnPass(cache, passRenderer);
            }
            else
            {
                for (int i = this.RenderInsts.Count - 1; i >= 0; i--)
                    this.RenderInsts[i].DrawOnPass(cache, passRenderer);
            }
        }

        public void Reset()
        {
            this.RenderInsts.Clear();
        }

        public void DrawOnPassRenderer(GfxRenderCache cache, GfxRenderPass passRenderer)
        {
            this.DrawOnPassRendererNoReset(cache, passRenderer);
            this.Reset();
        }
    }

}
