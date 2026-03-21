using System.Drawing;
using SDL3;

namespace Sprout.Graphics.SDLGPU;

internal sealed class SDLGraphicsDevice : GraphicsDevice
{
    public override bool IsDisposed { get; protected set; }

    private readonly IntPtr _window;
    private readonly IntPtr _device;
    
    public override Backend Backend => Backend.SDL;
    
    public override Size SwapchainSize { get; }
    
    public override Viewport Viewport { get; set; }
    
    public override BlendMode BlendMode { get; set; }

    public SDLGraphicsDevice(IntPtr window)
    {
        _window = window;

        uint properties = SDL.CreateProperties();
        SDL.SetBooleanProperty(properties, SDL.Props.GPUDeviceCreateShadersSPIRVBoolean, true);

        if (OperatingSystem.IsWindows())
            SDL.SetBooleanProperty(properties, SDL.Props.GPUDeviceCreateShadersDXBCBoolean, true);

        if (OperatingSystem.IsMacOS())
            SDL.SetBooleanProperty(properties, SDL.Props.GPUDeviceCreateShadersMSLBoolean, true);
        
#if DEBUG
        SDL.SetBooleanProperty(properties, SDL.Props.GPUDeviceCreateDebugModeBoolean, true);
#endif

        _device = SDL.CreateGPUDeviceWithProperties(properties).Check("Create GPU device");
        SDL.ClaimWindowForGPUDevice(_device, _window).Check("Claim window for device");
    }
    
    public override Shader CreateShader(in ShaderInfo info)
    {
        throw new NotImplementedException();
    }
    protected override unsafe Texture CreateTexture(uint width, uint height, PixelFormat format, TextureUsage usage, void* data)
    {
        throw new NotImplementedException();
    }
    
    public override Renderable CreateRenderable(in RenderableInfo info)
    {
        throw new NotImplementedException();
    }
    
    public override void SetRenderTextures(ReadOnlySpan<Texture> colorTextures)
    {
        throw new NotImplementedException();
    }
    
    public override void Clear(Color color)
    {
        throw new NotImplementedException();
    }
    
    public override void Present()
    {
        throw new NotImplementedException();
    }
    
    public override void ResizeSwapchain(uint width, uint height)
    {
        throw new NotImplementedException();
    }
    
    public override void Dispose()
    {
        SDL.WaitForGPUIdle(_device);
        SDL.ReleaseWindowFromGPUDevice(_device, _window);
        SDL.DestroyGPUDevice(_device);
    }
}