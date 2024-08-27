using Mango.Services.AuthAPI.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Confifuguring the DbContext Service for Sql Connection with EF Core
builder.Services.AddDbContext<AppDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString(""));
});

/*
 * Note : Here, we need to tell the EF Core that we will be using .Net Identity and .EF Core
 * where we have the DBContext with .Net Identity
 * That is how it will know to use DBContext to create all the Identity related tables
 */

// Using Default user and default role , AddEntityFrameworkStores -> acts as a bridge b/w
// dot net Identity core and Entity Framework core
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
    
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// As AuthAPI is responsible for Authentication and Authorization, include UseAuthenticatuin()
// Note : In the pipeline, Authentication() must always come before Authorization()
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
