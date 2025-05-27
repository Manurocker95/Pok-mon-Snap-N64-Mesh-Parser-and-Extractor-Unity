namespace VirtualPhenix.Nintendo64
{
    public interface GfxProgram : GfxResourceBase, GfxResource
    {
        GfxTypes GfxType 
        { 
            get 
            {
                return GfxTypes.Program;
            } 
        }
    }
}