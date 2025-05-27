namespace VirtualPhenix.Nintendo64
{
    public interface GfxRenderTarget : GfxResourceBase, GfxResource
    {
        GfxTypes GfxType 
        { 
            get 
            {
                return GfxTypes.RenderTarget;
            } 
        }
    }
}