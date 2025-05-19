using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public static class N64Utils 
    {
        // Jenkins One-at-a-Time hash
        public static long HashCodeNumberUpdate(long hash, long v)
        {
            hash += v;
            hash += (hash << 10);
            hash += (long)((ulong)hash >> 6);
            return (long)((ulong)hash & 0xFFFFFFFF);
        }

        public static long HashCodeNumberFinish(long hash)
        {
            hash += (hash << 3);
            hash ^= (long)((ulong)hash >> 11);
            hash += (hash << 15);
            return (long)((ulong)hash & 0xFFFFFFFF);
        }

        // Pass this as a hash function to use a one-bucket HashMap (equivalent to linear search in an array),
        // which can be efficient for small numbers of items.
        public static long NullHashFunc<T>(T k)
        {
            return 0;
        }
    }
}
