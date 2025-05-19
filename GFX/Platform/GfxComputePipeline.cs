namespace VirtualPhenix.Nintendo64
{
    public interface GfxComputePipeline: GfxResourceBase, GfxResource
    {
        GfxTypes GfxType 
        { 
            get 
            {
                return GfxTypes.ComputePipeline;
            } 
        }
    }
}