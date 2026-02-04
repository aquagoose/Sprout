namespace Sprout.Graphics;

public abstract class Shader : IDisposable
{
    /// <summary>
    /// Gets if this <see cref="Shader"/> has been disposed.
    /// </summary>
    public abstract bool IsDisposed { get; protected set; }
    
    /// <summary>
    /// Dispose of this <see cref="Shader"/>.
    /// </summary>
    public abstract void Dispose();
}