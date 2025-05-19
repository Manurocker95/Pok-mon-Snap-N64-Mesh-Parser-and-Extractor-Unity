namespace VirtualPhenix.Nintendo64
{
    public enum GfxChannelWriteMask
    {
        None = 0x00,
        Red = 0x01,
        Green = 0x02,
        Blue = 0x04,
        Alpha = 0x08,
        RGB = 0x07,
        AllChannels = 0x0F
    }
}