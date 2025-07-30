using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// registering ocelot in the application, adds ocelots to the conatiner
builder.Services.AddOcelot();

app.MapGet("/", () => "Hello World!");
app.UseOcelot();
app.Run();
