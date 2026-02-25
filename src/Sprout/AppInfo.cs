using System.Drawing;
using Sprout.Graphics;

namespace Sprout;

public struct AppInfo(string appName, string appVersion)
{
    public string AppName = appName;

    public string AppVersion = appVersion;

    public Backend Backend = Backend.Unknown;

    public WindowInfo Window = new WindowInfo(appName, new Size(1280, 720));
}