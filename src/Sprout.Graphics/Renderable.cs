namespace Sprout.Graphics;

public abstract class Renderable : IDisposable
{
    /// <summary>
    /// Gets if this <see cref="Renderable"/> has been disposed.
    /// </summary>
    public abstract bool IsDisposed { get; protected set; }

    public abstract void UpdateVertices<T>(uint offset, ReadOnlySpan<T> vertices) where T : unmanaged;

    public abstract void UpdateIndices(uint offset, ReadOnlySpan<uint> indices);

    public abstract void PushTexture(uint index, Texture texture);

    public abstract void Draw();
    
    public abstract void Draw(uint numElements);
    
    /// <summary>
    /// Dispose of this <see cref="Renderable"/>.
    /// </summary>
    public abstract void Dispose();
}