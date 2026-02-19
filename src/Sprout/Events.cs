using SDL3;

namespace Sprout;

public sealed class Events : IDisposable
{
    public event OnQuit Quit;

    public event OnKeyDown KeyDown;

    public event OnKeyUp KeyUp;
    
    public Events()
    {
        if (!SDL.Init(SDL.InitFlags.Events))
            throw new Exception($"Failed to initialize SDL: {SDL.GetError()}");

        Quit = delegate { };
        KeyDown = delegate { };
        KeyUp = delegate { };
    }

    public void PollEvents()
    {
        while (SDL.PollEvent(out SDL.Event winEvent))
        {
            switch ((SDL.EventType) winEvent.Type)
            {
                case SDL.EventType.WindowCloseRequested:
                case SDL.EventType.Quit:
                    Quit();
                    break;
                
                case SDL.EventType.KeyDown:
                    KeyDown(SdlUtils.KeycodeToKey(winEvent.Key.Key));
                    break;
                case SDL.EventType.KeyUp:
                    KeyUp(SdlUtils.KeycodeToKey(winEvent.Key.Key));
                    break;
            }
        }
    }
    
    public void Dispose()
    {
        SDL.QuitSubSystem(SDL.InitFlags.Events);
    }

    public delegate void OnQuit();

    public delegate void OnKeyDown(Key key);

    public delegate void OnKeyUp(Key key);
}