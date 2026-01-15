//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using Microsoft.EntityFrameworkCore;

namespace NosCore.PathFinder.Api.Database;

public class PathFinderContext : DbContext
{
    public PathFinderContext(DbContextOptions<PathFinderContext> options) : base(options)
    {
    }

    public virtual DbSet<Map> Map { get; set; } = null!;
    public virtual DbSet<MapMonster> MapMonster { get; set; } = null!;
    public virtual DbSet<MapNpc> MapNpc { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Map>().ToTable("Map");
        modelBuilder.Entity<MapMonster>().ToTable("MapMonster");
        modelBuilder.Entity<MapNpc>().ToTable("MapNpc");

        modelBuilder.Entity<Map>()
            .HasMany(e => e.MapMonster)
            .WithOne(e => e.Map)
            .HasForeignKey(e => e.MapId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Map>()
            .HasMany(e => e.MapNpc)
            .WithOne(e => e.Map)
            .HasForeignKey(e => e.MapId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
