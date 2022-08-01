using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace GSoft.Extensions.Configuration.Substitution;

internal sealed class ChainedSubstitutedConfigurationProvider : ConfigurationProvider
{
    private readonly bool _eagerValidation;

    public ChainedSubstitutedConfigurationProvider(IConfigurationRoot configuration, bool eagerValidation)
    {
        this.Configuration = configuration;
        this._eagerValidation = eagerValidation;
    }

    public IConfigurationRoot Configuration { get; }

    public override bool TryGet(string key, out string value)
    {
        var substituted = ConfigurationSubstitutor.GetSubstituted(this.Configuration, key);
        if (substituted == null)
        {
            value = string.Empty;
            return false;
        }

        value = substituted;
        return true;
    }

    public override void Set(string key, string value) => this.Configuration[key] = value;

    public override void Load()
    {
        if (this._eagerValidation)
        {
            this.EnsureAllKeysAreSubstituted();
        }
    }

    private void EnsureAllKeysAreSubstituted()
    {
        foreach (var kvp in this.Configuration.AsEnumerable())
        {
            // This loop goes through the entire configuration (even nested sections).
            // Reading each individual value triggers the substitution process and it will throw if a referenced key is unresolved.
            _ = kvp.Value;
        }
    }

    public override IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath)
    {
        IConfiguration config = this.Configuration;
        var section = parentPath == null ? config : config.GetSection(parentPath);
        var keys = section.GetChildren().Select(c => c.Key);
        return keys.Concat(earlierKeys).OrderBy(k => k, ConfigurationKeyComparer.Instance).ToArray();
    }
}