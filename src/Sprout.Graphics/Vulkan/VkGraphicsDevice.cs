using SDL3;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Image = Silk.NET.Vulkan.Image;

namespace Sprout.Graphics.Vulkan;

internal sealed unsafe class VkGraphicsDevice : GraphicsDevice
{
    private const uint FramesInFlight = 3;
    
    public override bool IsDisposed { get; protected set; }

    private readonly Vk _vk;
    private readonly KhrSurface _khrSurface;
    private readonly KhrSwapchain _khrSwapchain;
    
    private readonly Instance _instance;
    private readonly SurfaceKHR _surface;
    private readonly PhysicalDevice _physicalDevice;
    private readonly VkHelper.Queues _queues;
    private readonly Device _device;

    private SwapchainKHR _swapchain;
    private Image[] _swapchainImages;
    private ImageView[] _swapchainImageViews;
    
    public override Backend Backend => Backend.Vulkan;

    public VkGraphicsDevice(IntPtr sdlWindow)
    {
        _vk = Vk.GetApi();
        _instance = VkHelper.CreateInstance(_vk, AppDomain.CurrentDomain.FriendlyName);
        
        if (!_vk.TryGetInstanceExtension(_instance, out _khrSurface))
            throw new Exception("Failed to get KhrSurface extension!");
        
        if (!SDL.VulkanCreateSurface(sdlWindow, _instance.Handle, IntPtr.Zero, out IntPtr surface))
            throw new Exception($"Failed to create surface: {SDL.GetError()}");
        _surface = new SurfaceKHR((ulong) surface);
        
        _physicalDevice = VkHelper.PickPhysicalDevice(_vk, _instance, out _queues);
        _device = VkHelper.CreateDevice(_vk, _physicalDevice, ref _queues);
        
        if (!_vk.TryGetDeviceExtension(_instance, _device, out _khrSwapchain))
            throw new Exception("Failed to get KhrSwapchain extension!");

        _swapchain = VkHelper.CreateSwapchain(_khrSwapchain, _physicalDevice, _device, in _queues, _surface,
            _khrSurface, sdlWindow, new SwapchainKHR(), out Format format);
        _swapchainImages = VkHelper.GetSwapchainImages(_device, _khrSwapchain, _swapchain);

        _swapchainImageViews = new ImageView[_swapchainImages.Length];
        for (int i = 0; i < _swapchainImages.Length; i++)
            _swapchainImageViews[i] = VkHelper.CreateImageView(_vk, _device, _swapchainImages[i], format);
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
        
        foreach (ImageView view in _swapchainImageViews)
            _vk.DestroyImageView(_device, view, null);
        _khrSwapchain.DestroySwapchain(_device, _swapchain, null);
        _khrSwapchain.Dispose();
        _vk.DestroyDevice(_device, null);
        SDL.VulkanDestroySurface(_instance.Handle, (nint) _surface.Handle, IntPtr.Zero);
        _khrSurface.Dispose();
        _vk.DestroyInstance(_instance, null);
        _vk.Dispose();
    }
}