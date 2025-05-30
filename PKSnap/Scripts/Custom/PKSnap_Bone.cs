using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.PokemonSnap;

namespace VirtualPhenix.PokemonSnap64
{
    public class PKSnap_Bone : MonoBehaviour
    {
        public List<PKBoneData> BoneData = new List<PKBoneData>();



        public bool InitBone(PKSnap_Actor actor, NodeRenderer renderer, Transform trs, float _globalScale, int currentCount, bool _needToMirror, bool _debug = true)
        {
            BoneData = new List<PKBoneData>();
            
            transform.parent = trs;

            var a = renderer.ModelMatrix * renderer.Transform;

            
            Vector3 pos = renderer.Translation * _globalScale;
            Vector3 euler = renderer.Euler * Mathf.Rad2Deg;
            Vector3 scale = renderer.Scale;

            if (!_needToMirror)
            {
                pos.x *= -1;
                euler.y *= -1;
                euler.z *= -1;
               // scale = new Vector3(-1, 1, 1);
            }
       
            transform.localPosition = pos;
            transform.localEulerAngles = euler;
            transform.localScale = scale;

            if (_debug)
            {
                if (renderer.DrawCalls.Count == 0)
                {
                    //Debug.Log("["+currentCount + "] from ["+ actor.name+"] has no drawcalls");
                }
                else
                {
                    foreach (var drawCall in renderer.DrawCalls)
                    {
                        BoneData.Add(new PKBoneData()
                        {
                            FirstIndex =  drawCall.DrawCallInfo.FirstIndex,
                            IndexCount = drawCall.DrawCallInfo.IndexCount,
                        });
                    }
                    
                }
            }
            
            
            return true;
        }

        [System.Serializable]
        public class PKBoneData
        {
            public int FirstIndex = 0;
            public int IndexCount = 0; 
        }
    }
}