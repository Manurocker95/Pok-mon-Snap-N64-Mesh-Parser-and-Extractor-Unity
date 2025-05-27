using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public interface Expiry 
    {
        public long ExpireFrameNum { get; set; }
    }
}
