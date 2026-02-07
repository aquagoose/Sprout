using Silk.NET.Vulkan;

namespace Sprout.Graphics.Vulkan;

internal sealed unsafe class VkGraphicsDevice : GraphicsDevice
{
    public override bool IsDisposed { get; protected set; }

    private readonly Vk _vk;
    private readonly Instance _instance;
    private readonly PhysicalDevice _physicalDevice;
    private readonly VkHelper.Queues _queues;
    private readonly Device _device;
    
    public override Backend Backend => Backend.Vulkan;

    public VkGraphicsDevice(IntPtr sdlWindow)
    {
        _vk = Vk.GetApi();
        _instance = VkHelper.CreateInstance(_vk, AppDomain.CurrentDomain.FriendlyName);
        _physicalDevice = VkHelper.PickPhysicalDevice(_vk, _instance, out _queues);
        _device = VkHelper.CreateDevice(_vk, _physicalDevice, ref _queues);
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
        
        _vk.DestroyDevice(_device, null);
        _vk.DestroyInstance(_instance, null);
        _vk.Dispose();
    }
}