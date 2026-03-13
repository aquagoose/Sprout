using System.Runtime.CompilerServices;

namespace Sprout.Graphics;

public abstract class Shader : IDisposable
{
    /// <summary>
    /// Gets if this <see cref="Shader"/> has been disposed.
    /// </summary>
    public abstract bool IsDisposed { get; protected set; }
    
    public abstract unsafe void PushUniformData(uint index, uint offset, uint sizeInBytes, void* pData);
    
    public unsafe void PushUniformData<T>(uint index, T data) where T : unmanaged
        => PushUniformData(index, 0, (uint) sizeof(T), Unsafe.AsPointer(ref data));

    public unsafe void PushUniformData<T>(uint index, uint offset, ReadOnlySpan<T> data) where T : unmanaged
    {
        fixed (void* pData = data)
            PushUniformData(index, offset, (uint) (data.Length * sizeof(T)), pData);
    }

    public void PushUniformData<T>(uint index, uint offset, T[] data) where T : unmanaged
        => PushUniformData(index, offset, data.AsSpan());
    
    public abstract void PushTexture(uint index, Texture texture);
    
    /// <summary>
    /// Dispose of this <see cref="Shader"/>.
    /// </summary>
    public abstract void Dispose();
}