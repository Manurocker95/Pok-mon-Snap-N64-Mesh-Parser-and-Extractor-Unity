using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class DefaultGfxDevice : GfxDevice
    {
        public void BeginFrame()
        {
        
        }

        public void CheckForLeaks()
        {
   
        }

        public void CopySubTexture2D(GfxTexture dst, long dstX, long dstY, GfxTexture src, long srcX, long srcY)
        {
           
        }

        public GfxBindings CreateBindings(GfxBindingsDescriptor bindingsDescriptor)
        {
            return null;
        }

        public GfxBuffer CreateBuffer(long wordCount, GfxBufferUsage usage, GfxBufferFrequencyHint hint, VP_Uint8Array initialData = null)
        {
            return null;
        }

        public GfxComputePass CreateComputePass()
        {
            return null;
        }

        public GfxComputePipeline CreateComputePipeline(GfxComputePipelineDescriptor descriptor)
        {
            return null;
        }

        public GfxProgram CreateComputeProgram(GfxComputeProgramDescriptor descriptor)
        {
            return null;
        }

        public GfxInputLayout CreateInputLayout(GfxInputLayoutDescriptor inputLayoutDescriptor)
        {
            return null;
        }

        public GfxProgram CreateProgram(GfxRenderProgramDescriptor descriptor)
        {
            return null;
        }

        public GfxQueryPool CreateQueryPool(GfxQueryPoolType type, long elemCount)
        {
            return null;
        }

        public GfxReadback CreateReadback(long byteCount)
        {
            return null;
        }

        public GfxRenderPass CreateRenderPass(GfxRenderPassDescriptor renderPassDescriptor)
        {
            return null;
        }

        public GfxRenderPipeline CreateRenderPipeline(GfxRenderPipelineDescriptor descriptor)
        {
            return null;
        }

        public GfxRenderTarget CreateRenderTarget(GfxRenderTargetDescriptor descriptor)
        {
            return null;
        }

        public GfxRenderTarget CreateRenderTargetFromTexture(GfxTexture texture)
        {
            return null;
        }

        public GfxSampler CreateSampler(GfxSamplerDescriptor descriptor)
        {
            return null;
        }

        public GfxTexture CreateTexture(GfxTextureDescriptor descriptor)
        {
            return new GfxTexture()
            {

            };
        }

        public void DestroyBindings(GfxBindings o)
        {
           
        }

        public void DestroyBuffer(GfxBuffer o)
        {
           
        }

        public void DestroyComputePipeline(GfxComputePipeline o)
        {
           
        }

        public void DestroyInputLayout(GfxInputLayout o)
        {
           
        }

        public void DestroyProgram(GfxProgram o)
        {
           
        }

        public void DestroyQueryPool(GfxQueryPool o)
        {
           
        }

        public void DestroyReadback(GfxReadback o)
        {
           
        }

        public void DestroyRenderPipeline(GfxRenderPipeline o)
        {
           
        }

        public void DestroyRenderTarget(GfxRenderTarget o)
        {
           
        }

        public void DestroySampler(GfxSampler o)
        {
           
        }

        public void DestroyTexture(GfxTexture o)
        {
           
        }

        public void EndFrame()
        {
           
        }

        public void PipelineForceReady(GfxRenderPipeline o)
        {
           
        }

        public bool PipelineQueryReady(GfxRenderPipeline o)
        {
            return false;
        }

        public void ProgramPatched(GfxProgram o, GfxRenderProgramDescriptor descriptor)
        {
           
        }

        // Dummy values 
        public GfxDeviceLimits QueryLimits()
        {
            int uniformBufferOffsetAlignment = 256;
            int uniformBufferWordAlignment = uniformBufferOffsetAlignment / 4;
            int uniformBufferMaxPageByteSize = 4096;
            int uniformBufferMaxPageWordSize = uniformBufferMaxPageByteSize / 4;

            return new GfxDeviceLimits()
            {
                UniformBufferMaxPageWordSize = (int)uniformBufferMaxPageWordSize,
                UniformBufferWordAlignment = uniformBufferWordAlignment,

            };
        }

        public bool? QueryPoolResultOcclusion(GfxQueryPool o, long dstOffs)
        {
            return null;
        }

        public bool QueryReadbackFinished(VP_Uint32Array dst, long dstOffs, GfxReadback o)
        {
            return false;
        }

        public GfxRenderPassDescriptor QueryRenderPass(GfxRenderPass o)
        {
            return new GfxRenderPassDescriptor()
            {
                
            };
        }

        public GfxRenderTargetDescriptor QueryRenderTarget(GfxRenderTarget o)
        {
            return new GfxRenderTargetDescriptor()
            {

            };
        }

        public bool QueryTextureFormatSupported(GfxFormat format, long width, long height)
        {
            return false;
        }

        public GfxVendorInfo QueryVendorInfo()
        {
            return new GfxVendorInfo()
            {

            };
        }

        public void ReadBuffer(GfxReadback o, long dstOffset, GfxBuffer buffer, long srcOffset, long byteSize)
        {
           
        }

        public void ReadPixelFromTexture(GfxReadback o, long dstOffset, GfxTexture a, long x, long y)
        {
           
        }

        public void SetResourceLeakCheck(GfxResource o, bool v)
        {
           
        }

        public void SetResourceName(GfxResource o, string s)
        {
           
        }

        public void SetStatisticsGroup(GfxStatisticsGroup statisticsGroup)
        {
           
        }

        public void SubmitPass(IGfxPass o)
        {
           
        }

        public void SubmitReadback(GfxReadback o)
        {
           
        }

        public void UploadBufferData(GfxBuffer buffer, long dstByteOffset, VP_Uint8Array data, long? srcByteOffset = null, long? byteCount = null)
        {
           
        }

        public void UploadTextureData(GfxTexture texture, long firstMipLevel, List<VP_ArrayBufferView<VP_ArrayBuffer>> levelDatas)
        {
           
        }

        public void ZeroBuffer(GfxBuffer buffer, long dstByteOffset, long byteCount)
        {
           
        }
    }
}
