using SDL3;

namespace Sprout.Graphics.Tests;

public class TestBase(Backend backend)
{
    public const int WindowWidth = 800;
    public const int WindowHeight = 600;
    
    private IntPtr _window;
    private bool _alive;
    
    protected GraphicsDevice Device;
    
    [SetUp]
    public void Setup()
    {
        bool init = SDL.Init(SDL.InitFlags.Video);
        Assert.That(init, Is.True, SDL.GetError);

        _window = SDL.CreateWindow(TestContext.CurrentContext.Test.Name, WindowWidth, WindowHeight, SDL.WindowFlags.OpenGL);
        Assert.That(_window, Is.Not.EqualTo(0), SDL.GetError);

        Device = GraphicsDevice.Create(_window, backend);

        _alive = true;
    }

    [TearDown]
    public void TearDown()
    {
        Device.Dispose();
        SDL.DestroyWindow(_window);
        SDL.Quit();
    }

    protected void EnterLoop(Action<float>? loopEvent = null)
    {
        while (_alive)
        {
            while (SDL.PollEvent(out SDL.Event winEvent))
            {
                switch ((SDL.EventType) winEvent.Type)
                {
                    case SDL.EventType.WindowCloseRequested:
                    case SDL.EventType.Quit:
                        _alive = false;
                        break;
                }
            }

            loopEvent?.Invoke(1 / 60.0f);
            Device.Present();
        }
    }
}