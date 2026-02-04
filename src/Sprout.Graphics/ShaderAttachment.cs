namespace Sprout.Graphics;

public struct ShaderAttachment
{
    public ShaderStage Stage;

    public string Source;

    public string EntryPoint;

    public ShaderAttachment(ShaderStage stage, string source, string entryPoint)
    {
        Stage = stage;
        Source = source;
        EntryPoint = entryPoint;
    }
}