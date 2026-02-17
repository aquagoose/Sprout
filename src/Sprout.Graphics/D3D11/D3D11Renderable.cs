using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TerraFX.Interop.DirectX;
using static TerraFX.Interop.DirectX.D3D_PRIMITIVE_TOPOLOGY;
using static TerraFX.Interop.DirectX.D3D11_BIND_FLAG;
using static TerraFX.Interop.DirectX.D3D11_CPU_ACCESS_FLAG;
using static TerraFX.Interop.DirectX.D3D11_INPUT_CLASSIFICATION;
using static TerraFX.Interop.DirectX.D3D11_MAP;
using static TerraFX.Interop.DirectX.D3D11_USAGE;
using static TerraFX.Interop.DirectX.DXGI_FORMAT;

namespace Sprout.Graphics.D3D11;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
internal sealed unsafe class D3D11Renderable : Renderable
{
    public override bool IsDisposed { get; protected set; }

    private readonly ID3D11DeviceContext* _context;
    private readonly D3D11Shader _shader;
    private readonly bool _isDynamic;

    private readonly ID3D11Buffer* _vertexBuffer;
    private readonly ID3D11Buffer* _indexBuffer;
    private readonly ID3D11InputLayout* _inputLayout;
    private readonly uint _stride;

    private readonly uint _numElements;
    private readonly Dictionary<uint, DXUniform>? _uniforms;

