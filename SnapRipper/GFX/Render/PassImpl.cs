using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class PassImpl : IGfxrPass, IGfxrComputePass
    {
        public string DebugName { get; private set; } = string.Empty;

        public List<int> AttachmentRenderTargetIDs { get; } = new();
        public List<GfxTexture> AttachmentTextures { get; } = new();
        public List<GfxrAttachmentClearDescriptor> AttachmentClearDescriptors { get; } = new();
        public List<GfxRenderAttachmentView> AttachmentViews { get; } = new();

        public List<int> ResolveOutputIDs { get; } = new();
        public List<GfxTexture> ResolveOutputExternalTextures { get; } = new();
        public List<GfxRenderAttachmentView> ResolveOutputExternalTextureViews { get; } = new();

        public List<int> ResolveTextureInputIDs { get; } = new();
        public List<bool> RenderTargetExtraRefs { get; } = new();

        public List<GfxTexture> ResolveTextureInputTextures { get; } = new();

        public List<RenderTarget> RenderTargets { get; } = new();

        public GfxRenderPassDescriptor Descriptor { get; } = new GfxRenderPassDescriptor();

        public float ViewportX { get; private set; } = 0;
        public float ViewportY { get; private set; } = 0;
        public float ViewportW { get; private set; } = 1;
        public float ViewportH { get; private set; } = 1;

        private PassExecFunc _execFunc;
        private ComputePassExecFunc _computeExecFunc;
        private PassPostFunc _postFunc;

        public string PassType { get; }

        public PassImpl(string passType)
        {
            PassType = passType;
        }

        public void SetDebugName(string debugName)
        {
            DebugName = debugName;
        }

        public void SetViewport(float x, float y, float w, float h)
        {
            ViewportX = x;
            ViewportY = y;
            ViewportW = w;
            ViewportH = h;
        }

        public void AttachRenderTargetID(int slot, int renderTargetID, GfxRenderAttachmentView view = null)
        {
            EnsureListSize(AttachmentRenderTargetIDs, slot);
            EnsureListSize(AttachmentViews, slot);
            AttachmentRenderTargetIDs[slot] = renderTargetID;
            AttachmentViews[slot] = view ?? new GfxRenderAttachmentView(0, 0);
        }

        public void AttachTexture(int slot, GfxTexture texture, GfxRenderAttachmentView view = null, GfxrAttachmentClearDescriptor clearDescriptor = null)
        {
            EnsureListSize(AttachmentTextures, slot);
            EnsureListSize(AttachmentClearDescriptors, slot);
            EnsureListSize(AttachmentViews, slot);
            AttachmentTextures[slot] = texture;
            AttachmentClearDescriptors[slot] = clearDescriptor ?? new GfxrAttachmentClearDescriptor();
            AttachmentViews[slot] = view ?? new GfxRenderAttachmentView(0, 0);
        }

        public void AttachResolveTexture(int resolveTextureID)
        {
            ResolveTextureInputIDs.Add(resolveTextureID);
        }

        public void AttachOcclusionQueryPool(GfxQueryPool queryPool)
        {
            Descriptor.OcclusionQueryPool = queryPool;
        }

        public void Exec(PassExecFunc func)
        {
            _execFunc = func;
        }

        public void Exec(ComputePassExecFunc func)
        {
            _computeExecFunc = func;
        }

        public void Post(PassPostFunc func)
        {
            _postFunc = func;
        }

        public void AddExtraRef(int slot)
        {
            EnsureListSize(RenderTargetExtraRefs, slot);
            RenderTargetExtraRefs[slot] = true;
        }

        // Helper to resize list
        private void EnsureListSize<T>(List<T> list, int index)
        {
            while (list.Count <= index)
                list.Add(default!);
        }

        public void AttachRenderTargetID(GfxrAttachmentSlot attachmentSlot, GfxrRenderTargetID renderTargetID, GfxRenderAttachmentView view = null)
        {
            throw new System.NotImplementedException();
        }

        public void AttachTexture(GfxrAttachmentSlot attachmentSlot, GfxTexture texture, GfxRenderAttachmentView view = null, GfxrAttachmentClearDescriptor clearDescriptor = null)
        {
            throw new System.NotImplementedException();
        }

        public void AttachResolveTexture(GfxrResolveTextureID resolveTextureID)
        {
            throw new System.NotImplementedException();
        }

        public void AddExtraRef(GfxrAttachmentSlot renderTargetID)
        {
            throw new System.NotImplementedException();
        }
    }

}
