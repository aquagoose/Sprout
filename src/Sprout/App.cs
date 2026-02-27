using System.Diagnostics;
using System.Drawing;
using Sprout.Audio;
using Sprout.Graphics;

namespace Sprout;

public abstract class App : IDisposable
{
    private Window _window = null!;
    private GraphicsDevice _graphicsDevice = null!;
    private AudioDevice _audioDevice = null!;
    private EventManager _eventManager = null!;
    private InputManager _inputManager = null!;
    private bool _alive;

    public Window Window => _window;

    public GraphicsDevice Graphics => _graphicsDevice;

    public AudioDevice Audio => _audioDevice;

    public EventManager Events => _eventManager;

    public InputManager Input => _inputManager;

    protected virtual void Initialize() { }

    protected virtual void Update(float dt) { }

    protected virtual void Draw() { }

    protected virtual void Unload() { }

    public void Run(in AppInfo info)
    {
        Backend backend = info.Backend;
        if (backend == Backend.Unknown)
        {
            if (OperatingSystem.IsWindows())
                backend = Backend.D3D11;
            else
                backend = Backend.OpenGL;
        }

        _window = new Window(in info.Window, backend);
        _window.Resized += WindowOnResized;
        
        _graphicsDevice = GraphicsDevice.Create(_window.Handle, backend);
        _audioDevice = new AudioDevice();

        _eventManager = new EventManager(_window);
        _inputManager = new InputManager(_eventManager);

        _eventManager.Quit += Close;
        
        Initialize();
        
        Stopwatch sw = Stopwatch.StartNew();
        _alive = true;
        while (_alive)
        {
            _inputManager.Update();
            _eventManager.PollEvents();

            double dt = sw.Elapsed.TotalSeconds;
            sw.Restart();

            Update((float) dt);
            Draw();
            
            _graphicsDevice.Present();
        }
    }

    public void Close()
    {
        _alive = false;
    }

    public void Dispose()
    {
        Unload();
        _inputManager.Dispose();
        _eventManager.Dispose();
        _audioDevice.Dispose();
        _graphicsDevice.Dispose();
        _window.Dispose();
    }
    
    private void WindowOnResized(uint width, uint height)
    {
        Graphics.ResizeSwapchain(width, height);
    }
}