namespace VirtualPhenix.Nintendo64
{
    public interface GfxRenderPipeline: GfxResourceBase, GfxResource
    {
        GfxTypes GfxType 
        { 
            get 
            {
                return GfxTypes.RenderPipeline;
            } 
        }
    }
}