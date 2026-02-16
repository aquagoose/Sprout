using System.Drawing;
using Sprout.Graphics.Tests.Base;

namespace Sprout.Graphics.Tests.ClearColor;

public class ClearColorTest() : TestBase("Clear Color")
{
    private readonly Color[] _colors = 
    [
        new Color(255, 0, 0),
        new Color(0, 255, 0),
        new Color(0, 0, 255),
        new Color(255, 255, 0),
        new Color(255, 0, 255),
        new Color(0, 255, 255),
        new Color(255, 255, 255),
        new Color(0, 0, 0)
    ];

    private int _currentColor;
    private float _timer;
    
    protected override void Loop(float dt)
    {
        _timer += dt;
        if (_timer >= 1.0f)
        {
            _timer -= 1.0f;
            _currentColor++;
            if (_currentColor >= _colors.Length)
                _currentColor = 0;
        }
        
        Device.Clear(_colors[_currentColor]);
        Device.Present();
    }
}