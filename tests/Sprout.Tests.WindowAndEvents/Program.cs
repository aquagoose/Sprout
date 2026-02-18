using System.Drawing;
using Sprout;
using Sprout.Graphics;

bool alive = true;
Window window = new Window("Window and Events Test", new Size(1280, 720), Backend.OpenGL);

Events events = new Events();
events.Quit += () => alive = false;

while (alive)
{
    events.PollEvents();
}

events.Dispose();
window.Dispose();