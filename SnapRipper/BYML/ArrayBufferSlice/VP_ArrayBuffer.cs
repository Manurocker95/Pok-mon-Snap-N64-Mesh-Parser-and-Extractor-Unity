using System;

namespace VirtualPhenix.Nintendo64
{
    public class VP_ArrayBuffer : IArrayBufferLike
    {
        public virtual byte[] Buffer { get; private set; }

        public virtual long ByteLength
        {
            get { return Buffer != null ? Buffer.LongLength : 0; }
        }

        public virtual long LongLength
        {
            get { return Buffer.LongLength; }
        }

        public VP_ArrayBuffer()
        {
            Buffer = new byte[0];
        }

        public VP_ArrayBuffer(byte[] newBuffer)
        {
            Buffer = newBuffer;
        }

        public VP_ArrayBuffer(IArrayBufferLike alike)
        {
            Buffer = alike.Buffer;
        }

        public VP_ArrayBuffer(long byteLength)
        {
            if (byteLength < 0 || byteLength > int.MaxValue)
                throw new System.ArgumentOutOfRangeException("byteLength must be between 0 and Int32.MaxValue");

            Buffer = new byte[byteLength];
        }

        public static bool IsView(object obj)
        {
            return obj is ISpeciesTypedArray && obj is IArrayBufferLike;
        }

        public object this[long index]
        {
            get
            {
                if (index < 0 || index >= LongLength)
                    throw new System.IndexOutOfRangeException();
                return Buffer[index];
            }
            set
            {
                if (index < 0 || index >= Buffer.LongLength)
                    throw new System.IndexOutOfRangeException();
                Buffer[index] = (byte)value;
            }
        }

        public VP_ArrayBuffer Transfer(long? newByteLength = null)
        {
            if (Buffer == null)
            {
                Buffer = new byte[newByteLength != null ? (int)newByteLength : 0];
            }

            int originalLength = Buffer.Length;


            long size = newByteLength ?? originalLength;


            byte[] newBuffer = new byte[size];
            

            System.Array.Copy(Buffer, newBuffer, System.Math.Min(originalLength, size));


            return new VP_ArrayBuffer(newBuffer);
        }

        public static VP_ArrayBuffer Transfer(VP_ArrayBuffer source, long newByteLength)
        {
            if (newByteLength < 0)
                throw new ArgumentOutOfRangeException(nameof(newByteLength), "New length must be non-negative.");

            var newBuffer = new VP_ArrayBuffer((int)newByteLength);
            var sourceU8 = new VP_Uint8Array(source);
            var destU8 = new VP_Uint8Array(newBuffer);

            long lengthToCopy = Math.Min(sourceU8.Length, destU8.Length);
            for (int i = 0; i < lengthToCopy; i++)
                destU8[i] = sourceU8[i];

            return newBuffer;
        }

        public virtual IArrayBufferLike Slice(long? start = null, long? end = null)
        {
            start ??= 0;

            end ??= Buffer.Length;

            if (start < 0 || start >= Buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(start), "Start out of range!.");

            if (end < 0 || end > Buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(end), "'End out of range!'.");

            if (start > end)
                throw new ArgumentException("'start' can't be higher than 'end'.");

            byte[] sliced = new byte[end.Value - start.Value];
            Array.Copy(Buffer, start.Value, sliced, 0, sliced.Length);

            var ret = new VP_ArrayBuffer(sliced);

            return ret;
        }

        public virtual int GetBytesPerElement()
        {
            return 2;
        }
    }
}
