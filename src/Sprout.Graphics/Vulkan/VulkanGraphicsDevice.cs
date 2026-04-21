using System.Drawing;

namespace Sprout.Graphics.Vulkan;

internal sealed class VulkanGraphicsDevice : GraphicsDevice
{
    public override bool IsDisposed { get; protected set; }

    public override Backend Backend => Backend.Vulkan;
    
    public override Size SwapchainSize { get; }
    
    public override Viewport Viewport { get; set; }
    
    public override BlendMode BlendMode { get; set; }

    public VulkanGraphicsDevice(IntPtr sdlWindow)
    {
        
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
        if (IsDisposed)
            return;
        IsDisposed = true;
    }
}