using System.Drawing;
using Sprout.Graphics.Tests.Base;

namespace Sprout.Graphics.Tests.SimpleShader;

public class SimpleShaderTest() : TestBase("Simple Shader Test")
{
    private Shader _shader = null!;
    private Renderable _renderable = null!;
    
    protected override void Load()
    {
        string hlsl = File.ReadAllText("Shader.hlsl");
        _shader = Device.CreateShader(new ShaderAttachment(ShaderStage.Vertex, hlsl, "VSMain"),
            new ShaderAttachment(ShaderStage.Pixel, hlsl, "PSMain"));

        RenderableInfo info = new()
        {
            Shader = _shader
        };
        _renderable = Device.CreateRenderable(in info);
    }

    protected override void Loop(float dt)
    {
        Device.Clear(Color.CornflowerBlue);
        _renderable.Draw(3);
        Device.Present();
    }

    public override void Dispose()
    {
        _shader.Dispose();
        
        base.Dispose();
    }
}