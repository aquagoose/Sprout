using System.Runtime.InteropServices;

namespace Sprout.Graphics.ShaderUtils;

internal struct DxcString : IDisposable
{
    private GCHandle _handle;

    public nint Handle => _handle.AddrOfPinnedObject();

    public DxcString(string @string)
    {
        char[] chars = @string.ToCharArray();

        // On Windows, DXC is compiled with 2-byte chars, so we can just alloc the char array directly.
        if (OperatingSystem.IsWindows())
        {
            _handle = GCHandle.Alloc(chars, GCHandleType.Pinned);
            return;
        }
        
        // On other platforms, DXC is compiled with 4 byte chars for some reason, so we need to convert them to int.
        // This is absolutely terrible and I hate this but that's how it goes.
        int[] ints = new int[chars.Length];
        for (int i = 0; i < chars.Length; i++)
            ints[i] = chars[i];

        _handle = GCHandle.Alloc(ints, GCHandleType.Pinned);
    }

    public static implicit operator DxcString(string @string)
        => new DxcString(@string);

    public static unsafe implicit operator char*(DxcString str)
        => (char*) str.Handle;
    
    public void Dispose()
    {
        _handle.Free();
    }
}