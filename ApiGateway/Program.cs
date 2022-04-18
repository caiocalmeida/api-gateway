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

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("dotnet", (IServiceProvider sp, HttpClient c) =>
{
    HttpContext context = sp.GetRequiredService<IHttpContextAccessor>().HttpContext!;

    if (context!.Request.Headers.ContainsKey("X-API-KEY"))
    {
        c.DefaultRequestHeaders.Add("X-API-KEY", context!.Request.Headers["X-API-KEY"].ToString());
    }

    c.BaseAddress = new Uri(builder.Configuration["DotnetGraphQLUrl"]);
});
builder.Services
    .AddGraphQLServer()
    .AddRemoteSchema("dotnet");


var app = builder.Build();

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseEndpoints(endpoints => endpoints.MapGraphQL());

app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
    opt.DocumentTitle = "Caio's API Gateway Swagger";
    opt.ReConfigureUpstreamSwaggerJson = (_, json) =>
    {
        var swagger = JObject.Parse(json);
        swagger["info"]!["title"] = "Caio's API Gateway";
        return swagger.ToString(Formatting.Indented);
    };
});
app.UseOcelot().Wait();

app.Run();