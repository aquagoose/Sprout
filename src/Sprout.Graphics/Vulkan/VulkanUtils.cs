using SDL3;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Sprout.Graphics.Vulkan;

internal static unsafe class VulkanUtils
{
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
            ApiVersion = Vk.Version13
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
}