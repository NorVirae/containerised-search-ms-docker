using Nest;
using SearchAPi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle


var app = builder.Build();


app.MapGet("/search", async (string city, int rating) =>
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

});

app.Run();

