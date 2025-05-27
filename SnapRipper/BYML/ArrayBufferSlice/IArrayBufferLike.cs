using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    /// <summary>
    /// Common Interface for any object that acts like a binary buffer (ArrayBuffer-like).
    /// </summary>
    public interface IArrayBufferLike
    {
        byte[] Buffer { get; }
        long ByteLength { get; }
        long LongLength { get; }
        object this[long index] { get; set; }
        int GetBytesPerElement();
        public IArrayBufferLike Slice(long? start = null, long? end = null);
    }
}
