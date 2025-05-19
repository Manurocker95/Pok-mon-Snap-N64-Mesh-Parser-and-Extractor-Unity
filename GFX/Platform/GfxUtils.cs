using System.Collections.Generic;

namespace VirtualPhenix.Nintendo64
{
    public static class GfxUtils
    {
        public static List<T> ArrayCopy<T>(List<T> a, CopyFunc<T> copyFunc)
        {
            var b = new List<T>(a.Count);
            for (int i = 0; i < a.Count; i++)
                b.Add(copyFunc(a[i]));
            return b;
        }


        public static bool ArrayEqual<T>(List<T> a, List<T> b, EqualFunc<T> e)
        {
            if (a.Count != b.Count)
                return false;
            for (long i = 0; i < a.Count; i++)
                if (!e(a[(int)i], b[(int)i]))
                    return false;
            return true;
        }

        public static GfxBindingLayoutSamplerDescriptor DefaultBindingLayout()
        {
            var layout = new GfxBindingLayoutSamplerDescriptor()  
            {
                FormatKind = GfxSamplerFormatKind.Float,
                Dimension = GfxTextureDimension.n2D
            };

            return layout;
        }
      
        public static bool IsFormatSamplerKindCompatible(GfxSamplerFormatKind samplerKind, GfxSamplerFormatKind textureKind)
        {
            if (textureKind == samplerKind)
                return true;
            else if (samplerKind == GfxSamplerFormatKind.UnfilterableFloat && (textureKind == GfxSamplerFormatKind.Depth || textureKind == GfxSamplerFormatKind.Float))
                return true;

            return false;
        }

        public static uint MakeFormat(FormatTypeFlags type, FormatCompFlags comp, FormatFlags flags)
        {
            return ((uint)type << 16) | ((uint)comp << 8) | (uint)flags;
        }

        public static FormatCompFlags GetFormatCompFlags(GfxFormat fmt)
        {
            return fmt.GetFormatCompFlags();
        }

        public static FormatTypeFlags GetFormatTypeFlags(GfxFormat fmt)
        {
            return fmt.GetFormatTypeFlags();
        }

        public static FormatFlags GetFormatFlags(GfxFormat fmt)
        {
            return fmt.GetFormatFlags();
        }

        public static int GetFormatTypeFlagsByteSize(FormatTypeFlags typeFlags)
        {
            switch (typeFlags)
            {
                case FormatTypeFlags.F32:
                case FormatTypeFlags.U32:
                case FormatTypeFlags.S32:
                    return 4;

                case FormatTypeFlags.U16:
                case FormatTypeFlags.S16:
                case FormatTypeFlags.F16:
                    return 2;

                case FormatTypeFlags.U8:
                case FormatTypeFlags.S8:
                    return 1;

                default:
                    throw new System.Exception($"Unsupported FormatTypeFlags: {typeFlags}");
            }
        }
    }
}