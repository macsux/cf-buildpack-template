using FluentAssertions;
using FluentAssertions.Execution;
using NMica.Utils.IO;

namespace FluentAssertions;

public class PathAssertions : PathAssertions<PathAssertions>
{
    public PathAssertions(AbsolutePath? subject) : base(subject)
    {
    }
}
public class PathAssertions<TAssertions>
    where TAssertions : PathAssertions<TAssertions>
{
    public AbsolutePath? Subject { get; }
    public PathAssertions(AbsolutePath? subject)
    {
        Subject = subject;
    }

    public AndConstraint<TAssertions> Exists(string because = "", params object[] becauseArgs)
    {
        
        Execute.Assertion
            .ForCondition(File.Exists(Subject) || Directory.Exists(Subject))
            .BecauseOf(because, becauseArgs)
            .FailWith("Expected {context:path} {0} to be an existing file or directory{reason}.", Subject);

        return new AndConstraint<TAssertions>((TAssertions)this);
    }
    
    public AndConstraint<TAssertions> BeExistingFile(string because = "", params object[] becauseArgs)
    {
        
        Execute.Assertion
            .ForCondition(File.Exists(Subject))
            .BecauseOf(because, becauseArgs)
            .FailWith("Expected {context:path} {0} to be an existing file{reason}.", Subject);

        return new AndConstraint<TAssertions>((TAssertions)this);
    }
    public AndConstraint<TAssertions> BeExistingDirectory(string because = "", params object[] becauseArgs)
    {
        
        Execute.Assertion
            .ForCondition(Directory.Exists(Subject))
            .BecauseOf(because, becauseArgs)
            .FailWith("Expected {context:path} {0} to be an existing directory{reason}.", Subject);

        return new AndConstraint<TAssertions>((TAssertions)this);
    }
}