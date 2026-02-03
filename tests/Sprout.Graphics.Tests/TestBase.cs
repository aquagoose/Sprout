using SDL3;

namespace Sprout.Graphics.Tests;

public class TestBase(Backend backend)
{
    public const int WindowWidth = 800;
    public const int WindowHeight = 600;
    
    private IntPtr _window;
    
    protected GraphicsDevice Device;
    
    [SetUp]
    public void Setup()
    {
        bool init = SDL.Init(SDL.InitFlags.Video);
        Assert.That(init, Is.True, SDL.GetError);

        _window = SDL.CreateWindow("Test", WindowWidth, WindowHeight, SDL.WindowFlags.OpenGL);
        Assert.That(_window, Is.Not.EqualTo(0), SDL.GetError);

        Device = GraphicsDevice.Create(_window, backend);
    }

    [TearDown]
    public void TearDown()
    {
        Device.Dispose();
        SDL.DestroyWindow(_window);
        SDL.Quit();
    }

}