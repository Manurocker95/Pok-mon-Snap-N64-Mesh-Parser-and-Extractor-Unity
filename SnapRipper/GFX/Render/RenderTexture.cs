using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class RenderTarget : GfxRenderTargetDescriptor
    {
        public string DebugName { get; private set; } = string.Empty;

        public GfxTexture Texture { get; private set; }
        public GfxRenderTarget RenderTargetHandle { get; private set; }
        public bool NeedsClear { get; set; } = true;
        public int Age { get; private set; } = 0;

        public RenderTarget(GfxDevice device, GfxrRenderTargetDescription desc)
        {
            this.PixelFormat = desc.PixelFormat;
            this.Width = desc.Width;
            this.Height = desc.Height;
            this.NumLevels = desc.NumLevels;
            this.SampleCount = desc.SampleCount;
            this.Dimension = GfxTextureDimension.n2D;
            this.DepthOrArrayLayers = 1;
            this.Usage = GfxTextureUsage.RenderTarget;

            if (SampleCount > 1)
            {
                RenderTargetHandle = device.CreateRenderTarget(this);
            }
            else
            {
                Texture = device.CreateTexture(this);
                RenderTargetHandle = device.CreateRenderTargetFromTexture(Texture);
            }
        }

        public void SetDebugName(GfxDevice device, string debugName)
        {
            this.DebugName = debugName;

            if (Texture != null)
                device.SetResourceName(Texture, debugName);

            device.SetResourceName(RenderTargetHandle, debugName);
        }

        public bool MatchesDescription(GfxrRenderTargetDescription desc)
        {
            return PixelFormat == desc.PixelFormat &&
                   Width == desc.Width &&
                   Height == desc.Height &&
                   NumLevels == desc.NumLevels &&
                   SampleCount == desc.SampleCount;
        }

        public void Reset()
        {
            Age = 0;
        }

        public void Destroy(GfxDevice device)
        {
            if (Texture != null)
                device.DestroyTexture(Texture);

            device.DestroyRenderTarget(RenderTargetHandle);
        }
    }
}
