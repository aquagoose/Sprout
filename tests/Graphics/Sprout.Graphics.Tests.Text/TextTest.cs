using System.Numerics;
using Sprout.Graphics.Tests.Base;

namespace Sprout.Graphics.Tests.Text;

public class TextTest() : TestBase("Text Test")
{
    private SpriteRenderer _renderer = null!;
    private Font _font = null!;

    protected override void Load()
    {
        _renderer = new SpriteRenderer(Device);
        _font = new Font(Device, "/Users/aqua/Downloads/Noto_Sans_JP/static/NotoSansJP-Regular.ttf"u8);
    }

    protected override void Loop(float dt)
    {
        Device.Clear(Color.CornflowerBlue);
        
        _font.Draw(_renderer, Vector2.Zero, 48, "Hello world!\nこれは日本語のテキストです！", Color.White);
        _font.Draw(_renderer, new Vector2(0, 50), 232, "Huge Text Big Large", Color.White);
        _renderer.Render();
        
        Device.Present();
    }

    public override void Dispose()
    {
        _font.Dispose();
        _renderer.Dispose();
        base.Dispose();
    }
}