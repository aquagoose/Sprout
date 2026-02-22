namespace Sprout.EntitySystem;

public class Scene : IDisposable
{
    public virtual void Initialize() { }

    public virtual void Update(float dt) { }

    public virtual void Draw() { }

    public virtual void Unload() { }

    public void Dispose()
    {
        Unload();
    }
}
