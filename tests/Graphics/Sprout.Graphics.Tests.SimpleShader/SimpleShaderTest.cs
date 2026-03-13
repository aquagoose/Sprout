using System.Drawing;
using Sprout.Graphics.ShaderUtils;
using Sprout.Graphics.Tests.Base;

namespace Sprout.Graphics.Tests.SimpleShader;

public class SimpleShaderTest() : TestBase("Simple Shader Test")
{
    private Shader _shader = null!;
    private Renderable _renderable = null!;
    
    protected override void Load()
    {
        string hlsl = File.ReadAllText("Shader.hlsl");
        byte[] vtx = Compiler.TranspileHLSL(Device.Backend, ShaderStage.Vertex, hlsl, "VSMain");
        byte[] pxl = Compiler.TranspileHLSL(Device.Backend, ShaderStage.Pixel, hlsl, "PSMain");

        ShaderInfo shaderInfo = new()
        {
            VertexShader = new ShaderAttachment(vtx, "VSMain"),
            PixelShader = new ShaderAttachment(pxl, "PSMain")
        };
        
        _shader = Device.CreateShader(in shaderInfo);

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
        _renderable.Dispose();
        _shader.Dispose();
        
        base.Dispose();
    }
}