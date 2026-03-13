namespace Sprout.Graphics;

public struct ShaderAttachment
{
    public byte[] Source;

    public string EntryPoint;

    public ShaderAttachment(byte[] source, string entryPoint)
    {
        Source = source;
        EntryPoint = entryPoint;
    }
}