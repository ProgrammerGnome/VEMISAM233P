using Microsoft.EntityFrameworkCore;
using ProjectName.Models;

namespace ProjectName.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<ZH> ZH { get; set; } = null!;
        public DbSet<FeltoltottMegoldas> FeltoltottMegoldasok { get; set; } = null!;
        public DbSet<Hallgato> Hallgatok { get; set; } = null!;
        public DbSet<Prompt> PromptSablonok { get; set; } = null!;
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // PostgreSQL konvenció: minden táblanév kisbetűs
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                entityType.SetTableName(entityType.GetTableName()?.ToLowerInvariant());
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}