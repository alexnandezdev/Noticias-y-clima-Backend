using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WeatherNewsAPI.Models;

namespace WeatherNewsAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<WeatherNews> WeatherNews { get; set; }
        public DbSet<Weather> Weather { get; set; }
        public DbSet<News> News { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<WeatherNews>()
                .HasOne(wn => wn.Weather)
                .WithOne()
                .HasForeignKey<Weather>("WeatherNewsId");

            modelBuilder.Entity<WeatherNews>()
                .HasMany(wn => wn.News)
                .WithOne()
                .HasForeignKey("WeatherNewsId");
        }
    }
}