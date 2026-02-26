using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using SDL3;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.DirectX.D3D_FEATURE_LEVEL;
using static TerraFX.Interop.DirectX.D3D11_CREATE_DEVICE_FLAG;
using static TerraFX.Interop.DirectX.D3D11_FILTER;
using static TerraFX.Interop.DirectX.D3D11_TEXTURE_ADDRESS_MODE;
using static TerraFX.Interop.DirectX.D3D11;
using static TerraFX.Interop.DirectX.DirectX;
using static TerraFX.Interop.DirectX.DXGI_FORMAT;
using static TerraFX.Interop.DirectX.DXGI;

namespace Sprout.Graphics.D3D11;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
internal sealed unsafe class D3D11GraphicsDevice : GraphicsDevice
{
    private const uint MaxRenderTargets = 8;
    
    public override bool IsDisposed { get; protected set; }

    private readonly IDXGISwapChain* _swapChain;

    private Size _swapchainSize;
    private ID3D11Texture2D* _swapchainTexture;
    private ID3D11RenderTargetView* _swapchainTarget;

    private ID3D11RenderTargetView*[] _currentRenderTargets;
    private Size _renderTargetSize;
    private uint _numBoundRenderTargets;
    
    private readonly Dictionary<Sampler, D3D11Sampler> _samplers;
    
    public readonly ID3D11Device* Device;
    public readonly ID3D11DeviceContext* Context;

    public override Backend Backend => Backend.D3D11;

    public override Size SwapchainSize => _swapchainSize;

    public override Viewport Viewport
    {
        get;
        set
        {
            field = value;
            
            D3D11_VIEWPORT viewport = new()
            {
                TopLeftX = value.X,
                TopLeftY = value.Y,
                Width = value.Width,
                Height = value.Height,
                MinDepth = 0,
                MaxDepth = 1
            };

            Context->RSSetViewports(1, &viewport);
        }
    }

    public D3D11GraphicsDevice(IntPtr sdlWindow)
    {
        _samplers = [];
        
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
        _swapchainSize = new Size(width, height);

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
        fixed (ID3D11Device** device = &Device)
        fixed (ID3D11DeviceContext** context = &Context)
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
            Device->CreateRenderTargetView((ID3D11Resource*) _swapchainTexture, null, swapchainTarget)
                .Check("Create swapchain target");

        _currentRenderTargets = new ID3D11RenderTargetView*[MaxRenderTargets];
        Viewport = new Viewport(0, 0, (uint) width, (uint) height);
    }
    
    public override Shader CreateShader(params ReadOnlySpan<ShaderAttachment> attachments)
    {
        return new D3D11Shader(Device, attachments);
    }
    
    protected override Texture CreateTexture(uint width, uint height, PixelFormat format, TextureUsage usage, void* data)
    {
        return new D3D11Texture(Device, Context, width, height, format, usage, data);
    }
    
    public override Renderable CreateRenderable(in RenderableInfo info)
    {
        return new D3D11Renderable(this, in info);
    }

    public override void SetRenderTextures(ReadOnlySpan<Texture> colorTextures)
    {
        if (colorTextures.Length == 0)
        {
            _numBoundRenderTargets = 0;
            return;
        }
        
        _numBoundRenderTargets = (uint) colorTextures.Length;
        for (int i = 0; i < colorTextures.Length; i++)
        {
            D3D11Texture d3dTexture = (D3D11Texture) colorTextures[i];
            _currentRenderTargets[i] = d3dTexture.RenderTarget;
        }

        fixed (ID3D11RenderTargetView** renderTargets = _currentRenderTargets)
            Context->OMSetRenderTargets(_numBoundRenderTargets, renderTargets, null);

        Texture texture = colorTextures[0];
        _renderTargetSize = texture.Size;
        Viewport = new Viewport(0, 0, (uint) _renderTargetSize.Width, (uint) _renderTargetSize.Height);
    }

