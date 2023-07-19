using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace Workleap.Extensions.Configuration.Substitution;

[DebuggerDisplay("CachedConfigurationSource({_underlyingConfigurationSourceType})")]
internal sealed class CachedConfigurationSource : IConfigurationSource
{
    private readonly object _lockObject = new object();
    private readonly IConfigurationSource _underlyingConfigurationSource;
    private readonly string _underlyingConfigurationSourceType;
    private volatile IConfigurationProvider? _configurationProvider;

    public CachedConfigurationSource(IConfigurationSource underlyingConfigurationSource)
    {
        this._underlyingConfigurationSource = underlyingConfigurationSource ?? throw new ArgumentNullException(nameof(underlyingConfigurationSource));
        this._underlyingConfigurationSourceType = this._underlyingConfigurationSource.GetType().Name;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        if (this._configurationProvider != null)
        {
            return this._configurationProvider;
        }

        lock (this._lockObject)
        {
            if (this._configurationProvider != null)
            {
                return this._configurationProvider;
            }

            this._configurationProvider = this._underlyingConfigurationSource.Build(builder);
        }

        return this._configurationProvider;
    }
}