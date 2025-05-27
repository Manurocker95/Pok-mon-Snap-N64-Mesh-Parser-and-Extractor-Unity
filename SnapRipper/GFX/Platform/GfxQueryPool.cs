namespace VirtualPhenix.Nintendo64
{
    public interface GfxQueryPool: GfxResourceBase, GfxResource
    {
        GfxTypes GfxType 
        { 
            get 
            {
                return GfxTypes.QueryPool;
            } 
        }
    }
}