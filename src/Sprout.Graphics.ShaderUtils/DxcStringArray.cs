using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sprout.Graphics.ShaderUtils;

internal unsafe struct DxcStringArray : IDisposable
{
    private byte** _array;

    public readonly uint Length;

    public nint Handle => (nint) _array;

    public DxcStringArray(ReadOnlySpan<string> strings)
    {
        Length = (uint) strings.Length;
        _array = (byte**) NativeMemory.Alloc((nuint) strings.Length);

        for (int i = 0; i < strings.Length; i++)
        {
            char[] chars = strings[i].ToCharArray();
            if (OperatingSystem.IsWindows())
            {
                uint byteCount = (uint) ((chars.Length + 1) * sizeof(char));
                _array[i] = (byte*) NativeMemory.Alloc(byteCount);
                // Ensure null byte is at end of string.
                _array[i][byteCount - 1] = 0;
                _array[i][byteCount - 2] = 0;
                fixed (char* pChars = chars)
                    Unsafe.CopyBlock(_array[i], pChars, byteCount - 1);
            }
            else
            {
                int[] ints = new int[chars.Length + 1];
                for (int j = 0; j < chars.Length; j++)
                    ints[j] = chars[j];
                
                uint byteCount = (uint) (ints.Length * sizeof(int));
                _array[i] = (byte*) NativeMemory.Alloc(byteCount);
                
                fixed (int* pInts = ints)
                    Unsafe.CopyBlock(_array[i], pInts, byteCount);
            }
        }
    }

    public DxcStringArray(List<string> list) : this(CollectionsMarshal.AsSpan(list)) { }

    public static implicit operator char**(DxcStringArray array)
        => (char**) array._array;
    
    public void Dispose()
    {
        for (int i = 0; i < Length; i++)
            NativeMemory.Free(_array[i]);
        
        NativeMemory.Free(_array);
    }
}