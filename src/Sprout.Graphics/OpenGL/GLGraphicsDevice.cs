namespace Sprout.Graphics.OpenGL;

internal sealed class GLGraphicsDevice : GraphicsDevice
{
    public override bool IsDisposed { get; protected set; }
    
    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
    }
}