using System.Drawing;
using System.Numerics;
using Sprout.Graphics.ShaderUtils;
using Sprout.Graphics.Tests.Base;
using STexture = Sprout.Graphics.Texture;

namespace Sprout.Graphics.Tests.Texture;

public class TextureTest() : TestBase("Texture Test")
{
    private STexture _texture;
    private Shader _shader;
    private Renderable _renderable;

    private float _rotation;

    protected override void Load()
    {
        _texture = Device.CreateTexture("DEBUG.png");

        string hlsl = File.ReadAllText("Shader.hlsl");
        byte[] vtx = Compiler.TranspileHLSL(Device.Backend, ShaderStage.Vertex, hlsl, "VSMain");
        byte[] pxl = Compiler.TranspileHLSL(Device.Backend, ShaderStage.Pixel, hlsl, "PSMain");

        _shader = Device.CreateShader(new ShaderAttachment(ShaderStage.Vertex, vtx, "VSMain"),
            new ShaderAttachment(ShaderStage.Pixel, pxl, "PSMain"));
        
        ReadOnlySpan<float> vertices =
        [
            -0.5f, -0.5f,    0.0f, 1.0f,
            -0.5f,  0.5f,    0.0f, 0.0f,
             0.5f,  0.5f,    1.0f, 0.0f,
             0.5f, -0.5f,    1.0f, 1.0f
        ];

        ReadOnlySpan<uint> indices =
        [
            0, 1, 3,
            1, 2, 3
        ];

        RenderableInfo info = new()
        {
            NumVertices = (uint) vertices.Length,
            VertexSize = 4 * sizeof(float),
            NumIndices = (uint) indices.Length,
            Shader = _shader,
            VertexInput =
            [
                new VertexAttribute(0, Semantic.Position, 0, AttributeType.Float2, 0),
                new VertexAttribute(1, Semantic.TexCoord, 0, AttributeType.Float2, 8)
            ],
            Uniforms =
            [
                Uniform.Texture(0),
                Uniform.ConstantBuffer<Matrix4x4>(1)
            ]
        };

        _renderable = Device.CreateRenderable(in info);
        _renderable.UpdateVertices(0, vertices);
        _renderable.UpdateIndices(0, indices);
    }

    protected override void Loop(float dt)
    {
        _rotation += dt;
        if (_rotation >= float.Pi * 2)
            _rotation -= float.Pi * 2;
        
        Device.Clear(Color.CornflowerBlue);
        _renderable.PushTexture(0, _texture);
        _renderable.PushUniformData(1, Matrix4x4.CreateRotationZ(_rotation));
        _renderable.Draw();
        Device.Present();
    }

    public override void Dispose()
    {
        _renderable.Dispose();
        _shader.Dispose();
        _texture.Dispose();
        
        base.Dispose();
    }
}