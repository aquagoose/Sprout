namespace Sprout.EntitySystem;

public class SceneManager : IDisposable
{
    private readonly App _app;
    
    private Scene _currentScene;
    private Scene? _sceneToSwitch;

    public SceneManager(App app, Scene initialScene)
    {
        _app = app;
        
        _currentScene = initialScene;
        _currentScene.AppInternal = app;
        _currentScene.Initialize();
    }

    public void SetScene(Scene scene)
    {
        _sceneToSwitch = scene;
    }

    public void Update(float dt)
    {
        if (_sceneToSwitch != null)
        {
            _currentScene.Dispose();
            _currentScene = _sceneToSwitch;
            _sceneToSwitch = null;
            GC.Collect();
            _currentScene.AppInternal = _app;
            _currentScene.Initialize();
        }
        
        _currentScene.Update(dt);
    }

    public void Draw()
    {
        _currentScene.Draw();
    }
    
    public void Dispose()
    {
        _currentScene.Dispose();
    }
}