using Microsoft.Extensions.Configuration;

namespace GSoft.Extensions.Configuration.Substitution;

public static class ConfigurationSubstitutorBuilderExtensions
{
    /// <summary>
    /// Substitutes referenced configuration values in other configuration values, using the format ${MySection:MyValue}.
    /// In that exemple, ${MySection:MyValue} will be replaced by the actual string value.
    /// If a referenced configuration value does not exist, an exception will be thrown.
    /// You can escape values that must not be substituted using double curly braces, such as ${{Foo}}.
    /// </summary>
    /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="validate">Whether or not all the configuration must be validated to ensure there are no referenced configuration variables that does not exist.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddSubstitution(this IConfigurationBuilder configurationBuilder, bool validate = false)
    {
        return AddSubstitution(configurationBuilder, new ConfigurationSubstitutor(), validate);
    }

    private static IConfigurationBuilder AddSubstitution(this IConfigurationBuilder configurationBuilder, ConfigurationSubstitutor substitutor, bool validate)
    {
        var intermediateConfigurationBuilder = new ConfigurationBuilder();

        foreach (var source in configurationBuilder.Sources)
        {
            // Prevent infinite recursive loop where several instances of our configuration source type would wrap each other
            if (source is not ChainedSubstitutedConfigurationSource)
            {
                intermediateConfigurationBuilder.Add(source);
            }
        }

        var configuration = intermediateConfigurationBuilder.Build();

        return configurationBuilder.Add(new ChainedSubstitutedConfigurationSource(substitutor, configuration, validate));
    }
}