namespace VirtualPhenix.Nintendo64
{
    public class VP_Int16Array : VP_Int16Array<VP_ArrayBuffer>
    {
		public VP_Int16Array() : base(new VP_ArrayBuffer(), 0)
		{

		}
		public VP_Int16Array(VP_ArrayBuffer buffer, long byteOffset = 0, long? length = null) : base(buffer, byteOffset, length)
        {
        }
    }

    public class VP_Int16Array<T> : VP_ArrayBufferView<T> where T : IArrayBufferLike
    {
        public const int BYTES_PER_ELEMENT = 2;

        public VP_Int16Array(T buffer, long byteOffset = 0, long? length = null)
            : base(buffer, byteOffset, length ?? (buffer.ByteLength - byteOffset)) { }

        public override object this[long index]
        {
            get
            {
                long byteIndex = ByteOffset + index * BYTES_PER_ELEMENT;
                if (index < 0 || byteIndex + 1 >= Buffer.LongLength)
                    throw new System.IndexOutOfRangeException();
                return System.BitConverter.ToInt16(Buffer, (int)byteIndex);
            }
            set
            {
                long byteIndex = ByteOffset + index * BYTES_PER_ELEMENT;
                if (index < 0 || byteIndex + 1 >= Buffer.LongLength)
                    throw new System.IndexOutOfRangeException();
                byte[] bytes = System.BitConverter.GetBytes((short)value);
                Buffer[byteIndex] = bytes[0];
                Buffer[byteIndex + 1] = bytes[1];
            }
        }

        public override string Species
        {
            get { return "Int16Array"; }
        }
        public override TypedArrayKind Kind
        {
            get { return TypedArrayKind.Int16; }
        }
        public override int GetBytesPerElement() => BYTES_PER_ELEMENT;

        public override object GetElement(long index) => this[index];

        public override void SetElement(long index, object value)
        {
            this[index] = (short)value;
        }

        public override VP_ArrayBufferView<T> CreateInstance(long length)
        {
            var buffer = new VP_ArrayBuffer(length * BYTES_PER_ELEMENT);
            return (VP_Int16Array<T>)(object)new VP_Int16Array<VP_ArrayBuffer>(buffer);
        }

        public override VP_ArrayBufferView<T> CreateSubarrayInstance(T buffer, long byteOffset, long byteLength)
        {
            return new VP_Int16Array<T>(buffer, byteOffset, byteLength);
        }

        public override object ToArray()
        {
            long count = this.Length;
            short[] result = new short[count];

            for (long i = 0; i < count; i++)
            {
                result[i] = (short)this[i];
            }

            return result;
        }
    }
}
