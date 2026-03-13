using System.Drawing;
using Sprout.Graphics.Tests.Base;

namespace Sprout.Graphics.Tests.PCSH;

public class PCSHTest() : TestBase("PCSH Test")
{
    private Shader _shader = null!;
    private Renderable _renderable = null!;
    
    protected override void Load()
    {
        PreCompiledShader pcsh = PreCompiledShader.FromFile("Shader.pcsh");
        byte[] vtxSrc = pcsh.GetSource(Device.Backend, ShaderStage.Vertex, out string vtxEntry);
        byte[] pxlSrc = pcsh.GetSource(Device.Backend, ShaderStage.Pixel, out string pxlEntry);

        ShaderInfo shaderInfo = new()
        {
            VertexShader = new ShaderAttachment(vtxSrc, vtxEntry),
            PixelShader = new ShaderAttachment(pxlSrc, pxlEntry)
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
        Device.Clear(new Color(1.0f, 0.5f, 0.25f));
        _renderable.Draw(3);
        Device.Present();
    }

    public override void Dispose()
    {
        _shader.Dispose();
        
        base.Dispose();
    }
}