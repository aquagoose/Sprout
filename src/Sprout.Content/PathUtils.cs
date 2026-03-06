namespace Sprout.Content;

public class PathUtils
{
    public static string GetFullPath(string path)
    {
        if (Path.IsPathRooted(path))
            return path;
        
        return Path.Combine(AppContext.BaseDirectory, path);
    }
}
