using Sprout.Graphics;

namespace Sprout.EntitySystem;

public class Scene : IDisposable
{
    protected internal App App;

    protected Window Window => App.Window;

    protected GraphicsDevice Device => App.Device;
    
    public virtual void Initialize() { }

    public virtual void Update(float dt) { }

    public virtual void Draw() { }

    public virtual void Unload() { }

    public void Dispose()
    {
        Unload();
    }
}
