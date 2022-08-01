using System;
using System.Collections.Generic;
using GSoft.Extensions.Configuration.Substitution;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Configuration;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adjusts the registered <see cref="IConfiguration"/> so the configuration providers enumerable available through <see cref="IConfigurationRoot"/> contains those inside the substituted configuration provider.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The modified service collection.</returns>
    /// <exception cref="ArgumentException">No <see cref="IConfiguration"/> was found in the service descriptors.</exception>
    public static IServiceCollection AddSubstitution(this IServiceCollection services)
    {
        var existingConfigurationDescriptors = new List<(int, ServiceDescriptor)>(capacity: 1);

        // Find existing IConfiguration service descriptors with their indexes in the services list
        // IConfiguration is known to be registered as a singleton factory that returns a concrete instance that also implements IConfigurationRoot when used with .NET hosting:
        // https://github.com/dotnet/runtime/blob/v6.0.0/src/libraries/Microsoft.Extensions.Hosting/src/HostBuilder.cs#L243
        for (var index = 0; index < services.Count; index++)
        {
            if (services[index].ServiceType == typeof(IConfiguration))
            {
                existingConfigurationDescriptors.Add((index, services[index]));
            }
        }

        if (existingConfigurationDescriptors.Count == 0)
        {
            throw new ArgumentException("At least one IConfiguration service descriptor is required", nameof(services));
        }

        // Wrap each existing IConfiguration service descriptor
        foreach (var (index, existingConfigurationDescriptor) in existingConfigurationDescriptors)
        {
            var newConfigurationDescriptor = new ServiceDescriptor(
                typeof(IConfiguration),
                serviceProvider => ConfigurationRootWrapperFactory(serviceProvider, existingConfigurationDescriptor),
                existingConfigurationDescriptor.Lifetime);

            services.Insert(index, newConfigurationDescriptor);
            services.Remove(existingConfigurationDescriptor);
        }

        return services;
    }

    private static object ConfigurationRootWrapperFactory(IServiceProvider serviceProvider, ServiceDescriptor existingConfigurationDescriptor)
    {
        IConfigurationRoot existingConfigurationRoot;

        if (existingConfigurationDescriptor.ImplementationInstance != null)
        {
            existingConfigurationRoot = (IConfigurationRoot)existingConfigurationDescriptor.ImplementationInstance;
        }
        else if (existingConfigurationDescriptor.ImplementationFactory != null)
        {
            existingConfigurationRoot = (IConfigurationRoot)existingConfigurationDescriptor.ImplementationFactory(serviceProvider);
        }
        else if (existingConfigurationDescriptor.ImplementationType != null)
        {
            existingConfigurationRoot = (IConfigurationRoot)ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, existingConfigurationDescriptor.ImplementationType);
        }
        else
        {
            throw new InvalidOperationException("IConfiguration service descriptor must provide an implementation that also implements IConfigurationRoot");
        }

        return new ConfigurationRootWrapper(existingConfigurationRoot);
    }
}