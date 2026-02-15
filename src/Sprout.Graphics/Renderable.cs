using System.Runtime.CompilerServices;

namespace Sprout.Graphics;

public abstract class Renderable : IDisposable
{
    /// <summary>
    /// Gets if this <see cref="Renderable"/> has been disposed.
    /// </summary>
    public abstract bool IsDisposed { get; protected set; }

    public abstract void UpdateVertices<T>(uint offset, ReadOnlySpan<T> vertices) where T : unmanaged;

    public abstract void UpdateIndices(uint offset, ReadOnlySpan<uint> indices);

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

    public abstract void Draw();
    
    public abstract void Draw(uint numElements);
    
    /// <summary>
    /// Dispose of this <see cref="Renderable"/>.
    /// </summary>
    public abstract void Dispose();
}