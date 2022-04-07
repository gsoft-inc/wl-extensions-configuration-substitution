using System;
using System.Collections.Generic;

namespace GSoft.Extensions.Configuration.Substitution;

public sealed class RecursiveConfigurationVariableException : Exception
{
    public RecursiveConfigurationVariableException(IEnumerable<string> variableNames)
        : base("Recursive loop detected while substituting configuration variables: '" + string.Join(" > ", variableNames) + "'.")
    {
    }
}