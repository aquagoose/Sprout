using System.Diagnostics;
using System.Runtime.InteropServices;
using Silk.NET.Vulkan;
using static Sprout.Graphics.Vulkan.VMA.Vma;

namespace Sprout.Graphics.Vulkan;

internal sealed unsafe class VkRenderable : Renderable
{
    public override bool IsDisposed { get; protected set; }

    private readonly Vk _vk;
    private readonly VkGraphicsDevice _device;

    private readonly PipelineLayout _layout;
    private readonly Pipeline _pipeline;

    private readonly VkBuffer _vertexBuffer;
    private readonly VkBuffer _indexBuffer;

    public VkRenderable(Vk vk, VkGraphicsDevice device, ref readonly RenderableInfo info)
    {
        _vk = vk;
        _device = device;

        VkShader shader = (VkShader) info.Shader;
        
        PipelineLayoutCreateInfo layoutInfo = new()
        {
            SType = StructureType.PipelineLayoutCreateInfo
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

        PipelineVertexInputStateCreateInfo vertexInputState = new()
        {
            SType = StructureType.PipelineVertexInputStateCreateInfo
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

        if (info.NumVertices > 0)
        {
            Debug.Assert(info.VertexSize > 0);
            
            _vertexBuffer = VkHelper.CreateBuffer(_device.Allocator,
                BufferUsageFlags.VertexBufferBit | BufferUsageFlags.TransferDstBit, info.NumVertices * info.VertexSize);
        }

        if (info.NumIndices > 0)
        {
            Debug.Assert(info.NumVertices > 0);

            _indexBuffer = VkHelper.CreateBuffer(_device.Allocator,
                BufferUsageFlags.IndexBufferBit | BufferUsageFlags.TransferDstBit, info.NumIndices * sizeof(uint));
        }
    }
    
    public override void UpdateVertices<T>(uint offset, ReadOnlySpan<T> vertices)
    {
        throw new NotImplementedException();
    }
    
    public override void UpdateIndices(uint offset, ReadOnlySpan<uint> indices)
    {
        throw new NotImplementedException();
    }

    public override void PushUniformData(uint index, uint offset, uint sizeInBytes, void* pData)
    {
        throw new NotImplementedException();
    }

    public override void PushTexture(uint index, Texture texture)
    {
        throw new NotImplementedException();
    }
    
    public override void Draw()
    {
        throw new NotImplementedException();
    }
    
    public override void Draw(uint numElements)
    {
        CommandBuffer cb = _device.CommandBuffer;
        
        _vk.CmdBindPipeline(cb, PipelineBindPoint.Graphics, _pipeline);
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