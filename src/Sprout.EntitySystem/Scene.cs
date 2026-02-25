using Sprout.Audio;
using Sprout.Graphics;

namespace Sprout.EntitySystem;

public class Scene : IDisposable
{
    internal App AppInternal;
    
    protected App App => AppInternal;

    protected Window Window => App.Window;

    protected GraphicsDevice Device => App.Device;

    protected AudioDevice Audio => App.Audio;

    protected EventManager Events => App.Events;

    protected InputManager Input => App.Input;
    
    public virtual void Initialize() { }

    public virtual void Update(float dt) { }

    public virtual void Draw() { }

    public virtual void Unload() { }

    public void Dispose()
    {
        Unload();
    }
}
