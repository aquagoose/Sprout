using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using piko.SDL3;

namespace Sprout.Graphics.Tests.Base;

public abstract class TestBase(string testName) : IDisposable
{
    private SDL.Window _window;
    private bool _alive;

    protected GraphicsDevice Device;

    protected unsafe Size WindowSize
    {
        get
        {
            int w, h;
            SDL.GetWindowSizeInPixels(_window, &w, &h);
            return new Size(w, h);
        }
    }

    protected virtual void Load() { }

    protected abstract void Loop(float dt);

    public unsafe void Run()
    {
        if (!SDL.Init(SDL.InitFlags.Video | SDL.InitFlags.Events))
            throw new Exception($"Failed to initialize SDL: {SDL.GetError()}");

        Backend backend;
        
        string? forceBackend = Environment.GetEnvironmentVariable("SPROUT_TEST_BACKEND");
        if (forceBackend != null)
            backend = Enum.Parse<Backend>(forceBackend);
        else
        {

            Backend[] availableBackends = Enum.GetValues<Backend>();
            int numBackends = availableBackends.Length - 1;
            SDL.MessageBoxButtonData* buttons = stackalloc SDL.MessageBoxButtonData[numBackends];

            for (int i = 1; i < availableBackends.Length; i++)
            {
                buttons[i - 1] = new SDL.MessageBoxButtonData
                {
                    ButtonID = (int) availableBackends[i],
                    Text = (sbyte*) Marshal.StringToHGlobalAnsi(availableBackends[i].ToString())
                };
            }

            SDL.MessageBoxData boxData = new()
            {
                Flags = SDL.MessageBoxFlags.Information,
                Numbuttons = numBackends,
                Buttons = buttons,
                Title = (sbyte*) Marshal.StringToHGlobalAnsi("Select Backend"),
                Message = (sbyte*) Marshal.StringToHGlobalAnsi($"Select backend to run {testName}."),
            };

            int buttonId;
            SDL.ShowMessageBox(&boxData, &buttonId);
            backend = (Backend) buttonId;
        }

        SDL.WindowFlags flags = SDL.WindowFlags.Resizable | SDL.WindowFlags.HighPixelDensity;

        switch (backend)
        {
            /*case Backend.Vulkan:
            {
                flags |= SDL.WindowFlags.Vulkan;
                break;
            }*/
            
            case Backend.OpenGL:
            {
                flags |= SDL.WindowFlags.Opengl;
                SDL.GLSetAttribute(SDL.GLAttr.ContextMajorVersion, 3);
                SDL.GLSetAttribute(SDL.GLAttr.ContextMinorVersion, 3);
                SDL.GLSetAttribute(SDL.GLAttr.ContextProfileMask, (int) SDL.GLProfile.Core);
                if (OperatingSystem.IsMacOS())
                    SDL.GLSetAttribute(SDL.GLAttr.ContextFlags, (int) SDL.GLContextFlag.ForwardCompatibleFlag);

                break;
            }
        }

        string appName = $"{testName} ({backend})";
        SDL.SetAppMetadata(appName, "1.0.0", "");
        _window = SDL.CreateWindow(appName, 800, 600, flags);
        if (_window.IsNull)
            throw new Exception($"Failed to create SDL window: {SDL.GetError()}");

        Device = GraphicsDevice.Create(_window, backend);
        
        Load();

        Stopwatch sw = Stopwatch.StartNew();
        _alive = true;
        while (_alive)
        {
            SDL.Event winEvent;
            while (SDL.PollEvent(&winEvent))
            {
                switch ((SDL.EventType) winEvent.Type)
                {
                    case SDL.EventType.WindowCloseRequested:
                    case SDL.EventType.Quit:
                        _alive = false;
                        break;
                    
                    case SDL.EventType.WindowResized:
                        Size size = WindowSize;
                        Device.ResizeSwapchain((uint) size.Width, (uint) size.Height);
                        break;
                }
            }

            float dt = (float) sw.Elapsed.TotalSeconds;
            sw.Restart();
            Loop(dt);
            Device.Present();
        }
    }

    public virtual void Dispose()
    {
        Device.Dispose();
        SDL.DestroyWindow(_window);
        SDL.Quit();
    }
}