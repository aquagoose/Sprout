using System.Drawing;
using Silk.NET.Vulkan;

namespace Sprout.Graphics.Vulkan;

internal sealed unsafe class VulkanGraphicsDevice : GraphicsDevice
{
    public override bool IsDisposed { get; protected set; }

    private readonly Vk _vk;
    private readonly Instance _instance;
    private readonly PhysicalDevice _physicalDevice;
    private readonly Queues _queues;

    private readonly Device _device;

    public override Backend Backend => Backend.Vulkan;
    
    public override Size SwapchainSize { get; }
    
    public override Viewport Viewport { get; set; }
    
    public override BlendMode BlendMode { get; set; }

    public VulkanGraphicsDevice(IntPtr sdlWindow)
    {
        _vk = Vk.GetApi();
        _instance = VulkanUtils.CreateInstance(_vk, sdlWindow);
        _physicalDevice = VulkanUtils.PickPhysicalDevice(_vk, _instance, out _queues, out string deviceName);
        Console.WriteLine(deviceName);
        _device = VulkanUtils.CreateDevice(_vk, _physicalDevice, ref _queues);
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
        
        _vk.DeviceWaitIdle(_device).Check("Wait for device idle");
        
        _vk.DestroyDevice(_device, null);
        _vk.DestroyInstance(_instance, null);
        _vk.Dispose();
    }
}