using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class GfxRenderInstManager
    {
        public List<GfxRenderInst> TemplateStack = new List<GfxRenderInst>();
        public GfxRenderInstList CurrentList = null!;

        public GfxRenderCache GfxRenderCache;

        public GfxRenderInstManager(GfxRenderCache gfxRenderCache)
        {
            this.GfxRenderCache = gfxRenderCache;
        }

        public GfxRenderInst NewRenderInst()
        {
            var renderInst = new GfxRenderInst();
            if (this.TemplateStack.Count > 0)
                renderInst.CopyFrom(this.GetCurrentTemplate());
            return renderInst;
        }

        public void SubmitRenderInst(GfxRenderInst renderInst)
        {
            this.CurrentList.SubmitRenderInst(renderInst);
        }

        public void SetCurrentList(GfxRenderInstList list)
        {
            this.CurrentList = list;
        }

        public GfxRenderInst PushTemplate()
        {
            var newTemplate = new GfxRenderInst();
            if (this.TemplateStack.Count > 0)
                newTemplate.CopyFrom(this.GetCurrentTemplate());
            this.TemplateStack.Add(newTemplate);
            return newTemplate;
        }

        public void PopTemplate()
        {
            this.TemplateStack.RemoveAt(this.TemplateStack.Count - 1);
        }

        public GfxRenderInst GetCurrentTemplate()
        {
            return this.TemplateStack[this.TemplateStack.Count - 1];
        }
    }

}
