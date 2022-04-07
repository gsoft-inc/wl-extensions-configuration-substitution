using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace GSoft.Extensions.Configuration.Substitution.Tests;

public class ConfigurationSubstitutorTests
{
    [Fact]
    public void AddSubstitution_Returns_Value_Without_Substitution_When_No_Variable_To_Substitute()
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Foo", "Bar" },
            })
            .AddSubstitution();

        var configuration = configurationBuilder.Build();

        // Act
        var substituted = configuration["Foo"];

        // Assert
        Assert.Equal("Bar", substituted);
    }

    [Fact]
    public void AddSubstitution_Can_Substitute_Variables_In_The_Middle_Of_Value()
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "ConnectionString", "blablabla&password=${DatabasePassword}&server=localhost" },
                { "DatabasePassword", "ComplicatedPassword" },
            })
            .AddSubstitution();

        var configuration = configurationBuilder.Build();

        // Act
        var substituted = configuration["ConnectionString"];

        // Assert
        Assert.Equal("blablabla&password=ComplicatedPassword&server=localhost", substituted);
    }

    [Fact]
    public void AddSubstitution_Can_Substitute_Variables_At_The_Beginning_Of_Value()
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "ConnectionString", "${DatabasePassword}&server=localhost" },
                { "DatabasePassword", "ComplicatedPassword" },
            })
            .AddSubstitution();

        var configuration = configurationBuilder.Build();

        // Act
        var substituted = configuration["ConnectionString"];

        // Assert
        Assert.Equal("ComplicatedPassword&server=localhost", substituted);
    }

    [Fact]
    public void AddSubstitution_Can_Substitute_Variables_At_The_End_Of_Value()
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "ConnectionString", "blablabla&password=${DatabasePassword}" },
                { "DatabasePassword", "ComplicatedPassword" },
            })
            .AddSubstitution();

        var configuration = configurationBuilder.Build();

        // Act
        var substituted = configuration["ConnectionString"];

        // Assert
        Assert.Equal("blablabla&password=ComplicatedPassword", substituted);
    }

    [Fact]
    public void AddSubstitution_Can_Substitute_Variables_Using_Nested_Semicolon_Syntax()
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Foo", "Hello ${Bar1:Bar2}" },
                { "Bar1:Bar2", "world!" },
            })
            .AddSubstitution();

        var configuration = configurationBuilder.Build();

        // Act
        var substituted = configuration["Foo"];

        // Assert
        Assert.Equal("Hello world!", substituted);
    }

    [Fact]
    public void AddSubstitution_Can_Substitute_Multiple_Variables_In_Single_Value()
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Foo", "${Bar1}${Bar2}${Bar1}" },
                { "Bar1", "Doe" },
                { "Bar2", "-John-" },
            })
            .AddSubstitution();

        var configuration = configurationBuilder.Build();

        // Act
        var substituted = configuration["Foo"];

        // Assert
        Assert.Equal("Doe-John-Doe", substituted);
    }

    [Fact]
    public void AddSubstitution_Throws_When_Referenced_Variable_Does_Not_Exist()
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "TestKey", "Test value ${Foobar}" },
            })
            .AddSubstitution();

        // Act
        var configuration = configurationBuilder.Build();

        // Act & assert
        var ex = Assert.Throws<UnresolvedConfigurationVariableException>(() => configuration["TestKey"]);
        Assert.Contains("Foobar", ex.Message);
    }

    [Fact]
    public void AddSubstitution_Does_Not_Substitute_When_Prefix_Is_Incomplete()
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Foo", "Hello {world}" },
            })
            .AddSubstitution();

        var configuration = configurationBuilder.Build();

        // Act
        var substituted = configuration["Foo"];

        // Assert
        Assert.Equal("Hello {world}", substituted);
    }

    [Fact]
    public void AddSubstitution_Does_Not_Substitute_When_Suffix_Is_Incomplete()
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Foo", "Hello ${Var what's up ?" },
            })
            .AddSubstitution();

        var configuration = configurationBuilder.Build();

        // Act
        var substituted = configuration["Foo"];

        // Assert
        Assert.Equal("Hello ${Var what's up ?", substituted);
    }

    [Fact]
    public void AddSubstitution_Can_Substitute_Empty_String_Variable()
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Foo", "${Var1}" },
                { "Var1", string.Empty },
            })
            .AddSubstitution();

        var configuration = configurationBuilder.Build();

        // Act
        var substituted = configuration["Foo"];

        // Assert
        Assert.Equal(string.Empty, substituted);
    }

    [Fact]
    public void AddSubstitution_Throws_When_Substituted_Variable_Is_Null()
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Foo", "${Var1}" },
                { "Var1", null },
            })
            .AddSubstitution();

        var configuration = configurationBuilder.Build();

        // Act & assert
        var ex = Assert.Throws<UnresolvedConfigurationVariableException>(() => configuration["Foo"]);
        Assert.Contains("Var1", ex.Message);
    }

    [Fact]
    public void AddSubstitution_Can_Substitute_Multiple_Nested_Variables()
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Bar", "${Foo}" },
                { "Foo", "Hello ${Qux}" },
                { "Qux", "${WO}${Baz}!" },
                { "WO", "${W}${O}" },
                { "W", "w" },
                { "O", "o" },
                { "Baz", "rld" },
            })
            .AddSubstitution();

        var configuration = configurationBuilder.Build();

        // Act
        var substituted = configuration["Bar"];

        // Assert
        Assert.Equal("Hello world!", substituted);
    }

    [Fact]
    public void AddSubstitution_Throws_When_Simple_Circular_Variable_Reference()
    {
        var configurationBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Bar", "${bar}" },
            })
            .AddSubstitution();

        var configuration = configurationBuilder.Build();

        // Act & assert
        var ex = Assert.Throws<RecursiveConfigurationVariableException>(() => configuration["Bar"]);
        Assert.Contains("Bar > bar", ex.Message);
    }

    [Fact]
    public void AddSubstitution_Throws_When_Complex_Circular_Variable_Reference_In_Multiple_Variables()
    {
        var configurationBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Bar", "${Foo}" },
                { "Foo", "Hello ${Qux}" },
                { "Qux", "${WO}${Baz}!" },
                { "Baz", "rld" },
                { "WO", "${W}${Bar}" },
                { "W", "w" },
                { "O", "o" },
            })
            .AddSubstitution();

        var configuration = configurationBuilder.Build();

        // Act & assert
        var ex = Assert.Throws<RecursiveConfigurationVariableException>(() => configuration["Bar"]);
        Assert.Contains("Bar > Foo > Qux > WO > Bar", ex.Message);
    }

    [Fact]
    public void AddSubstitution_Does_Not_Substitute_Escaped_Variables_And_Removes_Escape_Characters()
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Foo", "Hello ${Bar}!" },
                { "Bar", "${{Bar}}" },
            })
            .AddSubstitution();

        var configuration = configurationBuilder.Build();

        // Act
        var substituted = configuration["Foo"];

        // Assert
        Assert.Equal("Hello ${Bar}!", substituted);
    }

    [Fact]
    public void AddSubstitution_Ignores_Incomplete_Escaped_Variable_And_Does_Not_Unescape()
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Foo", "Hello ${Bar}!" },
                { "Bar", "${{Bar}" },
            })
            .AddSubstitution();

        var configuration = configurationBuilder.Build();

        // Act
        var substituted = configuration["Foo"];

        // Assert
        Assert.Equal("Hello ${{Bar}!", substituted);
    }
}