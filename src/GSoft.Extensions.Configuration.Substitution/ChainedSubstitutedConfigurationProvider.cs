using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace GSoft.Extensions.Configuration.Substitution;

internal sealed class ChainedSubstitutedConfigurationProvider : ConfigurationProvider
{
    private readonly IConfiguration _config;
    private readonly ConfigurationSubstitutor _substitutor;
    private readonly bool _validate;

    public ChainedSubstitutedConfigurationProvider(IConfiguration config, ConfigurationSubstitutor substitutor, bool validate)
    {
        this._config = config;
        this._substitutor = substitutor;
        this._validate = validate;
    }

    public override bool TryGet(string key, out string value)
    {
        var substituted = this._substitutor.GetSubstituted(this._config, key);
        if (substituted == null)
        {
            value = string.Empty;
            return false;
        }

        value = substituted;
        return true;
    }

    public override void Set(string key, string value) => this._config[key] = value;

    public override void Load()
    {
        if (this._validate)
        {
            this.EnsureAllKeysAreSubstituted();
        }
    }

    private void EnsureAllKeysAreSubstituted()
    {
        foreach (var kvp in this._config.AsEnumerable())
        {
            // This loop goes through the entire configuration (even nested sections).
            // Reading each individual value triggers the substitution process and it will throw if a referenced key is unresolved.
            _ = kvp.Value;
        }
    }

    public override IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath)
    {
        var section = parentPath == null ? this._config : this._config.GetSection(parentPath);
        var keys = section.GetChildren().Select(c => c.Key);
        return keys.Concat(earlierKeys).OrderBy(k => k, ConfigurationKeyComparer.Instance).ToArray();
    }
}