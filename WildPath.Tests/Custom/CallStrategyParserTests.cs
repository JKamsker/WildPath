using WildPath.Internals;
using System;
using System.Collections.Immutable;
using System.ComponentModel.Design;
using Xunit;

namespace WildPath.Tests;

public class CustomStrategyCallParserTests
{
    [Fact]
    public void ExtractMethodCall_ValidInput_ReturnsCorrectCustomStrategyCall()
    {
        // Arrange
        var input = ":MethodName(param1, param2, param3):";
        var expectedMethodName = "MethodName";
        var expectedParameters = new[] { "param1", "param2", "param3" }
            .Select((value, index) => new StrategyCallParameterInfo(null, value, index))
            .ToImmutableArray();

        // Act
        var result = CustomStrategyCallParser.ExtractMethodCall(input);

        // Assert
        Assert.Equal(expectedMethodName, result.MethodName);
        Assert.Equal(expectedParameters, result.Parameters.ToImmutableArray());
    }
    
    // single parameter
    [Fact]
    public void ExtractMethodCall_ValidInputWithSingleParameter_ReturnsCorrectCustomStrategyCall()
    {
        // Arrange
        var input = ":MethodName(param1):";
        var expectedMethodName = "MethodName";
        var expectedParameters = new[] { "param1" }
            .Select((value, index) => new StrategyCallParameterInfo(null, value, index))
            .ToImmutableArray();

        // Act
        var result = CustomStrategyCallParser.ExtractMethodCall(input);

        // Assert
        Assert.Equal(expectedMethodName, result.MethodName);
        Assert.Equal(expectedParameters, result.Parameters.ToImmutableArray());
    }

    [Fact]
    public void ExtractMethodCall_InputWithNoParameters_ReturnsCustomStrategyCallWithEmptyParameters()
    {
        // Arrange
        var input = ":MethodName():";
        var expectedMethodName = "MethodName";
        var expectedParameters = Array.Empty<StrategyCallParameterInfo>();

        // Act
        var result = CustomStrategyCallParser.ExtractMethodCall(input);

        // Assert
        Assert.Equal(expectedMethodName, result.MethodName);
        Assert.Equal(expectedParameters, result.Parameters);
    }

    [Fact]
    public void ExtractMethodCall_InputWithSpacesAroundParameters_ReturnsTrimmedParameters()
    {
        // Arrange
        var input = ":MethodName( param1 , param2 , param3 ):";
        var expectedMethodName = "MethodName";
        var expectedParameters = new[] { "param1", "param2", "param3" }
            .Select((value, index) => new StrategyCallParameterInfo(null, value, index))
            .ToImmutableArray();

        // Act
        var result = CustomStrategyCallParser.ExtractMethodCall(input);

        // Assert
        Assert.Equal(expectedMethodName, result.MethodName);
        Assert.Equal(expectedParameters, result.Parameters.ToImmutableArray());
    }

    [Fact]
    public void ExtractMethodCall_InvalidInput_ThrowsArgumentException()
    {
        // Arrange
        var input = "InvalidInput";

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => CustomStrategyCallParser.ExtractMethodCall(input));
        Assert.Equal("Input string is not in the expected format.", ex.Message);
    }

    [Fact]
    public void ExtractMethodCall_EmptyInput_ThrowsArgumentException()
    {
        // Arrange
        var input = string.Empty;

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => CustomStrategyCallParser.ExtractMethodCall(input));
        Assert.Equal("Input string is not in the expected format.", ex.Message);
    }

    [Fact]
    public void ExtractMethodCall_InputWithSpecialCharactersInParameters_ReturnsCorrectParameters()
    {
        // Arrange
        var input = ":MethodName(param1, param@2, param-3):";
        var expectedMethodName = "MethodName";
        var expectedParameters = new[] { "param1", "param@2", "param-3" }
            .Select((value, index) => new StrategyCallParameterInfo(null, value, index))
            .ToImmutableArray();

        // Act
        var result = CustomStrategyCallParser.ExtractMethodCall(input);

        // Assert
        Assert.Equal(expectedMethodName, result.MethodName);
        Assert.Equal(expectedParameters, result.Parameters);
    }

    [Fact]
    public void ExtractMethodCall_InputWithEmptyParameters_ReturnsEmptyStringsAsParameters()
    {
        // Arrange
        var input = ":MethodName(,,):";
        var expectedMethodName = "MethodName";
        var expectedParameters = new[] { "", "", "" }
            .Select((value, index) => new StrategyCallParameterInfo(null, value, index))
            .ToImmutableArray();

        // Act
        var result = CustomStrategyCallParser.ExtractMethodCall(input);

        // Assert
        Assert.Equal(expectedMethodName, result.MethodName);
        Assert.Equal(expectedParameters, result.Parameters.ToImmutableArray());
    }
    
    // :hasJson(myJson.json, $..Products[?(@.Price >= 50)].Name, Anvil):
    [Fact]
    public void ExtractMethodCall_ValidInputWithClosingBracket_ReturnsCorrectCustomStrategyCall()
    {
        // Arrange
        var input = ":hasJson(myJson.json, $..Products[?(@.Price >= 50)].Name, Anvil):";
        var expectedMethodName = "hasJson";
        var expectedParameters = new[] { "myJson.json", "$..Products[?(@.Price >= 50)].Name", "Anvil" }
            .Select((value, index) => new StrategyCallParameterInfo(null, value, index))
            .ToImmutableArray();

        // Act
        var result = CustomStrategyCallParser.ExtractMethodCall(input);

        // Assert
        Assert.Equal(expectedMethodName, result.MethodName);
        Assert.Equal(expectedParameters, result.Parameters.ToImmutableArray());
    }
    
    // named
    [Fact]
    public void ExtractMethodCall_ValidInputWithNamedParameters_ReturnsCorrectCustomStrategyCall()
    {
        // Arrange
        var input = ":MethodName(param1: value1, param2: value2, param3: value3):";
        var expectedMethodName = "MethodName";
        var expectedParameters = new[] { "param1: value1", "param2: value2", "param3: value3" }
            .Select((value, index) => new StrategyCallParameterInfo(value.Split(':')[0], value.Split(':')[1].Trim(), index))
            .ToImmutableArray();

        // Act
        var result = CustomStrategyCallParser.ExtractMethodCall(input);

        // Assert
        Assert.Equal(expectedMethodName, result.MethodName);
        Assert.Equal(expectedParameters, result.Parameters.ToImmutableArray());
    }
}