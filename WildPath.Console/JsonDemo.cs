using Newtonsoft.Json.Linq;

namespace WildPath.Console;

using Console = System.Console;

public class JsonDemo
{
    public static void Test()
    {
        JObject o = JObject.Parse(
            """
            {
              "Stores": [
                "Lambton Quay",
                "Willis Street"
              ],
              "Manufacturers": [
                {
                  "Name": "Acme Co",
                  "Products": [
                    {
                      "Name": "Anvil",
                      "Price": 50
                    }
                  ]
                },
                {
                  "Name": "Contoso",
                  "Products": [
                    {
                      "Name": "Elbow Grease",
                      "Price": 99.95
                    },
                    {
                      "Name": "Headlight Fluid",
                      "Price": 4
                    }
                  ]
                }
              ]
            }
            """);

// manufacturer with the name 'Acme Co'
        JToken? acme = o.SelectToken("$.Manufacturers[?(@.Name == 'Acme Co')]");


        Console.WriteLine(acme);
        // { "Name": "Acme Co", Products: [{ "Name": "Anvil", "Price": 50 }] }

        // name of all products priced 50 and above
        IEnumerable<JToken> pricyProducts = o.SelectTokens("$..Products[?(@.Price >= 50)].Name");

        foreach (JToken item in pricyProducts)
        {
            Console.WriteLine(item);
        }
    }
}