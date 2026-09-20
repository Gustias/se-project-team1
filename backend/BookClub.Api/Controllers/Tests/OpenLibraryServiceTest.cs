using System;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

class OpenLibraryServiceTest
{
    static async Task Main()
    {
        using HttpClient client = new HttpClient();

        string url = "https://openlibrary.org/search.json?q=the+lord+of+the+rings";

        try
        {
            HttpResponseMessage response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            JsonNode? dataJson = JsonNode.Parse(json);

            Console.WriteLine("Response status code: " + (int)response.StatusCode);
            Console.WriteLine("Message: " + response.ReasonPhrase);
            Console.WriteLine("First book: " + dataJson?["docs"]?[0]?["title"]);
        }
        catch(HttpRequestException error)
        {
            Console.WriteLine("Exception with HTTP: " + error.Message);
        }
    }
}