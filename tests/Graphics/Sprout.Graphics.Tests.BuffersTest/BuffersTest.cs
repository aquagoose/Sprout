using System.Drawing;
using Sprout.Graphics.ShaderUtils;
using Sprout.Graphics.Tests.Base;

namespace Sprout.Graphics.Tests.BuffersTest;

public class BuffersTest() : TestBase("Buffers Test")
{
    private Shader _shader;
    private Renderable _renderable;
    
    protected override void Load()
    {
        string hlsl = File.ReadAllText("Shader.hlsl");
        byte[] vtx = Compiler.TranspileHLSL(Device.Backend, ShaderStage.Vertex, hlsl, "VSMain");
        byte[] pxl = Compiler.TranspileHLSL(Device.Backend, ShaderStage.Pixel, hlsl, "PSMain");

        _shader = Device.CreateShader(new ShaderAttachment(ShaderStage.Vertex, vtx, "VSMain"),
            new ShaderAttachment(ShaderStage.Pixel, pxl, "PSMain"));

        ReadOnlySpan<float> vertices =
        [
            -0.5f, -0.5f,   1.0f, 0.0f, 0.0f,
            -0.5f,  0.5f,   0.0f, 1.0f, 0.0f,
             0.5f,  0.5f,   0.0f, 0.0f, 1.0f,
             0.5f, -0.5f,   0.0f, 0.0f, 0.0f
        ];

        ReadOnlySpan<uint> indices =
        [
            0, 1, 3,
            1, 2, 3
        ];

        RenderableInfo info = new()
        {
            NumVertices = 4,
            VertexSize = 5 * sizeof(float),
            NumIndices = 6,
            Shader = _shader,
            VertexInput =
            [
                new VertexAttribute(0, AttributeType.Float2, 0),
                new VertexAttribute(1, AttributeType.Float3, 8)
            ]
        };
        
        _renderable = Device.CreateRenderable(in info);
        _renderable.UpdateVertices(0, vertices);
        _renderable.UpdateIndices(0, indices);
    }

    protected override void Loop(float dt)
    {
        Device.Clear(Color.CornflowerBlue);
        _renderable.Draw();
        Device.Present();
    }
}