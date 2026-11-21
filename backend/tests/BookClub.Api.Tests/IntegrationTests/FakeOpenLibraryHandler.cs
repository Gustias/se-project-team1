using System.Net;
using System.Text;

namespace BookClub.Api.Tests.IntegrationTests;

public class FakeOpenLibraryHandler : HttpMessageHandler // use a fake handler instead of sending real requests to openlibrary
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var json = """
                   {
                       "numFound": 1,
                       "start": 0,
                       "docs": [
                           {
                               "key": "/works/OL123W",
                               "title": "Dune",
                               "author_name": ["Frank Herbert"],
                               "first_publish_year": 1965
                           }
                       ]
                   }
                   """;

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json")
        };

        return Task.FromResult(response);
    }
}
