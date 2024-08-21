namespace CloudFoundry.Buildpack.V2;

[PublicAPI] public abstract record ValueAction(string Key, string Value);
[PublicAPI] public record AppendValueAction(string Key, string Delimiter, string Value) : ValueAction(Key, Value);
[PublicAPI] public record SetValueAction(string Key, string Value) : ValueAction(Key, Value);