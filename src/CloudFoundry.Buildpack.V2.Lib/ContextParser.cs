namespace CloudFoundry.Buildpack.V2;

internal class ContextHelper
{
    public VariablePath? BuildDirectory { get; internal set; }
    public VariablePath? CacheDirectory { get; internal set; }
    public VariablePath? DependenciesDirectory { get; internal set; }
    public int? BuildpackIndex { get; internal set; }
    
    public ContextHelper(string[] args)
    {
        var queue = new Queue<string>(args);
        if (queue.TryDequeue(out var buildPath))
        {
            Environment.SetEnvironmentVariable("HOME", buildPath);
            BuildDirectory = (VariablePath)buildPath!;
        }

        if (queue.TryDequeue(out var cachePath))
        {
            CacheDirectory = (VariablePath)cachePath!;
        }

        if (queue.TryDequeue(out var depsPath))
        {
            Environment.SetEnvironmentVariable("DEPS_DIR", depsPath);
            DependenciesDirectory = (VariablePath)depsPath!;
        }

        if (queue.TryDequeue(out var indexStr) && int.TryParse(indexStr, out var indexInt))
        {
            BuildpackIndex = indexInt;
        }
    }
   
}

internal static class Extensions
{
    public static bool TryDequeue<T>(this Queue<T> queue, out T? value)
    {
        if (queue.Count == 0)
        {
            value = default;
            return false;
        }

        value = queue.Dequeue();
        return true;

    }
}