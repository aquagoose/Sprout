using System.Diagnostics;
using SDL3;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Sprout.Graphics.Vulkan;

internal static unsafe class VulkanUtils
{
    private static Version32 ApiVersion = Vk.Version13;
    
    extension(Result result)
    {
        public void Check(string operation)
        {
            if (result != Result.Success)
                throw new Exception($"Vulkan operation '{operation}' failed: {result}");
        }
    }
    
    public static Instance CreateInstance(Vk vk, IntPtr sdlWindow)
    {
        string appName = SDL.GetAppMetadataProperty(SDL.Props.AppMetadataNameString) ?? "Sprout Application";
        
        nint pAppName = SilkMarshal.StringToPtr(appName);
        nint pEngineName = SilkMarshal.StringToPtr("Sprout");

        ApplicationInfo appInfo = new()
        {
            PApplicationName = (byte*) pAppName,
            ApplicationVersion = Vk.MakeVersion(0, 0),
            PEngineName = (byte*) pEngineName,
            EngineVersion = Vk.MakeVersion(1, 0),
            ApiVersion = ApiVersion
        };

        string[] instanceExtensions = SDL.VulkanGetInstanceExtensions(out uint numExtensions);
        nint pInstanceExtensions = SilkMarshal.StringArrayToMemory(instanceExtensions);

        InstanceCreateInfo instanceInfo = new()
        {
            SType = StructureType.InstanceCreateInfo,
            PApplicationInfo = &appInfo,
            
            PpEnabledExtensionNames = (byte**) pInstanceExtensions,
            EnabledExtensionCount = numExtensions
        };
        
        Instance instance;
        vk.CreateInstance(&instanceInfo, null, &instance).Check("Create instance");

        SilkMarshal.Free(pInstanceExtensions);
        SilkMarshal.FreeString(pEngineName);
        SilkMarshal.FreeString(pAppName);

        return instance;
    }

    public static PhysicalDevice PickPhysicalDevice(Vk vk, Instance instance, out Queues queues, out string deviceName)
    {
        uint numPhysicalDevices;
        vk.EnumeratePhysicalDevices(instance, &numPhysicalDevices, null);
        PhysicalDevice* physicalDevices = stackalloc PhysicalDevice[(int) numPhysicalDevices];
        vk.EnumeratePhysicalDevices(instance, &numPhysicalDevices, physicalDevices);

        // There should never be more than 16 queue families. If there are, whoops.
        QueueFamilyProperties* queueFamilies = stackalloc QueueFamilyProperties[16];
        for (uint i = 0; i < numPhysicalDevices; i++)
        {
            PhysicalDevice device = physicalDevices[i];
            PhysicalDeviceProperties properties;
            vk.GetPhysicalDeviceProperties(device, &properties);
            
            if (properties.ApiVersion < ApiVersion)
                continue;

            uint numQueues;
            vk.GetPhysicalDeviceQueueFamilyProperties(device, &numQueues, null);
            Debug.Assert(numQueues < 16);
            vk.GetPhysicalDeviceQueueFamilyProperties(device, &numQueues, queueFamilies);

            uint? graphicsIndex = null;
            uint? presentIndex = null;
            
            for (uint f = 0; f < numQueues; f++)
            {
                ref readonly QueueFamilyProperties property = ref queueFamilies[f];

                if ((property.QueueFlags & QueueFlags.GraphicsBit) != 0)
                    graphicsIndex = f;

                if (SDL.VulkanGetPresentationSupport(instance.Handle, device.Handle, f))
                    presentIndex = f;

                if (graphicsIndex.HasValue && presentIndex.HasValue)
                    break;
            }
            
            if (graphicsIndex == null || presentIndex == null)
                continue;

            queues = new Queues
            {
                GraphicsIndex = graphicsIndex.Value,
                PresentIndex = presentIndex.Value
            };

            deviceName = new string((sbyte*) properties.DeviceName);
            return device;
        }

        throw new NotSupportedException("No supported Vulkan devices were found.");
    }
}