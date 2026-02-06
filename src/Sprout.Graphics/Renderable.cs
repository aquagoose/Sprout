namespace Sprout.Graphics;

public abstract class Renderable : IDisposable
{
    /// <summary>
    /// Gets if this <see cref="Renderable"/> has been disposed.
    /// </summary>
    public abstract bool IsDisposed { get; protected set; }

    public abstract void UpdateVertices<T>(ReadOnlySpan<T> vertices) where T : unmanaged;

    public abstract void UpdateIndices(ReadOnlySpan<uint> indices);

    public abstract void Draw(uint numElements);
    
    /// <summary>
    /// Dispose of this <see cref="Renderable"/>.
    /// </summary>
    public abstract void Dispose();
}