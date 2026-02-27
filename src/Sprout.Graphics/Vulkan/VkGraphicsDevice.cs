using System.Drawing;
using System.Runtime.CompilerServices;
using SDL3;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Sprout.Graphics.Vulkan.VMA;
using static Sprout.Graphics.Vulkan.VMA.Vma;
using Buffer = Silk.NET.Vulkan.Buffer;
using Image = Silk.NET.Vulkan.Image;
using Semaphore = Silk.NET.Vulkan.Semaphore;
using VkViewport = Silk.NET.Vulkan.Viewport;
using VkSampler = Silk.NET.Vulkan.Sampler;

namespace Sprout.Graphics.Vulkan;

internal sealed unsafe class VkGraphicsDevice : GraphicsDevice
{
    private const uint FramesInFlight = 3;
    private const uint TransferBufferSize = 64 * 1024 * 1024; // 64mb transfer buffer
    private const uint UniformBufferSize = 4 * 1024 * 1024; // 4mb uniform buffer.
    
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

    private readonly CommandBuffer[] _commandBuffers;
    private readonly Semaphore[] _semaphores;
    
    private uint _currentFrameInFlight;
    private readonly Fence _fence;

    private readonly VkBuffer _transferBuffer;
    private ulong _transferBufferOffset;
    private readonly VkBuffer _globalUniformBuffer;
    private ulong _uniformBufferOffset;
    
    public readonly Device Device;
    public readonly Allocator* Allocator;
    public readonly KhrPushDescriptor KhrPushDescriptor;

    private readonly Dictionary<Sampler, VkSampler> _samplerCache;

    public CommandBuffer CurrentCommandBuffer => _commandBuffers[_currentFrameInFlight];

    public Semaphore CurrentSemaphore => _semaphores[_currentFrameInFlight];
    
    public override Backend Backend => Backend.Vulkan;

    public override Size SwapchainSize => new Size((int) _swapchainSize.Width, (int) _swapchainSize.Height);
    
    public override Viewport Viewport { get; set; }
    
    public override BlendMode BlendMode { get; set; }

    public VkGraphicsDevice(IntPtr sdlWindow)
    {
        _vk = Vk.GetApi();
        _window = sdlWindow;
        _instance = VkHelper.CreateInstance(_vk, AppDomain.CurrentDomain.FriendlyName);
        _samplerCache = [];
        
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

        _transferBuffer = VkHelper.CreateBuffer(Allocator, BufferUsageFlags.TransferSrcBit, TransferBufferSize, true);
        _globalUniformBuffer =
            VkHelper.CreateBuffer(Allocator, BufferUsageFlags.UniformBufferBit, UniformBufferSize, true);
        
        if (!_vk.TryGetDeviceExtension(_instance, Device, out _khrSwapchain))
            throw new Exception("Failed to get KhrSwapchain extension!");
        if (!_vk.TryGetDeviceExtension(_instance, Device, out KhrPushDescriptor))
            throw new Exception("Failed to get KhrPushDescriptor extension!");
        
        _swapchain = VkHelper.CreateSwapchain(_khrSwapchain, _physicalDevice, Device, in _queues, _surface,
            _khrSurface, sdlWindow, new SwapchainKHR(), out Format format, out _swapchainSize);
        _swapchainImages = VkHelper.GetSwapchainImages(Device, _khrSwapchain, _swapchain);
        
        _swapchainImageViews = new ImageView[_swapchainImages.Length];
        for (int i = 0; i < _swapchainImages.Length; i++)
            _swapchainImageViews[i] = VkHelper.CreateImageView(_vk, Device, _swapchainImages[i], format);
        
        _fence = VkHelper.CreateFence(_vk, Device);
        _commandBuffers = VkHelper.CreateCommandBuffers(_vk, Device, _commandPool, FramesInFlight);
        _semaphores = VkHelper.CreateSemaphores(_vk, Device, FramesInFlight);
        
        NextFrame();
    }
    
    public override Shader CreateShader(params ReadOnlySpan<ShaderAttachment> attachments)
    {
        return new VkShader(_vk, Device, in attachments);
    }
    
    protected override Texture CreateTexture(uint width, uint height, PixelFormat format, TextureUsage usage, void* data)
    {
        return new VkTexture(_vk, this, width, height, format, usage, data);
    }
    
