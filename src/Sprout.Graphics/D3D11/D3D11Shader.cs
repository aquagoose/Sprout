using System.Diagnostics.CodeAnalysis;
using TerraFX.Interop.DirectX;

namespace Sprout.Graphics.D3D11;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
internal sealed unsafe class D3D11Shader : Shader
{
    public override bool IsDisposed { get; protected set; }

    public readonly ID3D11VertexShader* VertexShader;
    public readonly ID3D11PixelShader* PixelShader;

    public readonly byte[] VertexShaderSource;

    public D3D11Shader(ID3D11Device* device, ReadOnlySpan<ShaderAttachment> attachments)
    {
        for (int i = 0; i < attachments.Length; i++)
        {
            ref readonly ShaderAttachment attachment = ref attachments[i];

            fixed (byte* pSource = attachment.Source)
            {
                nuint sourceLength = (nuint) attachment.Source.Length;
                switch (attachment.Stage)
                {
                    case ShaderStage.Vertex:
                        fixed (ID3D11VertexShader** vertexShader = &VertexShader)
                        {
                            device->CreateVertexShader(pSource, sourceLength, null, vertexShader)
                                .Check("Create vertex shader");

                            VertexShaderSource = attachment.Source;
                        }
                        break;
                    case ShaderStage.Pixel:
                        fixed (ID3D11PixelShader** pixelShader = &PixelShader)
                        {
                            device->CreatePixelShader(pSource, sourceLength, null, pixelShader)
                                .Check("Create pixel shader");
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
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
}