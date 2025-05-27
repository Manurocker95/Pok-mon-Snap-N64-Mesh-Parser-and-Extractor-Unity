using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class GfxRenderDynamicUniformBuffer
    {
        private long _UniformBufferWordAlignment;
        private long _UniformBufferMaxPageWordSize;

        private long _CurrentBufferWordSize = -1;
        private long _CurrentWordOffset = 0;
        public GfxBuffer GfxBuffer = null;

        private VP_Float32Array _ShadowBufferF32 = null;
        private VP_Uint8Array _ShadowBufferU8 = null;

        private GfxDevice _Device;

        public GfxRenderDynamicUniformBuffer(GfxDevice device)
        {
            this._Device = device;
            var limits = device.QueryLimits();
            this._UniformBufferWordAlignment = limits.UniformBufferWordAlignment;
            this._UniformBufferMaxPageWordSize = limits.UniformBufferMaxPageWordSize;
        }

        private long FindPageIndex(long wordOffset)
        {
            return wordOffset / this._UniformBufferMaxPageWordSize;
        }

        public long AllocateChunk(long wordCount)
        {
            wordCount = GfxPlatformUtils.AlignNonPowerOfTwo(wordCount, this._UniformBufferWordAlignment);
            GfxPlatformUtils.Assert(wordCount <= this._UniformBufferMaxPageWordSize);

            long wordOffset = this._CurrentWordOffset;

            if (FindPageIndex(wordOffset) != FindPageIndex(wordOffset + wordCount - 1))
                wordOffset = GfxPlatformUtils.AlignNonPowerOfTwo(wordOffset, this._UniformBufferMaxPageWordSize);

            this._CurrentWordOffset = wordOffset + wordCount;
            this.EnsureShadowBuffer(wordOffset, wordCount);

            return wordOffset;
        }

        private void EnsureShadowBuffer(long wordOffset, long wordCount)
        {
            if (this._ShadowBufferU8 == null)
            {
                long newWordCount = GfxPlatformUtils.AlignNonPowerOfTwo(this._CurrentWordOffset, this._UniformBufferMaxPageWordSize);
                var buffer = new VP_ArrayBuffer(newWordCount << 2);
                this._ShadowBufferU8 = new VP_Uint8Array(buffer);
                this._ShadowBufferF32 = new VP_Float32Array(buffer);
            }
            else if (wordOffset + wordCount >= this._ShadowBufferF32!.Length)
            {
                GfxPlatformUtils.Assert(wordOffset < this._CurrentWordOffset && wordOffset + wordCount <= this._CurrentWordOffset);

                long newWordCount = GfxPlatformUtils.AlignNonPowerOfTwo(
                    Math.Max(this._CurrentWordOffset, this._ShadowBufferF32!.Length * 2),
                    this._UniformBufferMaxPageWordSize
                );

                var buffer = this._ShadowBufferU8.BufferSource;
                var newBuffer = buffer.Transfer(newWordCount << 2);

                this._ShadowBufferU8 = new VP_Uint8Array(newBuffer);
                this._ShadowBufferF32 = new VP_Float32Array(newBuffer);

                if (!(this._CurrentWordOffset <= newWordCount))
                    throw new Exception($"Assert fail: this._CurrentWordOffset [{this._CurrentWordOffset}] <= newWordCount [{newWordCount}]");
            }
        }

        public VP_Float32Array MapBufferF32()
        {
            return this._ShadowBufferF32!;
        }

        public void PrepareToRender()
        {
            if (this._ShadowBufferF32 == null)
                return;

            var shadowBufferF32 = GfxPlatformUtils.AssertExists(this._ShadowBufferF32);

            if (shadowBufferF32.Length != this._CurrentBufferWordSize)
            {
                this._CurrentBufferWordSize = shadowBufferF32.Length;

                if (this.GfxBuffer != null)
                    this._Device.DestroyBuffer(this.GfxBuffer);

                this.GfxBuffer = this._Device.CreateBuffer(
                    this._CurrentBufferWordSize,
                    GfxBufferUsage.Uniform,
                    GfxBufferFrequencyHint.Dynamic
                );
            }

            long wordCount = GfxPlatformUtils.AlignNonPowerOfTwo(this._CurrentWordOffset, this._UniformBufferMaxPageWordSize);

            if (!(wordCount <= this._CurrentBufferWordSize))
                throw new Exception($"Assert fail: wordCount [{wordCount}] ({this._CurrentWordOffset} aligned {this._UniformBufferMaxPageWordSize}) <= this._CurrentBufferWordSize [{this._CurrentBufferWordSize}]");

            var gfxBuffer = GfxPlatformUtils.AssertExists(this.GfxBuffer);

            this._Device.UploadBufferData(gfxBuffer, 0, this._ShadowBufferU8!, 0, wordCount * 4);

            // Reset offset for next frame
            this._CurrentWordOffset = 0;
        }

        public void Destroy()
        {
            if (this.GfxBuffer != null)
                this._Device.DestroyBuffer(this.GfxBuffer);

            this._ShadowBufferF32 = null;
            this._ShadowBufferU8 = null;
        }

    }

}
