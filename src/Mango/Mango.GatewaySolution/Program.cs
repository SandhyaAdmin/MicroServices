using Mango.Services.GatewaySolution.Extensions;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
// registering ocelot in the application, adds ocelots to the conatiner
builder.Services.AddOcelot();

/* add authentication to ocelot, the reason to add authentication to ocelot is
 * from the web project when we are calling the gateway, it needs to pass the jwt tokens to
 * the individual web services, only then api return back the repsonse.
 * */
// it’s the perfect hook to ensure downstream APIs only process valid and secure JWTs
builder.AddAppAuthentication();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.UseOcelot();
app.Run();


/* When the rquest comes from the web project with the api url, we need to tell the ocelot
 * which api it should call exatly.
 */


