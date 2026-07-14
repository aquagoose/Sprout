using System.Diagnostics;
using System.Drawing;
using Sprout.Audio;
using Sprout.Graphics;

namespace Sprout;

public sealed class App
{
    public readonly Window Window;

    public readonly GraphicsDevice Graphics;

    public readonly AudioDevice Audio;

    public readonly EventManager Events;

    public readonly InputManager Input;

    public bool IsRunning;

    private App(Window window, GraphicsDevice graphics, AudioDevice audio, EventManager events, InputManager input)
    {
        Window = window;
        Graphics = graphics;
        Audio = audio;
        Events = events;
        Input = input;
    }

    private void AssignEvents()
    {
        Window.Resized += WindowOnResized;
        Events.Quit += Close;
    }

    public static void Run(IApp app, in AppInfo info)
    {
        Backend backend = info.Backend;
        if (backend == Backend.Unknown)
        {
            if (OperatingSystem.IsWindows())
                backend = Backend.D3D11;
            else
                backend = Backend.OpenGL;
        }

        Window window = new Window(in info.Window, backend);
        
        GraphicsDevice graphics = GraphicsDevice.Create(window.Handle, backend);
        AudioDevice audio = new AudioDevice();

        EventManager events = new EventManager(window);
        InputManager input = new InputManager(events);
        
        App mainApp = new App(window, graphics, audio, events, input);
        mainApp.AssignEvents();
        
        app.Initialize(mainApp);
        
        Stopwatch sw = Stopwatch.StartNew();
        mainApp.IsRunning = true;
        while (mainApp.IsRunning)
        {
            mainApp.Input.Update();
            mainApp.Events.PollEvents();

            double dt = sw.Elapsed.TotalSeconds;
            sw.Restart();

            app.Update(mainApp, (float) dt);
            app.Draw(mainApp);
            
            mainApp.Graphics.Present();
        }
    }

    public void Close()
    {
        IsRunning = false;
    }
    
    private void WindowOnResized(uint width, uint height)
    {
        Graphics.ResizeSwapchain(width, height);
    }
}