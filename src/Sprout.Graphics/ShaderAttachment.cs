namespace Sprout.Graphics;

public struct ShaderAttachment
{
    public ShaderStage Stage;

    public byte[] Source;

    public string EntryPoint;

    public ShaderAttachment(ShaderStage stage, byte[] source, string entryPoint)
    {
        Stage = stage;
        Source = source;
        EntryPoint = entryPoint;
    }
}