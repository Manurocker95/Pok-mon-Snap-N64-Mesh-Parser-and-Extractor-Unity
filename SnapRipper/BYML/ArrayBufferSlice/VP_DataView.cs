using System;
using System.Linq;

namespace VirtualPhenix.Nintendo64
{
    public class VP_DataView : VP_DataView<VP_ArrayBuffer>
    {
        public VP_DataView(VP_ArrayBuffer buffer, long byteOffset = 0, long? byteLength = null) : base(buffer, byteOffset, byteLength)
        {

        }

        public VP_DataView(VP_ArrayBufferSlice subArray) : base (subArray)
        {

        }
    }


    public class VP_DataView<T> where T : IArrayBufferLike
    {
        public T BufferSource { get; private set; }
        public byte[] Buffer
        {
            get { return BufferSource.Buffer; }
        }
        public long ByteOffset { get; private set; }
        public long ByteLength { get; private set; }
        public virtual long LongLength
        {
            get { return Buffer.LongLength; }
        }
        
        public VP_DataView(VP_ArrayBufferSlice subArray) 
        {
            var byteLength = subArray.ByteLength;
            var byteOffset = subArray.ByteOffset;
            var buffer = (T)subArray.Buffer;

            if (buffer == null)
                throw new System.ArgumentNullException("Buffer in DataView by Slice");


            InitDataView(buffer, byteOffset, byteLength);
        }

        public VP_DataView(T buffer, long byteOffset = 0, long? byteLength = null)
        {
            if (buffer == null)
                throw new System.ArgumentNullException("Buffer in DataView");

            InitDataView(buffer, byteOffset, byteLength);
        }

        public void InitDataView(T buffer, long byteOffset = 0, long? byteLength = null)
        {
            long resolvedLength = byteLength ?? (buffer.ByteLength - byteOffset);

            if (byteOffset < 0 || resolvedLength < 0 || (byteOffset + resolvedLength) > buffer.ByteLength)
                throw new System.ArgumentOutOfRangeException("Invalid byteOffset or byteLength");

            BufferSource = buffer;
            ByteOffset = byteOffset;
            ByteLength = resolvedLength;
        }

        private int ResolveIndex(long offset, int size)
        {
            if (offset >= Buffer.LongLength)
                return 0xFF;

            long index = ByteOffset + offset;
            if (offset < 0 || (index + size > Buffer.LongLength))
                throw new System.IndexOutOfRangeException();
            return (int)index;
        }

        private byte[] AdjustEndian(byte[] data, bool littleEndian)
        {
            if (System.BitConverter.IsLittleEndian != littleEndian)
                System.Array.Reverse(data);
            return data;
        }

        // ------- Get Methods -------

        public float GetFloat32(long offset, bool littleEndian = false)
        {
            return System.BitConverter.ToSingle(AdjustEndian(Buffer.Skip(ResolveIndex(offset, 4)).Take(4).ToArray(), littleEndian), 0);
        }

        public double GetFloat64(long offset, bool littleEndian = false)
        {
            return System.BitConverter.ToDouble(AdjustEndian(Buffer.Skip(ResolveIndex(offset, 8)).Take(8).ToArray(), littleEndian), 0);
        }

        public sbyte GetInt8(long offset)
        {
            return (sbyte)Buffer[ResolveIndex(offset, 1)];
        }

        public short GetInt16(long offset, bool littleEndian = false)
        {
            return System.BitConverter.ToInt16(AdjustEndian(Buffer.Skip(ResolveIndex(offset, 2)).Take(2).ToArray(), littleEndian), 0);
        }

        public int GetInt32(long offset, bool littleEndian = false)
        {
            return System.BitConverter.ToInt32(AdjustEndian(Buffer.Skip(ResolveIndex(offset, 4)).Take(4).ToArray(), littleEndian), 0);
        }

        public byte GetUint8(long offset)
        {
            return Buffer[ResolveIndex(offset, 1)];
        }

        public ushort GetUint16(long offset, bool littleEndian = false)
        {
            return System.BitConverter.ToUInt16(AdjustEndian(Buffer.Skip(ResolveIndex(offset, 2)).Take(2).ToArray(), littleEndian), 0);
        }

        public uint GetUint32(long offset, bool littleEndian = false)
        {
            var idx = ResolveIndex(offset, 4);
            var skip = Buffer.Skip(idx).Take(4).ToArray();
            var data = AdjustEndian(skip, littleEndian);
           uint ret = System.BitConverter.ToUInt32(data, 0);

            return ret;
        }

        public long GetBigInt64(long offset, bool littleEndian = false)
        {

            var bytes = Buffer.Skip(ResolveIndex(offset, 8)).Take(8).ToArray();


            bytes = AdjustEndian(bytes, littleEndian);


            return System.BitConverter.ToInt64(bytes, 0);
        }

        public ulong GetBigUint64(long offset, bool littleEndian = false)
        {
  
            var bytes = Buffer.Skip(ResolveIndex(offset, 8)).Take(8).ToArray();


            bytes = AdjustEndian(bytes, littleEndian);

            return System.BitConverter.ToUInt64(bytes, 0);
        }

        // ------- Set Methods -------

        public void SetFloat32(long offset, float value, bool littleEndian = false)
        {
            var bytes = AdjustEndian(System.BitConverter.GetBytes(value), littleEndian);
            SetBytes(offset, bytes);
        }

        public void SetFloat64(long offset, double value, bool littleEndian = false)
        {
            var bytes = AdjustEndian(System.BitConverter.GetBytes(value), littleEndian);
            SetBytes(offset, bytes);
        }

        public void SetInt8(long offset, sbyte value)
        {
            Buffer[ResolveIndex(offset, 1)] = (byte)value;
        }

        public void SetInt16(long offset, short value, bool littleEndian = false)
        {
            var bytes = AdjustEndian(System.BitConverter.GetBytes(value), littleEndian);
            SetBytes(offset, bytes);
        }

        public void SetInt32(long offset, int value, bool littleEndian = false)
        {
            var bytes = AdjustEndian(System.BitConverter.GetBytes(value), littleEndian);
            SetBytes(offset, bytes);
        }

        public void SetUint8(long offset, byte value)
        {
            Buffer[ResolveIndex(offset, 1)] = value;
        }

        public void SetUint16(long offset, ushort value, bool littleEndian = false)
        {
            var bytes = AdjustEndian(System.BitConverter.GetBytes(value), littleEndian);
            SetBytes(offset, bytes);
        }

        public void SetUint32(long offset, uint value, bool littleEndian = false)
        {
            var bytes = AdjustEndian(System.BitConverter.GetBytes(value), littleEndian);
            SetBytes(offset, bytes);
        }

        public void SetBigInt64(long offset, long value, bool littleEndian = false)
        {
            var bytes = System.BitConverter.GetBytes(value);
            bytes = AdjustEndian(bytes, littleEndian);

            SetBytes(offset, bytes);
        }

        public void SetBigUint64(long offset, ulong value, bool littleEndian = false)
        {
            var bytes = System.BitConverter.GetBytes(value);
            bytes = AdjustEndian(bytes, littleEndian);
            SetBytes(offset, bytes);
        }

        private void SetBytes(long offset, byte[] bytes)
        {
            int index = ResolveIndex(offset, bytes.Length);
            for (int i = 0; i < bytes.Length; i++)
                Buffer[index + i] = bytes[i];
        }
    }
}
