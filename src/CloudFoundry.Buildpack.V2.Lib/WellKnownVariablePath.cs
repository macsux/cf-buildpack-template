using System.ComponentModel;
using System.Globalization;
using JetBrains.Annotations;
using static NMica.Utils.IO.PathConstruction;
using NMica.Utils;
using NMica.Utils.IO;

namespace CloudFoundry.Buildpack.V2;

[PublicAPI]
public class WellKnownVariablePath : VariablePath
{
    readonly AbsolutePath? _stagingRoot;
    readonly AbsolutePath _runtimeRoot;

    internal WellKnownVariablePath(string path, AbsolutePath runtimeRoot, char? separator = null) : this(path, (AbsolutePath)path, runtimeRoot, separator)
    {
        
    }

    internal WellKnownVariablePath(string path, WellKnownVariablePath root, char? separator = null) : this(path, root.CurrentAbsolutePath, root.RuntimeAbsolutePath, separator)
    {
        
    }
    internal WellKnownVariablePath(string path, AbsolutePath? stagingRoot, AbsolutePath runtimeRoot, char? separator = null) : base(path, separator)
    {
        _stagingRoot = stagingRoot;
        _runtimeRoot = runtimeRoot;
    }

    public AbsolutePath RuntimeAbsolutePath => _stagingRoot == null ? _runtimeRoot : _runtimeRoot / CurrentAbsolutePath.GetRelativePathTo(_stagingRoot);
    public static AbsolutePath HomeDirectory { get; } = (AbsolutePath)(EnvironmentInfo.IsWin ? $@"C:\Users\{Environment.UserName}" : $"/home/{Environment.UserName}");
    public static WellKnownVariablePath operator /(WellKnownVariablePath left, string right)
    {
        var separator = left.NotNull("left != null")._separator;
        return new WellKnownVariablePath(NormalizePath(Combine(left.ToString(), (RelativePath) right, separator), separator), left._stagingRoot, left._runtimeRoot, separator);
    }
    
    public static explicit operator string(WellKnownVariablePath path)
    {
        return path._path;
    }
}