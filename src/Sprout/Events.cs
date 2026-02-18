using SDL3;

namespace Sprout;

public sealed class Events : IDisposable
{
    public event OnQuit Quit;
    
    public Events()
    {
        if (!SDL.Init(SDL.InitFlags.Events))
            throw new Exception($"Failed to initialize SDL: {SDL.GetError()}");

        Quit = delegate { };
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
            }
        }
    }
    
    public void Dispose()
    {
        SDL.QuitSubSystem(SDL.InitFlags.Events);
    }

    public delegate void OnQuit();
}