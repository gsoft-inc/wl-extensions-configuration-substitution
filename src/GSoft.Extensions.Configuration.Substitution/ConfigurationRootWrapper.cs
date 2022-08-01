using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace GSoft.Extensions.Configuration.Substitution;

internal sealed class ConfigurationRootWrapper : IConfigurationRoot
{
    public ConfigurationRootWrapper(IConfigurationRoot underlyingConfigurationRoot)
    {
        this.UnderlyingConfigurationRoot = underlyingConfigurationRoot;
    }

    public IConfigurationRoot UnderlyingConfigurationRoot { get; }

    public IEnumerable<IConfigurationProvider> Providers => this.GetProviders();

    public string this[string key]
    {
        get => this.UnderlyingConfigurationRoot[key];
        set => this.UnderlyingConfigurationRoot[key] = value;
    }

    public IConfigurationSection GetSection(string key)
    {
        return this.UnderlyingConfigurationRoot.GetSection(key);
    }

    public IEnumerable<IConfigurationSection> GetChildren()
    {
        return this.UnderlyingConfigurationRoot.GetChildren();
    }

    public IChangeToken GetReloadToken()
    {
        return this.UnderlyingConfigurationRoot.GetReloadToken();
    }

    public void Reload()
    {
        this.UnderlyingConfigurationRoot.Reload();
    }

    private IEnumerable<IConfigurationProvider> GetProviders()
    {
        var existingProviders = this.UnderlyingConfigurationRoot.Providers.ToArray();
        var returnedProviders = new List<IConfigurationProvider>(existingProviders.Length);
        return GetProviders(returnedProviders, existingProviders);
    }

    // Flattens the list of configuration providers contained inside our substituted configuration provider
    private static IEnumerable<IConfigurationProvider> GetProviders(IList<IConfigurationProvider> returnedProviders, IReadOnlyList<IConfigurationProvider> existingProviders)
    {
        for (var i = existingProviders.Count - 1; i >= 0; i--)
        {
            returnedProviders.Insert(0, existingProviders[i]);

            if (existingProviders[i] is ChainedSubstitutedConfigurationProvider substitutedProvider)
            {
                return GetProviders(returnedProviders, substitutedProvider.Configuration.Providers.ToArray());
            }
        }

        return returnedProviders;
    }
}