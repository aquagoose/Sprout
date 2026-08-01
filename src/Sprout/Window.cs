using System.Drawing;
using piko.SDL3;
using Sprout.Graphics;

namespace Sprout;

public sealed class Window : IDisposable
{
    public event OnResized Resized;
    
    private readonly SDL.Window _window;

    public SDL.Window Handle => _window;

    public string Title
    {
        get => SDL.GetWindowTitle(_window);
        set => SDL.SetWindowTitle(_window, value);
    }

    public Size Size
    {
        get
        {
            int w, h;
            unsafe { SDL.GetWindowSizeInPixels(_window, &w, &h); }
            return new Size(w, h);
        }
        set => SDL.SetWindowSize(_window, value.Width, value.Height);
    }

    public bool Fullscreen
    {
        get => (SDL.GetWindowFlags(_window) & SDL.WindowFlags.Fullscreen) != 0;
        set => SDL.SetWindowFullscreen(_window, (byte) (value ? 1 : 0));
    }

    public float PixelDensity => SDL.GetWindowPixelDensity(_window);

    public Window(in WindowInfo info, Backend backend)
    {
        if (!SDL.Init(SDL.InitFlags.Video))
            throw new Exception($"Failed to initialize SDL: {SDL.GetError()}");

        SDL.WindowFlags flags = SDL.WindowFlags.HighPixelDensity;
        if (info.Resizable)
            flags |= SDL.WindowFlags.Resizable;
        if (info.Fullscreen)
            flags |= SDL.WindowFlags.Fullscreen;
        
        switch (backend)
        {
            case Backend.Unknown:
                break;
            /*case Backend.Vulkan:
                flags |= SDL.WindowFlags.Vulkan;
                break;*/
            case Backend.D3D11:
                break;
            case Backend.OpenGL:
                flags |= SDL.WindowFlags.Opengl;
                SDL.GLSetAttribute(SDL.GLAttr.ContextMajorVersion, 3);
                SDL.GLSetAttribute(SDL.GLAttr.ContextMinorVersion, 3);
                SDL.GLSetAttribute(SDL.GLAttr.ContextProfileMask, (int) SDL.GLProfile.Core);
                if (OperatingSystem.IsMacOS())
                    SDL.GLSetAttribute(SDL.GLAttr.ContextFlags, (int) SDL.GLContextFlag.ForwardCompatibleFlag);

                // Disable D/S buffer as it's not needed. Anything needing to write to D/S buffers will use a framebuffer.
                SDL.GLSetAttribute(SDL.GLAttr.DepthSize, 0);
                SDL.GLSetAttribute(SDL.GLAttr.StencilSize, 0);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(backend), backend, null);
        }
        
        string title = $"{info.Title}";
        _window = SDL.CreateWindow(title, info.Size.Width, info.Size.Height, flags);
        if (_window.IsNull)
            throw new Exception($"Failed to create SDL window: {SDL.GetError()}");

        Resized = delegate { };
    }

    public void Dispose()
    {
        Resized = delegate { };
        SDL.DestroyWindow(_window);
        SDL.Quit();
    }

    internal void InvokeResized()
    {
        Size size = Size;
        Resized((uint) size.Width, (uint) size.Height);
    }

    public delegate void OnResized(uint width, uint height);
}