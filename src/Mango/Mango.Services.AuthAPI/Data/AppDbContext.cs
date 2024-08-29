
using Mango.Services.AuthAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.AuthAPI.Data
{

    /* Typically when you are working with Entity Framework core, we only needed DBContext,
     but here, we will be using dot net identity, To use Identity for authentication and authorization
    we need to implement AppDbContext from IdentityDbContext
    */
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        // As ApplicationUser extending from IdentityUser, it will add one more column Name to the ASPNetUsers
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        // this method is used to seed the record in a database table
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
             base.OnModelCreating(modelBuilder);
        }

    }
}
