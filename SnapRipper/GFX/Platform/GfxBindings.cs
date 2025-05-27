namespace VirtualPhenix.Nintendo64
{
    public interface GfxBindings : GfxResourceBase, GfxResource
    {
        GfxTypes GfxType 
        { 
            get 
            {
                return GfxTypes.Bindings;
            } 
        }
    }
}