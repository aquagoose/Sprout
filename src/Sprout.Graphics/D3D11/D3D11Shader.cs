using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using TerraFX.Interop.DirectX;
using static TerraFX.Interop.DirectX.D3D11_BIND_FLAG;
using static TerraFX.Interop.DirectX.D3D11_CPU_ACCESS_FLAG;
using static TerraFX.Interop.DirectX.D3D11_MAP;
using static TerraFX.Interop.DirectX.D3D11_USAGE;

namespace Sprout.Graphics.D3D11;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
internal sealed unsafe class D3D11Shader : Shader
{
    public override bool IsDisposed { get; protected set; }

    private readonly D3D11GraphicsDevice _device;
    
    public readonly ID3D11VertexShader* VertexShader;
    public readonly ID3D11PixelShader* PixelShader;

    public readonly byte[] VertexShaderSource;
    public readonly Dictionary<uint, DXUniform>? Uniforms;

    public D3D11Shader(D3D11GraphicsDevice device, ref readonly ShaderInfo info)
    {
        _device = device;
        ID3D11Device* d3dDevice = _device.Device;
        
        if (info.VertexShader is ShaderAttachment vertexAttachment)
        {
            fixed (ID3D11VertexShader** vertexShader = &VertexShader)
            fixed (byte* pSource = vertexAttachment.Source)
            {
                d3dDevice->CreateVertexShader(pSource, (nuint) vertexAttachment.Source.Length, null, vertexShader)
                    .Check("Create vertex shader");

                VertexShaderSource = vertexAttachment.Source;
            }
        }

        if (info.PixelShader is ShaderAttachment pixelAttachment)
        {
            fixed (ID3D11PixelShader** pixelShader = &PixelShader)
            fixed (byte* pSource = pixelAttachment.Source)
            {
                d3dDevice->CreatePixelShader(pSource, (nuint) pixelAttachment.Source.Length, null, pixelShader)
                    .Check("Create pixel shader");
            }
        }
        
        if (info.Uniforms != null)
        {
            Uniforms = new Dictionary<uint, DXUniform>(info.Uniforms.Length);
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
                        d3dDevice->CreateBuffer(&cbufferDesc, null, &cbuffer).Check("Create constant buffer");

                        dxUniform = new DXUniform(UniformType.ConstantBuffer, uniform.ConstantBufferSize, cbuffer);
                        break;
                    }
                    
                    case UniformType.Texture:
                        dxUniform = new DXUniform(UniformType.Texture, 0, null);
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                Uniforms.Add(uniform.Index, dxUniform);
            }
        }
    }
    
    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;

        if (PixelShader != null)
            PixelShader->Release();

        if (VertexShader != null)
            VertexShader->Release();
    }
    
    public override void PushUniformData(uint index, uint offset, uint sizeInBytes, void* pData)
    {
        if (!(Uniforms?.TryGetValue(index, out DXUniform uniform) ?? false))
            throw new Exception("Invalid uniform index!");
        
        Debug.Assert(uniform.Type == UniformType.ConstantBuffer, "Uniform index is not a Constant Buffer uniform");
        Debug.Assert(offset + sizeInBytes >= uniform.ConstantBufferSize);

        ID3D11DeviceContext* context = _device.Context;
        
        D3D11_MAPPED_SUBRESOURCE subresource;
        context->Map((ID3D11Resource*) uniform.ConstantBuffer, 0, D3D11_MAP_WRITE_DISCARD, 0, &subresource)
            .Check("Map buffer");

        Unsafe.CopyBlock(subresource.pData, pData, sizeInBytes);
        
        context->Unmap((ID3D11Resource*) uniform.ConstantBuffer, 0);
    }
    
    public override void PushTexture(uint index, Texture texture)
    {
        if (!(Uniforms?.TryGetValue(index, out DXUniform uniform) ?? false))
            throw new Exception("Invalid uniform index!");
        
        Debug.Assert(uniform.Type == UniformType.Texture, "Texture index is not a texture uniform");
        
        D3D11Texture d3dTexture = (D3D11Texture) texture;
        uniform.CurrentTexture = d3dTexture.TextureSrv;
    }

    
    // TODO: Again make classn't as written in GLShader 
    public class DXUniform
    {
        public readonly UniformType Type;

        public readonly uint ConstantBufferSize;
        
        public readonly ID3D11Buffer* ConstantBuffer;

        public ID3D11ShaderResourceView* CurrentTexture;

        public ID3D11SamplerState* CurrentSampler;

        public DXUniform(UniformType type, uint constantBufferSize, ID3D11Buffer* constantBuffer)
        {
            Type = type;
            ConstantBufferSize = constantBufferSize;
            ConstantBuffer = constantBuffer;
        }
    }
}