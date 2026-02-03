using SDL3;

namespace Sprout.Graphics.Tests;

public class BasicTests
{
    private const int WindowWidth = 800;
    private const int WindowHeight = 600;
    
    private IntPtr _window;
    private GraphicsDevice _device;
    
    [SetUp]
    public void Setup()
    {
        bool init = SDL.Init(SDL.InitFlags.Video);
        Assert.That(init, Is.True, SDL.GetError);

        _window = SDL.CreateWindow("Test", WindowWidth, WindowHeight, SDL.WindowFlags.OpenGL);
        Assert.That(_window, Is.Not.EqualTo(0), SDL.GetError);

        _device = GraphicsDevice.Create(_window, Backend.OpenGL);
    }

    [TearDown]
    public void TearDown()
    {
        _device.Dispose();
        SDL.DestroyWindow(_window);
        SDL.Quit();
    }

    [Test]
    public void BasicPresentation()
    {
        _device.Present();
        Assert.Pass();
    }
}