    public override Renderable CreateRenderable(in RenderableInfo info)
    {
        return new VkRenderable(_vk, this, in info);
    }

    public override void SetRenderTextures(ReadOnlySpan<Texture> colorTextures)
    {
        throw new NotImplementedException();
    }

    public override void Clear(Color color)
    {
        Image image = _swapchainImages[_currentImage];
        VkHelper.TransitionImage(_vk, CurrentCommandBuffer, image, ImageLayout.Undefined,
            ImageLayout.ColorAttachmentOptimal, PipelineStageFlags.ColorAttachmentOutputBit,
            PipelineStageFlags.ColorAttachmentOutputBit, AccessFlags.ColorAttachmentReadBit,
            AccessFlags.ColorAttachmentReadBit);

        VkHelper.BeginRendering(_vk, CurrentCommandBuffer, [_swapchainImageViews[_currentImage]],
            new ClearColorValue(color.R, color.G, color.B, color.A), _swapchainSize);

        VkViewport viewport = new VkViewport(0, _swapchainSize.Height, _swapchainSize.Width, -_swapchainSize.Height, 0, 1);
        _vk.CmdSetViewport(CurrentCommandBuffer, 0, 1, &viewport);

        Rect2D scissor = new Rect2D(extent: new Extent2D(_swapchainSize.Width, _swapchainSize.Height));
        _vk.CmdSetScissor(CurrentCommandBuffer, 0, 1, &scissor);
    }
    
    public override void Present()
    {
        Image image = _swapchainImages[_currentImage];
        Semaphore semaphore = CurrentSemaphore;
        
        VkHelper.EndRendering(_vk, CurrentCommandBuffer);

        VkHelper.TransitionImage(_vk, CurrentCommandBuffer, image, ImageLayout.ColorAttachmentOptimal,
            ImageLayout.PresentSrcKhr, PipelineStageFlags.ColorAttachmentOutputBit,
            PipelineStageFlags.ColorAttachmentOutputBit, AccessFlags.ColorAttachmentReadBit,
            AccessFlags.ColorAttachmentReadBit);
        
        VkHelper.ExecuteCommandBuffer(_vk, CurrentCommandBuffer, in _queues, null, semaphore);

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
        
        NextFrame();
    }

    public override void ResizeSwapchain(uint width, uint height)
    {
        _swapchainSize = new Extent2D(width, height);
        RecreateSwapchain();
    }

    public void CopyToBuffer(VkBuffer buffer, uint offset, uint size, void* pData)
    {
        if (_transferBufferOffset + size >= TransferBufferSize)
            throw new NotImplementedException("Buffer offset too high and resizing hasn't been implemented yet!");
        
        Unsafe.CopyBlock((byte*) _transferBuffer.MappedPtr + _transferBufferOffset, pData, size);

        BufferCopy copy = new()
        {
            SrcOffset = _transferBufferOffset,
            DstOffset = offset,
            Size = size
        };
        
        _vk.CmdCopyBuffer(CurrentCommandBuffer, _transferBuffer.Buffer, buffer.Buffer, 1, &copy);
        _transferBufferOffset += size;
    }

    public void CopyToTexture(Image image, Size textureSize, uint bpp, void* pData)
    {
        uint totalSize = (uint) (textureSize.Width * textureSize.Height * bpp);
        
        if (_transferBufferOffset + totalSize >= TransferBufferSize)
            throw new NotImplementedException("Buffer offset too high and resizing hasn't been implemented yet!");
        
        Unsafe.CopyBlock((byte*) _transferBuffer.MappedPtr + _transferBufferOffset, pData, totalSize);
        
        BufferImageCopy imageCopy = new()
        {
            BufferOffset = _transferBufferOffset,
            ImageExtent = new Extent3D { Width = (uint) textureSize.Width, Height = (uint) textureSize.Height, Depth = 1 },
            ImageSubresource = new ImageSubresourceLayers
            {
                AspectMask = ImageAspectFlags.ColorBit,
                LayerCount = 1,
                BaseArrayLayer = 0,
                MipLevel = 0
            }
        };

        VkHelper.TransitionImage(_vk, CurrentCommandBuffer, image, ImageLayout.Undefined,
            ImageLayout.TransferDstOptimal, PipelineStageFlags.TopOfPipeBit, PipelineStageFlags.TransferBit,
            AccessFlags.None, AccessFlags.TransferWriteBit);
        
        _vk.CmdCopyBufferToImage(CurrentCommandBuffer, _transferBuffer.Buffer, image,
            ImageLayout.TransferDstOptimal, 1, &imageCopy);

        VkHelper.TransitionImage(_vk, CurrentCommandBuffer, image, ImageLayout.TransferDstOptimal,
            ImageLayout.ShaderReadOnlyOptimal, PipelineStageFlags.TransferBit, PipelineStageFlags.FragmentShaderBit,
            AccessFlags.TransferWriteBit, AccessFlags.ShaderReadBit);

        _transferBufferOffset += totalSize;
    }

