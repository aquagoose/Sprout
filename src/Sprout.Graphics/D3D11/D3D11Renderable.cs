using System.Diagnostics.CodeAnalysis;
using TerraFX.Interop.DirectX;
using static TerraFX.Interop.DirectX.D3D_PRIMITIVE_TOPOLOGY;

namespace Sprout.Graphics.D3D11;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
internal sealed unsafe class D3D11Renderable : Renderable
{
    public override bool IsDisposed { get; protected set; }

    private readonly ID3D11DeviceContext* _context;

    private readonly D3D11Shader _shader;

    public D3D11Renderable(ID3D11Device* device, ID3D11DeviceContext* context, ref readonly RenderableInfo info)
    {
        _context = context;
        _shader = (D3D11Shader) info.Shader;
    }
    
    public override void UpdateVertices<T>(uint offset, ReadOnlySpan<T> vertices)
    {
        throw new NotImplementedException();
    }
    
    public override void UpdateIndices(uint offset, ReadOnlySpan<uint> indices)
    {
        throw new NotImplementedException();
    }
    
    public override unsafe void PushUniformData(uint index, uint offset, uint sizeInBytes, void* pData)
    {
        throw new NotImplementedException();
    }
    
    public override void PushTexture(uint index, Texture texture)
    {
        throw new NotImplementedException();
    }
    
    public override void Draw()
    {
        throw new NotImplementedException();
    }
    
    public override void Draw(uint numElements)
    {
        _context->VSSetShader(_shader.VertexShader, null, 0);
        _context->PSSetShader(_shader.PixelShader, null, 0);

        _context->IASetPrimitiveTopology(D3D_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

        _context->Draw(numElements, 0);
    }
    
    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
    }
}