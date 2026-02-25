using System.Numerics;
using SDL3;

namespace Sprout;

public class EventManager : IDisposable
{
    public event OnQuit Quit;

    public event OnKeyDown KeyDown;

    public event OnKeyUp KeyUp;

    public event OnMouseButtonDown MouseButtonDown;

    public event OnMouseButtonUp MouseButtonUp;

    public event OnMouseMove MouseMove;

    private readonly Window _window;
    
    public EventManager(Window window)
    {
        _window = window;
        
        if (!SDL.Init(SDL.InitFlags.Events))
            throw new Exception($"Failed to initialize SDL: {SDL.GetError()}");

        Quit = delegate { };
        KeyDown = delegate { };
        KeyUp = delegate { };
        MouseButtonDown = delegate { };
        MouseButtonUp = delegate { };
        MouseMove = delegate { };
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
                    if (winEvent.Key.Repeat)
                        break;
                    
                    KeyDown(SdlUtils.KeycodeToKey(winEvent.Key.Key));
                    break;
                case SDL.EventType.KeyUp:
                    KeyUp(SdlUtils.KeycodeToKey(winEvent.Key.Key));
                    break;
                
                case SDL.EventType.MouseButtonDown:
                    MouseButtonDown(SdlUtils.ButtonIndexToButton(winEvent.Button.Button));
                    break;
                case SDL.EventType.MouseButtonUp:
                    MouseButtonUp(SdlUtils.ButtonIndexToButton(winEvent.Button.Button));
                    break;
                case SDL.EventType.MouseMotion:
                    // TODO: Scale for the window scale factor
                    MouseMove(new Vector2(winEvent.Motion.X, winEvent.Motion.Y) * _window.ContentScale,
                        new Vector2(winEvent.Motion.XRel, winEvent.Motion.YRel) * _window.ContentScale);
                    break;
            }
        }
    }

    public void Dispose()
    {
        MouseMove = delegate { };
        MouseButtonUp = delegate { };
        MouseButtonDown = delegate { };
        KeyUp = delegate { };
        KeyDown = delegate { };
        Quit = delegate { };
    }

    public delegate void OnQuit();

    public delegate void OnKeyDown(Key key);

    public delegate void OnKeyUp(Key key);

    public delegate void OnMouseButtonDown(MouseButton button);

    public delegate void OnMouseButtonUp(MouseButton button);

    public delegate void OnMouseMove(Vector2 mousePos, Vector2 mouseDelta);
}