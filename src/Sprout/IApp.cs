namespace Sprout;

public interface IApp
{
    public void Initialize(App app) { }

    public void Update(App app, float dt) { }
    
    public void Draw(App app) { }
    
    public void Unload(App app) { }
}