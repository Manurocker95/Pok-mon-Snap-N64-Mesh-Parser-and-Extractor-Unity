using System.Collections.Generic;

namespace VirtualPhenix.Nintendo64
{
    public class VP_Uint32Array : VP_Uint32Array<VP_ArrayBuffer>
    {
		public VP_Uint32Array() : base(new VP_ArrayBuffer(), 0)
		{

		}
		public VP_Uint32Array(VP_ArrayBuffer buffer, long byteOffset = 0, long? length = null) : base(buffer, byteOffset, length)
        {

        }

        public VP_Uint32Array(IArrayBufferLike buffer, long byteOffset = 0, long? length = null) : base(new VP_ArrayBuffer(buffer), byteOffset, length)
        {

        }
    }
    public class VP_Uint32Array<T> : VP_ArrayBufferView<T> where T : IArrayBufferLike
    {
        public const int BYTES_PER_ELEMENT = 4;

        public VP_Uint32Array(T buffer, long byteOffset = 0, long? length = null)
            : base(buffer, byteOffset, length ?? (buffer.ByteLength - byteOffset)) { }

        public override object this[long index]
        {
            get
            {
                long byteIndex = ByteOffset + index * BYTES_PER_ELEMENT;
                if (index < 0 || byteIndex + 3 >= Buffer.LongLength)
                    throw new System.IndexOutOfRangeException();
                return System.BitConverter.ToUInt32(Buffer, (int)byteIndex);
            }
            set
            {
                long byteIndex = ByteOffset + index * BYTES_PER_ELEMENT;
                if (index < 0 || byteIndex + 3 >= Buffer.LongLength)
                    throw new System.IndexOutOfRangeException();
                byte[] bytes = System.BitConverter.GetBytes((uint)value);
                Buffer[byteIndex + 0] = bytes[0];
                Buffer[byteIndex + 1] = bytes[1];
                Buffer[byteIndex + 2] = bytes[2];
                Buffer[byteIndex + 3] = bytes[3];
            }
        }

        public override string Species
        {
            get { return "Uint32Array"; }
        }
        public override TypedArrayKind Kind
        {
            get { return TypedArrayKind.Uint32; }
        }
        public override int GetBytesPerElement() => BYTES_PER_ELEMENT;

        public override object GetElement(long index) => this[index];

        public override void SetElement(long index, object value)
        {
            this[index] = (uint)value;
        }

        public override VP_ArrayBufferView<T> CreateInstance(long length)
        {
            var buffer = new VP_ArrayBuffer(length * BYTES_PER_ELEMENT);
            return (VP_Uint32Array<T>)(object)new VP_Uint32Array<VP_ArrayBuffer>(buffer);
        }

        public override VP_ArrayBufferView<T> CreateSubarrayInstance(T buffer, long byteOffset, long byteLength)
        {
            return new VP_Uint32Array<T>(buffer, byteOffset, byteLength);
        }

        public override object ToArray()
        {
            long count = this.Length; 
            uint[] result = new uint[count];

            for (long i = 0; i < count; i++)
            {
                result[i] = (uint)this[i];
            }

            return result;
        }
    }
}
