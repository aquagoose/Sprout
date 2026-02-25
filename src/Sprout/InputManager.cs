using System.Numerics;

namespace Sprout;

public class InputManager : IDisposable
{
    private readonly EventManager _eventManager;
    
    private readonly HashSet<Key> _keysDown;
    private readonly HashSet<Key> _newKeysDown;
    
    private readonly HashSet<MouseButton> _buttonsDown;
    private readonly HashSet<MouseButton> _newButtonsDown;

    private Vector2 _mousePosition;
    private Vector2 _mouseDelta;

    public Vector2 MousePosition => _mousePosition;

    public Vector2 MouseDelta => _mouseDelta;
    
    public InputManager(EventManager eventManager)
    {
        _eventManager = eventManager;
        
        _keysDown = [];
        _newKeysDown = [];
        _buttonsDown = [];
        _newButtonsDown = [];
        
        _eventManager.KeyDown += EventManagerOnKeyDown;
        _eventManager.KeyUp += EventManagerOnKeyUp;
        _eventManager.MouseButtonDown += EventManagerOnMouseButtonDown;
        _eventManager.MouseButtonUp += EventManagerOnMouseButtonUp;
        _eventManager.MouseMove += EventManagerOnMouseMove;
    }

    public void Dispose()
    {
        _eventManager.MouseMove += EventManagerOnMouseMove;
        _eventManager.MouseButtonUp += EventManagerOnMouseButtonUp;
        _eventManager.MouseButtonDown += EventManagerOnMouseButtonDown;
        _eventManager.KeyUp += EventManagerOnKeyUp;
        _eventManager.KeyDown += EventManagerOnKeyDown;
    }

    public bool IsKeyDown(Key key) => _keysDown.Contains(key);

    public bool IsKeyPressed(Key key) => _newKeysDown.Contains(key);

    public bool IsMouseButtonDown(MouseButton button) => _buttonsDown.Contains(button);

    public bool IsMouseButtonPressed(MouseButton button) => _newButtonsDown.Contains(button);

    public void Update()
    {
        _newKeysDown.Clear();
        _newButtonsDown.Clear();

        _mouseDelta = Vector2.Zero;
    }

    private void EventManagerOnKeyDown(Key key)
    {
        _keysDown.Add(key);
        _newKeysDown.Add(key);
    }

    private void EventManagerOnKeyUp(Key key)
    {
        _keysDown.Remove(key);
        _newKeysDown.Remove(key);
    }
    
    private void EventManagerOnMouseButtonDown(MouseButton button)
    {
        _buttonsDown.Add(button);
        _newButtonsDown.Add(button);
    }
    
    private void EventManagerOnMouseButtonUp(MouseButton button)
    {
        _buttonsDown.Remove(button);
        _newButtonsDown.Remove(button);
    }
    
    private void EventManagerOnMouseMove(Vector2 mousePos, Vector2 mouseDelta)
    {
        _mousePosition = mousePos;
        _mouseDelta += mouseDelta;
    }
}