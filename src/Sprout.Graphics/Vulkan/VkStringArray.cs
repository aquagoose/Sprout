using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Sprout.Graphics.Vulkan;

internal unsafe struct VkStringArray : IDisposable
{
    private byte** _array;

    public readonly uint Length;

    public nint Handle => (nint) _array;

    public VkStringArray(params ReadOnlySpan<string> strings)
    {
        Length = (uint) strings.Length;
        _array = (byte**) NativeMemory.Alloc((nuint) (Length * sizeof(byte*)));

        for (int i = 0; i < Length; i++)
        {
            byte[] data = Encoding.UTF8.GetBytes(strings[i]);
            uint dataSize = (uint) (data.Length * sizeof(byte));
            _array[i] = (byte*) NativeMemory.Alloc(dataSize + 1);
            // Ensure null byte at end of string.
            _array[i][data.Length] = 0;
            fixed (byte* pData = data)
                Unsafe.CopyBlock(_array[i], pData, dataSize);
        }
    }

    public VkStringArray(List<string> list) : this(CollectionsMarshal.AsSpan(list)) { }

    public static implicit operator byte**(VkStringArray arr)
        => arr._array;

    public void Dispose()
    {
        for (int i = 0; i < Length; i++)
            NativeMemory.Free(_array[i]);
        NativeMemory.Free(_array);
    }
}