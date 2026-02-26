using System.Drawing;
using System.Numerics;
using Sprout.Graphics.Tests.Base;

namespace Sprout.Graphics.Tests.RenderTexture;

public class RenderTextureTest() : TestBase("Render Texture Test")
{
    private SpriteRenderer _renderer = null!;
    private Texture _texture = null!;
    private Texture _renderTexture = null!;
    private float _rotation;
    
    protected override void Load()
    {
        _renderer = new SpriteRenderer(Device);
        _texture = Device.CreateTexture("DEBUG.png");
        _renderTexture =
            Device.CreateTexture(128, 128, PixelFormat.RGBA8, TextureUsage.RenderTexture | TextureUsage.Shader);
        _renderTexture.Sampler = Sampler.PointWrap;
    }

    protected override void Loop(float dt)
    {
        _rotation += dt;
        if (_rotation >= float.Pi * 2)
            _rotation -= float.Pi * 2;
        
        Device.SetRenderTexture(_renderTexture);
        Device.Clear(Color.CornflowerBlue);

        _renderer.Draw(_texture, new Vector2(128) / 2, _rotation, new Vector2(0.2f),
            new Vector2(_texture.Size.Width, _texture.Size.Height) / 2);
        _renderer.Render();
        
        Device.SetRenderTexture(null);
        Device.Clear(Color.Black);
        
        _renderer.Draw(_renderTexture, new Vector2(0), WindowSize);
        _renderer.Render();
        
        Device.Present();
    }

    public override void Dispose()
    {
        _renderTexture.Dispose();
        _texture.Dispose();
        _renderer.Dispose();
        
        base.Dispose();
    }
}