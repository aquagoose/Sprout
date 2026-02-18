using System.Drawing;
using SDL3;
using Sprout.Graphics;

namespace Sprout;

public sealed class Window : IDisposable
{
    private readonly IntPtr _window;

    public IntPtr Handle => _window;

    public Size Size
    {
        get
        {
            SDL.GetWindowSizeInPixels(_window, out int w, out int h);
            return new Size(w, h);
        }
        set => SDL.SetWindowSize(_window, value.Width, value.Height);
    }

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
                flags |= SDL.WindowFlags.Vulkan;
                break;
            case Backend.D3D11:
                break;
            case Backend.OpenGL:
                flags |= SDL.WindowFlags.OpenGL;
                SDL.GLSetAttribute(SDL.GLAttr.ContextMajorVersion, 3);
                SDL.GLSetAttribute(SDL.GLAttr.ContextMinorVersion, 3);
                SDL.GLSetAttribute(SDL.GLAttr.ContextProfileMask, (int) SDL.GLProfile.Core);
                if (OperatingSystem.IsMacOS())
                    SDL.GLSetAttribute(SDL.GLAttr.ContextFlags, (int) SDL.GLContextFlag.ForwardCompatible);

                // Disable D/S buffer as it's not needed. Anything needing to write to D/S buffers will use a framebuffer.
                SDL.GLSetAttribute(SDL.GLAttr.DepthSize, 0);
                SDL.GLSetAttribute(SDL.GLAttr.StencilSize, 0);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(backend), backend, null);
        }
        
        _window = SDL.CreateWindow(title, size.Width, size.Height, flags);
        if (_window == 0)
            throw new Exception($"Failed to create SDL window: {SDL.GetError()}");
    }

    public void Dispose()
    {
        SDL.DestroyWindow(_window);
        SDL.Quit();
    }
}