    public D3D11Renderable(ID3D11Device* device, ID3D11DeviceContext* context, ref readonly RenderableInfo info)
    {
        _context = context;
        _shader = (D3D11Shader) info.Shader;
        _isDynamic = info.Dynamic;

        if (info.NumVertices > 0)
        {
            Debug.Assert(info.VertexSize > 0);
            
            D3D11_BUFFER_DESC bufferDesc = new()
            {
                BindFlags = (uint) D3D11_BIND_VERTEX_BUFFER,
                ByteWidth = info.NumVertices * info.VertexSize,
                Usage = info.Dynamic ? D3D11_USAGE_DYNAMIC : D3D11_USAGE_DEFAULT,
                CPUAccessFlags = (uint) (info.Dynamic ? D3D11_CPU_ACCESS_WRITE : 0)
            };

            fixed (ID3D11Buffer** vertexBuffer = &_vertexBuffer)
                device->CreateBuffer(&bufferDesc, null, vertexBuffer).Check("Create vertex buffer");

            _numElements = info.NumVertices;
            _stride = info.VertexSize;
        }

        if (info.NumIndices > 0)
        {
            Debug.Assert(info.NumVertices > 0);

            D3D11_BUFFER_DESC bufferDesc = new()
            {
                BindFlags = (uint) D3D11_BIND_INDEX_BUFFER,
                ByteWidth = info.NumIndices * sizeof(uint),
                Usage = info.Dynamic ? D3D11_USAGE_DYNAMIC : D3D11_USAGE_DEFAULT,
                CPUAccessFlags = (uint) (info.Dynamic ? D3D11_CPU_ACCESS_WRITE : 0)
            };

            fixed (ID3D11Buffer** indexBuffer = &_indexBuffer)
                device->CreateBuffer(&bufferDesc, null, indexBuffer).Check("Create index buffer");

            _numElements = info.NumIndices;
        }

        if (info.VertexInput != null)
        {
            D3D11_INPUT_ELEMENT_DESC* inputElements = stackalloc D3D11_INPUT_ELEMENT_DESC[info.VertexInput.Length];
            
            for (int i = 0; i < info.VertexInput.Length; i++)
            {
                ref readonly VertexAttribute attribute = ref info.VertexInput[i];
                string semanticName = D3D11Utils.SemanticToString(attribute.Semantic);

                DXGI_FORMAT format = attribute.Type switch
                {
                    AttributeType.Float => DXGI_FORMAT_R32_FLOAT,
                    AttributeType.Float2 => DXGI_FORMAT_R32G32_FLOAT,
                    AttributeType.Float3 => DXGI_FORMAT_R32G32B32_FLOAT,
                    AttributeType.Float4 => DXGI_FORMAT_R32G32B32A32_FLOAT,
                    _ => throw new ArgumentOutOfRangeException()
                };

                inputElements[i] = new D3D11_INPUT_ELEMENT_DESC
                {
                    SemanticName = (sbyte*) Marshal.StringToHGlobalAnsi(semanticName),
                    SemanticIndex = attribute.SemanticIndex,
                    Format = format,
                    AlignedByteOffset = attribute.Offset,
                    InputSlot = 0,
                    InputSlotClass = D3D11_INPUT_PER_VERTEX_DATA,
                    InstanceDataStepRate = 0
                };
            }
            
            fixed (byte* pVertexSource = _shader.VertexShaderSource)
            fixed (ID3D11InputLayout** inputLayout = &_inputLayout)
            {
                device->CreateInputLayout(inputElements, (uint) info.VertexInput.Length, pVertexSource,
                    (nuint) _shader.VertexShaderSource.Length, inputLayout).Check("Create input layout");
            }
        }

        if (info.Uniforms != null)
        {
            _uniforms = new Dictionary<uint, DXUniform>(info.Uniforms.Length);
            for (int i = 0; i < info.Uniforms.Length; i++)
            {
                ref readonly Uniform uniform = ref info.Uniforms[i];

                DXUniform dxUniform;
                switch (uniform.Type)
                {
                    case UniformType.ConstantBuffer:
                    {
                        D3D11_BUFFER_DESC cbufferDesc = new()
                        {
                            BindFlags = (uint) D3D11_BIND_CONSTANT_BUFFER,
                            ByteWidth = uniform.ConstantBufferSize,
                            Usage = D3D11_USAGE_DYNAMIC,
                            CPUAccessFlags = (uint) D3D11_CPU_ACCESS_WRITE
                        };

                        ID3D11Buffer* cbuffer;
                        device->CreateBuffer(&cbufferDesc, null, &cbuffer).Check("Create constant buffer");

                        dxUniform = new DXUniform(UniformType.ConstantBuffer, uniform.ConstantBufferSize, cbuffer);
                        break;
                    }
                    
                    case UniformType.Texture:
                        dxUniform = new DXUniform(UniformType.Texture, 0, null);
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                _uniforms.Add(uniform.Index, dxUniform);
            }
        }
    }
    
    public override void UpdateVertices<T>(uint offset, ReadOnlySpan<T> vertices)
    {
        UpdateBuffer(_vertexBuffer, offset, vertices);
    }
    
    public override void UpdateIndices(uint offset, ReadOnlySpan<uint> indices)
    {
        UpdateBuffer(_indexBuffer, offset, indices);
    }
    
    public override unsafe void PushUniformData(uint index, uint offset, uint sizeInBytes, void* pData)
    {
        if (!(_uniforms?.TryGetValue(index, out DXUniform uniform) ?? false))
            throw new Exception("Invalid uniform index!");
        
        Debug.Assert(uniform.Type == UniformType.ConstantBuffer, "Uniform index is not a Constant Buffer uniform");
        Debug.Assert(offset + sizeInBytes >= uniform.ConstantBufferSize);

        D3D11_MAPPED_SUBRESOURCE subresource;
        _context->Map((ID3D11Resource*) uniform.ConstantBuffer, 0, D3D11_MAP_WRITE_DISCARD, 0, &subresource)
            .Check("Map buffer");

        Unsafe.CopyBlock(subresource.pData, pData, sizeInBytes);
        
        _context->Unmap((ID3D11Resource*) uniform.ConstantBuffer, 0);

        ID3D11Buffer* cbuffer = uniform.ConstantBuffer;
        _context->VSSetConstantBuffers(index, 1, &cbuffer);
    }
    
    public override void PushTexture(uint index, Texture texture)
    {
        Debug.Assert((_uniforms?.TryGetValue(index, out DXUniform uniform) ?? false) && uniform.Type == UniformType.Texture,
            "Texture index is not valid or is not a texture uniform");

        D3D11Texture d3dTexture = (D3D11Texture) texture;
        ID3D11ShaderResourceView* srv = d3dTexture.TextureSrv;
        _context->PSSetShaderResources(index, 1, &srv);
    }
    
    public override void Draw()
    {
        Draw(_numElements);
    }
    
    public override void Draw(uint numElements)
    {
        _context->VSSetShader(_shader.VertexShader, null, 0);
        _context->PSSetShader(_shader.PixelShader, null, 0);

        _context->IASetPrimitiveTopology(D3D_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

        if (_inputLayout != null)
        {
            _context->IASetInputLayout(_inputLayout);

            if (_vertexBuffer != null)
            {
                ID3D11Buffer* vertexBuffer = _vertexBuffer;
                uint stride = _stride;
                uint offset = 0;
                _context->IASetVertexBuffers(0, 1, &vertexBuffer, &stride, &offset);
            }

            if (_indexBuffer != null)
                _context->IASetIndexBuffer(_indexBuffer, DXGI_FORMAT_R32_UINT, 0);
        }

        if (_indexBuffer != null)
            _context->DrawIndexed(numElements, 0, 0);
        else
            _context->Draw(numElements, 0);
    }
    
    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;

        if (_inputLayout != null)
            _inputLayout->Release();

        if (_indexBuffer != null)
            _indexBuffer->Release();

        if (_vertexBuffer != null)
            _vertexBuffer->Release();
    }

    private void UpdateBuffer<T>(ID3D11Buffer* buffer, uint offset, ReadOnlySpan<T> data) where T : unmanaged
    {
        uint sizeInBytes = (uint) (data.Length * sizeof(T));
        
        if (_isDynamic)
        {
            Debug.Assert(offset == 0,
                "Offsets greater than 0 are not yet supported for dynamic renderables in the D3D11 backend");

            D3D11_MAPPED_SUBRESOURCE subresource;
            _context->Map((ID3D11Resource*) buffer, 0, D3D11_MAP_WRITE_DISCARD, 0, &subresource)
                .Check("Map buffer");

            fixed (void* pData = data)
                Unsafe.CopyBlock(subresource.pData, pData, sizeInBytes);

            _context->Unmap((ID3D11Resource*) buffer, 0);
        }
        else
        {
            D3D11_BOX box = new D3D11_BOX((int) offset, 0, 0, (int) (offset + sizeInBytes), 1, 1);
            fixed (void* pData = data)
                _context->UpdateSubresource((ID3D11Resource*) buffer, 0, &box, pData, 0, 0);
        }
    }

    private readonly struct DXUniform
    {
        public readonly UniformType Type;

        public readonly uint ConstantBufferSize;
        
        public readonly ID3D11Buffer* ConstantBuffer;

        public DXUniform(UniformType type, uint constantBufferSize, ID3D11Buffer* constantBuffer)
        {
            Type = type;
            ConstantBufferSize = constantBufferSize;
            ConstantBuffer = constantBuffer;
        }
    }
}