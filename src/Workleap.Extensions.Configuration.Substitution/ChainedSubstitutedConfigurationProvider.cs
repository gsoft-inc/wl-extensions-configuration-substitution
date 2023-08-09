using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Workleap.Extensions.Configuration.Substitution;

internal sealed class ChainedSubstitutedConfigurationProvider : IConfigurationProvider, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly bool _eagerValidation;

    public ChainedSubstitutedConfigurationProvider(IConfiguration configuration, bool eagerValidation)
    {
        this._configuration = configuration;
        this._eagerValidation = eagerValidation;
    }

    public bool TryGet(string key, out string value)
    {
        var substituted = ConfigurationSubstitutor.GetSubstituted(this._configuration, key);
        if (substituted == null)
        {
            value = string.Empty;
            return false;
        }

        value = substituted;
        return true;
    }

    public void Set(string key, string? value)
    {
        this._configuration[key] = value;
    }

    public IChangeToken GetReloadToken()
    {
        return this._configuration.GetReloadToken();
    }

    public void Load()
    {
        if (this._eagerValidation)
        {
            this.EnsureAllKeysAreSubstituted();
        }
    }

    private void EnsureAllKeysAreSubstituted()
    {
        foreach (var kvp in this._configuration.AsEnumerable())
        {
            // This loop goes through the entire configuration (even nested sections).
            // Reading each individual value triggers the substitution process and it will throw if a referenced key is unresolved.
            _ = kvp.Value;
        }
    }

    public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath)
    {
        var section = parentPath == null ? this._configuration : this._configuration.GetSection(parentPath);
        var keys = section.GetChildren().Select(c => c.Key);
        return keys.Concat(earlierKeys).OrderBy(k => k, ConfigurationKeyComparer.Instance).ToArray();
    }

    public void Dispose()
    {
        // ConfigurationRoot could have disposable configuration providers:
        // https://github.com/dotnet/runtime/blob/v6.0.0/src/libraries/Microsoft.Extensions.Configuration/src/ConfigurationRoot.cs#L99
        // this.Dispose() is called when the IServiceProvider is disposed (mostly on app shutdown)
        (this._configuration as IDisposable)?.Dispose();
    }
}