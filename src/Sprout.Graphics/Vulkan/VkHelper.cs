global using VkFilter = Silk.NET.Vulkan.Filter;
using SDL3;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Sprout.Graphics.Vulkan.VMA;
using static Sprout.Graphics.Vulkan.VMA.AllocationCreateFlags;
using static Sprout.Graphics.Vulkan.VMA.Vma;
using static Sprout.Graphics.Vulkan.VMA.VmaMemoryUsage;
using Buffer = Silk.NET.Vulkan.Buffer;
using Image = Silk.NET.Vulkan.Image;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace Sprout.Graphics.Vulkan;

internal static unsafe class VkHelper
{
    public static Version32 ApiVersion = Vk.Version13;
    
    public static void Check(this Result result, string operation)
    {
        if (result != Result.Success)
            throw new Exception($"Vulkan operation '{operation}' failed: {result}");
    }

    public static VkFilter ToVk(this Filter filter)
    {
        return filter switch
        {
            Filter.Linear => VkFilter.Linear,
            Filter.Point => VkFilter.Nearest,
            _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, null)
        };
    }

    public static SamplerAddressMode ToVk(this TextureAddress address)
    {
        return address switch
        {
            TextureAddress.Repeat => SamplerAddressMode.Repeat,
            TextureAddress.RepeatMirrored => SamplerAddressMode.MirroredRepeat,
            TextureAddress.ClampToEdge => SamplerAddressMode.ClampToEdge,
            TextureAddress.ClampToBorder => SamplerAddressMode.ClampToBorder,
            _ => throw new ArgumentOutOfRangeException(nameof(address), address, null)
        };
    }

    public static DescriptorType ToVk(this UniformType type)
    {
        return type switch
        {
            UniformType.ConstantBuffer => DescriptorType.UniformBuffer,
            UniformType.Texture => DescriptorType.CombinedImageSampler,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
    
    public static Instance CreateInstance(Vk vk, string appName)
    {
        using VkString pAppName = appName;
        using VkString pEngineName = "Sprout";

        ApplicationInfo info = new()
        {
            SType = StructureType.ApplicationInfo,
            PApplicationName = pAppName,
            ApplicationVersion = Vk.MakeVersion(1, 0),
            PEngineName = pEngineName,
            EngineVersion = Vk.MakeVersion(1, 0),
            ApiVersion = ApiVersion
        };

        InstanceCreateFlags flags = 0;
        if (OperatingSystem.IsMacOS())
            flags |= InstanceCreateFlags.EnumeratePortabilityBitKhr;

        string[]? extensions = SDL.VulkanGetInstanceExtensions(out _);
        if (extensions == null)
            throw new Exception($"Failed to get instance extensions! {SDL.GetError()}");

        VkStringArray pExtensions = new VkStringArray(extensions);

        InstanceCreateInfo instanceInfo = new()
        {
            SType = StructureType.InstanceCreateInfo,
            PApplicationInfo = &info,
            Flags = flags,

            EnabledExtensionCount = pExtensions.Length,
            PpEnabledExtensionNames = pExtensions,
        };

        Instance instance;
        vk.CreateInstance(&instanceInfo, null, &instance).Check("Create instance");

        return instance;
    }

    public static PhysicalDevice PickPhysicalDevice(Vk vk, Instance instance, out Queues queues)
    {
        queues = new Queues();
        
        uint numPhysicalDevices;
        vk.EnumeratePhysicalDevices(instance, &numPhysicalDevices, null);
        PhysicalDevice* devices = stackalloc PhysicalDevice[(int) numPhysicalDevices];
        vk.EnumeratePhysicalDevices(instance, &numPhysicalDevices, devices);

        // 16 should be enough. If it isn't, welp.
        Span<QueueFamilyProperties> queueFamilies = stackalloc QueueFamilyProperties[16];
        
        for (int i = 0; i < numPhysicalDevices; i++)
        {
            PhysicalDevice device = devices[i];
            
            PhysicalDeviceProperties properties;
            vk.GetPhysicalDeviceProperties(device, &properties);
            Console.WriteLine(new string((sbyte*) properties.DeviceName));
            
            if (properties.ApiVersion < ApiVersion)
                continue;

            uint numQueues;
            vk.GetPhysicalDeviceQueueFamilyProperties(device, &numQueues, null);
            fixed (QueueFamilyProperties* pProps = queueFamilies)
                vk.GetPhysicalDeviceQueueFamilyProperties(device, &numQueues, pProps);

            uint? graphicsFamily = null;
            uint? presentFamily = null;
            
            for (uint j = 0; j < numQueues; j++)
            {
                if ((queueFamilies[(int) j].QueueFlags & QueueFlags.GraphicsBit) != 0)
                    graphicsFamily = j;

                if (SDL.VulkanGetPresentationSupport(instance.Handle, device.Handle, j))
                    presentFamily = j;

                if (graphicsFamily != null && presentFamily != null)
                    break;
            }
            
            // Physical device not supported, carry on...
            if (graphicsFamily == null || presentFamily == null)
                continue;

            queues.GraphicsFamily = graphicsFamily.Value;
            queues.PresentFamily = presentFamily.Value;

            return device;
        }

        throw new NotSupportedException("No supported physical devices. Use a different backend.");
    }

    public static Device CreateDevice(Vk vk, PhysicalDevice physicalDevice, ref Queues queues)
    {
        using VkStringArray extensions = new(KhrSwapchain.ExtensionName, KhrPushDescriptor.ExtensionName);
        
        HashSet<uint> uniqueQueues = queues.UniqueFamilies;
        int numUniqueQueues = uniqueQueues.Count;
        DeviceQueueCreateInfo* queueInfos = stackalloc DeviceQueueCreateInfo[numUniqueQueues];

        int i = 0;
        float priority = 1.0f;
        foreach (uint queue in uniqueQueues)
        {
            queueInfos[i++] = new DeviceQueueCreateInfo
            {
                SType = StructureType.DeviceQueueCreateInfo,
                QueueFamilyIndex = queue,
                QueueCount = 1,
                PQueuePriorities = &priority
            };
        }

        PhysicalDeviceFeatures features = new();

        DeviceCreateInfo deviceInfo = new()
        {
            SType = StructureType.DeviceCreateInfo,

            EnabledExtensionCount = extensions.Length,
            PpEnabledExtensionNames = extensions,

            QueueCreateInfoCount = (uint) numUniqueQueues,
            PQueueCreateInfos = queueInfos,

            PEnabledFeatures = &features
        };

        PhysicalDeviceDynamicRenderingFeatures dynamicRendering = new()
        {
            SType = StructureType.PhysicalDeviceDynamicRenderingFeatures,
            DynamicRendering = true
        };
        deviceInfo.PNext = &dynamicRendering;

        Device device;
        vk.CreateDevice(physicalDevice, &deviceInfo, null, &device).Check("Create device");

        vk.GetDeviceQueue(device, queues.GraphicsFamily, 0, out queues.Graphics);
        vk.GetDeviceQueue(device, queues.PresentFamily, 0, out queues.Present);

        return device;
    }

    // When passing in a surface, pass the surface to the surface, not the khrSurface.
    // Pass the KhrSurface to the khrSurface, not the surface, and make sure the surface is in surface, not khrSurface
    // Got it? Great!
    public static SwapchainKHR CreateSwapchain(KhrSwapchain khrSwapchain, PhysicalDevice physicalDevice, Device device,
        ref readonly Queues queues, SurfaceKHR surface, KhrSurface khrSurface, IntPtr sdlWindow,
        SwapchainKHR oldSwapchain, out Format format, out Extent2D size)
    {
        SurfaceCapabilitiesKHR capabilities;
        khrSurface.GetPhysicalDeviceSurfaceCapabilities(physicalDevice, surface, &capabilities);

        size = capabilities.CurrentExtent;
        if (size.Width == uint.MaxValue || size.Height == uint.MaxValue)
        {
            int w, h;
            SDL.GetWindowSizeInPixels(sdlWindow, out w, out h);
            size.Width = (uint) w;
            size.Height = (uint) h;
        }
        // Gonna skip the extra size checking here, I'm hoping it won't be necessary.

        // Some impls have a min image count of 1. We want to have at least 2 buffers but respect the min image count
        // if it wants more.
        uint numImages = uint.Max(2, capabilities.MinImageCount);

        // TODO: Probably should do additional checking on this. Or even allow the user to specify their own back buffer format.
        format = Format.B8G8R8A8Unorm;
        ColorSpaceKHR colorSpace = ColorSpaceKHR.SpaceSrgbNonlinearKhr;
        PresentModeKHR presentMode = PresentModeKHR.FifoKhr;
        
        SwapchainCreateInfoKHR swapchainInfo = new()
        {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = surface,
            OldSwapchain = oldSwapchain,
            
            MinImageCount = numImages,
            ImageExtent = size,
            ImageFormat = format,
            ImageColorSpace = colorSpace,
            ImageArrayLayers = 1,
            ImageUsage = ImageUsageFlags.ColorAttachmentBit,
            ImageSharingMode = SharingMode.Exclusive,
            
            PresentMode = presentMode,
            
            Clipped = true,
            CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
            PreTransform = SurfaceTransformFlagsKHR.IdentityBitKhr
        };

        if (queues.GraphicsFamily != queues.PresentFamily)
        {
            throw new NotImplementedException(
                "Currently the Vulkan implementation doesn't support a separate Graphics and Present queue. Please use a different backend.");
        }

        SwapchainKHR swapchain;
        khrSwapchain.CreateSwapchain(device, &swapchainInfo, null, &swapchain).Check("Create swapchain");

        return swapchain;
    }

    public static Allocator* CreateAllocator(Instance instance, PhysicalDevice physicalDevice, Device device, GetInstanceProcAddressFunc getInstanceProcAddressFunc, GetDeviceProcAddressFunc getDeviceProcAddressFunc)
    {
        VmaVulkanFunctions functions = new()
        {
            vkGetInstanceProcAddr =
                (delegate* unmanaged[Cdecl]<Instance, sbyte*, delegate* unmanaged[Cdecl]<void>>) SilkMarshal
                    .DelegateToPtr(getInstanceProcAddressFunc),
            vkGetDeviceProcAddr =
                (delegate* unmanaged[Cdecl]<Device, sbyte*, delegate* unmanaged[Cdecl]<void>>)
                SilkMarshal.DelegateToPtr(getDeviceProcAddressFunc)
        };

        AllocatorCreateInfo allocatorInfo = new()
        {
            instance = instance,
            physicalDevice = physicalDevice,
            device = device,
            pVulkanFunctions = &functions,
            vulkanApiVersion = ApiVersion
        };

        Allocator* allocator;
        vmaCreateAllocator(&allocatorInfo, &allocator).Check("Create allocator");

        return allocator;
    }

    public static Image[] GetSwapchainImages(Device device, KhrSwapchain khrSwapchain, SwapchainKHR swapchain)
    {
        uint numImages;
        khrSwapchain.GetSwapchainImages(device, swapchain, &numImages, null);
        Image[] images = new Image[numImages];
        fixed (Image* pImages = images)
            khrSwapchain.GetSwapchainImages(device, swapchain, &numImages, pImages);

        return images;
    }

    public static VkImage CreateImage(Allocator* allocator, uint width, uint height, Format format, ImageUsageFlags usage)
    {
        ImageCreateInfo imageInfo = new()
        {
            SType = StructureType.ImageCreateInfo,
            ImageType = ImageType.Type2D,
            Extent = new Extent3D { Width = width, Height = height, Depth = 1 },
            Format = format,
            Usage = usage,
            ArrayLayers = 1,
            MipLevels = 1, // TODO: Calculate mip levels
            InitialLayout = ImageLayout.Undefined,
            Samples = SampleCountFlags.Count1Bit,
        };

        AllocationCreateInfo allocatorInfo = new()
        {
            usage = VMA_MEMORY_USAGE_AUTO_PREFER_DEVICE,
            flags = 0
        };

        Image image;
        Allocation* allocation;
        vmaCreateImage(allocator, &imageInfo, &allocatorInfo, &image, &allocation, null).Check("Create image");

        return new VkImage(image, allocation);
    }

    public static ImageView CreateImageView(Vk vk, Device device, Image image, Format format)
    {
        ImageViewCreateInfo imageViewInfo = new()
        {
            SType = StructureType.ImageViewCreateInfo,
            Image = image,
            Format = format,
            ViewType = ImageViewType.Type2D,
            Components = new ComponentMapping
            {
                R = ComponentSwizzle.Identity,
                G = ComponentSwizzle.Identity,
                B = ComponentSwizzle.Identity,
                A = ComponentSwizzle.Identity
            },
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = ImageAspectFlags.ColorBit,
                BaseArrayLayer = 0,
                LayerCount = 1,
                BaseMipLevel = 0,
                LevelCount = 1
            }
        };

        ImageView view;
        vk.CreateImageView(device, &imageViewInfo, null, &view).Check("Create image view");

        return view;
    }

    public static CommandPool CreateCommandPool(Vk vk, Device device, ref readonly Queues queues)
    {
        CommandPoolCreateInfo commandPoolInfo = new()
        {
            SType = StructureType.CommandPoolCreateInfo,
            Flags = CommandPoolCreateFlags.ResetCommandBufferBit,
            QueueFamilyIndex = queues.GraphicsFamily
        };

        CommandPool commandPool;
        vk.CreateCommandPool(device, &commandPoolInfo, null, &commandPool).Check("Create command pool");

        return commandPool;
    }

    public static CommandBuffer[] CreateCommandBuffers(Vk vk, Device device, CommandPool pool, uint numCommandBuffers)
    {
        CommandBufferAllocateInfo allocInfo = new()
        {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = pool,
            CommandBufferCount = numCommandBuffers,
            Level = CommandBufferLevel.Primary
        };

        CommandBuffer[] buffers = new CommandBuffer[numCommandBuffers];
        fixed (CommandBuffer* pBuffers = buffers)
            vk.AllocateCommandBuffers(device, &allocInfo, pBuffers).Check("Allocate command buffers");

        return buffers;
    }

    public static Semaphore[] CreateSemaphores(Vk vk, Device device, uint numSemaphores)
    {
        SemaphoreCreateInfo semaphoreInfo = new()
        {
            SType = StructureType.SemaphoreCreateInfo
        };

        Semaphore[] semaphores = new Semaphore[numSemaphores];
        for (uint i = 0; i < numSemaphores; i++)
            vk.CreateSemaphore(device, &semaphoreInfo, null, out semaphores[i]).Check("Create semaphore");

        return semaphores;
    }

    public static Fence CreateFence(Vk vk, Device device)
    {
        FenceCreateInfo fenceInfo = new()
        {
            SType = StructureType.FenceCreateInfo
        };

        Fence fence;
        vk.CreateFence(device, &fenceInfo, null, &fence).Check("Create fence");

        return fence;
    }

    public static VkBuffer CreateBuffer(Allocator* allocator, BufferUsageFlags usage, uint size, bool mappable)
    {
        BufferCreateInfo bufferInfo = new()
        {
            SType = StructureType.BufferCreateInfo,
            Usage = usage,
            Size = size
        };

        AllocationCreateInfo allocationInfo = new()
        {
            usage = VMA_MEMORY_USAGE_AUTO,
            flags = mappable
                ? (uint) VMA_ALLOCATION_CREATE_HOST_ACCESS_SEQUENTIAL_WRITE_BIT | (uint) VMA_ALLOCATION_CREATE_MAPPED_BIT
                : 0
        };

        Buffer buffer;
        Allocation* allocation;
        VmaAllocationInfo allocInfo;
        vmaCreateBuffer(allocator, &bufferInfo, &allocationInfo, &buffer, &allocation, &allocInfo).Check("Create buffer");

        return new VkBuffer(buffer, allocation, allocInfo.pMappedData);
    }

    public static bool NextFrame(KhrSwapchain khrSwapchain, SwapchainKHR swapchain, Device device, Fence fence, out uint imageIndex)
    {
        // TODO: Handle swapchain recreation
        imageIndex = 0;
        Result result =
            khrSwapchain.AcquireNextImage(device, swapchain, ulong.MaxValue, new Semaphore(), fence, ref imageIndex);

        switch (result)
        {
            case Result.ErrorOutOfDateKhr:
            case Result.SuboptimalKhr:
                return false;
            default:
                result.Check("Acquire next image");
                break;
        }

        return true;
    }

    public static void BeginCommandBuffer(Vk vk, CommandBuffer cb)
    {
        CommandBufferBeginInfo beginInfo = new()
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit
        };
        
        vk.BeginCommandBuffer(cb, &beginInfo).Check("Begin command buffer");
    }

    public static void ExecuteCommandBuffer(Vk vk, CommandBuffer cb, ref readonly Queues queues, Semaphore? wait, Semaphore? signal)
    {
        vk.EndCommandBuffer(cb).Check("End command buffer");

        SubmitInfo submitInfo = new()
        {
            SType = StructureType.SubmitInfo,

            CommandBufferCount = 1,
            PCommandBuffers = &cb,
        };

        if (wait is Semaphore waitSemaphore)
        {
            submitInfo.WaitSemaphoreCount = 1;
            // We can do this because waitSemaphore isn't just limited to this scope and therefore remains valid.
            submitInfo.PWaitSemaphores = &waitSemaphore;
        }

        if (signal is Semaphore signalSemaphore)
        {
            submitInfo.SignalSemaphoreCount = 1;
            submitInfo.PSignalSemaphores = &signalSemaphore;
        }

        vk.QueueSubmit(queues.Graphics, 1, &submitInfo, new Fence()).Check("Submit queue");
    }

    public static void BeginRendering(Vk vk, CommandBuffer cb, ReadOnlySpan<ImageView> colorAttachments, ClearColorValue? clearColor, Extent2D renderSize)
    {
        RenderingAttachmentInfo* colorRenderAttachments = stackalloc RenderingAttachmentInfo[colorAttachments.Length];
        for (int i = 0; i < colorAttachments.Length; i++)
        {
            colorRenderAttachments[i] = new RenderingAttachmentInfo
            {
                SType = StructureType.RenderingAttachmentInfo,
                ClearValue = new ClearValue(clearColor),
                ImageLayout = ImageLayout.ColorAttachmentOptimal,
                ImageView = colorAttachments[i],
                LoadOp = clearColor == null ? AttachmentLoadOp.Load : AttachmentLoadOp.Clear,
                StoreOp = AttachmentStoreOp.Store
            };
        }

        RenderingInfo renderingInfo = new()
        {
            SType = StructureType.RenderingInfo,
            ColorAttachmentCount = (uint) colorAttachments.Length,
            PColorAttachments = colorRenderAttachments,
            RenderArea = new Rect2D(extent: renderSize),
            LayerCount = 1
        };

        vk.CmdBeginRendering(cb, &renderingInfo);
    }

    public static void EndRendering(Vk vk, CommandBuffer cb)
    {
        vk.CmdEndRendering(cb);
    }

    public static void TransitionImage(Vk vk, CommandBuffer cb, Image image, ImageLayout old, ImageLayout @new,
        PipelineStageFlags srcStage, PipelineStageFlags dstStage, AccessFlags srcAccessMask, AccessFlags dstAccessMask)
    {
        ImageMemoryBarrier imageBarrier = new()
        {
            SType = StructureType.ImageMemoryBarrier,
            Image = image,
            OldLayout = old,
            NewLayout = @new,
            SrcAccessMask = srcAccessMask,
            DstAccessMask = dstAccessMask,
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = ImageAspectFlags.ColorBit,
                BaseArrayLayer = 0,
                LayerCount = 1,
                BaseMipLevel = 0,
                LevelCount = 1
            }
        };

        vk.CmdPipelineBarrier(cb, srcStage, dstStage, 0, 0, null, 0, null, 1, &imageBarrier);
    }

    public struct Queues
    {
        public uint GraphicsFamily;
        public Queue Graphics;

        public uint PresentFamily;
        public Queue Present;

        public HashSet<uint> UniqueFamilies => [GraphicsFamily, PresentFamily];
    }

    public delegate nint GetInstanceProcAddressFunc(Instance instance, byte* name);

    public delegate nint GetDeviceProcAddressFunc(Device device, byte* name);
}