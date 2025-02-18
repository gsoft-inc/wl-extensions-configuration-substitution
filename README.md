# Workleap.Extensions.Configuration.Substitution

This package adds variable substitution configuration provider implementation for [Microsoft.Extensions.Configuration](https://docs.microsoft.com/en-us/dotnet/core/extensions/configuration).

[![nuget](https://img.shields.io/nuget/v/Workleap.Extensions.Configuration.Substitution.svg?logo=nuget)](https://www.nuget.org/packages/Workleap.Extensions.Configuration.Substitution/)
[![build](https://img.shields.io/github/actions/workflow/status/workleap/wl-extensions-configuration-substitution/publish.yml?logo=github)](https://github.com/workleap/wl-extensions-configuration-substitution/actions/workflows/publish.yml)

## Getting started

```
dotnet add package Workleap.Extensions.Configuration.Substitution
```

```csharp
// Example for an ASP.NET Core web application
var builder = WebApplication.CreateBuilder(args);

// Setup your configuration
builder.Configuration.AddJsonFile("appsettings.json");
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddSubstitution(); // <-- Add this after other configuration providers
```


## How it works

You can reference configuration values inside other configuration values by enclosing the referenced configuration key like this: `${ReferencedConfigurationKey}`.


### Examples

Consider this `appsettings.json`:

```json
{
  "Credentials": {
    "Username": "alice1",
    "Password": "P@ssw0rd"
  },
  "ConnectionString": "usr=${Credentials:Username};pwd=${Credentials:Password}"
}
```

Evaluating the configuration value `ConnectionString` would return `usr=alice1;pwd=P@ssw0rd`.


**This also works if you're using multiple configuration providers**. For instance, one could have the `Credentials:Password` configuration value provided by a secret from Azure Key Vault and this value would have been injected into the `ConnectionString` value too.

It also works with **arrays**:

```json
{
  "Credentials": [ "alice1", "P@ssw0rd" ],
  "ConnectionString": "usr=${Credentials:0};pwd=${Credentials:1}"
}
```

Again, you're not limited to JSON file providers, **you could use substitution with any configuration providers**. It was easier to use JSON files in these examples.


### Escaping values

You might not want a specific value to be substituted. In that case, escape it using double curly braces:

```json
{
  "Foo": "foo",
  "Bar": "${{Foo}}"
}
```

Evaluating the configuration value `Bar` would return `${Foo}`.


### Exceptions

You can encounter two kinds of exceptions if your configuration is incorrect:

* `UnresolvedConfigurationKeyException`, if you're trying to substitute a configuration value that is undefined (i.e. the key does not exist).
* `RecursiveConfigurationKeyException`, if you have many configuration values that reference each other in a recursive manner, no matter how deep the recursion is. The exception will give you details about the recursive path.

`UnresolvedConfigurationKeyException` can also be triggered sooner than later by using `AddSubstitution(eagerValidation: true)`. Using `eagerValidation` with value `true` (default is `false`) instructs the library to check for undefined values in **all the existing configuration values** once, instead of checking for a particular value. This happens as soon as any configuration value is loaded.


### Configuration providers order

When using .NET's [IConfigurationBuilder](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration.iconfigurationbuilder), the order of configuration providers matters . Any configuration provider added after `AddSubstitution()` would not benefit from the substitution process.


## License

Copyright © 2022, Workleap. This code is licensed under the Apache License, Version 2.0. You may obtain a copy of this license at https://github.com/workleap/gsoft-license/blob/master/LICENSE.
