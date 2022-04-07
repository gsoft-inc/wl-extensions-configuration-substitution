using Microsoft.Extensions.Configuration;

namespace GSoft.Extensions.Configuration.Substitution;

internal sealed class ChainedSubstitutedConfigurationSource : IConfigurationSource
{
    private readonly ConfigurationSubstitutor _substitutor;
    private readonly IConfiguration _configuration;
    private readonly bool _validate;

    public ChainedSubstitutedConfigurationSource(ConfigurationSubstitutor substitutor, IConfiguration configuration, bool validate)
    {
        this._substitutor = substitutor;
        this._configuration = configuration;
        this._validate = validate;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new ChainedSubstitutedConfigurationProvider(this._configuration, this._substitutor, this._validate);
    }
}