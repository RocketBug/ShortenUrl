using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ShortenUrl.Models
{
    class ApiDbContext : DbContext
    {
        public virtual DbSet<Url> Urls { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
        {

        }
    }
}
