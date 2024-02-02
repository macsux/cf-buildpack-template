using System.ComponentModel;
using System.Globalization;
using JetBrains.Annotations;
using static NMica.Utils.IO.PathConstruction;
using NMica.Utils;
using NMica.Utils.IO;

namespace CloudFoundry.Buildpack.V2;

public class VariablePath
{
    private readonly string _path;
    private readonly char? _separator;

    public class TypeConverter : System.ComponentModel.TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object? value)
        {
            if (value is string stringValue)
            {
                return HasPathRoot(stringValue)
                    ? (AbsolutePath) stringValue
                    : EnvironmentInfo.WorkingDirectory / stringValue;
            }

            if (value is null)
                return null;

            return base.ConvertFrom(context, culture, value);
        }
    }

    public static VariablePath FromEnvironmentalVariable(string name)
    {
        if (EnvironmentInfo.IsWin)
            return ((VariablePath)$"%{name}%")!;
        return ((VariablePath)$"${name}")!;
        
    }
    public AbsolutePath CurrentAbsolutePath => (AbsolutePath)Environment.ExpandEnvironmentVariables(_path);
    protected VariablePath(string path, char? separator = null)
    {
        _path = path;
        _separator = separator;
    }

    public static explicit operator VariablePath?(string? path)
    {
        if (path is null)
            return null;

        return new VariablePath(NormalizePath(path));
    }

    public static implicit operator string?(VariablePath path)
    {
        return path?._path;
    }

    public static VariablePath operator /(VariablePath left, [CanBeNull] string right)
    {
        var separator = left.NotNull("left != null")._separator;
        return new VariablePath(NormalizePath(Combine(left, (RelativePath) right, separator), separator), separator);
    }

    public override string ToString()
    {
        return _path;
    }
}

