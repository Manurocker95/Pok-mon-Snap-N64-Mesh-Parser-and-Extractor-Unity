using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class DeviceProgram : GfxRenderProgramDescriptor
    {
        public string Name = "(unnamed DeviceProgram)";

        // Inputs.
        public string Both = string.Empty;
        public string Vert = string.Empty;
        public string Frag = string.Empty;
        public Dictionary<string, string> Defines = new Dictionary<string, string>();

        public void DefinesChanged()
        {
            this.PreprocessedVert = string.Empty;
            this.PreprocessedFrag = string.Empty;
        }

        public bool SetDefineString(string name, string v)
        {
            if (v != null)
            {
                if (this.Defines.TryGetValue(name, out var current) && current == v)
                    return false;
                this.Defines[name] = v;
            }
            else
            {
                if (!this.Defines.ContainsKey(name))
                    return false;
                this.Defines.Remove(name);
            }
            this.DefinesChanged();
            return true;
        }

        public bool SetDefineBool(string name, bool v)
        {
            return this.SetDefineString(name, v ? "1" : null);
        }

        public string GetDefineString(string name)
        {
            return GfxPlatformUtils.Nullify(this.Defines.TryGetValue(name, out var value) ? value : null);
        }

        public bool GetDefineBool(string name)
        {
            var str = this.GetDefineString(name);
            if (str != null)
                GfxPlatformUtils.Assert(str == "1");
            return str != null;
        }

        public void EnsurePreprocessed(GfxVendorInfo vendorInfo)
        {
            if (this.PreprocessedVert == string.Empty)
            {
                this.PreprocessedVert = GfxShaderCompiler.PreprocessShader_GLSL(vendorInfo, "vert", this.Both + this.Vert, this.Defines);
                this.PreprocessedFrag = GfxShaderCompiler.PreprocessShader_GLSL(vendorInfo, "frag", this.Both + this.Frag, this.Defines);
            }
        }

        private GfxDevice _gfxDevice = null;
        private GfxProgram _gfxProgram = null;

        public void Associate(GfxDevice device, GfxProgram program)
        {
            this._gfxDevice = device;
            this._gfxProgram = program;

            if (this.Name == "(unnamed DeviceProgram)")
                this.Name = this.GetType().Name;

            this._gfxDevice.SetResourceName(program, this.Name);
        }

        private void _EditShader(string n /* "vert" | "frag" | "both" */)
        {

        }

        public void Editb()
        {
            this._EditShader("both");
        }

        public void Editv()
        {
            this._EditShader("vert");
        }

        public void Editf()
        {
            this._EditShader("frag");
        }

    }
}
