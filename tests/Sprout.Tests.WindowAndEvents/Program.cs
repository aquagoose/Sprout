using System.Drawing;
using Sprout;
using Sprout.Graphics;

bool alive = true;

Window window = new Window("Window and Events Test", new Size(1280, 720), Backend.OpenGL);
EventManager eventManager = new EventManager(window);

while (alive)
{
    eventManager.PollEvents();
}

window.Dispose();