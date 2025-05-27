namespace VirtualPhenix.Nintendo64
{
    public class VP_Float64Array : VP_Float64Array<VP_ArrayBuffer>
    {
		public VP_Float64Array() : base(new VP_ArrayBuffer(), 0)
		{

		}
		public VP_Float64Array(VP_ArrayBuffer buffer, long byteOffset = 0, long? length = null) : base(buffer, byteOffset, length)
        {
        }
    }

    public class VP_Float64Array<T> : VP_ArrayBufferView<T> where T : IArrayBufferLike
    {
        public const int BYTES_PER_ELEMENT = 8;

        public VP_Float64Array(T buffer, long byteOffset = 0, long? length = null)
            : base(buffer, byteOffset, length ?? (buffer.ByteLength - byteOffset)) { }

        public override object this[long index]
        {
            get
            {
                long byteIndex = ByteOffset + index * BYTES_PER_ELEMENT;
                if (index < 0 || byteIndex + 7 >= Buffer.LongLength)
                    throw new System.IndexOutOfRangeException();
                return System.BitConverter.ToDouble(Buffer, (int)byteIndex);
            }
            set
            {
                long byteIndex = ByteOffset + index * BYTES_PER_ELEMENT;
                if (index < 0 || byteIndex + 7 >= Buffer.LongLength)
                    throw new System.IndexOutOfRangeException();
                byte[] bytes = System.BitConverter.GetBytes((double)value);
                Buffer[byteIndex + 0] = bytes[0];
                Buffer[byteIndex + 1] = bytes[1];
                Buffer[byteIndex + 2] = bytes[2];
                Buffer[byteIndex + 3] = bytes[3];
                Buffer[byteIndex + 4] = bytes[4];
                Buffer[byteIndex + 5] = bytes[5];
                Buffer[byteIndex + 6] = bytes[6];
                Buffer[byteIndex + 7] = bytes[7];
            }
        }

        public override string Species
        {
            get { return "Float64Array"; }
        }

        public override TypedArrayKind Kind
        {
            get { return TypedArrayKind.Float64; }
        }

        public override int GetBytesPerElement() => BYTES_PER_ELEMENT;

        public override object GetElement(long index) => this[index];

        public override void SetElement(long index, object value)
        {
            this[index] = (double)value;
        }

        public override VP_ArrayBufferView<T> CreateInstance(long length)
        {
            var buffer = new VP_ArrayBuffer(length * BYTES_PER_ELEMENT);
            return (VP_Float64Array<T>)(object)new VP_Float64Array<VP_ArrayBuffer>(buffer);
        }

        public override VP_ArrayBufferView<T> CreateSubarrayInstance(T buffer, long byteOffset, long byteLength)
        {
            return new VP_Float64Array<T>(buffer, byteOffset, byteLength);
        }

        public override object ToArray()
        {
            long count = this.Length;
            double[] result = new double[count];

            for (long i = 0; i < count; i++)
            {
                result[i] = (double)this[i]; 
            }

            return result;
        }
    }
}
