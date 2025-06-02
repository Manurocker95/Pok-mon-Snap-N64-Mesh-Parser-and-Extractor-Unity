using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.PokemonSnap;

namespace VirtualPhenix.PokemonSnap64
{
    public class PKSnap_Bone : MonoBehaviour
    {
        public List<PKBoneData> BoneData = new List<PKBoneData>();

        public static Quaternion N64EulerToUnityQuaternion(Vector3 eulerDeg)
        {
            // N64 orden Z -> Y -> X
            Quaternion rz = Quaternion.AngleAxis(eulerDeg.z, Vector3.forward);
            Quaternion ry = Quaternion.AngleAxis(eulerDeg.y, Vector3.up);
            Quaternion rx = Quaternion.AngleAxis(eulerDeg.x, Vector3.right);
            return rz * ry * rx;
        }

        public bool InitBone(PKSnap_Actor actor, NodeRenderer renderer, Transform trs, float _globalScale, int currentCount, bool _needToMirror, bool _debug = true)
        {
            BoneData = new List<PKBoneData>();
            
            transform.parent = trs;

            var a = renderer.ModelMatrix * renderer.Transform;

            
            Vector3 pos = renderer.Translation * _globalScale;
            Vector3 euler = renderer.Euler * Mathf.Rad2Deg;
            Vector3 scale = renderer.Scale;
            Quaternion rot = N64EulerToUnityQuaternion(euler);
            if (!_needToMirror)
            {
                pos.x *= -1;
               // scale = new Vector3(-1, 1, 1);
            }
       
            transform.localPosition = pos;
            transform.localRotation = rot;
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