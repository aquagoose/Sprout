using System.Numerics;
using Sprout.Graphics.Tests.Base;

namespace Sprout.Graphics.Tests.RenderTexture;

public class RenderTextureTest() : TestBase("Render Texture Test")
{
    private SpriteRenderer _renderer = null!;
    private Texture _texture = null!;
    //private Texture _renderTexture = null!;
    
    protected override void Load()
    {
        _renderer = new SpriteRenderer(Device);
        _texture = Device.CreateTexture("DEBUG.png");
        //_renderTexture =
        //    Device.CreateTexture(128, 128, PixelFormat.RGBA8, TextureUsage.RenderTexture | TextureUsage.Shader);
    }

    protected override void Loop(float dt)
    {
        //Device.SetRenderTexture(_renderTexture);
        //Device.Clear(Color.CornflowerBlue);
        
        //Device.SetRenderTexture(null);
        Device.Clear(Color.Black);
        
        //_renderer.Draw(_renderTexture, Vector2.Zero);
        _renderer.Draw(_texture, new Vector2(100));
        _renderer.Render();
        
        Device.Present();
    }

    public override void Dispose()
    {
        //_renderTexture.Dispose();
        _texture.Dispose();
        
        base.Dispose();
    }
}