using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]   
    public class SceneContext
    {
        public GfxDevice GFXDevice;
        public float InitialSceneTime;
        public ViewerRenderInput ViewerInput;

        public SceneContext()
        {

        }

        public SceneContext(GfxDevice device)
        {
            GFXDevice = device;
        }
    }
}
