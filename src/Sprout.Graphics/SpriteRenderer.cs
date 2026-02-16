using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Sprout.Graphics;

public class SpriteRenderer : IDisposable
{
    /// <summary>
    /// The maximum number of sprites in a single batch. If any more sprites are added, the batch will automatically
    /// be flushed and a new batch will begin.
    /// </summary>
    public const uint MaxSpritesPerBatch = 1 << 16;

    private const uint NumVertices = 4;
    private const uint NumIndices = 6;

    private readonly Shader _shader;
    private readonly Renderable _renderable;

    private readonly Vertex[] _vertices;
    private readonly uint[] _indices;
    private readonly List<Sprite> _drawList;

    public SpriteRenderer(GraphicsDevice device)
    {
        PreCompiledShader pcsh = PreCompiledShader.FromEmbeddedResource("Sprout.Graphics.Shaders.SpriteRenderer.pcsh");
        byte[] vertexShader = pcsh.GetSource(device.Backend, ShaderStage.Vertex, out string vertexEntry);
        byte[] pixelShader = pcsh.GetSource(device.Backend, ShaderStage.Pixel, out string pixelEntry);

        _shader = device.CreateShader(new ShaderAttachment(ShaderStage.Vertex, vertexShader, vertexEntry),
            new ShaderAttachment(ShaderStage.Pixel, pixelShader, pixelEntry));

        RenderableInfo info = new()
        {
            NumVertices = NumVertices * MaxSpritesPerBatch,
            NumIndices = NumIndices * MaxSpritesPerBatch,
            VertexSize = Vertex.SizeInBytes,
            Shader = _shader,
            VertexInput =
            [
                new VertexAttribute(0, AttributeType.Float2, 0), // Position
                new VertexAttribute(1, AttributeType.Float2, 8), // TexCoord
                new VertexAttribute(2, AttributeType.Float4, 16) // Tint
            ],
            Uniforms =
            [
                new Uniform(0, UniformType.ConstantBuffer, TransformMatrices.SizeInBytes), // TransformMatrices
                new Uniform(1, UniformType.Texture) // Texture
            ],
            Dynamic = true
        };

        _renderable = device.CreateRenderable(in info);

        _vertices = new Vertex[NumVertices * MaxSpritesPerBatch];
        _indices = new uint[NumIndices * MaxSpritesPerBatch];
        _drawList = [];
    }

    public void Draw(Texture texture, Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight,
        Color tint)
    {
        _drawList.Add(new Sprite(texture, topLeft, topRight, bottomLeft, bottomRight,
            new Vector4(tint.R / 255.0f, tint.G / 255.0f, tint.B / 255.0f, tint.A / 255.0f)));
    }

    public void Draw(Texture texture, Vector2 position)
    {
        Size textureSize = texture.Size;
        
        Vector2 topLeft = position;
        Vector2 topRight = new Vector2(position.X + textureSize.Width, position.Y);
        Vector2 bottomLeft = new Vector2(position.X, position.Y + textureSize.Height);
        Vector2 bottomRight = new Vector2(topRight.X, bottomLeft.Y);
        
        _drawList.Add(new Sprite(texture, topLeft, topRight, bottomLeft, bottomRight, new Vector4(1, 1, 1, 1)));
    }

    /// <summary>
    /// Clear the draw list. Use this to "cancel", if you will not be calling <see cref="Render"/>.
    /// </summary> 
    public void Clear()
    {
        _drawList.Clear();
    }

    public void Render()
    {
        TransformMatrices matrices =
            new TransformMatrices(Matrix4x4.CreateOrthographicOffCenter(0, 800, 600, 0, -1, 1), Matrix4x4.Identity);
        
        _renderable.PushUniformData(0, matrices);
        
        Texture? currentTexture = null;
        uint currentDraw = 0;

        foreach (Sprite sprite in _drawList)
        {
            if (sprite.Texture != currentTexture)
            {
                Flush(currentDraw, currentTexture!);
                currentDraw = 0;
                currentTexture = sprite.Texture;
            }

            uint vOffset = currentDraw * NumVertices;
            uint iOffset = currentDraw * NumIndices;

            _vertices[vOffset + 0] = new Vertex(sprite.TopLeft, new Vector2(0, 0), sprite.Tint);
            _vertices[vOffset + 1] = new Vertex(sprite.TopRight, new Vector2(1, 0), sprite.Tint);
            _vertices[vOffset + 2] = new Vertex(sprite.BottomRight, new Vector2(1, 1), sprite.Tint);
            _vertices[vOffset + 3] = new Vertex(sprite.BottomLeft, new Vector2(0, 1), sprite.Tint);

            _indices[iOffset + 0] = 0 + vOffset;
            _indices[iOffset + 1] = 1 + vOffset;
            _indices[iOffset + 2] = 3 + vOffset;
            _indices[iOffset + 3] = 1 + vOffset;
            _indices[iOffset + 4] = 2 + vOffset;
            _indices[iOffset + 5] = 3 + vOffset;

            currentDraw++;
        }
        
        if (currentTexture != null)
            Flush(currentDraw, currentTexture);
        
        _drawList.Clear();
    }

    private void Flush(uint numDraws, Texture texture)
    {
        if (numDraws == 0)
            return;
        
        _renderable.UpdateVertices(0, _vertices);
        _renderable.UpdateIndices(0, _indices);
        
        _renderable.PushTexture(1, texture);
        _renderable.Draw(numDraws * NumIndices);
    }
    
    public void Dispose()
    {
        _renderable.Dispose();
        _shader.Dispose();
    }

    private readonly struct Sprite
    {
        public readonly Texture Texture;
        public readonly Vector2 TopLeft;
        public readonly Vector2 TopRight;
        public readonly Vector2 BottomLeft;
        public readonly Vector2 BottomRight;
        public readonly Vector4 Tint;

        public Sprite(Texture texture, Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight, Vector4 tint)
        {
            Texture = texture;
            TopLeft = topLeft;
            TopRight = topRight;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
            Tint = tint;
        }
    }

    private readonly struct Vertex
    {
        // (sizeof(Vector2) = 8, * 2 = 16, + sizeof(Vector4) = 16) = 32
        public const uint SizeInBytes = 32;
        
        public readonly Vector2 Position;
        public readonly Vector2 TexCoord;
        public readonly Vector4 Tint;

        public Vertex(Vector2 position, Vector2 texCoord, Vector4 tint)
        {
            Position = position;
            TexCoord = texCoord;
            Tint = tint;
        }
    }

    public readonly struct TransformMatrices
    {
        // sizeof(Matrix4x4) = 64, * 2 = 128
        public const uint SizeInBytes = 128;
        
        public readonly Matrix4x4 Projection;
        public readonly Matrix4x4 Transform;

        public TransformMatrices(Matrix4x4 projection, Matrix4x4 transform)
        {
            Projection = projection;
            Transform = transform;
        }
    }
}