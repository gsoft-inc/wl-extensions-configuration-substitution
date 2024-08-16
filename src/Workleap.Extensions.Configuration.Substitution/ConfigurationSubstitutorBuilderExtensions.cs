using Workleap.Extensions.Configuration.Substitution;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.Configuration;

public static class ConfigurationSubstitutorBuilderExtensions
{
    /// <summary>
    /// Substitutes referenced configuration values in other configuration values, using the format <c>${MySection:MyValue}</c>.
    /// In that example, <c>${MySection:MyValue}</c> will be replaced by the actual string value.
    /// If a referenced configuration value does not exist, an exception will be thrown.
    /// You can escape values that must not be substituted using double curly braces, such as <c>${{Foo}}</c>.
    /// </summary>
    /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="eagerValidation">Whether or not all the configuration must be validated to ensure there are no referenced configuration keys that does not exist.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddSubstitution(this IConfigurationBuilder configurationBuilder, bool eagerValidation = false)
    {
        for (var i = 0; i < configurationBuilder.Sources.Count; i++)
        {
            var configurationSource = configurationBuilder.Sources[i];
            if (configurationSource is not CachedConfigurationSource)
            {
                configurationBuilder.Sources[i] = new CachedConfigurationSource(configurationSource);
            }
        }

        return configurationBuilder.Add(new ChainedSubstitutedConfigurationSource(eagerValidation));
    }
}