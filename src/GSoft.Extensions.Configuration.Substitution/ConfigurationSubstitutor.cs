using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace GSoft.Extensions.Configuration.Substitution;

internal sealed class ConfigurationSubstitutor
{
    // Matches two kind of strings:
    // Configuration keys that needs to be substituted, such as ${Foo}.
    // Escaped keys that must not be substituted, such as ${{Foo}}. In this case, it will be replaced by ${Foo}.
    private static readonly Regex SubstitutionsRegex = new Regex(@"(?<=\$\{)([^\{\}]+|\{[^\{\}]+\})(?=\})", RegexOptions.Compiled);

    public string? GetSubstituted(IConfiguration configuration, string key)
    {
        var value = configuration[key];
        return value == null ? value : this.ApplySubstitutionRecursive(configuration, key, value, alreadySubstitutedKeys: null);
    }

    private string ApplySubstitutionRecursive(IConfiguration configuration, string key, string value, List<string>? alreadySubstitutedKeys)
    {
        alreadySubstitutedKeys?.Add(key);

        var keysToSubstitute = SubstitutionsRegex.Matches(value).Cast<Match>().SelectMany(m => m.Captures.Cast<Capture>()).Select(c => c.Value);

        foreach (var keyToSubstitute in keysToSubstitute)
        {
            var isKeyEscaped = keyToSubstitute[0] == '{' && keyToSubstitute[keyToSubstitute.Length - 1] == '}';
            if (isKeyEscaped)
            {
                value = UnescapeConfigurationKey(value, keyToSubstitute);
                continue;
            }

            // This prevents an useless list allocation when there is nothing to substitute
            alreadySubstitutedKeys ??= new List<string>(1) { key };

            // Microsoft.Extensions.Configuration use case-insentitive keys
            if (alreadySubstitutedKeys.Contains(keyToSubstitute, StringComparer.OrdinalIgnoreCase))
            {
                alreadySubstitutedKeys.Add(keyToSubstitute);
                throw new RecursiveConfigurationKeyException(alreadySubstitutedKeys);
            }

            var substitutedValue = configuration[keyToSubstitute];
            if (substitutedValue == null)
            {
                throw new UnresolvedConfigurationKeyException(keyToSubstitute, key);
            }

            var recursivelySubstitutedValue = this.ApplySubstitutionRecursive(configuration, keyToSubstitute, substitutedValue, alreadySubstitutedKeys);

            value = value.Replace("${" + keyToSubstitute + "}", recursivelySubstitutedValue);
        }

        alreadySubstitutedKeys?.Remove(key);

        return value;
    }

    private static string UnescapeConfigurationKey(string value, string? keyToSubstitute)
    {
        // When {Foo} is captured, replace ${{Foo}} by ${Foo} without substitution
        var escapedKey = "${" + keyToSubstitute + "}";

        var idx = value.IndexOf(escapedKey, StringComparison.Ordinal);
        if (idx >= 0)
        {
            var unescapedKey = "$" + keyToSubstitute;
            return value.Remove(idx, escapedKey.Length).Insert(idx, unescapedKey);
        }

        return value;
    }
}