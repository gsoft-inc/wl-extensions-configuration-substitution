using System;
using System.Collections.Generic;

namespace GSoft.Extensions.Configuration.Substitution;

public sealed class RecursiveConfigurationKeyException : Exception
{
    public RecursiveConfigurationKeyException(IEnumerable<string> keyNames)
        : base("Recursive loop detected while substituting configuration keys: '" + string.Join(" > ", keyNames) + "'.")
    {
    }
}