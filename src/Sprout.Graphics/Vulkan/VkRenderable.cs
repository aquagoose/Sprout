using System.Diagnostics;
using System.Runtime.InteropServices;
using Silk.NET.Vulkan;
using static Sprout.Graphics.Vulkan.VMA.Vma;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Sprout.Graphics.Vulkan;

internal sealed unsafe class VkRenderable : Renderable
{
    public override bool IsDisposed { get; protected set; }

    private readonly Vk _vk;
    private readonly VkGraphicsDevice _device;

    private readonly DescriptorSetLayout _descriptorLayout;
    private readonly DescriptorPool _descriptorPool;
    private readonly DescriptorSet _descriptorSet;

    private readonly PipelineLayout _layout;
    private readonly Pipeline _pipeline;

    private readonly VkBuffer _vertexBuffer;
    private readonly VkBuffer _indexBuffer;

    private readonly uint _numElements;
    
    public VkRenderable(Vk vk, VkGraphicsDevice device, ref readonly RenderableInfo info)
    {
        _vk = vk;
        _device = device;

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
                    DescriptorCount = 1
                };
            }

            DescriptorSetLayoutCreateInfo descriptorLayoutInfo = new()
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                BindingCount = (uint) numUniforms,
                PBindings = bindings
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
            
            DescriptorPoolCreateInfo descriptorPoolInfo = new()
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
                .Check("Create descriptor set");
        }

        VkShader shader = (VkShader) info.Shader;

        VertexInputBindingDescription vertexInputBinding = new VertexInputBindingDescription();
        if (info.NumVertices > 0)
        {
            Debug.Assert(info.VertexSize > 0);

            _vertexBuffer = VkHelper.CreateBuffer(_device.Allocator,
                BufferUsageFlags.VertexBufferBit | BufferUsageFlags.TransferDstBit, info.NumVertices * info.VertexSize,
                false);

            vertexInputBinding = new VertexInputBindingDescription
            {
                Binding = 0,
                InputRate = VertexInputRate.Vertex,
                Stride = info.VertexSize
            };

            _numElements = info.NumVertices;
        }

        if (info.NumIndices > 0)
        {
            Debug.Assert(info.NumVertices > 0);

            _indexBuffer = VkHelper.CreateBuffer(_device.Allocator,
                BufferUsageFlags.IndexBufferBit | BufferUsageFlags.TransferDstBit, info.NumIndices * sizeof(uint),
                false);

            _numElements = info.NumIndices;
        }

        DescriptorSetLayout descriptorLayout = _descriptorLayout;
        PipelineLayoutCreateInfo layoutInfo = new()
        {
            SType = StructureType.PipelineLayoutCreateInfo,
            SetLayoutCount = _descriptorLayout.Handle == 0 ? 0u : 1u,
            PSetLayouts = &descriptorLayout
        };
        _vk.CreatePipelineLayout(_device.Device, &layoutInfo, null, out _layout).Check("Create pipeline layout");

        int numShaderStages = shader.ShaderModules.Count;
        PipelineShaderStageCreateInfo* shaderStages = stackalloc PipelineShaderStageCreateInfo[numShaderStages];
        int currentStage = 0;
        foreach ((ShaderStage stage, (string entryPoint, ShaderModule module)) in shader.ShaderModules)
        {
            shaderStages[currentStage++] = new PipelineShaderStageCreateInfo
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Module = module,
                PName = (byte*) Marshal.StringToHGlobalAnsi(entryPoint),
                Stage = stage switch
                {
                    ShaderStage.Vertex => ShaderStageFlags.VertexBit,
                    ShaderStage.Pixel => ShaderStageFlags.FragmentBit,
                    _ => throw new ArgumentOutOfRangeException()
                }
            };
        }

        int vertexInputLength = info.VertexInput?.Length ?? 0;
        VertexInputAttributeDescription* vertexAttributes =
            stackalloc VertexInputAttributeDescription[vertexInputLength];

        if (vertexInputLength > 0)
        {
            for (int i = 0; i < vertexInputLength; i++)
            {
                ref readonly VertexAttribute attribute = ref info.VertexInput![i];

                Format format = attribute.Type switch
                {
                    AttributeType.Float => Format.R32Sfloat,
                    AttributeType.Float2 => Format.R32G32Sfloat,
                    AttributeType.Float3 => Format.R32G32B32Sfloat,
                    AttributeType.Float4 => Format.R32G32B32A32Sfloat,
                    _ => throw new ArgumentOutOfRangeException()
                };

                vertexAttributes[i] = new VertexInputAttributeDescription
                {
                    Location = attribute.Location,
                    Binding = 0,
                    Format = format,
                    Offset = attribute.Offset
                };
            }
        }

        PipelineVertexInputStateCreateInfo vertexInputState = new()
        {
            SType = StructureType.PipelineVertexInputStateCreateInfo,
            
            VertexAttributeDescriptionCount = (uint) vertexInputLength,
            PVertexAttributeDescriptions = vertexAttributes,
            
            VertexBindingDescriptionCount = vertexInputBinding.Stride > 0 ? 1u : 0u,
            PVertexBindingDescriptions = &vertexInputBinding
        };

        PipelineInputAssemblyStateCreateInfo inputAssemblyState = new()
        {
            SType = StructureType.PipelineInputAssemblyStateCreateInfo,
            Topology = PrimitiveTopology.TriangleList
        };

        PipelineViewportStateCreateInfo viewportState = new()
        {
            SType = StructureType.PipelineViewportStateCreateInfo,
            ViewportCount = 1,
            ScissorCount = 1
        };

        PipelineRasterizationStateCreateInfo rasterizerState = new()
        {
            SType = StructureType.PipelineRasterizationStateCreateInfo,
            CullMode = CullModeFlags.None,
            LineWidth = 1.0f,
            PolygonMode = PolygonMode.Fill
        };

        PipelineMultisampleStateCreateInfo multisampleState = new()
        {
            SType = StructureType.PipelineMultisampleStateCreateInfo,
            RasterizationSamples = SampleCountFlags.Count1Bit
        };

        PipelineDepthStencilStateCreateInfo depthStencilState = new()
        {
            SType = StructureType.PipelineDepthStencilStateCreateInfo,
            DepthTestEnable = false
        };

        DynamicState* dynamicStates = stackalloc DynamicState[]
        {
            DynamicState.Viewport,
            DynamicState.Scissor
        };

        PipelineDynamicStateCreateInfo dynamicState = new()
        {
            SType = StructureType.PipelineDynamicStateCreateInfo,
            DynamicStateCount = 2,
            PDynamicStates = dynamicStates
        };

        PipelineColorBlendAttachmentState blendAttachment = new()
        {
            BlendEnable = false,
            ColorWriteMask = ColorComponentFlags.RBit | ColorComponentFlags.GBit | ColorComponentFlags.BBit | ColorComponentFlags.ABit
        };

        PipelineColorBlendStateCreateInfo blendState = new()
        {
            SType = StructureType.PipelineColorBlendStateCreateInfo,
            AttachmentCount = 1,
            PAttachments = &blendAttachment
        };

        // TODO: Obviously, temporary. This will have to be changed once render targets are implemented.
        Format colorAttachmentFormat = Format.B8G8R8A8Unorm;
        PipelineRenderingCreateInfo renderingInfo = new()
        {
            SType = StructureType.PipelineRenderingCreateInfo,
            ColorAttachmentCount = 1,
            PColorAttachmentFormats = &colorAttachmentFormat
        };

        GraphicsPipelineCreateInfo pipelineInfo = new()
        {
            SType = StructureType.GraphicsPipelineCreateInfo,
            Layout = _layout,

            StageCount = (uint) numShaderStages,
            PStages = shaderStages,
            PVertexInputState = &vertexInputState,
            PInputAssemblyState = &inputAssemblyState,
            PViewportState = &viewportState,
            PRasterizationState = &rasterizerState,
            PMultisampleState = &multisampleState,
            PDepthStencilState = &depthStencilState,
            PColorBlendState = &blendState,
            PDynamicState = &dynamicState,
            PNext = &renderingInfo
        };

        _vk.CreateGraphicsPipelines(_device.Device, new PipelineCache(), 1, &pipelineInfo, null, out _pipeline)
            .Check("Create pipeline");
    }
    
    public override void UpdateVertices<T>(uint offset, ReadOnlySpan<T> vertices)
    {
        fixed (void* pVertices = vertices)
            _device.CopyToBuffer(_vertexBuffer, offset, (uint) (vertices.Length * sizeof(T)), pVertices);
    }
    
    public override void UpdateIndices(uint offset, ReadOnlySpan<uint> indices)
    {
        fixed (void* pVertices = indices)
            _device.CopyToBuffer(_indexBuffer, offset, (uint) (indices.Length * sizeof(uint)), pVertices);
    }

    public override void PushUniformData(uint index, uint offset, uint sizeInBytes, void* pData)
    {
        
    }

    public override void PushTexture(uint index, Texture texture)
    {
        VkTexture vkTexture = (VkTexture) texture;

        DescriptorImageInfo imageInfo = new()
        {
            ImageView = vkTexture.ImageView,
            Sampler = _device.GetSampler(texture.Sampler)
        };

        WriteDescriptorSet writeDescriptor = new()
        {
            SType = StructureType.WriteDescriptorSet,
            DescriptorType = DescriptorType.CombinedImageSampler,
            DstBinding = index,
            DescriptorCount = 1,
            DstSet = _descriptorSet,
            PImageInfo = &imageInfo
        };
        
        _vk.UpdateDescriptorSets(_device.Device, 1, &writeDescriptor, null);
    }
    
    public override void Draw()
    {
        Draw(_numElements);
    }
    
    public override void Draw(uint numElements)
    {
        CommandBuffer cb = _device.CurrentCommandBuffer;
        
        _vk.CmdBindPipeline(cb, PipelineBindPoint.Graphics, _pipeline);

        if (_vertexBuffer.Buffer.Handle != 0)
        {
            Buffer buffer = _vertexBuffer.Buffer;
            ulong offset = 0;
            _vk.CmdBindVertexBuffers(cb, 0, 1, &buffer, &offset);
        }

        if (_vertexBuffer.Buffer.Handle != 0)
        {
            _vk.CmdBindIndexBuffer(cb, _indexBuffer.Buffer, 0, IndexType.Uint32);
            _vk.CmdDrawIndexed(cb, numElements, 1, 0, 0, 0);
        }
        else
            _vk.CmdDraw(cb, numElements, 1, 0, 0);
    }
    
    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        
        if (_vertexBuffer.Buffer.Handle != 0)
            vmaDestroyBuffer(_device.Allocator, _vertexBuffer.Buffer, _vertexBuffer.Allocation);

        if (_indexBuffer.Buffer.Handle != 0)
            vmaDestroyBuffer(_device.Allocator, _indexBuffer.Buffer, _indexBuffer.Allocation);
        
        _vk.DestroyPipeline(_device.Device, _pipeline, null);
        _vk.DestroyPipelineLayout(_device.Device, _layout, null);
    }
}