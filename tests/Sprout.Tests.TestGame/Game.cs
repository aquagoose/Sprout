using System.Numerics;
using Sprout.Graphics;

namespace Sprout.Tests.TestGame;

public class Game : IApp
{
    private SpriteRenderer _spriteRenderer = null!;
    private Texture _texture = null!;
    private Vector2 _position;
    
    public void Initialize(App app)
    {
        _spriteRenderer = new SpriteRenderer(app.Graphics);
        _texture = app.Graphics.CreateTexture("Content/BAGELMIP.png");
    }

    public void Update(App app, float dt)
    {
        const float speed = 100;
        
        if (app.Input.IsKeyPressed(Key.Escape))
            app.Close();

        if (app.Input.IsKeyDown(Key.W))
            _position.Y -= speed * dt;
        if (app.Input.IsKeyDown(Key.S))
            _position.Y += speed * dt;
        if (app.Input.IsKeyDown(Key.D))
            _position.X += speed * dt;
        if (app.Input.IsKeyDown(Key.A))
            _position.X -= speed * dt;
        if (app.Input.IsKeyPressed(Key.Space))
            _position = app.Input.MousePosition;

        if (app.Input.IsMouseButtonDown(MouseButton.Left))
            _position += app.Input.MouseDelta;
        if (app.Input.IsMouseButtonPressed(MouseButton.Right))
            _position = app.Input.MousePosition;
    }

    public void Draw(App app)
    {
        app.Graphics.Clear(Color.CornflowerBlue);
        
        _spriteRenderer.Draw(_texture, _position);
        _spriteRenderer.Render();
    }
}