using SDL3;
using Silk.NET.Core;
using Silk.NET.Vulkan;

namespace Sprout.Graphics.Vulkan;

internal static unsafe class VkHelper
{
    public static Version32 ApiVersion = Vk.Version13;
    
    public static void Check(this Result result, string operation)
    {
        if (result != Result.Success)
            throw new Exception($"Vulkan operation '{operation}' failed: {result}");
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

    public static PhysicalDevice PickPhysicalDevice(Vk vk, Instance instance, IntPtr sdlWindow, out Queues queues)
    {
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

    public struct Queues
    {
        public uint GraphicsFamily;

        public uint PresentFamily;

        public HashSet<uint> UniqueFamilies => [GraphicsFamily, PresentFamily];
    }
}