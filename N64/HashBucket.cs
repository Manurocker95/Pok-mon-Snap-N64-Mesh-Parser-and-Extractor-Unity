using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    [System.Serializable]
    public class HashBucket<K, V>
    {
        public List<K> Keys = new List<K>();
        public List<V> Values = new List<V>();
    }
}
