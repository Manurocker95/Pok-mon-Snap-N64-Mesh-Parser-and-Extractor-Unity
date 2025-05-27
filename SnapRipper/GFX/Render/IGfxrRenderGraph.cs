using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public interface IGfxrRenderGraph
    {
        IGfxrGraphBuilder NewGraphBuilder();
        void Execute(IGfxrGraphBuilder builder);
        void Destroy();
    }

}
