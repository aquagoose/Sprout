using Silk.NET.Vulkan;

namespace Sprout.Graphics.Vulkan;

internal sealed unsafe class VkGraphicsDevice : GraphicsDevice
{
    public override bool IsDisposed { get; protected set; }

    private Vk _vk;
    private Instance _instance;
    
    public override Backend Backend => Backend.Vulkan;

    public VkGraphicsDevice(IntPtr sdlWindow)
    {
        _vk = Vk.GetApi();
        _instance = VkHelper.CreateInstance(_vk, sdlWindow, AppDomain.CurrentDomain.FriendlyName);
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
    
    public override void Clear(float r, float g, float b, float a = 1)
    {
        throw new NotImplementedException();
    }
    
    public override void Present()
    {
        throw new NotImplementedException();
    }
    
    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        
        _vk.DestroyInstance(_instance, null);
        _vk.Dispose();
    }
}