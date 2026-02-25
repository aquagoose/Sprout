using System.Drawing;
using Sprout;
using Sprout.Graphics;

bool alive = true;
Events.Quit += () => alive = false;

Window window = new Window("Window and Events Test", new Size(1280, 720), Backend.OpenGL);

while (alive)
{
    Events.PollEvents();
}

window.Dispose();