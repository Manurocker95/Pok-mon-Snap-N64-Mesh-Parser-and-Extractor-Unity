using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class GfxrRenderTargetDescription
    {
        public int Width { get; set; } = 0;
        public int Height { get; set; } = 0;
        public int NumLevels { get; set; } = 1;
        public int SampleCount { get; set; } = 0;

        public object ClearColor { get; set; } = "load"; // Either GfxColor or "load"
        public object ClearDepth { get; set; } = "load"; // Either float or "load"
        public object ClearStencil { get; set; } = "load"; // Either int or "load"

        public GfxFormat PixelFormat { get; }

        public GfxrRenderTargetDescription(GfxFormat pixelFormat)
        {
            this.PixelFormat = pixelFormat;
        }

        public void SetDimensions(int width, int height, int sampleCount)
        {
            this.Width = width;
            this.Height = height;
            this.SampleCount = sampleCount;
        }

        public void CopyDimensions(GfxrRenderTargetDescription desc)
        {
            this.Width = desc.Width;
            this.Height = desc.Height;
            this.SampleCount = desc.SampleCount;
        }
    }

}
