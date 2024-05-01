namespace CloudFoundry.Buildpack.V2.Testing;

public static class Extensions
{
    public static AbsolutePath AsWindowsPath(this AbsolutePath path)
    {
        var pathStr = path.ToString();
        if (pathStr.StartsWith(@"c:\"))
            return path;
        pathStr = $"c:{pathStr}";
        return (AbsolutePath)pathStr;
    }
    public static AbsolutePath AsLinuxPath(this AbsolutePath path)
    {
        var pathStr = path.ToString();
        if (pathStr.StartsWith("/"))
            return path;
        pathStr = pathStr.Remove(0, 2).Replace(@"\","/");
        return (AbsolutePath)pathStr;
    }
    
    
}