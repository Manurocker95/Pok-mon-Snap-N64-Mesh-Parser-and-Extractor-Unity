using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.PokemonSnap;

namespace VirtualPhenix.Nintendo64
{
    public class GfxRenderHelper : GfxRenderHelperBase
    {
        private SceneContext _context;

        public GfxRenderHelper(GfxDevice device, SceneContext context = null, GfxRenderCache renderCache = null) : base(device, renderCache)
        {
            _context = context;
        }
    }
}
