namespace VirtualPhenix.Nintendo64
{
    public interface GfxBuffer : GfxResourceBase, GfxResource
    {
        GfxTypes GfxType 
        { 
            get 
            {
                return GfxTypes.Buffer;
            } 
        }
    }
}