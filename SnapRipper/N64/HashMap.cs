using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public delegate T CopyFunc<T>(T a);
    public delegate bool EqualFunc<K>(K a, K b);
    public delegate long HashFunc<K>(K a);

    [System.Serializable]
    public class HashMap<K, V>
    {
        public Dictionary<long, HashBucket<K, V>> Buckets = new Dictionary<long, HashBucket<K, V>>();

        private EqualFunc<K> keyEqualFunc;
        private HashFunc<K> keyHashFunc;

        public HashMap(EqualFunc<K> keyEqualFunc, HashFunc<K> keyHashFunc)
        {
            this.keyEqualFunc = keyEqualFunc;
            this.keyHashFunc = keyHashFunc;
        }

        private long FindBucketIndex(HashBucket<K, V> bucket, K k)
        {
            for (long i = 0; i < bucket.Keys.Count; i++)
                if (this.keyEqualFunc(k, bucket.Keys[(int)i]))
                    return i;
            return -1;
        }

        public V Get(K k)
        {
            long bw = this.keyHashFunc(k);
            if (!this.Buckets.TryGetValue(bw, out var bucket))
                return default(V);
            long bi = this.FindBucketIndex(bucket, k);
            if (bi < 0)
                return default(V);
            return bucket.Values[(int)bi];
        }

        public void Add(K k, V v)
        {
            long bw = this.keyHashFunc(k);
            if (!this.Buckets.ContainsKey(bw))
                this.Buckets[bw] = new HashBucket<K, V>();
            var bucket = this.Buckets[bw];
            bucket.Keys.Add(k);
            bucket.Values.Add(v);
        }

        public void Delete(K k)
        {
            long bw = this.keyHashFunc(k);
            if (!this.Buckets.TryGetValue(bw, out var bucket))
                return;
            long bi = this.FindBucketIndex(bucket, k);
            if (bi == -1)
                return;
            bucket.Keys.RemoveAt((int)bi);
            bucket.Values.RemoveAt((int)bi);
            if (bucket.Keys.Count == 0)
                this.Buckets.Remove(bw);
        }

        public void Clear()
        {
            this.Buckets.Clear();
        }

        public long Size()
        {
            long acc = 0;
            foreach (var bucket in this.Buckets.Values)
                acc += bucket.Values.Count;
            return acc;
        }

        public IEnumerable<V> Values()
        {
            foreach (var bucket in this.Buckets.Values)
                for (int j = bucket.Values.Count - 1; j >= 0; j--)
                    yield return bucket.Values[j];
        }

        public IEnumerable<KeyValuePair<K, V>> Items()
        {
            foreach (var bucket in this.Buckets.Values)
                for (int j = bucket.Keys.Count - 1; j >= 0; j--)
                    yield return new KeyValuePair<K, V>(bucket.Keys[j], bucket.Values[j]);
        }
    }
}