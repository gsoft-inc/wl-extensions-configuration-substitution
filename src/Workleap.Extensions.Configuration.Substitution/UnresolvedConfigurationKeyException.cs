namespace Workleap.Extensions.Configuration.Substitution;

public sealed class UnresolvedConfigurationKeyException : Exception
{
    public UnresolvedConfigurationKeyException(string keyName, string requestingKeyName)
        : base($"Configuration key '{keyName}' does not have a value but is referenced in '{requestingKeyName}'.")
    {
    }
}