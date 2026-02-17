using System.Diagnostics.CodeAnalysis;
using SDL3;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.DirectX.D3D_FEATURE_LEVEL;
using static TerraFX.Interop.DirectX.D3D11_CREATE_DEVICE_FLAG;
using static TerraFX.Interop.DirectX.D3D11;
using static TerraFX.Interop.DirectX.DirectX;
using static TerraFX.Interop.DirectX.DXGI_FORMAT;
using static TerraFX.Interop.DirectX.DXGI;

namespace Sprout.Graphics.D3D11;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
internal sealed unsafe class D3D11GraphicsDevice : GraphicsDevice
{
    public override bool IsDisposed { get; protected set; }

    private readonly IDXGISwapChain* _swapChain;
    private readonly ID3D11Device* _device;
    private readonly ID3D11DeviceContext* _context;

    private ID3D11Texture2D* _swapchainTexture;
    private ID3D11RenderTargetView* _swapchainTarget;

    public override Backend Backend => Backend.D3D11;

    public D3D11GraphicsDevice(IntPtr sdlWindow)
    {
        IntPtr hwnd;
        if (OperatingSystem.IsWindows())
        {
            uint props = SDL.GetWindowProperties(sdlWindow);
            hwnd = SDL.GetPointerProperty(props, SDL.Props.WindowWin32HWNDPointer, 0);
        }
        // DXVK compatibility
        else
            hwnd = sdlWindow;

        SDL.GetWindowSizeInPixels(sdlWindow, out int width, out int height);

        DXGI_SWAP_CHAIN_DESC swapchainDesc = new()
        {
            OutputWindow = (HWND) hwnd,
            Windowed = true,
            BufferCount = 2,
            BufferDesc = new DXGI_MODE_DESC
            {
                Width = (uint) width,
                Height = (uint) height,
                Format = DXGI_FORMAT_B8G8R8A8_UNORM
            },
            BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT,
            SampleDesc = new DXGI_SAMPLE_DESC(1, 0),
            SwapEffect = DXGI_SWAP_EFFECT.DXGI_SWAP_EFFECT_FLIP_DISCARD,
            Flags = 0
        };

        D3D11_CREATE_DEVICE_FLAG flags = D3D11_CREATE_DEVICE_BGRA_SUPPORT;
        D3D_FEATURE_LEVEL featureLevel = D3D_FEATURE_LEVEL_11_0;
        
        fixed (IDXGISwapChain** swapchain = &_swapChain)
        fixed (ID3D11Device** device = &_device)
        fixed (ID3D11DeviceContext** context = &_context)
        {
            D3D11CreateDeviceAndSwapChain(null, D3D_DRIVER_TYPE.D3D_DRIVER_TYPE_HARDWARE, HMODULE.NULL, (uint) flags,
                    &featureLevel, 1, D3D11_SDK_VERSION, &swapchainDesc, swapchain, device, null, context)
                .Check("Create device & swapchain");
        }

        fixed (ID3D11Texture2D** swapchainTexture = &_swapchainTexture)
        {
            _swapChain->GetBuffer(0, Windows.__uuidof<ID3D11Texture2D>(), (void**) swapchainTexture)
                .Check("Get swapchain texture");
        }

        fixed (ID3D11RenderTargetView** swapchainTarget = &_swapchainTarget)
            _device->CreateRenderTargetView((ID3D11Resource*) _swapchainTexture, null, swapchainTarget)
                .Check("Create swapchain target");
    }
    
    public override Shader CreateShader(params ReadOnlySpan<ShaderAttachment> attachments)
    {
        throw new NotImplementedException();
    }
    
    protected override unsafe Texture CreateTexture(uint width, uint height, PixelFormat format, void* data)
    {
        throw new NotImplementedException();
    }
    
    public override Renderable CreateRenderable(in RenderableInfo info)
    {
        throw new NotImplementedException();
    }
    
    public override void Clear(Color color)
    {
        ID3D11RenderTargetView* target = _swapchainTarget;
        _context->OMSetRenderTargets(1, &target, null);
        _context->ClearRenderTargetView(target, &color.R);
    }
    
    public override void Present()
    {
        _swapChain->Present(1, 0);
    }
    
    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;

        _swapChain->Release();
        _context->Release();
        _device->Release();
    }
}