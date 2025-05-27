using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class VP_Uint8Array : VP_Uint8Array<VP_ArrayBuffer>
    {
		public VP_Uint8Array() : base(new VP_ArrayBuffer(), 0)
		{

		}
		public VP_Uint8Array(VP_ArrayBuffer buffer, long byteOffset = 0, long? length = null) : base(buffer, byteOffset, length)
        {

        }

        public VP_Uint8Array(IArrayBufferLike buffer, long byteOffset = 0, long? length = null) : base (new VP_ArrayBuffer(buffer), byteOffset, length)
        {

        }
    }

    public class VP_Uint8Array<T> : VP_ArrayBufferView<T> where T : IArrayBufferLike
    {
        public const int BYTES_PER_ELEMENT = 1;

        public VP_Uint8Array(T buffer, long byteOffset = 0, long? length = null) : base(buffer, byteOffset, length ?? (buffer.ByteLength - byteOffset))
        {

        }

        public override object this[long index]
        {
            get
            {
                if (index < 0 || index >= ByteLength / BYTES_PER_ELEMENT)
                    throw new System.IndexOutOfRangeException();
                return Buffer[ByteOffset + index];
            }
            set
            {
                if (index < 0 || index >= ByteLength / BYTES_PER_ELEMENT)
                    throw new System.IndexOutOfRangeException();
                Buffer[ByteOffset + index] = (byte)value;
            }
        }

        public override string Species { get { return "Uint8Array"; } }
        public override TypedArrayKind Kind
        {
            get { return TypedArrayKind.Uint8; }
        }
        public override int GetBytesPerElement() => BYTES_PER_ELEMENT;

        public override object GetElement(long index) => this[index];

        public override void SetElement(long index, object value)
        {
            this[index] = (byte)value;
        }

        public override VP_ArrayBufferView<T> CreateInstance(long length)
        {
            var buffer = new VP_ArrayBuffer(length * BYTES_PER_ELEMENT);
            return (VP_Uint8Array<T>)(object)new VP_Uint8Array<VP_ArrayBuffer>(buffer);
        }

        public override VP_ArrayBufferView<T> CreateSubarrayInstance(T buffer, long byteOffset, long byteLength)
        {
            return new VP_Uint8Array<T>(buffer, byteOffset, byteLength);
        }

        public override object ToArray()
        {
            long count = this.Length;
            byte[] result = new byte[count];

            for (long i = 0; i < count; i++)
            {
                result[i] = (byte)this[i];
            }

            return result;
        }
    }
}
