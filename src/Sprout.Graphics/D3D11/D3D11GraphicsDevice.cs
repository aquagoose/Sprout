namespace Sprout.Graphics.D3D11;

internal sealed class D3D11GraphicsDevice : GraphicsDevice
{
    public override bool IsDisposed { get; protected set; }

    public override Backend Backend => Backend.D3D11;

    public D3D11GraphicsDevice(IntPtr sdlWindow)
    {
        
    }
    
    public override Shader CreateShader(params ReadOnlySpan<ShaderAttachment> attachments)
    {
        throw new NotImplementedException();
    }
    
    protected override unsafe Texture CreateTexture(uint width, uint height, PixelFormat format, void* data)
    {
        throw new NotImplementedException();
    }
    
    public override Renderable CreateRenderable(in RenderableInfo info)
    {
        throw new NotImplementedException();
    }
    
    public override void Clear(Color color)
    {
        throw new NotImplementedException();
    }
    
    public override void Present()
    {
        throw new NotImplementedException();
    }
    
    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
    }
}