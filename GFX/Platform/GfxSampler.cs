namespace VirtualPhenix.Nintendo64
{
    public interface GfxSampler : GfxResourceBase, GfxResource
    {
        GfxTypes GfxType 
        { 
            get 
            {
                return GfxTypes.Sampler;
            } 
        }
    }
}