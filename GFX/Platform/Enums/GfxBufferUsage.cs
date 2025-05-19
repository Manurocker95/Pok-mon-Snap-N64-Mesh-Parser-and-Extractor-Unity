namespace VirtualPhenix.Nintendo64
{
    public enum GfxBufferUsage : byte
    {
        Index   = 0b00001,
        Vertex  = 0b00010,
        Uniform = 0b00100,
        Storage = 0b01000,
        CopySrc = 0b10000
        // All buffers are implicitly CopyDst so they can be filled by the CPU... maybe they shouldn't be...
    }
}
