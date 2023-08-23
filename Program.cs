using Nest;
using Polly;
using Polly.CircuitBreaker;
using SearchAPi.Models;
using System.Net.Http.Headers;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle


var app = builder.Build();

var circuiteBreakerPolicy = Polly.Policy<List<HotelEvent>>
    .Handle<Exception>()
    .CircuitBreakerAsync(3, TimeSpan.FromSeconds(30));

app.MapGet("/search", async (string city, int rating) =>
{
    var result = new HttpResponseMessage();

    try
    {
        var hotels = circuiteBreakerPolicy.ExecuteAsync(async () =>
        {
            return await ListHotels(city, rating);

        });
        result.StatusCode = System.Net.HttpStatusCode.OK;
        result.Content = new StringContent(JsonSerializer.Serialize(hotels));
        result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
    }
    catch (BrokenCircuitException)
    {
        result.StatusCode = System.Net.HttpStatusCode.NotAcceptable;
        result.ReasonPhrase = "CircuitOpen";
    }
    catch(Exception e)
    {
        result.StatusCode = System.Net.HttpStatusCode.BadRequest;
        result.ReasonPhrase = e.Message;
    }

    return result;
});

async Task<List<HotelEvent>> ListHotels(string city, int rating)
{
    Console.WriteLine("Got In");
    var host = "https://05ba5b06face46848e24ff8ae7637179.us-central1.gcp.cloud.es.io";
    var userName = "elastic";
    var password = "RcMeOiw2kiUCBc7j08Hv1dDN";
    var indexName = "event";

    var conSett = new ConnectionSettings(new Uri(host));
    conSett.BasicAuthentication(userName, password);
    conSett.DefaultIndex(indexName);
    conSett.DefaultMappingFor<HotelEvent>(m => m.IdProperty(p => p.id));

    var client = new ElasticClient(conSett);

    if (rating == null)
    {
        rating = 1;
    }


    ISearchResponse<HotelEvent> result;

    if (city is null)
    {
        result = await client.SearchAsync<HotelEvent>(s =>
            s.Query(q =>
                q.MatchAll()
                &&
                q.Range(r => r.Field(f => f.Rating).GreaterThanOrEquals(rating))
                )
            );
    }
    else
    {
        result = await client.SearchAsync<HotelEvent>(s =>
            s.Query(q =>
                q.Prefix(p => p.Field(f => f.CityName).Value(city).Value(city).CaseInsensitive())
                &&
                q.Range(r => r.Field(f => f.Rating).GreaterThanOrEquals(rating))
                )
            );
    }

    Console.WriteLine(result.Hits.ToString());
    return result.Hits.Select(x => x.Source).ToList();
}

app.Run();

