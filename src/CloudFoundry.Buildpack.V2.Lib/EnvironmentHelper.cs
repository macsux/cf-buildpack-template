﻿using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using NMica.Utils;
using NMica.Utils.IO;

namespace CloudFoundry.Buildpack.V2;

/// <summary>
/// Provides static information about the runtime environment
/// </summary>
public static class EnvironmentHelper
{
    /// <summary>
    /// Provides TRUE name of the entrypoint executable as it was invoked from shell. This is different from what is reported by normal .NET methods ensuring
    /// it correctly returns the name of symlinks and executable name if app is self-contained. 
    /// </summary>
    public static AbsolutePath EntrypointExecutable
    {
        get
        {
            var args = EnvironmentHelper.GetCommandLineArgsNative();
            //Console.WriteLine(string.Join(" ", args));
            // var args = Environment.GetCommandLineArgs().ToList().ToArray();
            if (EnvironmentInfo.IsWin && args[0].StartsWith(@"\"))
            {
                args[0] = $"c:{args[0]}";
            }
            if(!Path.IsPathRooted(args[0]))
            {
                args[0] = (AbsolutePath)Directory.GetCurrentDirectory() / args[0];
            }
            var entrypointExecutable = (AbsolutePath)args[0];
            return entrypointExecutable;
        }
    }
    internal const string BuildpackRootEnvironmentalVariableName = "BUILDPACK_ROOT";
    internal static VariablePath BuildpackDirectory => (VariablePath)$"%{BuildpackRootEnvironmentalVariableName}%";

    [DllImport("kernel32.dll", EntryPoint = "GetCommandLineW")]
    internal static extern unsafe char* GetCommandLine();

    internal static unsafe string[] GetCommandLineArgsNative()
    {
        if (EnvironmentInfo.IsWin)
        {
            char* lpCmdLine = GetCommandLine();
            Debug.Assert(lpCmdLine != null);

            return SegmentCommandLine(lpCmdLine);
        }
        else
        {
            var input = File.ReadAllText("/proc/self/cmdline").TrimEnd('\0');
            return input.Split('\0').ToArray();
        }
    }

    private static unsafe string[] SegmentCommandLine(char* cmdLine)
    {
        // Parse command line arguments using the rules documented at
        // https://learn.microsoft.com/cpp/cpp/main-function-command-line-args#parsing-c-command-line-arguments

        // CommandLineToArgvW API cannot be used here since
        // it has slightly different behavior.

        List<string> arrayBuilder = new();

        Span<char> stringBuffer = stackalloc char[260]; // Use MAX_PATH for a typical maximum
        StringBuilder stringBuilder = new();

        char c;

        // First scan the program name, copy it, and count the bytes

        char* p = cmdLine;

        // A quoted program name is handled here. The handling is much
        // simpler than for other arguments. Basically, whatever lies
        // between the leading double-quote and next one, or a terminal null
        // character is simply accepted. Fancier handling is not required
        // because the program name must be a legal NTFS/HPFS file name.
        // Note that the double-quote characters are not copied, nor do they
        // contribyte to character_count.

        bool inQuotes = false;

        do
        {
            if (*p == '"')
            {
                inQuotes = !inQuotes;
                c = *p++;
                continue;
            }

            c = *p++;
            stringBuilder.Append(c);
        } while (c != '\0' && (inQuotes || (c is not (' ' or '\t'))));

        if (c == '\0')
        {
            p--;
        }

        stringBuilder.Length--;
        arrayBuilder.Add(stringBuilder.ToString());
        inQuotes = false;

        // loop on each argument
        while (true)
        {
            if (*p != '\0')
            {
                while (*p is ' ' or '\t')
                {
                    ++p;
                }
            }

            if (*p == '\0')
            {
                // end of args
                break;
            }

            // scan an argument
            stringBuilder = new();

            // loop through scanning one argument
            while (true)
            {
                bool copyChar = true;

                // Rules:
                // 2N   backslashes + " ==> N backslashes and begin/end quote
                // 2N+1 backslashes + " ==> N backslashes + literal "
                // N    backslashes     ==> N backslashes
                int numSlash = 0;

                while (*p == '\\')
                {
                    // Count number of backslashes for use below
                    ++p;
                    ++numSlash;
                }

                if (*p == '"')
                {
                    // if 2N backslashes before, start / end quote, otherwise
                    // copy literally:
                    if (numSlash % 2 == 0)
                    {
                        if (inQuotes && p[1] == '"')
                        {
                            p++; // Double quote inside quoted string
                        }
                        else
                        {
                            // Skip first quote char and copy second:
                            copyChar = false; // Don't copy quote
                            inQuotes = !inQuotes;
                        }
                    }

                    numSlash /= 2;
                }

                // Copy slashes:
                while (numSlash-- > 0)
                {
                    stringBuilder.Append('\\');
                }

                // If at end of arg, break loop:
                if (*p == '\0' || (!inQuotes && *p is ' ' or '\t'))
                {
                    break;
                }

                // Copy character into argument:
                if (copyChar)
                {
                    stringBuilder.Append(*p);
                }

                ++p;
            }

            arrayBuilder.Add(stringBuilder.ToString());
        }

        return arrayBuilder.ToArray();
    }
}