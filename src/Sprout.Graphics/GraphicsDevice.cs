namespace Sprout.Graphics;

public abstract class GraphicsDevice : IDisposable
{
    public abstract bool IsDisposed { get; protected set; }

    public abstract void Dispose();
}