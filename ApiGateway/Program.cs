using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("ocelot.json");

builder.Services.AddOcelot();
builder.Services.AddSwaggerForOcelot(builder.Configuration);
builder.Services.AddControllers();


var app = builder.Build();

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
    opt.DocumentTitle = "Caio's API Gateway";
    opt.ReConfigureUpstreamSwaggerJson = (_, json) =>
    {
        var swagger = JObject.Parse(json);
        swagger["info"]!["title"] = "Caio's API Gateway";
        return swagger.ToString(Formatting.Indented);
    };
});
app.UseOcelot().Wait();

app.Run();