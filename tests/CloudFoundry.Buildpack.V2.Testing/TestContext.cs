using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace CloudFoundry.Buildpack.V2.Testing;

internal class TestContext
{
    public static (string ClassName, string TestName, bool isTheory) GetTestIdentifier()
    {
        var stackTrace = new StackTrace();
        var frames = stackTrace.GetFrames();
        foreach (var frame in frames)
        {
            var method = frame.GetMethod()!;
            var isFact = method.GetCustomAttributes(false).Cast<Attribute>().Any(x => x is FactAttribute);
            var isTheory = method.GetCustomAttributes(false).Cast<Attribute>().Any(x => x is TheoryAttribute);
            if (isFact || isTheory)
                return (method.DeclaringType!.Name, method.Name, isTheory);
        }

        throw new InvalidOperationException("None of the methods in call stack have [Fact] or [Theory] attribute");
    }

    static AsyncLocal<ITestOutputHelper?> _outputHelper = new();
    public static ITestOutputHelper? TestOutputHelper
    {
        get => _outputHelper.Value;
        set
        {
            _outputHelper.Value = value;
            if (value != null)
            {
                _testOutputStreamContext.Value = new TestOutputStream(value);
            }
        }
    }

    static AsyncLocal<Stream?> _testOutputStreamContext = new();
    public static Stream? TestOutputStream
    {
        get => _testOutputStreamContext.Value;
    }

    // public static Lazy<string> _testRunId => new(() => DateTime.Now.ToString("o").Replace(":","."), LazyThreadSafetyMode.ExecutionAndPublication);
    // public static string TestRunId => _testRunId.Value;
    public static string TestRunId { get; } =  $"testrun-{DateTime.Now.ToString("o").Replace(":",".")}";
    public static AbsolutePath TestRunDirectory => (AbsolutePath)Directory.GetCurrentDirectory() / TestRunId;
    public static AbsolutePath GetTestCaseDirectory()
    {
        
        var (testClass, testMethod, isTheory) = GetTestIdentifier();
        var identifier = $"{testClass}.{testMethod}";
        lock (_lock)
        {
            if (!_theoryIdSet.Value)
            {
                _theoryIdSet.Value = true;
                _currentThearyCount.Value = _theoryCounter.AddOrUpdate(identifier, _ => 1, (_, i) => i + 1);
            }
        }

        var testDirectory = isTheory ? $"{identifier}-{_currentThearyCount.Value:000}" : identifier; 
        var testCaseDirectory = TestRunDirectory / testDirectory;
        // testCaseDirectory.CreateDirectory();
        return testCaseDirectory;
    }

    static ConcurrentDictionary<string, int> _theoryCounter = new(); 
    static AsyncLocal<bool> _theoryIdSet = new();
    static AsyncLocal<int> _currentThearyCount = new();
    static AsyncLocal<ConcurrentDictionary<string, ILogger>?> _logger = new();

    public static ILogger GetLogger(string name)
    {
        lock (_lock)
        {
            _logger.Value ??= new();
        }

        return _logger.Value.GetOrAdd(name, loggerName => TestOutputHelper?.BuildLogger(loggerName) ?? throw new InvalidOperationException($"{nameof(TestOutputHelper)} is not set"));
    }

    static object _lock = new();

}