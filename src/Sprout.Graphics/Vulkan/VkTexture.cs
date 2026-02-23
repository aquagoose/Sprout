using System.Drawing;
using Silk.NET.Vulkan;

namespace Sprout.Graphics.Vulkan;

internal sealed unsafe class VkTexture : Texture
{
    public override bool IsDisposed { get; protected set; }

    private readonly Vk _vk;
    private readonly VkGraphicsDevice _device;
    
    public override Sampler Sampler { get; set; }

    public readonly Image Image;

    public readonly ImageView ImageView;
    
    public VkTexture(Vk vk, VkGraphicsDevice device, uint width, uint height, PixelFormat format, TextureUsage usage,
        void* pData) : base(new Size((int) width, (int) height), format, usage)
    {
        _vk = vk;
        _device = device;

        (Format vkFormat, uint bytesPerPixel) = format switch
        {
            PixelFormat.RGBA8 => (Silk.NET.Vulkan.Format.R8G8B8A8Unorm, 4u),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };

        ImageUsageFlags usageFlags = 0;
        if ((usage & TextureUsage.Shader) != 0)
            usageFlags |= ImageUsageFlags.SampledBit;
        if ((usage & TextureUsage.RenderTexture) != 0)
            usageFlags |= ImageUsageFlags.ColorAttachmentBit;

        Image = VkHelper.CreateImage(vk, _device.Device, width, height, vkFormat, usageFlags);
        ImageView = VkHelper.CreateImageView(vk, _device.Device, Image, vkFormat);
        
        _device.CopyToTexture(Image, Size, bytesPerPixel, pData);
    }
    
    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        
        _vk.DestroyImageView(_device.Device, ImageView, null);
        _vk.DestroyImage(_device.Device, Image, null);
    }
}