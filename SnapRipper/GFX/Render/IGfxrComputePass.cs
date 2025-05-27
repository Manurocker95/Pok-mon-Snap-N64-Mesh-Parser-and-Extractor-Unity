using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public delegate void PassExecFunc(GfxRenderPass passRenderer, IGfxrPassScope scope);

    public delegate void ComputePassExecFunc(GfxComputePass pass, IGfxrPassScope scope);

    public delegate void PassPostFunc(IGfxrPassScope scope);

    public interface IGfxrComputePass : IGfxrPassBase
    {
        /// <summary>
        /// Set the pass's execution callback. This will be called with the GfxRenderPass for the
        /// pass, along with the GfxrPassScope to access any resources that the system has allocated.
        /// </summary>
        void Exec(ComputePassExecFunc func);
    }
}
