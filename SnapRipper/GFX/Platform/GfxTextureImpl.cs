namespace VirtualPhenix.Nintendo64
{
    public interface GfxTextureImpl : GfxResourceBase
    {
        GfxTypes GfxType 
        { 
            get 
            {
                return GfxTypes.Texture;
            } 
        }
    }
}