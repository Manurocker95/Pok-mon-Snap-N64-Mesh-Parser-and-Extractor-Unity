using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class ObjParam
    {
        public float Constant;        
        public StoredValue Stored;    

        public static ObjParam FromFloat(float value) => new ObjParam { Constant = value };
        public static ObjParam FromStored(StoredValue stored) => new ObjParam { Stored = stored };
    }
}
