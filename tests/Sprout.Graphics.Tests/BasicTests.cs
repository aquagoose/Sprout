using System.Drawing;

namespace Sprout.Graphics.Tests;

public class BasicTests() : TestBase(Backend.OpenGL)
{
    [Test]
    public void CheckBackend()
    {
        Assert.That(Device.Backend, Is.EqualTo(Backend.OpenGL));
    }
    
    [Test]
    public void BasicPresentation()
    {
        Device.Present();
    }

    [Test]
    public void ClearColorTest()
    {
        Color[] colors =
        [
            Color.FromArgb(255, 0, 0),
            Color.FromArgb(0, 255, 0),
            Color.FromArgb(0, 0, 255),
            Color.FromArgb(255, 255, 0),
            Color.FromArgb(0, 255, 255),
            Color.FromArgb(255, 0, 255),
            Color.FromArgb(0, 0, 0),
            Color.FromArgb(255, 255, 255)
        ];

        foreach (Color color in colors)
        {
            Device.Clear(color);
            Device.Present();
        }
    }
}