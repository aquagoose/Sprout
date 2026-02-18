using System.Numerics;
using Sprout.Graphics;

namespace Sprout.Tests.TestGame;

public class Game : App
{
    private SpriteRenderer _spriteRenderer = null!;
    private Texture _texture = null!;
    
    protected override void Initialize()
    {
        _spriteRenderer = new SpriteRenderer(GraphicsDevice);
        _texture = GraphicsDevice.CreateTexture("Content/BAGELMIP.png");
    }

    protected override void Draw()
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        _spriteRenderer.Draw(_texture, new Vector2(0, 0));
        _spriteRenderer.Render();
    }
}