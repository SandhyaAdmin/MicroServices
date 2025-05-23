
using AutoMapper;
using Mango.MessageBus;
using Mango.Services.OrderAPI;
using Mango.Services.OrderAPI.Data;
using Mango.Services.OrderAPI.Extensions;
using Mango.Services.OrderAPI.Service.Iservice;
using Mango.Services.OrderAPI.Utility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stripe;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Confifuguring the DbContext Service for Sql Connection with EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString"));
});

//// AutoMapper configuration
IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
// register the mapper to the services
builder.Services.AddSingleton(mapper);
// use automapper using dependency injection
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//register httpClient with base address
builder.Services.AddHttpClient("Product", u => u.BaseAddress =
new Uri(builder.Configuration["ServiceUrls:ProductAPI"])).AddHttpMessageHandler<BackendAPIAuthenticationHttpClientHandler>();


//Adding the prodcut service
builder.Services.AddScoped<IProductService, Mango.Services.OrderAPI.Service.ProductService>();

builder.Services.AddScoped<IMessageBus, MessageBus>();

/* Here we have added backendapi authentication handlers, that we are cretes, but when we are registering 
the services, at that point we need to add http message handlers
that will make sure that it will add the token, pass that for both product api and coupon api
*/
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<BackendAPIAuthenticationHttpClientHandler>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


// Enabling authentication in swagger generation
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(name: JwtBearerDefaults.AuthenticationScheme, securityScheme: new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter the Bearer Authorization string as following : `Bearer Generated-JWT-Token`",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {{
                new OpenApiSecurityScheme
        {
            Reference=  new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = JwtBearerDefaults.AuthenticationScheme
            }
        }, new string[]{}
        }
    });
});

builder.AddAppAuthentication();
builder.Services.AddAuthentication();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();

app.UseHttpsRedirection();

// If we are authenticating, we are validating 3 things, We have the secret, we have the issuer and we have the audience, we have to validate our token using all 3 of them
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

ApplyMigration();

app.Run();


// This method will run all the pending migrations, when the app gets started instead of using update-database
void ApplyMigration()
{

    using (var scope = app.Services.CreateScope())  // this line of code gets all the services
    {
        // Get application DBContext here and check if there are any pending migrations

        var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (_db.Database.GetPendingMigrations().Count() > 0)
        {
            _db.Database.Migrate();
        }
    }
}
