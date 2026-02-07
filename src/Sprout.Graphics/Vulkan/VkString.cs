using System.Runtime.InteropServices;
using System.Text;

namespace Sprout.Graphics.Vulkan;

internal unsafe struct VkString : IDisposable
{
    private GCHandle _handle;

    public nint Handle => _handle.AddrOfPinnedObject();
    
    public VkString(string @string)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(@string);
        _handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
    }

    public static implicit operator VkString(string @string)
        => new VkString(@string);

    public static implicit operator byte*(VkString str)
        => (byte*) str.Handle;

    public void Dispose()
    {
        _handle.Free();
    }
}