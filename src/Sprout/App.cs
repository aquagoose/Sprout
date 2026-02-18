using System.Diagnostics;
using System.Drawing;
using Sprout.Graphics;

namespace Sprout;

public abstract class App : IDisposable
{
    private Window _window = null!;
    private Events _events = null!;
    private GraphicsDevice _device = null;
    private bool _alive;

    public GraphicsDevice GraphicsDevice => _device;

    protected virtual void Initialize() { }

    protected virtual void Update(float dt) { }

    protected virtual void Draw() { }

    protected virtual void Unload() { }

    public void Run(in AppInfo info)
    {
        Backend backend;
        if (OperatingSystem.IsWindows())
            backend = Backend.D3D11;
        else
            backend = Backend.OpenGL;

        _window = new Window($"{info.AppName} ({backend})", new Size(1280, 720), backend);
        _events = new Events();
        _events.Quit += Close;

        _device = GraphicsDevice.Create(_window.Handle, backend);

        Initialize();
        
        Stopwatch sw = Stopwatch.StartNew();
        _alive = true;
        while (_alive)
        {
            _events.PollEvents();

            double dt = sw.Elapsed.TotalSeconds;
            sw.Restart();

            Update((float) dt);
            Draw();
            
            _device.Present();
        }
    }

    public void Close()
    {
        _alive = false;
    }

    public void Dispose()
    {
        Unload();
        _device.Dispose();
        _events.Dispose();
        _window.Dispose();
    }
}