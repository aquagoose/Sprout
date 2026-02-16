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
        
        // TEST PREFACE:
        // All sprites should be at their respective positions. If they are not, it means that the position code is
        // incorrect. These tests assume that the basic texture drawing & positioning is functional.
        
        // Test to ensure the TL, TR, BL, and BR coordinates function correctly.
        // The texture should be distinctly distorted enough to determine each point is correct,
        // but should be entirely visible.
        _renderer.Draw(_texture, new Vector2(10, 0), new Vector2(192, 0), new Vector2(20, 100), new Vector2(128, 128));
        
        // Test the source rectangle functionality.
        // This should show a 128x64 box, showing the (64,64) and (128,64) squares.
        _renderer.Draw(_texture, new Vector2(600, 500), new Rectangle(64, 64, 128, 64));
        
        // Test the manual sizing function.
        // The sprite should be squished with a size of 256x64
        _renderer.Draw(_texture, new Vector2(300, 0), new Size(256, 64));
        
        // Test the matrix function and sprite tinting.
        // The sprite should be rotating around its (0,0) coordinate, and the white box at (128,128) should be orange.
        _renderer.Draw(_texture, Matrix3x2.CreateRotation(_rotation) * Matrix3x2.CreateTranslation(200, 150), tint: Color.Orange);
        
        // Test the rotation, scale, and origin function, and to ensure that the origin works as intended when scaled.
        // The sprite should be stretched by a factor of 2 in the X axis, and should be rotating around its right edge
        // (yellow box) and should be centered in the Y axis.
        _renderer.Draw(_texture, new Vector2(600, 300), _rotation * 2, new Vector2(2, 1),
            new Vector2(_texture.Size.Width, _texture.Size.Height / 2));

        // Test the various flipping modes, and to check that flipping with a source rectangle only flips the visible
        // sprite, and not the whole sprite sheet.
        int xSize = 70;
        int yPos = WindowHeight - 100;
        // Test X flipping. The text "Flip X" should be at the correct orientation,
        // and the sprite should show only the (0,64) box.
        _renderer.Draw(_texture, new Vector2(0, yPos), new Rectangle(64, 0, 64, 64), flip: SpriteFlip.FlipX);
        // Test Y flipping. The text "Flip Y" should be at the correct orientation,
        // and the sprite should only show the (64,64) box.
        _renderer.Draw(_texture, new Vector2(xSize, yPos), new Rectangle(64, 64, 64, 64), flip: SpriteFlip.FlipY);
        // Test XY flipping. The text "Flip XY" should be at the correct orientation,
        // and the sprite should show only the (64,128) box.
        _renderer.Draw(_texture, new Vector2(xSize * 2, yPos), new Rectangle(64, 128, 64, 64), flip: SpriteFlip.FlipXY);
        
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