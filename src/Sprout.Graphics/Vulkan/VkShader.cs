using Silk.NET.Vulkan;

namespace Sprout.Graphics.Vulkan;

internal sealed unsafe class VkShader : Shader
{
    public override bool IsDisposed { get; protected set; }

    private readonly Vk _vk;
    private readonly Device _device;

    public readonly Dictionary<ShaderStage, (string entryPoint, ShaderModule module)> ShaderModules;
    
    public VkShader(Vk vk, Device device, ref readonly ReadOnlySpan<ShaderAttachment> attachments)
    {
        _vk = vk;
        _device = device;
        ShaderModules = new Dictionary<ShaderStage, (string entryPoint, ShaderModule module)>(attachments.Length);

        foreach (ShaderAttachment attachment in attachments)
        {
            fixed (byte* pSource = attachment.Source)
            {
                ShaderModuleCreateInfo moduleInfo = new()
                {
                    SType = StructureType.ShaderModuleCreateInfo,
                    CodeSize = (nuint) attachment.Source.Length,
                    PCode = (uint*) pSource
                };

                ShaderModule module;
                _vk.CreateShaderModule(device, &moduleInfo, null, &module).Check("Create shader module");
                
                ShaderModules.Add(attachment.Stage, (attachment.EntryPoint, module));
            }
        }
    }
    
    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        
        foreach ((_, (_, ShaderModule module)) in ShaderModules)
            _vk.DestroyShaderModule(_device, module, null);
    }
}