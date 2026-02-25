using System.Numerics;

namespace Sprout;

public static class Input
{
    private static readonly HashSet<Key> _keysDown;
    private static readonly HashSet<Key> _newKeysDown;
    
    private static readonly HashSet<MouseButton> _buttonsDown;
    private static readonly HashSet<MouseButton> _newButtonsDown;

    private static Vector2 _mousePosition;
    private static Vector2 _mouseDelta;

    public static Vector2 MousePosition => _mousePosition;

    public static Vector2 MouseDelta => _mouseDelta;
    
    static Input()
    {
        _keysDown = [];
        _newKeysDown = [];
        _buttonsDown = [];
        _newButtonsDown = [];
        
        Events.KeyDown += EventsOnKeyDown;
        Events.KeyUp += EventsOnKeyUp;
        Events.MouseButtonDown += EventsOnMouseButtonDown;
        Events.MouseButtonUp += EventsOnMouseButtonUp;
        Events.MouseMove += EventsOnMouseMove;
    }

    public static bool IsKeyDown(Key key) => _keysDown.Contains(key);

    public static bool IsKeyPressed(Key key) => _newKeysDown.Contains(key);

    public static bool IsMouseButtonDown(MouseButton button) => _buttonsDown.Contains(button);

    public static bool IsMouseButtonPressed(MouseButton button) => _newButtonsDown.Contains(button);

    public static void Update()
    {
        _newKeysDown.Clear();
        _newButtonsDown.Clear();

        _mouseDelta = Vector2.Zero;
    }

    private static void EventsOnKeyDown(Key key)
    {
        _keysDown.Add(key);
        _newKeysDown.Add(key);
    }

    private static void EventsOnKeyUp(Key key)
    {
        _keysDown.Remove(key);
        _newKeysDown.Remove(key);
    }
    
    private static void EventsOnMouseButtonDown(MouseButton button)
    {
        _buttonsDown.Add(button);
        _newButtonsDown.Add(button);
    }
    
    private static void EventsOnMouseButtonUp(MouseButton button)
    {
        _buttonsDown.Remove(button);
        _newButtonsDown.Remove(button);
    }
    
    private static void EventsOnMouseMove(Vector2 mousePos, Vector2 mouseDelta)
    {
        _mousePosition = mousePos;
        _mouseDelta += mouseDelta;
    }
}