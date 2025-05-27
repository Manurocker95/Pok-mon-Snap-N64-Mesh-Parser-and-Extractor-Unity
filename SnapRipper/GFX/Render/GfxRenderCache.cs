using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class GfxRenderCache
    {
        private HashMap<GfxBindingsDescriptor, ExpiryBindings> gfxBindingsCache = new HashMap<GfxBindingsDescriptor, ExpiryBindings>(GfxPlatformObjUtils.GfxBindingsDescriptorEquals, GfxRenderCacheUtils.GfxBindingsDescriptorHash);
        private HashMap<GfxRenderPipelineDescriptor, GfxRenderPipeline> gfxRenderPipelinesCache = new HashMap<GfxRenderPipelineDescriptor, GfxRenderPipeline>(GfxPlatformObjUtils.GfxRenderPipelineDescriptorEquals, GfxRenderCacheUtils.GfxRenderPipelineDescriptorHash);
        private HashMap<GfxInputLayoutDescriptor, GfxInputLayout> gfxInputLayoutsCache = new HashMap<GfxInputLayoutDescriptor, GfxInputLayout>(GfxPlatformObjUtils.GfxInputLayoutDescriptorEquals, N64Utils.NullHashFunc);
        private HashMap<GfxRenderProgramDescriptor, GfxProgram> gfxProgramCache = new HashMap<GfxRenderProgramDescriptor, GfxProgram>(GfxRenderCacheUtils.GfxProgramDescriptorEquals, N64Utils.NullHashFunc);
        private HashMap<GfxSamplerDescriptor, GfxSampler> gfxSamplerCache = new HashMap<GfxSamplerDescriptor, GfxSampler>(GfxPlatformObjUtils.GfxSamplerDescriptorEquals, N64Utils.NullHashFunc);

        public GfxDevice device;

        public GfxRenderCache(GfxDevice device)
        {
            this.device = device;
        }

        public GfxBindings CreateBindings(GfxBindingsDescriptor descriptor)
        {
            var bindings = this.gfxBindingsCache.Get(descriptor);
            if (bindings == null)
            {
                var descriptorCopy = GfxRenderCacheUtils.GfxBindingsDescriptorCopy(descriptor);
                bindings = (ExpiryBindings)this.device.CreateBindings(descriptorCopy);
                this.gfxBindingsCache.Add(descriptorCopy, bindings);
            }
            bindings.ExpireFrameNum = 4;
            return bindings;
        }

        public GfxRenderPipeline CreateRenderPipeline(GfxRenderPipelineDescriptor descriptor)
        {
            var renderPipeline = this.gfxRenderPipelinesCache.Get(descriptor);
            if (renderPipeline == null)
            {
                var descriptorCopy = GfxRenderCacheUtils.GfxRenderPipelineDescriptorCopy(descriptor);
                renderPipeline = this.device.CreateRenderPipeline(descriptorCopy);
                this.gfxRenderPipelinesCache.Add(descriptorCopy, renderPipeline);
            }
            return renderPipeline;
        }

        public GfxInputLayout CreateInputLayout(GfxInputLayoutDescriptor descriptor)
        {
            var inputLayout = this.gfxInputLayoutsCache.Get(descriptor);
            if (inputLayout == null)
            {
                var descriptorCopy = GfxRenderCacheUtils.GfxInputLayoutDescriptorCopy(descriptor);
                inputLayout = this.device.CreateInputLayout(descriptorCopy);
                this.gfxInputLayoutsCache.Add(descriptorCopy, inputLayout);
            }
            return inputLayout;
        }

        public GfxProgram CreateProgramSimple(GfxRenderProgramDescriptor descriptor)
        {
            var program = this.gfxProgramCache.Get(descriptor);
            if (program == null)
            {
                var descriptorCopy = GfxRenderCacheUtils.GfxProgramDescriptorCopy(descriptor);
                program = this.device.CreateProgram(descriptorCopy);
                this.gfxProgramCache.Add(descriptorCopy, program);


                if (descriptor is GfxProgramDescriptorPreproc p)
                {
                    p.Associate(this.device, program);
                    ((dynamic)descriptorCopy).orig = p;
                }
            }

            return program;
        }

        public GfxProgram CreateProgram(GfxRenderProgramDescriptor descriptor)
        {
            // TODO (jstpierre): Remove the ensurePreprocessed here... this should be done by higher-level code.
            var p = (GfxProgramDescriptorPreproc)descriptor;
            p.EnsurePreprocessed(this.device.QueryVendorInfo());
            return this.CreateProgramSimple(descriptor);
        }

        public GfxSampler CreateSampler(GfxSamplerDescriptor descriptor)
        {
            var sampler = this.gfxSamplerCache.Get(descriptor);
            if (sampler == null)
            {
                sampler = this.device.CreateSampler(descriptor);
                this.gfxSamplerCache.Add(descriptor, sampler);
            }
            return sampler;
        }

        public long NumBindings()
        {
            return this.gfxBindingsCache.Size();
        }

        public void PrepareToRender()
        {
            foreach (var pair in this.gfxBindingsCache.Items())
            {
                var key = pair.Key;
                var value = pair.Value;
                if (--value.ExpireFrameNum <= 0)
                {
                    this.gfxBindingsCache.Delete(key);
                    this.device.DestroyBindings(value);
                }
            }
        }

        public void Destroy()
        {
            foreach (var bindings in this.gfxBindingsCache.Values())
                this.device.DestroyBindings(bindings);
            foreach (var renderPipeline in this.gfxRenderPipelinesCache.Values())
                this.device.DestroyRenderPipeline(renderPipeline);
            foreach (var inputLayout in this.gfxInputLayoutsCache.Values())
                this.device.DestroyInputLayout(inputLayout);
            foreach (var program in this.gfxProgramCache.Values())
                this.device.DestroyProgram(program);
            foreach (var sampler in this.gfxSamplerCache.Values())
                this.device.DestroySampler(sampler);

            this.gfxBindingsCache.Clear();
            this.gfxRenderPipelinesCache.Clear();
            this.gfxInputLayoutsCache.Clear();
            this.gfxProgramCache.Clear();
            this.gfxSamplerCache.Clear();
        }
    }
}
