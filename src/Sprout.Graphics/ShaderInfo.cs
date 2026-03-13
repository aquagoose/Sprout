namespace Sprout.Graphics;

public struct ShaderInfo
{
    public ShaderAttachment? VertexShader;

    public ShaderAttachment? PixelShader;

    public Uniform[]? Uniforms;

    public ShaderInfo(ShaderAttachment? vertexShader, ShaderAttachment? pixelShader, Uniform[]? uniforms)
    {
        VertexShader = vertexShader;
        PixelShader = pixelShader;
        Uniforms = uniforms;
    }
}