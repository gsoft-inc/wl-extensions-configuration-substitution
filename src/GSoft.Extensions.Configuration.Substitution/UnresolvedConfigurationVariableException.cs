using System;

namespace GSoft.Extensions.Configuration.Substitution;

public sealed class UnresolvedConfigurationVariableException : Exception
{
    public UnresolvedConfigurationVariableException(string variableName, string requestingVariableName)
        : base($"Configuration variable '{variableName}' does not have a value but is referenced in '{requestingVariableName}'.")
    {
    }
}