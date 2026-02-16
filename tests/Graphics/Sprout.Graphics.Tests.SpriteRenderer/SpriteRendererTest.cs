using System.Drawing;
using System.Numerics;
using Sprout.Graphics.Tests.Base;

namespace Sprout.Graphics.Tests.SpriteRenderer;

public sealed class SpriteRendererTest() : TestBase("Sprite Renderer Test")
{
    private Graphics.SpriteRenderer _renderer;
    private Texture _texture;

    private float _rotation;

    protected override void Load()
    {
        _renderer = new Graphics.SpriteRenderer(Device);
        _texture = Device.CreateTexture("DEBUG.png");
    }

    protected override void Loop(float dt)
    {
        _rotation += dt;
        if (_rotation >= float.Pi * 2)
            _rotation -= float.Pi * 2;
        
        Device.Clear(Color.CornflowerBlue);
        
        _renderer.Draw(_texture, new Vector2(0, 0), new Vector2(256, 0), new Vector2(0, 128), new Vector2(128, 128));
        _renderer.Draw(_texture, new Vector2(50, 500), new Rectangle(64, 64, 128, 64));
        _renderer.Draw(_texture, new Vector2(300, 0), new Size(256, 64));
        _renderer.Draw(_texture, Matrix3x2.CreateRotation(_rotation) * Matrix3x2.CreateTranslation(200, 150), tint: Color.Orange);
        _renderer.Draw(_texture, new Vector2(600, 300), _rotation * 2, new Vector2(2, 1), new Vector2(_texture.Size.Width, _texture.Size.Height / 2));
        
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