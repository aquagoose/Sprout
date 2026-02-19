namespace Sprout;

public class Input : IDisposable
{
    private readonly Events _events;
    
    private readonly HashSet<Key> _keysDown;
    private readonly HashSet<Key> _newKeysDown;

    public Input(Events events)
    {
        _keysDown = [];
        _newKeysDown = [];
        
        _events = events;
        _events.KeyDown += EventsOnKeyDown;
        _events.KeyUp += EventsOnKeyUp;
    }

    public bool IsKeyDown(Key key) => _keysDown.Contains(key);

    public bool IsKeyPressed(Key key) => _newKeysDown.Contains(key);

    public void Update()
    {
        _newKeysDown.Clear();
    }

    private void EventsOnKeyDown(Key key)
    {
        _keysDown.Add(key);
        _newKeysDown.Add(key);
    }

    private void EventsOnKeyUp(Key key)
    {
        _keysDown.Remove(key);
        _newKeysDown.Remove(key);
    }
    
    public void Dispose()
    {
        _events.KeyUp -= EventsOnKeyUp;
        _events.KeyDown -= EventsOnKeyDown;
    }
}