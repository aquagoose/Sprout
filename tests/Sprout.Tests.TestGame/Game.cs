using System.Numerics;
using Sprout.Graphics;

namespace Sprout.Tests.TestGame;

public class Game : App
{
    private SpriteRenderer _spriteRenderer = null!;
    private Texture _texture = null!;
    private Vector2 _position;
    
    protected override void Initialize()
    {
        _spriteRenderer = new SpriteRenderer(Device);
        _texture = Device.CreateTexture("Content/BAGELMIP.png");
    }

    protected override void Update(float dt)
    {
        const float speed = 100;
        
        if (Input.IsKeyPressed(Key.Escape))
            Close();

        if (Input.IsKeyDown(Key.W))
            _position.Y -= speed * dt;
        if (Input.IsKeyDown(Key.S))
            _position.Y += speed * dt;
        if (Input.IsKeyDown(Key.D))
            _position.X += speed * dt;
        if (Input.IsKeyDown(Key.A))
            _position.X -= speed * dt;
        if (Input.IsKeyPressed(Key.Space))
            _position = Input.MousePosition;

        if (Input.IsMouseButtonDown(MouseButton.Left))
            _position += Input.MouseDelta;
        if (Input.IsMouseButtonPressed(MouseButton.Right))
            _position = Input.MousePosition;
    }

    protected override void Draw()
    {
        Device.Clear(Color.CornflowerBlue);
        
        _spriteRenderer.Draw(_texture, _position);
        _spriteRenderer.Render();
    }
}