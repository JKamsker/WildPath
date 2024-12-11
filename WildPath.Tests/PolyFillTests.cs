using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WildPath.Extensions;

namespace WildPath.Tests;

public class PolyFillTests
{
    [Fact]
    public void ConvertToString_Returns_Correct_Slice()
    {
        var myString = "Hello World";
        var span = myString.AsSpan()[1..5];

        var result = span.ConvertToString();

        Assert.Equal("ello", result);

    }

}
