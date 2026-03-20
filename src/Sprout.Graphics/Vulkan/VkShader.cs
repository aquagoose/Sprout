using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Sprout.Graphics.Vulkan;

internal sealed unsafe class VkShader : Shader
{
    public override bool IsDisposed { get; protected set; }

    private readonly Vk _vk;
    private readonly VkGraphicsDevice _device;
    
    private readonly DescriptorSetLayout _descriptorLayout;
    internal readonly PipelineLayout PipelineLayout;

    public readonly ShaderModule VertexModule;
    public readonly ShaderModule PixelModule;
    
    public VkShader(Vk vk, VkGraphicsDevice device, ref readonly ShaderInfo info)
    {
        _vk = vk;
        _device = device;

        if (info.VertexShader is ShaderAttachment vertexShader)
        {
            VkShaderModule module = VkHelper.CreateShaderModule(_vk, _device.Device, vertexShader.Source);
            VertexModule = new ShaderModule(module, vertexShader.EntryPoint);
        }

        if (info.PixelShader is ShaderAttachment pixelShader)
        {
            VkShaderModule module = VkHelper.CreateShaderModule(_vk, _device.Device, pixelShader.Source);
            PixelModule = new ShaderModule(module, pixelShader.EntryPoint);
        }
        
        if (info.Uniforms != null)
        {
            int numUniforms = info.Uniforms?.Length ?? 0;
            DescriptorSetLayoutBinding* bindings = stackalloc DescriptorSetLayoutBinding[numUniforms];

            for (int i = 0; i < numUniforms; i++)
            {
                ref readonly Uniform uniform = ref info.Uniforms![i];
                
                bindings[i] = new DescriptorSetLayoutBinding
                {
                    Binding = uniform.Index,
                    DescriptorType = uniform.Type.ToVk(),
                    DescriptorCount = 1,
                    StageFlags = ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit
                };
            }

            DescriptorSetLayoutCreateInfo descriptorLayoutInfo = new()
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                Flags = DescriptorSetLayoutCreateFlags.PushDescriptorBit,
                BindingCount = (uint) numUniforms,
                PBindings = bindings,
            };
            
            _vk.CreateDescriptorSetLayout(_device.Device, &descriptorLayoutInfo, null, out _descriptorLayout)
                .Check("Create descriptor layout");

            DescriptorPoolSize* poolSizes = stackalloc DescriptorPoolSize[numUniforms];
            for (int i = 0; i < numUniforms; i++)
            {
                poolSizes[i] = new DescriptorPoolSize()
                {
                    DescriptorCount = 1,
                    Type = info.Uniforms![i].Type.ToVk()
                };
            }
            
            /*DescriptorPoolCreateInfo descriptorPoolInfo = new()
            {
                SType = StructureType.DescriptorPoolCreateInfo,
                
                MaxSets = 1,
                PoolSizeCount = (uint) numUniforms,
                PPoolSizes = poolSizes
            };
            _vk.CreateDescriptorPool(_device.Device, &descriptorPoolInfo, null, out _descriptorPool)
                .Check("Create descriptor pool");

            DescriptorSetLayout setLayout = _descriptorLayout;
            DescriptorSetAllocateInfo setAllocateInfo = new()
            {
                SType = StructureType.DescriptorSetAllocateInfo,
                DescriptorPool = _descriptorPool,
                PSetLayouts = &setLayout,
                DescriptorSetCount = 1
            };

            _vk.AllocateDescriptorSets(_device.Device, &setAllocateInfo, out _descriptorSet)
                .Check("Create descriptor set");*/
        }
        
        DescriptorSetLayout descriptorLayout = _descriptorLayout;
        PipelineLayoutCreateInfo layoutInfo = new()
        {
            SType = StructureType.PipelineLayoutCreateInfo,
            SetLayoutCount = _descriptorLayout.Handle == 0 ? 0u : 1u,
            PSetLayouts = &descriptorLayout
        };
        _vk.CreatePipelineLayout(_device.Device, &layoutInfo, null, out PipelineLayout).Check("Create pipeline layout");
    }
    
    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        
        _vk.DestroyPipelineLayout(_device.Device, PipelineLayout, null);
        _vk.DestroyDescriptorSetLayout(_device.Device, _descriptorLayout, null);
        
        if (PixelModule.Module.Handle != 0)
            _vk.DestroyShaderModule(_device.Device, PixelModule.Module, null);
        
        if (VertexModule.Module.Handle != 0)
            _vk.DestroyShaderModule(_device.Device, VertexModule.Module, null);
    }
    
    public override void PushUniformData(uint index, uint offset, uint sizeInBytes, void* pData)
    {
        Buffer uniformBuffer = _device.PushUniform(sizeInBytes, pData, out ulong bufferOffset);

        DescriptorBufferInfo bufferInfo = new()
        {
            Buffer = uniformBuffer,
            Offset = bufferOffset,
            Range = sizeInBytes
        };

        WriteDescriptorSet writeDescriptor = new()
        {
            SType = StructureType.WriteDescriptorSet,
            DescriptorType = DescriptorType.UniformBuffer,
            DstBinding = index,
            DescriptorCount = 1,
            PBufferInfo = &bufferInfo
        };

        _device.KhrPushDescriptor.CmdPushDescriptorSet(_device.CurrentCommandBuffer, PipelineBindPoint.Graphics,
            PipelineLayout, 0, 1, &writeDescriptor);
    }

    public override void PushTexture(uint index, Texture texture)
    {
        VkTexture vkTexture = (VkTexture) texture;

        DescriptorImageInfo imageInfo = new()
        {
            ImageView = vkTexture.ImageView,
            Sampler = _device.GetSampler(texture.Sampler),
            ImageLayout = ImageLayout.ShaderReadOnlyOptimal
        };

        WriteDescriptorSet writeDescriptor = new()
        {
            SType = StructureType.WriteDescriptorSet,
            DescriptorType = DescriptorType.CombinedImageSampler,
            DstBinding = index,
            DescriptorCount = 1,
            //DstSet = _descriptorSet,
            PImageInfo = &imageInfo
        };

        _device.KhrPushDescriptor.CmdPushDescriptorSet(_device.CurrentCommandBuffer, PipelineBindPoint.Graphics,
            PipelineLayout, 0, 1, &writeDescriptor);

        //_vk.UpdateDescriptorSets(_device.Device, 1, &writeDescriptor, null);
    }

    public readonly struct ShaderModule
    {
        public readonly VkShaderModule Module;
        public readonly string EntryPoint;

        public ShaderModule(VkShaderModule module, string entryPoint)
        {
            Module = module;
            EntryPoint = entryPoint;
        }
    }
}