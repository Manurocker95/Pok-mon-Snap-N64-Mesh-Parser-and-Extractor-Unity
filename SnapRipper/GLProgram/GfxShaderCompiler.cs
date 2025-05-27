using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public static class GfxShaderCompiler
    {
        public static string PreprocessShader_GLSL(GfxVendorInfo vendorInfo, string type, string source, Dictionary<string, string> defines = null, int maxSamplerBinding = -1)
        {
            // This is dummy as we don't use this
            return string.Empty;

        }
    }
}
