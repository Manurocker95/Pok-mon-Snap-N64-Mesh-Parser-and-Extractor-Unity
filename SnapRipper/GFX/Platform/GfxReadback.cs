namespace VirtualPhenix.Nintendo64
{
    public interface GfxReadback : GfxResourceBase, GfxResource
    {
        GfxTypes GfxType 
        { 
            get 
            {
                return GfxTypes.Readback;
            } 
        }
    }
}