using SDL3;

namespace Sprout.Graphics.Tests.Base;

public abstract class TestBase(string testName) : IDisposable
{
    public const int WindowWidth = 800;
    public const int WindowHeight = 600;
    
    private IntPtr _window;
    private bool _alive;

    protected GraphicsDevice Device;

    protected abstract void Loop(float dt);

    public void Run()
    {
        if (!SDL.Init(SDL.InitFlags.Video | SDL.InitFlags.Events))
            throw new Exception($"Failed to initialize SDL: {SDL.GetError()}");

        const Backend backend = Backend.OpenGL;

        SDL.WindowFlags flags = 0;

        switch (backend)
        {
            case Backend.OpenGL:
            {
                flags |= SDL.WindowFlags.OpenGL;
                SDL.GLSetAttribute(SDL.GLAttr.ContextMajorVersion, 3);
                SDL.GLSetAttribute(SDL.GLAttr.ContextMinorVersion, 3);
                SDL.GLSetAttribute(SDL.GLAttr.ContextProfileMask, (int) SDL.GLProfile.Core);
                if (OperatingSystem.IsMacOS())
                    SDL.GLSetAttribute(SDL.GLAttr.ContextFlags, (int) SDL.GLContextFlag.ForwardCompatible);

                break;
            }
        }

        string appName = $"{testName} ({backend})";
        SDL.SetAppMetadata(appName, "1.0.0", "");
        _window = SDL.CreateWindow(appName, WindowWidth, WindowHeight, flags);
        if (_window == 0)
            throw new Exception($"Failed to create SDL window: {SDL.GetError()}");

        Device = GraphicsDevice.Create(_window, backend);
        
        _alive = true;
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
            
            Loop(1f / 60.0f);
        }
    }

    public virtual void Dispose()
    {
        Device.Dispose();
        SDL.DestroyWindow(_window);
        SDL.Quit();
    }
}