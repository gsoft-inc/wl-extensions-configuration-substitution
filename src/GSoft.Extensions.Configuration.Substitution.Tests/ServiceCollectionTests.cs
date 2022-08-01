using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GSoft.Extensions.Configuration.Substitution.Tests;

public class ServiceCollectionTests
{
    [Fact]
    public void ConfigurationRootWrapper_Flattens_Configuration_Providers()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["foo"] = "${bar}" })
            .AddInMemoryCollection(new Dictionary<string, string> { ["bar"] = "2" })
            .AddSubstitution()
            .AddInMemoryCollection(new Dictionary<string, string> { ["qux"] = "3" });

        // Memorize original configuration providers for later comparison
        var originalConfigRoot = configBuilder.Build();
        var originalProviders = originalConfigRoot.Providers.ToArray();
        Assert.Equal(4, originalProviders.Length);

        var originalMemoryProviderFoo = Assert.IsType<MemoryConfigurationProvider>(originalProviders[0]);
        var originalMemoryProviderBar = Assert.IsType<MemoryConfigurationProvider>(originalProviders[1]);
        var originalSubstitutedProvider = Assert.IsType<ChainedSubstitutedConfigurationProvider>(originalProviders[2]);
        var originalMemoryProviderQux = Assert.IsType<MemoryConfigurationProvider>(originalProviders[3]);

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(originalConfigRoot);

        // Act
        services.AddSubstitution();

        // Assert
        using var serviceProvider = services.BuildServiceProvider();
        var actualConfig = serviceProvider.GetRequiredService<IConfiguration>();
        var actualConfigRoot = Assert.IsType<ConfigurationRootWrapper>(actualConfig);
        Assert.Same(originalConfigRoot, actualConfigRoot.UnderlyingConfigurationRoot);

        // Retrieve actual configuration providers from the wrapped configuration root
        var actualProviders = actualConfigRoot.Providers.ToArray();
        Assert.Equal(4, actualProviders.Length);
        var actualMemoryProviderFoo = Assert.IsType<MemoryConfigurationProvider>(actualProviders[0]);
        var actualmemoryProviderBar = Assert.IsType<MemoryConfigurationProvider>(actualProviders[1]);
        var actualSubstitutedProvider = Assert.IsType<ChainedSubstitutedConfigurationProvider>(actualProviders[2]);
        var actualmemoryProviderBarQux = Assert.IsType<MemoryConfigurationProvider>(actualProviders[3]);

        // Same configuration keys
        Assert.Equal(new[] { "foo" }, actualMemoryProviderFoo.Select(x => x.Key).ToArray());
        Assert.Equal(new[] { "bar" }, actualmemoryProviderBar.Select(x => x.Key).ToArray());
        Assert.Equal(new[] { "qux" }, actualmemoryProviderBarQux.Select(x => x.Key).ToArray());

        // The first two memory providers actually come from the substituted provider
        var substitutedProviderUnderlyingProviders = actualSubstitutedProvider.Configuration.Providers.ToArray();
        Assert.Equal(2, substitutedProviderUnderlyingProviders.Length);

        Assert.Same(actualMemoryProviderFoo, substitutedProviderUnderlyingProviders[0]);
        Assert.Same(actualmemoryProviderBar, substitutedProviderUnderlyingProviders[1]);

        Assert.NotSame(originalMemoryProviderFoo, actualMemoryProviderFoo);
        Assert.NotSame(originalMemoryProviderBar, actualmemoryProviderBar);

        // The others providers that are not INSIDE the substituted provider are the same instances
        // Their type is ChainedSubstitutedConfigurationProvider or they were added AFTER the substitution configuration source
        Assert.Same(originalSubstitutedProvider, actualSubstitutedProvider);
        Assert.Same(originalMemoryProviderQux, actualmemoryProviderBarQux);

        // The substitution still works
        Assert.Equal("2", actualConfig["foo"]);
        Assert.Equal("2", actualConfig["bar"]);
        Assert.Equal("3", actualConfig["qux"]);
    }
}