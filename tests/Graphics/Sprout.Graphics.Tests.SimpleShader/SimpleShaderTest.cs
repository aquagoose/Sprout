using Sprout.Graphics.Tests.Base;

namespace Sprout.Graphics.Tests.SimpleShader;

public class SimpleShaderTest() : TestBase("Simple Shader Test")
{
    private Shader _shader;
    
    protected override void Load()
    {
        string hlsl = File.ReadAllText("Shader.hlsl");
        _shader = Device.CreateShader(new ShaderAttachment(ShaderStage.Vertex, hlsl, "VSMain"),
            new ShaderAttachment(ShaderStage.Pixel, hlsl, "PSMain"));
    }

    protected override void Loop(float dt)
    {
        
    }

    public override void Dispose()
    {
        _shader.Dispose();
        
        base.Dispose();
    }
}