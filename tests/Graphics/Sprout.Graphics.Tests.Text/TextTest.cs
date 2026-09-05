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
        _font = new Font(Device, "NotoSans-Regular.ttf");
        _font.AddFont("NotoSansJP-Regular.ttf");
        _font.AddFont("NotoEmoji-Regular.ttf");
    }

    protected override void Loop(float dt)
    {
        Device.Clear(Color.CornflowerBlue);
        using SpriteRenderer.Pass pass = _renderer.BeginPass();

        _font.Draw(pass, Vector2.Zero, 48, "Hello world!😀🌱👍\nこれは日本語のテキストです！", Color.White);
        _font.Draw(pass, new Vector2(0, 60), 232, "Huge Text Big Large", Color.White);
    }

    public override void Dispose()
    {
        _font.Dispose();
        _renderer.Dispose();
        base.Dispose();
    }
}