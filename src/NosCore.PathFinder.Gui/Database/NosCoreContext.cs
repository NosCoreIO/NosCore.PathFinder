//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using Microsoft.EntityFrameworkCore;

namespace NosCore.PathFinder.Gui.Database
{
    public class NosCoreContext : DbContext
    {
        public NosCoreContext(DbContextOptions? options) : base(options)
        {
        }

        public virtual DbSet<Map>? Map { get; set; } = null!;

        public virtual DbSet<MapMonster>? MapMonster { get; set; } = null!;

        public virtual DbSet<MapNpc>? MapNpc { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // remove automatic pluralization
            modelBuilder.RemovePluralizingTableNameConvention();

            modelBuilder.Entity<Map>()
                .HasMany(e => e.MapMonster)
                .WithOne(e => e.Map)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Map>()
                .HasMany(e => e.MapMonster)
                .WithOne(e => e.Map)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}