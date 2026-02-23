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
        _currentScene.App = app;
        _currentScene.Initialize();
    }

    public void Update(float dt)
    {
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