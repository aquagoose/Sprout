using System.Drawing;
using SDL3;
using Sprout.Graphics;

namespace Sprout;

public class Window : IDisposable
{
    private readonly IntPtr _window;

    public IntPtr Handle => _window;

    public Window(string title, Size size, Backend backend)
    {
        if (!SDL.Init(SDL.InitFlags.Video))
            throw new Exception($"Failed to initialize SDL: {SDL.GetError()}");

        SDL.WindowFlags flags = 0;
        
        switch (backend)
        {
            case Backend.Unknown:
                break;
            case Backend.Vulkan:
                break;
            case Backend.OpenGL:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(backend), backend, null);
        }
        
        _window = SDL.CreateWindow(title, size.Width, size.Height, 0);
        if (_window == 0)
            throw new Exception($"Failed to create SDL window: {SDL.GetError()}");
    }

    public void Dispose()
    {
        
    }
}