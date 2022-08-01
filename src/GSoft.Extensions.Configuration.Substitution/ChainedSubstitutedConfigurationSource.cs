using Microsoft.Extensions.Configuration;

namespace GSoft.Extensions.Configuration.Substitution;

internal sealed class ChainedSubstitutedConfigurationSource : IConfigurationSource
{
    private readonly IConfigurationSource[] _configurationSources;
    private readonly bool _eagerValidation;

    public ChainedSubstitutedConfigurationSource(IConfigurationSource[] configurationSources, bool eagerValidation)
    {
        this._configurationSources = configurationSources;
        this._eagerValidation = eagerValidation;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        var configurationBuilder = new ConfigurationBuilder();

        foreach (var configurationSource in this._configurationSources)
        {
            configurationBuilder.Add(configurationSource);
        }

        return new ChainedSubstitutedConfigurationProvider(configurationBuilder.Build(), this._eagerValidation);
    }
}