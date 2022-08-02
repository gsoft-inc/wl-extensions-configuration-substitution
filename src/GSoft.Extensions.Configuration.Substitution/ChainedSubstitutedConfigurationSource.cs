using Microsoft.Extensions.Configuration;

namespace GSoft.Extensions.Configuration.Substitution;

internal sealed class ChainedSubstitutedConfigurationSource : IConfigurationSource
{
    private readonly bool _eagerValidation;

    public ChainedSubstitutedConfigurationSource(bool eagerValidation)
    {
        this._eagerValidation = eagerValidation;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        var configurationBuilder = new ConfigurationBuilder();

        for (var i = 0; i < builder.Sources.Count && builder.Sources[i] is not ChainedSubstitutedConfigurationSource; i++)
        {
            configurationBuilder.Add(builder.Sources[i]);
        }

        return new ChainedSubstitutedConfigurationProvider(configurationBuilder.Build(), this._eagerValidation);
    }
}