    public Buffer PushUniform(uint size, void* pData, out ulong offset)
    {
        if (_transferBufferOffset + size >= TransferBufferSize)
            throw new NotImplementedException("Buffer offset too high and resizing hasn't been implemented yet!");
        
        Unsafe.CopyBlock((byte*) _globalUniformBuffer.MappedPtr + _transferBufferOffset, pData, size);
        offset = _transferBufferOffset;

        _transferBufferOffset += size;
        
        return _globalUniformBuffer.Buffer;
    }

    public VkSampler GetSampler(Sampler sampler)
    {
        if (_samplerCache.TryGetValue(sampler, out VkSampler vkSampler))
            return vkSampler;

        SamplerCreateInfo samplerInfo = new()
        {
            SType = StructureType.SamplerCreateInfo,
            MinFilter = sampler.MinFilter.ToVk(),
            MagFilter = sampler.MagFilter.ToVk(),
            MipmapMode = (SamplerMipmapMode) sampler.MipFilter.ToVk(),
            AddressModeU = SamplerAddressMode.Repeat,
            AddressModeV = SamplerAddressMode.Repeat,
            MinLod = 0,
            MaxLod = float.MaxValue
        };

        _vk.CreateSampler(Device, &samplerInfo, null, &vkSampler).Check("Create sampler");
        _samplerCache.Add(sampler, vkSampler);
        return vkSampler;
    }
    
    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;

        _vk.DeviceWaitIdle(Device);
        
        foreach ((_, VkSampler sampler) in _samplerCache)
            _vk.DestroySampler(Device, sampler, null);
        
        foreach (Semaphore semaphore in _semaphores)
            _vk.DestroySemaphore(Device, semaphore, null);
        
        _vk.DestroyFence(Device, _fence, null);
        
        foreach (ImageView view in _swapchainImageViews)
            _vk.DestroyImageView(Device, view, null);
        
        _khrSwapchain.DestroySwapchain(Device, _swapchain, null);
        _khrSwapchain.Dispose();
        
        vmaDestroyBuffer(Allocator, _globalUniformBuffer.Buffer, _globalUniformBuffer.Allocation);
        vmaDestroyBuffer(Allocator, _transferBuffer.Buffer, _transferBuffer.Allocation);
        
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
        _swapchain = VkHelper.CreateSwapchain(_khrSwapchain, _physicalDevice, Device, in _queues, _surface, _khrSurface,
            _window, oldSwapchain, out Format format, out _swapchainSize);
        
        _khrSwapchain.DestroySwapchain(Device, oldSwapchain, null);
        foreach (ImageView view in _swapchainImageViews)
            _vk.DestroyImageView(Device, view, null);

        _swapchainImages = VkHelper.GetSwapchainImages(Device, _khrSwapchain, _swapchain);
        _swapchainImageViews = new ImageView[_swapchainImages.Length];
        for (int i = 0; i < _swapchainImageViews.Length; i++)
            _swapchainImageViews[i] = VkHelper.CreateImageView(_vk, Device, _swapchainImages[i], format);
    }

    private void NextFrame()
    {
        if (!VkHelper.NextFrame(_khrSwapchain, _swapchain, Device, _fence, out _currentImage))
            RecreateSwapchain();
        
        _vk.WaitForFences(Device, 1, in _fence, false, ulong.MaxValue).Check("Wait for fence");
        _vk.ResetFences(Device, 1, in _fence);
        _currentFrameInFlight++;
        if (_currentFrameInFlight >= FramesInFlight)
            _currentFrameInFlight = 0;
        
        VkHelper.BeginCommandBuffer(_vk, CurrentCommandBuffer);
        _transferBufferOffset = 0;
        _uniformBufferOffset = 0;
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