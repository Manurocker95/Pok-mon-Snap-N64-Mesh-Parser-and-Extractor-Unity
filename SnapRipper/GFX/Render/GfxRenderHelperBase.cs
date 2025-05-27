using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class GfxRenderHelperBase
    {
        public GfxRenderCache RenderCache { get; private set; }
        public IGfxrRenderGraph RenderGraph { get; private set; }
        public GfxRenderInstManager RenderInstManager { get; private set; }
        public GfxRenderDynamicUniformBuffer UniformBuffer { get; private set; }

        private GfxRenderCache _renderCacheOwn;

        public GfxRenderHelperBase(GfxDevice device, GfxRenderCache renderCache = null)
        {
            if (renderCache == null)
            {
                _renderCacheOwn = new GfxRenderCache(device);
                RenderCache = _renderCacheOwn;
            }
            else
            {
                RenderCache = renderCache;
            }

            RenderGraph = new GfxrRenderGraphImpl(device);
            RenderInstManager = new GfxRenderInstManager(RenderCache);
            UniformBuffer = new GfxRenderDynamicUniformBuffer(device);
        }

        public GfxRenderInst PushTemplateRenderInst()
        {
            var template = RenderInstManager.PushTemplate();
            template.SetUniformBuffer(UniformBuffer);
            return template;
        }

        public void PrepareToRender()
        {
            RenderCache.PrepareToRender();
            UniformBuffer.PrepareToRender();
        }

        public void Destroy()
        {
            if (_renderCacheOwn != null)
                _renderCacheOwn.Destroy();
            UniformBuffer.Destroy();
            RenderGraph.Destroy();      
        }
    }
}
