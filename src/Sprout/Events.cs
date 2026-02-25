using System.Numerics;
using SDL3;

namespace Sprout;

public static class Events
{
    public static event OnQuit Quit;

    public static event OnKeyDown KeyDown;

    public static event OnKeyUp KeyUp;

    public static event OnMouseButtonDown MouseButtonDown;

    public static event OnMouseButtonUp MouseButtonUp;

    public static event OnMouseMove MouseMove;
    
    static Events()
    {
        if (!SDL.Init(SDL.InitFlags.Events))
            throw new Exception($"Failed to initialize SDL: {SDL.GetError()}");

        Quit = delegate { };
        KeyDown = delegate { };
        KeyUp = delegate { };
        MouseButtonDown = delegate { };
        MouseButtonUp = delegate { };
        MouseMove = delegate { };
    }

    public static void PollEvents()
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
                    MouseMove(new Vector2(winEvent.Motion.X, winEvent.Motion.Y),
                        new Vector2(winEvent.Motion.XRel, winEvent.Motion.YRel));
                    break;
            }
        }
    }

    public delegate void OnQuit();

    public delegate void OnKeyDown(Key key);

    public delegate void OnKeyUp(Key key);

    public delegate void OnMouseButtonDown(MouseButton button);

    public delegate void OnMouseButtonUp(MouseButton button);

    public delegate void OnMouseMove(Vector2 mousePos, Vector2 mouseDelta);
}