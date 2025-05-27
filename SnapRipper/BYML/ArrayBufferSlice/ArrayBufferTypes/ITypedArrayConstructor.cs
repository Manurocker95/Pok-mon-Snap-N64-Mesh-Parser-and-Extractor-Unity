using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public interface ITypedArrayConstructor<T>
    {
        int BytesPerElement { get; }
        T Create(IArrayBufferLike buffer, long byteOffset, long length = -1);
    }

}
