using Nest;
using SearchAPi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle


var app = builder.Build();


app.MapGet("/weatherforecast", async (string city, int rating) =>
{
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

    
});

app.Run();

