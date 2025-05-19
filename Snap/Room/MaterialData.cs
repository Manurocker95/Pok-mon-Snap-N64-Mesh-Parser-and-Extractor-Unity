using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class MaterialData 
    {
        public MaterialFlags Flags;
        public long TextureStart;
        public long PaletteStart;
        public float Scale;
        public float Shift;
        public float Halve;
        public float yScale;
        public float xScale;

        public List<TileParams> Tiles = new List<TileParams>();
        public List<TextureParams> Textures = new List<TextureParams>();

        public float PrimaryLOD;
        public Vector4 PrimaryColor;
        public Vector4 EnviromentalColor;
        public Vector4 BlendColor;
        public Vector4 Diffuse;
        public Vector4 Ambient;

        public List<TextureEntry> UsedTextures = new List<TextureEntry>();
        public bool Optional;
    }
}
