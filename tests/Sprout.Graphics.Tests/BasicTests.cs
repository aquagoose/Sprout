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
        EnterLoop();
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

        int currentColor = 0;
        float counter = 0;
        
        EnterLoop(dt =>
        {
            counter += dt;
            if (counter >= 1.0f)
            {
                counter -= 1.0f;
                currentColor++;
                if (currentColor >= colors.Length)
                    currentColor = 0;
            }

            Color color = colors[currentColor];
            
            Device.Clear(color);
        });
    }
}