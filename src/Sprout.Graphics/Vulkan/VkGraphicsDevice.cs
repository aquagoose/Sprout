using System.Drawing;
using System.Runtime.CompilerServices;
using SDL3;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Sprout.Graphics.Vulkan.VMA;
using static Sprout.Graphics.Vulkan.VMA.Vma;
using Image = Silk.NET.Vulkan.Image;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace Sprout.Graphics.Vulkan;

internal sealed unsafe class VkGraphicsDevice : GraphicsDevice
{
    private const uint FramesInFlight = 3;
    
    public override bool IsDisposed { get; protected set; }

    private readonly Vk _vk;
    private readonly IntPtr _window;
    private readonly KhrSurface _khrSurface;
    private readonly KhrSwapchain _khrSwapchain;
    
    private readonly Instance _instance;
    private readonly SurfaceKHR _surface;
    private readonly PhysicalDevice _physicalDevice;
    private readonly VkHelper.Queues _queues;
    private readonly CommandPool _commandPool;

    private SwapchainKHR _swapchain;
    private Image[] _swapchainImages;
    private ImageView[] _swapchainImageViews;
    private Extent2D _swapchainSize;
    private uint _currentImage;

    private uint _currentFrameInFlight;
    private readonly Fence _fence;

    private readonly Queue<CommandBuffer> _availableCommandBuffers;
    private readonly Queue<Semaphore> _availableSemaphores;

    private readonly List<CommandBuffer> _finishedCommandBuffers;
    private readonly List<Semaphore> _finishedSemaphores;

    private readonly VkBuffer _transferBuffer;
    
    public readonly Device Device;
    public readonly Allocator* Allocator;

    public CommandBuffer CurrentCommandBuffer;
    
    public override Backend Backend => Backend.Vulkan;

    public override Size SwapchainSize => new Size((int) _swapchainSize.Width, (int) _swapchainSize.Height);

    public VkGraphicsDevice(IntPtr sdlWindow)
    {
        _vk = Vk.GetApi();
        _window = sdlWindow;
        _instance = VkHelper.CreateInstance(_vk, AppDomain.CurrentDomain.FriendlyName);

        _availableCommandBuffers = [];
        _availableSemaphores = [];
        _finishedCommandBuffers = [];
        _finishedSemaphores = [];
        
        if (!_vk.TryGetInstanceExtension(_instance, out _khrSurface))
            throw new Exception("Failed to get KhrSurface extension!");
        
        if (!SDL.VulkanCreateSurface(sdlWindow, _instance.Handle, IntPtr.Zero, out IntPtr surface))
            throw new Exception($"Failed to create surface: {SDL.GetError()}");
        _surface = new SurfaceKHR((ulong) surface);
        
        _physicalDevice = VkHelper.PickPhysicalDevice(_vk, _instance, out _queues);
        Device = VkHelper.CreateDevice(_vk, _physicalDevice, ref _queues);
        _commandPool = VkHelper.CreateCommandPool(_vk, Device, in _queues);
        Allocator = VkHelper.CreateAllocator(_instance, _physicalDevice, Device, GetInstanceProcAddress,
            GetDeviceProcAddress);

        _transferBuffer = VkHelper.CreateBuffer(Allocator, BufferUsageFlags.TransferSrcBit, 64 * 1024 * 1024, true);
        
        if (!_vk.TryGetDeviceExtension(_instance, Device, out _khrSwapchain))
            throw new Exception("Failed to get KhrSwapchain extension!");
        
        _swapchain = VkHelper.CreateSwapchain(_khrSwapchain, _physicalDevice, Device, in _queues, _surface,
            _khrSurface, sdlWindow, new SwapchainKHR(), out Format format, out _swapchainSize);
        _swapchainImages = VkHelper.GetSwapchainImages(Device, _khrSwapchain, _swapchain);
        
        _swapchainImageViews = new ImageView[_swapchainImages.Length];
        for (int i = 0; i < _swapchainImages.Length; i++)
            _swapchainImageViews[i] = VkHelper.CreateImageView(_vk, Device, _swapchainImages[i], format);
        
        _fence = VkHelper.CreateFence(_vk, Device);
    }
    
    public override Shader CreateShader(params ReadOnlySpan<ShaderAttachment> attachments)
    {
        return new VkShader(_vk, Device, in attachments);
    }
    
    protected override unsafe Texture CreateTexture(uint width, uint height, PixelFormat format, void* data)
    {
        throw new NotImplementedException();
    }
    
    public override Renderable CreateRenderable(in RenderableInfo info)
    {
        return new VkRenderable(_vk, this, in info);
    }
    
    public override void Clear(Color color)
    {
        if (!VkHelper.NextFrame(_khrSwapchain, _swapchain, Device, _fence, out _currentImage))
            RecreateSwapchain();
        
        foreach (CommandBuffer cb in _finishedCommandBuffers)
            _availableCommandBuffers.Enqueue(cb);
        _finishedCommandBuffers.Clear();
        
        foreach (Semaphore s in _finishedSemaphores)
            _availableSemaphores.Enqueue(s);
        _finishedSemaphores.Clear();
        
        _vk.WaitForFences(Device, 1, in _fence, false, ulong.MaxValue).Check("Wait for fence");
        _vk.ResetFences(Device, 1, in _fence);
        _currentFrameInFlight++;
        if (_currentFrameInFlight >= FramesInFlight)
            _currentFrameInFlight = 0;
        
        Image image = _swapchainImages[_currentImage];
        CurrentCommandBuffer = BeginCommandBuffer();
        VkHelper.TransitionImage(_vk, CurrentCommandBuffer, image, ImageLayout.Undefined,
            ImageLayout.ColorAttachmentOptimal);

        VkHelper.BeginRendering(_vk, CurrentCommandBuffer, [_swapchainImageViews[_currentImage]],
            new ClearColorValue(color.R, color.G, color.B, color.A), _swapchainSize);

        Viewport viewport = new Viewport(0, _swapchainSize.Height, _swapchainSize.Width, -_swapchainSize.Height, 0, 1);
        _vk.CmdSetViewport(CurrentCommandBuffer, 0, 1, &viewport);

        Rect2D scissor = new Rect2D(extent: new Extent2D(_swapchainSize.Width, _swapchainSize.Height));
        _vk.CmdSetScissor(CurrentCommandBuffer, 0, 1, &scissor);
    }
    
