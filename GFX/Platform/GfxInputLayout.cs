namespace VirtualPhenix.Nintendo64
{
    public interface GfxInputLayout: GfxResourceBase, GfxResource
    {
        GfxTypes GfxType 
        { 
            get 
            {
                return GfxTypes.InputLayout;
            } 
        }
    }
}