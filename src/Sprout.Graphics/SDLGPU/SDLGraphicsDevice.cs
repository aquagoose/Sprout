using System.Drawing;
using SDL3;

namespace Sprout.Graphics.SDLGPU;

internal sealed class SDLGraphicsDevice : GraphicsDevice
{
    public override bool IsDisposed { get; protected set; }

    private readonly IntPtr _window;
    private readonly IntPtr _device;

    public IntPtr CommandBuffer;
    public IntPtr RenderPass;
    private IntPtr _swapchainTexture;
    private bool _hasAttemptedToAcquireSwapchainTextureThisFrame;
    
    private Color _clearColor;
    private bool _shouldClearThisFrame;
    
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
        _shouldClearThisFrame = true;
        _clearColor = color;
    }
    
    public override void Present()
    {
        if (!EnsureInRenderPass())
        {
            ResetState();
            return;
        }
        
        SDL.EndGPURenderPass(RenderPass);
        SDL.SubmitGPUCommandBuffer(CommandBuffer).Check("Submit command buffer");
        
        ResetState();
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
    
    public unsafe bool EnsureInRenderPass()
    {
        if (RenderPass != IntPtr.Zero)
            return true;
        
        if (_hasAttemptedToAcquireSwapchainTextureThisFrame)
            return false;
        
        if (CommandBuffer == IntPtr.Zero)
            CommandBuffer = SDL.AcquireGPUCommandBuffer(_device).Check("Acquire command buffer");
        
        if (_swapchainTexture == IntPtr.Zero)
        {
            _hasAttemptedToAcquireSwapchainTextureThisFrame = true;
            SDL.WaitAndAcquireGPUSwapchainTexture(CommandBuffer, _window, out _swapchainTexture, out _, out _)
                .Check("Acquire swapchain texture");

            if (_swapchainTexture == IntPtr.Zero)
            {
                SDL.CancelGPUCommandBuffer(CommandBuffer);
                CommandBuffer = IntPtr.Zero;
                return false;
            }
        }

        SDL.GPUColorTargetInfo colorTarget = new()
        {
            Texture = _swapchainTexture,
            ClearColor = new SDL.FColor(_clearColor.R, _clearColor.G, _clearColor.B, _clearColor.A),
            LoadOp = _shouldClearThisFrame ? SDL.GPULoadOp.Clear : SDL.GPULoadOp.Load,
            StoreOp = SDL.GPUStoreOp.Store
        };

        RenderPass = SDL.BeginGPURenderPass(CommandBuffer, new IntPtr(&colorTarget), 1, IntPtr.Zero)
            .Check("Begin render pass");

        return true;
    }

    private void ResetState()
    {
        _shouldClearThisFrame = false;
        _hasAttemptedToAcquireSwapchainTextureThisFrame = false;
        RenderPass = IntPtr.Zero;
        CommandBuffer = IntPtr.Zero;
        _swapchainTexture = IntPtr.Zero;
    }
}