using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class VP_Partial<T> where T : class, new()
    {
        public T Value { get; set; }

        public VP_Partial()
        {
            Value = new T();
        }

        public VP_Partial(T existing)
        {
            Value = existing;
        }
    }
}
