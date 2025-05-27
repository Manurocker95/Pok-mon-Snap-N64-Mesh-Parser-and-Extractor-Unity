using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class SpriteData
    {
        public GfxBuffer VertexBuffer;
        public GfxBuffer IndexBuffer;
        public GfxInputLayout InputLayout;
        public List<GfxVertexBufferDescriptor> VertexBufferDescriptors;
        public GfxIndexBufferDescriptor IndexBufferDescriptor;
    }
}
