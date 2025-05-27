using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class VP_ArrayBufferSlice : IArrayBufferLike
    {
        public virtual IArrayBufferLike Buffer { get; protected set; }
        public virtual long ByteOffset { get; protected set; }
        public virtual long ByteLength { get; protected set; }

        public VP_ArrayBufferSlice(byte[] bufferBytes, long byteOffset = 0, long? byteLength = null)
        {
            VP_ArrayBuffer buffer = new VP_ArrayBuffer(bufferBytes);
            InitBuffer(buffer, byteOffset, byteLength);
        }

        public VP_ArrayBufferSlice(IArrayBufferLike buffer, long byteOffset = 0, long? byteLength = null)
        {
            InitBuffer(buffer, byteOffset, byteLength);
        }

        public virtual void InitBuffer(IArrayBufferLike buffer, long byteOffset = 0, long? byteLength = null)
        {
            long bufferLength = buffer.LongLength;

            if (byteOffset < 0)
                throw new System.ArgumentOutOfRangeException(nameof(byteOffset));

            long actualLength = byteLength ?? (bufferLength - byteOffset);

            if (actualLength < 0 || (actualLength > 0 && byteOffset + actualLength > bufferLength))
                throw new System.ArgumentOutOfRangeException(nameof(byteLength));

            Buffer = buffer;
            ByteOffset = byteOffset;
            ByteLength = actualLength;
        }

        public virtual IArrayBufferLike Slice(long? start = null, long? end = null)
        {
            return Buffer.Slice(start, end);
        }

        public static VP_ArrayBufferSlice FromView<T>(VP_ArrayBufferView<T> view) where T : IArrayBufferLike
        {
            return new VP_ArrayBufferSlice(view.BufferSource, view.ByteOffset, view.ByteLength);
        }

        public object this[long index]
        {
            get
            {
                if (Buffer == null)
                    throw new System.ObjectDisposedException(nameof(VP_ArrayBufferSlice));
                if (index < 0 || index >= ByteLength)
                    throw new System.IndexOutOfRangeException();
                return Buffer[ByteOffset + index];
            }
            set
            {
                if (Buffer == null)
                    throw new System.ObjectDisposedException(nameof(VP_ArrayBufferSlice));
                if (index < 0 || index >= ByteLength)
                    throw new System.IndexOutOfRangeException();
                Buffer[ByteOffset + index] = (byte)value;
            }
        }

        public long LongLength
        {
            get
            {
                return ByteLength;
            }
        }

        byte[] IArrayBufferLike.Buffer
        {
            get
            {
                return Buffer.Buffer;
            }
        }

        public virtual VP_ArrayBufferSlice Slice(long begin, long end = 0, bool copyData = false)
        {
            long absBegin = ByteOffset + begin;
            long absEnd = ByteOffset + (end != 0 ? end : ByteLength);
            long sliceLength = absEnd - absBegin;

            if (sliceLength < 0 || sliceLength > ByteLength)
                throw new System.ArgumentOutOfRangeException("Slice range is invalid relative to current slice.");

            if (Buffer == null)
                throw new System.ObjectDisposedException(nameof(VP_ArrayBufferSlice));

            if (copyData)
            {
                var newBuffer = new VP_ArrayBuffer(sliceLength);
                for (long i = 0; i < sliceLength; i++)
                {
                    newBuffer[i] = Buffer[absBegin + i];
                }
                return new VP_ArrayBufferSlice(newBuffer);
            }
            else
            {
                return new VP_ArrayBufferSlice(Buffer, absBegin, sliceLength);
            }
        }

        public virtual VP_ArrayBufferSlice Subarray(long begin, long? byteLength = null, bool copyData = false)
        {
            if (Buffer == null)
                throw new System.ObjectDisposedException(nameof(VP_ArrayBufferSlice));

            long absBegin = ByteOffset + begin;
            long length = byteLength ?? (ByteLength - begin);

            if (length < 0 || begin < 0 || begin + length > ByteLength)
                throw new System.ArgumentOutOfRangeException("Subarray range is invalid relative to current slice.");

            if (copyData)
            {
                var newBuffer = new VP_ArrayBuffer(length);
                for (long i = 0; i < length; i++)
                {
                    newBuffer[i] = Buffer[absBegin + i];
                }
                return new VP_ArrayBufferSlice(newBuffer);
            }
            else
            {
                return new VP_ArrayBufferSlice(Buffer, absBegin, length);
            }
        }

        public virtual VP_ArrayBuffer CopyToBuffer(long begin = 0, long? byteLength = null)
        {
            var absBegin = ByteOffset + begin;

            if (!byteLength.HasValue)
                byteLength = ByteLength - begin;

            return (VP_ArrayBuffer)Buffer.Slice(absBegin, absBegin + byteLength) as VP_ArrayBuffer;
        }

        public virtual VP_ArrayBuffer CopyToBufferOld(long begin = 0, long? byteLength = null)
        {
            if (Buffer == null)
                throw new System.ObjectDisposedException(nameof(VP_ArrayBufferSlice));

            if (begin < 0 || begin > ByteLength)
                throw new System.ArgumentOutOfRangeException(nameof(begin));

            long length = byteLength ?? (ByteLength - begin);

            if (length < 0 || begin + length > ByteLength)
                throw new System.ArgumentOutOfRangeException(nameof(byteLength));

            long absBegin = ByteOffset + begin;
            var result = new VP_ArrayBuffer(length);

            for (long i = 0; i < length; i++)
                result[i] = Buffer[absBegin + i];

            return result;
        }

        public virtual VP_DataView CreateDefaultDataViewFloat64(double offs = 0, long? length = null)
        {
            if (Buffer == null)
                throw new System.ObjectDisposedException(nameof(VP_ArrayBufferSlice));

            if (offs == 0 && length == null)
            {
                return new VP_DataView(new VP_ArrayBuffer(Buffer), ByteOffset, ByteLength);
            }
            else
            {
                var sub = Subarray((long)offs, length);
                return sub.CreateDefaultDataViewFloat64();
            }
        }

        public virtual VP_DataView CreateDefaultDataView(long offs = 0, long? length = null)
        {
            if (Buffer == null)
                throw new System.ObjectDisposedException(nameof(VP_ArrayBufferSlice));

            if (offs == 0 && length == null)
            {
                return new VP_DataView(new VP_ArrayBuffer(Buffer), ByteOffset, ByteLength);
            }
            else
            {
                var sub = Subarray(offs, length);
                return new VP_DataView(sub);
            }
        }

        public virtual VP_DataView<IArrayBufferLike> CreateDataView(long offs = 0, long? length = null)
        {
            if (Buffer == null)
                throw new System.ObjectDisposedException(nameof(VP_ArrayBufferSlice));

            if (offs == 0 && length == null)
            {
                return new VP_DataView<IArrayBufferLike>(Buffer, ByteOffset, ByteLength);
            }
            else
            {
                var sub = Subarray(offs, length);
                return sub.CreateDataView();
            }
        }

        public virtual VP_ArrayBufferSlice Bswap16()
        {
            if (ByteLength % 2 != 0)
                throw new System.InvalidOperationException("Buffer length must be a multiple of 2 for bswap16.");

            if (Buffer == null)
                throw new System.ObjectDisposedException(nameof(VP_ArrayBufferSlice));

            var output = new VP_ArrayBuffer(ByteLength);

            for (long i = 0; i < ByteLength; i += 2)
            {
                byte b0 = Buffer.Buffer[ByteOffset + i];
                byte b1 = Buffer.Buffer[ByteOffset + i + 1];

                output[i + 0] = b1;
                output[i + 1] = b0;
            }

            return new VP_ArrayBufferSlice(output);
        }

        public virtual VP_ArrayBufferSlice Bswap32()
        {
            if (ByteLength % 4 != 0)
                throw new System.InvalidOperationException("Buffer length must be a multiple of 4 for bswap32.");

            if (Buffer == null)
                throw new System.ObjectDisposedException(nameof(VP_ArrayBufferSlice));

            var output = new VP_ArrayBuffer(ByteLength);

            for (long i = 0; i < ByteLength; i += 4)
            {
                long srcIndex = ByteOffset + i;

                byte b0 = Buffer.Buffer[srcIndex];
                byte b1 = Buffer.Buffer[srcIndex + 1];
                byte b2 = Buffer.Buffer[srcIndex + 2];
                byte b3 = Buffer.Buffer[srcIndex + 3];

                output[i + 0] = b3;
                output[i + 1] = b2;
                output[i + 2] = b1;
                output[i + 3] = b0;
            }

            return new VP_ArrayBufferSlice(output);
        }

        public virtual VP_ArrayBufferSlice Bswap(int componentSize)
        {
            if (componentSize == 2)
            {
                return Bswap16();
            }
            else if (componentSize == 4)
            {
                return Bswap32();
            }
            else
            {
                throw new System.ArgumentException("Invalid componentSize: must be 2 or 4.", "componentSize");
            }
        }

        public virtual VP_ArrayBufferSlice ConvertFromEndianness(Endianness endianness, int componentSize)
        {
            if (componentSize != 1 && endianness != VP_EndianUtils.GetSystemEndianness())
                return Bswap(componentSize);
            else
                return this;
        }

        public virtual VP_ArrayBufferView<T> CreateInstance<T>(TypedArrayKind kind, IArrayBufferLike buffer, long byteOffset, long byteLength) where T : IArrayBufferLike
        {
     
            switch (kind)
            {
                case TypedArrayKind.Int8:
                    return new VP_Int8Array<T>((T)buffer, byteOffset, byteLength);
                case TypedArrayKind.Uint8:
                    return new VP_Uint8Array<T>((T)buffer, byteOffset, byteLength);
                case TypedArrayKind.Int16:
                    return new VP_Int16Array<T>((T)buffer, byteOffset, byteLength);
                case TypedArrayKind.Uint16:
                    return new VP_Uint16Array<T>((T)buffer, byteOffset, byteLength);
                case TypedArrayKind.Int32:
                    return new VP_Int32Array<T>((T)buffer, byteOffset, byteLength);
                case TypedArrayKind.Uint32:
                    return new VP_Uint32Array<T>((T)buffer, byteOffset, byteLength);
                case TypedArrayKind.Int64:
                    return new VP_Int64Array<T>((T)buffer, byteOffset, byteLength);
                case TypedArrayKind.Uint64:
                    return new VP_Uint64Array<T>((T)buffer, byteOffset, byteLength);
                case TypedArrayKind.Float32:
                    return new VP_Float32Array<T>((T)buffer, byteOffset, byteLength);
                case TypedArrayKind.Float64:
                    return new VP_Float64Array<T>((T)buffer, byteOffset, byteLength);
                default:
                    throw new System.ArgumentOutOfRangeException("Unsupported TypedArrayKind");
            }
        }
        
        public virtual VP_ArrayBufferView<IArrayBufferLike> CreateInstance(TypedArrayKind kind, IArrayBufferLike buffer, long byteOffset, long byteLength) 
        {
     
            switch (kind)
            {
                case TypedArrayKind.Int8:
                    return new VP_Int8Array<IArrayBufferLike>(buffer, byteOffset, byteLength);
                case TypedArrayKind.Uint8:
                    return new VP_Uint8Array<IArrayBufferLike>(buffer, byteOffset, byteLength);
                case TypedArrayKind.Int16:
                    return new VP_Int16Array<IArrayBufferLike>(buffer, byteOffset, byteLength);
                case TypedArrayKind.Uint16:
                    return new VP_Uint16Array<IArrayBufferLike>(buffer, byteOffset, byteLength);
                case TypedArrayKind.Int32:
                    return new VP_Int32Array<IArrayBufferLike>(buffer, byteOffset, byteLength);
                case TypedArrayKind.Uint32:
                    return new VP_Uint32Array<IArrayBufferLike>(buffer, byteOffset, byteLength);
                case TypedArrayKind.Int64:
                    return new VP_Int64Array<IArrayBufferLike>(buffer, byteOffset, byteLength);
                case TypedArrayKind.Uint64:
                    return new VP_Uint64Array<IArrayBufferLike>(buffer, byteOffset, byteLength);
                case TypedArrayKind.Float32:
                    return new VP_Float32Array<IArrayBufferLike>(buffer, byteOffset, byteLength);
                case TypedArrayKind.Float64:
                    return new VP_Float64Array<IArrayBufferLike>(buffer, byteOffset, byteLength);
                default:
                    throw new System.ArgumentOutOfRangeException("Unsupported TypedArrayKind");
            }
        }

        public virtual VP_ArrayBufferView<VP_ArrayBuffer> CreateInstance(TypedArrayKind kind, VP_ArrayBuffer buffer, long byteOffset, long byteLength)
        {

            switch (kind)
            {
                case TypedArrayKind.Int8:
                    return new VP_Int8Array(buffer, byteOffset, byteLength);
                case TypedArrayKind.Uint8:
                    return new VP_Uint8Array<VP_ArrayBuffer>(buffer, byteOffset, byteLength);
                case TypedArrayKind.Int16:
                    return new VP_Int16Array<VP_ArrayBuffer>(buffer, byteOffset, byteLength);
                case TypedArrayKind.Uint16:
                    return new VP_Uint16Array<VP_ArrayBuffer>(buffer, byteOffset, byteLength);
                case TypedArrayKind.Int32:
                    return new VP_Int32Array<VP_ArrayBuffer>(buffer, byteOffset, byteLength);
                case TypedArrayKind.Uint32:
                    return new VP_Uint32Array<VP_ArrayBuffer>(buffer, byteOffset, byteLength);
                case TypedArrayKind.Int64:
                    return new VP_Int64Array<VP_ArrayBuffer>(buffer, byteOffset, byteLength);
                case TypedArrayKind.Uint64:
                    return new VP_Uint64Array<VP_ArrayBuffer>(buffer, byteOffset, byteLength);
                case TypedArrayKind.Float32:
                    return new VP_Float32Array<VP_ArrayBuffer>(buffer, byteOffset, byteLength);
                case TypedArrayKind.Float64:
                    return new VP_Float64Array<VP_ArrayBuffer>(buffer, byteOffset, byteLength);
                default:
                    throw new System.ArgumentOutOfRangeException("Unsupported TypedArrayKind");
            }
        }

        public virtual VP_ArrayBufferView<VP_ArrayBuffer> CreateDefaultArrayBufferInstance(TypedArrayKind kind)
        {

            switch (kind)
            {
                case TypedArrayKind.Int8:
                    return new VP_Int8Array();
                case TypedArrayKind.Uint8:
                    return new VP_Uint8Array();
                case TypedArrayKind.Int16:
                    return new VP_Int16Array();
                case TypedArrayKind.Uint16:
                    return new VP_Uint16Array();
                case TypedArrayKind.Int32:
                    return new VP_Int32Array();
                case TypedArrayKind.Uint32:
                    return new VP_Uint32Array();
                case TypedArrayKind.Int64:
                    return new VP_Int64Array();
                case TypedArrayKind.Uint64:
                    return new VP_Uint64Array();
                case TypedArrayKind.Float32:
                    return new VP_Float32Array();
                case TypedArrayKind.Float64:
                    return new VP_Float64Array();
                default:
                    throw new System.ArgumentOutOfRangeException("Unsupported TypedArrayKind");
            }
        }

        public virtual T CreateTypedArray<T>(TypedArrayKind kind, long offs = 0, long? count = null, Endianness endianness = Endianness.LittleEndian) where T : VP_ArrayBufferView<VP_ArrayBuffer>
        {
            var clazz = CreateDefaultArrayBufferInstance(kind);
            long begin = ByteOffset + offs;
            var bytesPerElement = clazz.GetBytesPerElement();

            long byteLength;
            if (count.HasValue)
            {
                byteLength = bytesPerElement * count.Value;
            }
            else
            {
                byteLength = ByteLength - offs;
                count = byteLength / bytesPerElement;
            }

            int componentSize = bytesPerElement;
            bool needsEndianSwap = componentSize > 1 && endianness != VP_EndianUtils.GetSystemEndianness();

            if (needsEndianSwap)
            {
                int componentSize_ = componentSize;
                var copy = Subarray(offs, byteLength).Bswap(componentSize_);
                return copy.CreateTypedArray<T>(kind);
            }
            else if (VP_BYMLUtils.IsAligned(begin, componentSize))
            {
                var t = CreateInstance(kind, new VP_ArrayBuffer(this.Buffer), begin, count.Value);

                if (typeof(T) != t.GetType())
                {
                    Debug.LogError(typeof(T));
                    Debug.LogError(t.GetType());
                }

                return (T)t;
            }
            else
            {
                return (T)CreateInstance(kind, CopyToBuffer(offs, byteLength), begin, count.Value);
            }
        }

        public virtual int GetBytesPerElement()
        {
            return Buffer.GetBytesPerElement();
        }
    }
}