    public override void Present()
    {
        Image image = _swapchainImages[_currentImage];
        
        VkHelper.EndRendering(_vk, CurrentCommandBuffer);

        VkHelper.TransitionImage(_vk, CurrentCommandBuffer, image, ImageLayout.ColorAttachmentOptimal,
            ImageLayout.PresentSrcKhr);
        
        Semaphore semaphore = ExecuteCommandBuffer(CurrentCommandBuffer);

        PresentInfoKHR presentInfo = new()
        {
            SType = StructureType.PresentInfoKhr,
            SwapchainCount = 1,
            PImageIndices = (uint*) Unsafe.AsPointer(ref _currentImage),
            PSwapchains = (SwapchainKHR*) Unsafe.AsPointer(ref _swapchain),
            WaitSemaphoreCount = 1,
            PWaitSemaphores = &semaphore
        };

        Result result = _khrSwapchain.QueuePresent(_queues.Present, &presentInfo);

        switch (result)
        {
            case Result.ErrorOutOfDateKhr:
            case Result.SuboptimalKhr:
                RecreateSwapchain();
                break;
            default:
                result.Check("Present");
                break;
        }
    }

    public void CopyToBuffer(CommandBuffer cb, VkBuffer buffer, uint offset, uint size, void* pData)
    {
        // TODO: Persistently mapped buffers
        void* mappedData;
        vmaMapMemory(Allocator, _transferBuffer.Allocation, &mappedData).Check("Map memory");
        Unsafe.CopyBlock(mappedData, pData, size);
        vmaUnmapMemory(Allocator, _transferBuffer.Allocation);

        BufferCopy copy = new()
        {
            SrcOffset = 0,
            DstOffset = offset,
            Size = size
        };
        
        _vk.CmdCopyBuffer(cb, _transferBuffer.Buffer, buffer.Buffer, 1, &copy);
    }

    public CommandBuffer BeginCommandBuffer()
    {
        if (_availableCommandBuffers.TryDequeue(out CommandBuffer buffer))
            return buffer;
        
        // No command buffers available!
        buffer = VkHelper.CreateCommandBuffer(_vk, Device, _commandPool);
        VkHelper.BeginCommandBuffer(_vk, buffer);

        return buffer;
    }

    public Semaphore ExecuteCommandBuffer(CommandBuffer cb)
    {
        Semaphore? waitSemaphore = null;
        if (_finishedSemaphores.Count > 0)
            waitSemaphore = _finishedSemaphores[^1];

        Semaphore signalSemaphore = GetSemaphore();
        VkHelper.ExecuteCommandBuffer(_vk, cb, in _queues, waitSemaphore, signalSemaphore);
        _finishedCommandBuffers.Add(cb);
        _finishedSemaphores.Add(signalSemaphore);

        return signalSemaphore;
    }
    
    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        
        _vk.DestroyFence(Device, _fence, null);
        foreach (ImageView view in _swapchainImageViews)
            _vk.DestroyImageView(Device, view, null);
        _khrSwapchain.DestroySwapchain(Device, _swapchain, null);
        _khrSwapchain.Dispose();
        vmaDestroyAllocator(Allocator);
        _vk.DestroyCommandPool(Device, _commandPool, null);
        _vk.DestroyDevice(Device, null);
        SDL.VulkanDestroySurface(_instance.Handle, (nint) _surface.Handle, IntPtr.Zero);
        _khrSurface.Dispose();
        _vk.DestroyInstance(_instance, null);
        _vk.Dispose();
    }

    private void RecreateSwapchain()
    {
        SwapchainKHR oldSwapchain = _swapchain;
        VkHelper.CreateSwapchain(_khrSwapchain, _physicalDevice, Device, in _queues, _surface, _khrSurface, _window,
            oldSwapchain, out Format format, out _swapchainSize);
        
        _khrSwapchain.DestroySwapchain(Device, oldSwapchain, null);
        foreach (ImageView view in _swapchainImageViews)
            _vk.DestroyImageView(Device, view, null);

        _swapchainImages = VkHelper.GetSwapchainImages(Device, _khrSwapchain, _swapchain);
        _swapchainImageViews = new ImageView[_swapchainImages.Length];
        for (int i = 0; i < _swapchainImageViews.Length; i++)
            _swapchainImageViews[i] = VkHelper.CreateImageView(_vk, Device, _swapchainImages[i], format);
    }

    private Semaphore GetSemaphore()
    {
        if (_availableSemaphores.TryDequeue(out Semaphore semaphore))
            return semaphore;
        
        return VkHelper.CreateSemaphore(_vk, Device);
    }

    private nint GetInstanceProcAddress(Instance instance, byte* name)
    {
        return _vk.GetInstanceProcAddr(instance, name);
    }

    private nint GetDeviceProcAddress(Device device, byte* name)
    {
        return _vk.GetDeviceProcAddr(device, name);
    }
}