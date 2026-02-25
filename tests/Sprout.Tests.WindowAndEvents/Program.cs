using System.Drawing;
using Sprout;
using Sprout.Graphics;

bool alive = true;

Window window = new Window(new WindowInfo(), Backend.OpenGL);
EventManager eventManager = new EventManager(window);

while (alive)
{
    eventManager.PollEvents();
}

window.Dispose();