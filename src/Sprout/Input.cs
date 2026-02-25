namespace Sprout;

public static class Input
{
    private static readonly HashSet<Key> _keysDown;
    private static readonly HashSet<Key> _newKeysDown;

    static Input()
    {
        _keysDown = [];
        _newKeysDown = [];
        
        Events.KeyDown += EventsOnKeyDown;
        Events.KeyUp += EventsOnKeyUp;
    }

    public static bool IsKeyDown(Key key) => _keysDown.Contains(key);

    public static bool IsKeyPressed(Key key) => _newKeysDown.Contains(key);

    public static void Update()
    {
        _newKeysDown.Clear();
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
}