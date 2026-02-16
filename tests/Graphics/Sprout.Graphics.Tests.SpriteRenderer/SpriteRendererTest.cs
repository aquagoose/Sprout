using System.Drawing;
using System.Numerics;
using Sprout.Graphics.Tests.Base;

namespace Sprout.Graphics.Tests.SpriteRenderer;

public sealed class SpriteRendererTest() : TestBase("Sprite Renderer Test")
{
    private Graphics.SpriteRenderer _renderer;
    private Texture _texture;

    protected override void Load()
    {
        _renderer = new Graphics.SpriteRenderer(Device);
        _texture = Device.CreateTexture("DEBUG.png");
    }

    protected override void Loop(float dt)
    {
        Device.Clear(Color.CornflowerBlue);
        
        _renderer.Draw(_texture, new Vector2(0));
        _renderer.Draw(_texture, new Vector2(50));
        _renderer.Draw(_texture, new Vector2(100));
        _renderer.Render();
        
        Device.Present();
    }

    public override void Dispose()
    {
        _renderer.Dispose();
        _texture.Dispose();
        
        base.Dispose();
    }
}