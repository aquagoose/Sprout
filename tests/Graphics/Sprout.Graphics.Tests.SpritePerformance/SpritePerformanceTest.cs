using System.Numerics;
using Sprout.Graphics.Tests.Base;

namespace Sprout.Graphics.Tests.SpritePerformance;

public class SpritePerformanceTest() : TestBase("Sprite Renderer Performance Test")
{
    private SpriteRenderer _renderer;
    private Texture _white;
    private float _value;

    protected override void Load()
    {
        _renderer = new Graphics.SpriteRenderer(Device);
        _white = Device.CreateTexture<byte>(1, 1, PixelFormat.RGBA8, [255, 255, 255, 255]);
    }

    protected override void Loop(float dt)
    {
        _value += dt;
        if (_value >= float.Pi * 2)
            _value -= float.Pi * 2;
        
        Device.Clear(Color.Black);

        float h = _value;
        for (int y = 0; y < WindowHeight; y++)
        {
            for (int x = 0; x < WindowWidth; x++)
            {
                _renderer.Draw(_white, new Vector2(x, y), tint: Color.FromHSV(h, 1, 1));
                h += 0.0001f;
            }
        }
        
        _renderer.Render();
        
        Device.Present();
    }

    public override void Dispose()
    {
        _white.Dispose();
        _renderer.Dispose();
        base.Dispose();
    }
}