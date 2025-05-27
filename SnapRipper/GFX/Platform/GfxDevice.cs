using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public interface GfxDevice
    {
        GfxBuffer CreateBuffer(long wordCount, GfxBufferUsage usage, GfxBufferFrequencyHint hint, VP_Uint8Array initialData = null);
        GfxTexture CreateTexture(GfxTextureDescriptor descriptor);
        GfxSampler CreateSampler(GfxSamplerDescriptor descriptor);
        GfxRenderTarget CreateRenderTarget(GfxRenderTargetDescriptor descriptor);
        GfxRenderTarget CreateRenderTargetFromTexture(GfxTexture texture);
        GfxProgram CreateProgram(GfxRenderProgramDescriptor descriptor);
        GfxProgram CreateComputeProgram(GfxComputeProgramDescriptor descriptor);
        GfxBindings CreateBindings(GfxBindingsDescriptor bindingsDescriptor);
        GfxInputLayout CreateInputLayout(GfxInputLayoutDescriptor inputLayoutDescriptor);
        GfxComputePipeline CreateComputePipeline(GfxComputePipelineDescriptor descriptor);
        GfxRenderPipeline CreateRenderPipeline(GfxRenderPipelineDescriptor descriptor);
        GfxReadback CreateReadback(long byteCount);
        GfxQueryPool CreateQueryPool(GfxQueryPoolType type, long elemCount);

        void DestroyBuffer(GfxBuffer o);
        void DestroyTexture(GfxTexture o);
        void DestroySampler(GfxSampler o);
        void DestroyRenderTarget(GfxRenderTarget o);
        void DestroyProgram(GfxProgram o);
        void DestroyBindings(GfxBindings o);
        void DestroyInputLayout(GfxInputLayout o);
        void DestroyComputePipeline(GfxComputePipeline o);
        void DestroyRenderPipeline(GfxRenderPipeline o);
        void DestroyReadback(GfxReadback o);
        void DestroyQueryPool(GfxQueryPool o);

        bool PipelineQueryReady(GfxRenderPipeline o);
        void PipelineForceReady(GfxRenderPipeline o);

        GfxRenderPass CreateRenderPass(GfxRenderPassDescriptor renderPassDescriptor);
        GfxComputePass CreateComputePass();
        void SubmitPass(IGfxPass o);
        void BeginFrame();
        void EndFrame();

        void CopySubTexture2D(GfxTexture dst, long dstX, long dstY, GfxTexture src, long srcX, long srcY);

        void ZeroBuffer(GfxBuffer buffer, long dstByteOffset, long byteCount);
        void UploadBufferData(GfxBuffer buffer, long dstByteOffset, VP_Uint8Array data, long? srcByteOffset = null, long? byteCount = null);
        void UploadTextureData(GfxTexture texture, long firstMipLevel, List<VP_ArrayBufferView<VP_ArrayBuffer>> levelDatas);

        void ReadBuffer(GfxReadback o, long dstOffset, GfxBuffer buffer, long srcOffset, long byteSize);
        void ReadPixelFromTexture(GfxReadback o, long dstOffset, GfxTexture a, long x, long y);
        void SubmitReadback(GfxReadback o);
        bool QueryReadbackFinished(VP_Uint32Array dst, long dstOffs, GfxReadback o);

        bool? QueryPoolResultOcclusion(GfxQueryPool o, long dstOffs);

        GfxDeviceLimits QueryLimits();
        bool QueryTextureFormatSupported(GfxFormat format, long width, long height);
        GfxVendorInfo QueryVendorInfo();
        GfxRenderPassDescriptor QueryRenderPass(GfxRenderPass o);
        GfxRenderTargetDescriptor QueryRenderTarget(GfxRenderTarget o);

        void SetResourceName(GfxResource o, string s);
        void SetResourceLeakCheck(GfxResource o, bool v);
        void CheckForLeaks();
        void ProgramPatched(GfxProgram o, GfxRenderProgramDescriptor descriptor);
        void SetStatisticsGroup(GfxStatisticsGroup statisticsGroup);
    }

}
