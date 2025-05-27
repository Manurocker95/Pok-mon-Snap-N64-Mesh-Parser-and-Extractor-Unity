using System;
using System.Diagnostics;


namespace VirtualPhenix.Nintendo64
{
    public class VP_Float32Array : VP_Float32Array<VP_ArrayBuffer>
    {
        public VP_Float32Array() : base(new VP_ArrayBuffer(), 0)
        {

        }

		public VP_Float32Array(VP_ArrayBuffer buffer, long byteOffset = 0, long? length = null) : base(buffer, byteOffset, length)
        {
        }

        public VP_Float32Array(long bufferLength) : base(new VP_ArrayBuffer(bufferLength))
        {

        }
    }

    public class VP_Float32Array<T> : VP_ArrayBufferView<T> where T : IArrayBufferLike
    {
        public const int BYTES_PER_ELEMENT = 4;

        public VP_Float32Array(T buffer, long byteOffset = 0, long? length = null)
            : base(buffer, byteOffset, length ?? (buffer.ByteLength - byteOffset)) { }

        public override object this[long index]
        {
            get
            {
                long byteIndex = ByteOffset + index * BYTES_PER_ELEMENT;
                if (index < 0 || byteIndex + 3 >= Buffer.LongLength)
                    throw new System.IndexOutOfRangeException();
                return System.BitConverter.ToSingle(Buffer, (int)byteIndex);
            }
            set
            {
                long byteIndex = ByteOffset + index * BYTES_PER_ELEMENT;
                if (index < 0 || byteIndex + 3 >= Buffer.LongLength)
                {
                    return;
                    //throw new System.IndexOutOfRangeException();
                }

                float fVal = 0f;

                if (value is double)
                {
                    double dVal = (double)value;
                    fVal = (float)dVal;
                }
                else if (value is long)
                {
                    long longVal = (long)value;
                    fVal = (float)longVal;
                }
                else if (value is int)
                {
                    int longVal = (int)value;
                    fVal = (float)longVal;
                }
                else if (value is System.Single)
                {
                    System.Single longVal = (System.Single)value;
                    fVal = (float)longVal;
                }
                else
                {
                    UnityEngine.Debug.LogError(value.GetType());
                    fVal = (float)value;
                }

                byte[] bytes = System.BitConverter.GetBytes(fVal);
                Buffer[byteIndex + 0] = bytes[0];
                Buffer[byteIndex + 1] = bytes[1];
                Buffer[byteIndex + 2] = bytes[2];
                Buffer[byteIndex + 3] = bytes[3];
            }
        }

        public override string Species
        {
            get { return "Float32Array"; }
        }

        public override TypedArrayKind Kind
        {
            get { return TypedArrayKind.Float32; }
        }

        public override int GetBytesPerElement() => BYTES_PER_ELEMENT;

        public override object GetElement(long index) => this[index];

        public override void SetElement(long index, object value)
        {
            this[index] = (float)value;
        }

        public override VP_ArrayBufferView<T> CreateInstance(long length)
        {
            var buffer = new VP_ArrayBuffer(length * BYTES_PER_ELEMENT);
            return (VP_Float32Array<T>)(object)new VP_Float32Array<VP_ArrayBuffer>(buffer);
        }

        public override VP_ArrayBufferView<T> CreateSubarrayInstance(T buffer, long byteOffset, long byteLength)
        {
            return new VP_Float32Array<T>(buffer, byteOffset, byteLength);
        }

        public override object ToArray()
        {
            long count = this.Length; 
            float[] result = new float[count];

            for (long i = 0; i < count; i++)
            {
                result[i] = (float)this[i]; 
            }

            return result;
        }
    }
}
