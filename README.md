# GSoft.Extensions.Configuration.Substitution

This package adds variable substitution configuration provider implementation for [Microsoft.Extensions.Configuration](https://docs.microsoft.com/en-us/dotnet/core/extensions/configuration).


## Getting started

```
dotnet add package GSoft.Extensions.Configuration.Substitution
```

```csharp
// Example for an ASP.NET Core web application
var builder = WebApplication.CreateBuilder(args);

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
  "Bar": "${{Foo}}" // configuration["Bar"] would return "${Foo}"
}
```


### Exceptions

You can encounter two kinds of exceptions if your configuration is incorrect:

* `UnresolvedConfigurationKeyException`, if you're trying to substitute a configuration value that is undefined (i.e. the key does not exist).
* `RecursiveConfigurationKeyException`, if you have many configuration values that reference each other in a recursive manner, no matter how deep the recursion is. The exception will give you details about the recursive path.

`UnresolvedConfigurationKeyException` can also be triggered sooner than later by using `AddSubstitution(eagerValidate: true)`. Using `eagerValidate` with value `true` (default is `false`) instructs the library to immediately go through all configuration values and check if there are referenced configuration values that are undefined.


### Configuration providers order

When using .NET's [IConfigurationBuilder](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration.iconfigurationbuilder), the order of configuration providers matters . Any configuration provider added after `AddSubstitution()` would not benefit from the substitution process.


## 🤝 Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change. If you're interested, definitely check out our Contributing Guide!


## License

Copyright © 2022, GSoft inc. This code is licensed under the Apache License, Version 2.0. You may obtain a copy of this license at https://github.com/gsoft-inc/gsoft-license/blob/master/LICENSE.