    public override void Clear(Color color)
    {
        if (_numBoundRenderTargets == 0)
        {
            ID3D11RenderTargetView* target = _swapchainTarget;
            Context->OMSetRenderTargets(1, &target, null);
            Context->ClearRenderTargetView(target, &color.R);

            Viewport = new Viewport(0, 0, (uint) _swapchainSize.Width, (uint) _swapchainSize.Height);
        }
        else
        {
            // TODO: Is this necessary every frame? I vaguely remember on some AMD platforms that you need to set a
            //       render target every frame otherwise it might not render correctly, but this feels dumb.
            fixed (ID3D11RenderTargetView** renderTargets = _currentRenderTargets)
                Context->OMSetRenderTargets(_numBoundRenderTargets, renderTargets, null);
            for (uint i = 0; i < _numBoundRenderTargets; i++)
                Context->ClearRenderTargetView(_currentRenderTargets[i], &color.R);

            Viewport = new Viewport(0, 0, (uint) _renderTargetSize.Width, (uint) _renderTargetSize.Height);
        }
    }
    
    public override void Present()
    {
        _swapChain->Present(1, 0);
    }

    public override void ResizeSwapchain(uint width, uint height)
    {
        Context->ClearState();
        _swapchainTarget->Release();
        _swapchainTexture->Release();
        
        _swapChain->ResizeBuffers(0, width, height, DXGI_FORMAT_UNKNOWN, 0).Check("Resize swapchain");

        fixed (ID3D11Texture2D** swapchainTexture = &_swapchainTexture)
        {
            _swapChain->GetBuffer(0, Windows.__uuidof<ID3D11Texture2D>(), (void**) swapchainTexture)
                .Check("Get swapchain texture");
        }

        fixed (ID3D11RenderTargetView** swapchainTarget = &_swapchainTarget)
        {
            Device->CreateRenderTargetView((ID3D11Resource*) _swapchainTexture, null, swapchainTarget)
                .Check("Create swapchain target");
        }

        _swapchainSize = new Size((int) width, (int) height);
    }

    public ID3D11SamplerState* GetSampler(Sampler sampler)
    {
        if (_samplers.TryGetValue(sampler, out D3D11Sampler d3dSampler))
            return d3dSampler.Sampler;
        
        Console.WriteLine("Creating sampler");

        D3D11_FILTER filter = (sampler.MinFilter, sampler.MagFilter, sampler.MipFilter) switch
        {
            (Filter.Linear, Filter.Linear, Filter.Linear) => D3D11_FILTER_MIN_MAG_MIP_LINEAR,
            (Filter.Linear, Filter.Linear, Filter.Point) => D3D11_FILTER_MIN_MAG_LINEAR_MIP_POINT,
            (Filter.Linear, Filter.Point, Filter.Linear) => D3D11_FILTER_MIN_LINEAR_MAG_POINT_MIP_LINEAR,
            (Filter.Linear, Filter.Point, Filter.Point) => D3D11_FILTER_MIN_LINEAR_MAG_MIP_POINT,
            (Filter.Point, Filter.Linear, Filter.Linear) => D3D11_FILTER_MIN_POINT_MAG_MIP_LINEAR,
            (Filter.Point, Filter.Linear, Filter.Point) => D3D11_FILTER_MIN_POINT_MAG_LINEAR_MIP_POINT,
            (Filter.Point, Filter.Point, Filter.Linear) => D3D11_FILTER_MIN_MAG_POINT_MIP_LINEAR,
            (Filter.Point, Filter.Point, Filter.Point) => D3D11_FILTER_MIN_MAG_MIP_POINT
        };

        D3D11_SAMPLER_DESC samplerDesc = new()
        {
            Filter = filter,
            AddressU = sampler.AddressU.ToD3D(),
            AddressV = sampler.AddressV.ToD3D(),
            AddressW = D3D11_TEXTURE_ADDRESS_WRAP,
            MinLOD = 0,
            MaxLOD = float.MaxValue
        };

        ID3D11SamplerState* state;
        Device->CreateSamplerState(&samplerDesc, &state).Check("Create sampler state");

        d3dSampler = new D3D11Sampler(state);
        _samplers.Add(sampler, d3dSampler);

        return d3dSampler.Sampler;
    }
    
    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;

        _swapChain->Release();
        Context->Release();
        Device->Release();
    }

    public struct D3D11Sampler
    {
        public readonly ID3D11SamplerState* Sampler;

        public D3D11Sampler(ID3D11SamplerState* sampler)
        {
            Sampler = sampler;
        }
    }
}