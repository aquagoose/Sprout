using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using TerraFX.Interop.DirectX;
using static TerraFX.Interop.DirectX.D3D_SRV_DIMENSION;
using static TerraFX.Interop.DirectX.D3D11_BIND_FLAG;
using static TerraFX.Interop.DirectX.D3D11_RESOURCE_MISC_FLAG;
using static TerraFX.Interop.DirectX.DXGI_FORMAT;

namespace Sprout.Graphics.D3D11;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
internal sealed unsafe class D3D11Texture : Texture
{
    public override bool IsDisposed { get; protected set; }

    public readonly ID3D11Texture2D* Texture;
    public readonly ID3D11ShaderResourceView* TextureSrv;

    public D3D11Texture(ID3D11Device* device, ID3D11DeviceContext* context, uint width, uint height, PixelFormat format,
        void* pData) : base(new Size((int) width, (int) height))
    {
        DXGI_FORMAT dxgiFormat = format switch
        {
            PixelFormat.RGBA8 => DXGI_FORMAT_R8G8B8A8_UNORM,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };

        D3D11_TEXTURE2D_DESC textureDesc = new()
        {
            Width = width,
            Height = height,
            Format = dxgiFormat,
            ArraySize = 1,
            MipLevels = 0,
            BindFlags = (uint) (D3D11_BIND_SHADER_RESOURCE | D3D11_BIND_RENDER_TARGET),
            Usage = D3D11_USAGE.D3D11_USAGE_DEFAULT,
            SampleDesc = new DXGI_SAMPLE_DESC(1, 0),
            MiscFlags = (uint) D3D11_RESOURCE_MISC_GENERATE_MIPS,
        };

        fixed (ID3D11Texture2D** texture = &Texture)
            device->CreateTexture2D(&textureDesc, null, texture).Check("Create texture");

        D3D11_SHADER_RESOURCE_VIEW_DESC srvDesc = new()
        {
            Format = dxgiFormat,
            ViewDimension = D3D11_SRV_DIMENSION_TEXTURE2D,
            Texture2D = new D3D11_TEX2D_SRV
            {
                MipLevels = uint.MaxValue,
                MostDetailedMip = 0
            }
        };

        fixed (ID3D11ShaderResourceView** textureSrv = &TextureSrv)
        {
            device->CreateShaderResourceView((ID3D11Resource*) Texture, &srvDesc, textureSrv)
                .Check("Create Texture SRV");
        }

        if (pData != null)
        {
            context->UpdateSubresource((ID3D11Resource*) Texture, 0, null, pData, 4 * width, 0);
            context->GenerateMips(TextureSrv);
        }
    }
    
    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;

        TextureSrv->Release();
        Texture->Release();
    }
}