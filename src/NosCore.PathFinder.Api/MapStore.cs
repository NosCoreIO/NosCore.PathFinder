//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using Microsoft.EntityFrameworkCore;
using NosCore.PathFinder.Api.Database;
using NosCore.PathFinder.Interfaces;

namespace NosCore.PathFinder.Api;

public class MapStore
{
    private readonly Dictionary<int, IMapGrid> _sampleMaps = new();
    private readonly Dictionary<int, IMapGrid> _databaseMaps = new();
    private readonly Dictionary<int, List<EntityPosition>> _monsters = new();
    private readonly Dictionary<int, List<EntityPosition>> _npcs = new();
    private readonly IServiceProvider? _serviceProvider;
    private readonly ILogger<MapStore>? _logger;
    private bool _databaseLoaded;

    public MapStore()
    {
        InitializeSampleMaps();
    }

    public MapStore(IServiceProvider serviceProvider, ILogger<MapStore> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        InitializeSampleMaps();
    }

    private void InitializeSampleMaps()
    {
        _sampleMaps[-1] = new SimpleMap(new byte[][]
        {
            new byte[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}
        });

        _sampleMaps[-2] = new SimpleMap(GenerateMaze(51, 51));
    }

    public async Task LoadFromDatabaseAsync()
    {
        if (_serviceProvider == null || _databaseLoaded) return;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetService<PathFinderContext>();
            if (context == null) return;

            var maps = await context.Map.ToListAsync();
            foreach (var map in maps)
            {
                try
                {
                    var dbMap = new DatabaseMap(map.Data);
                    if (dbMap.Width > 0 && dbMap.Height > 0)
                    {
                        _databaseMaps[map.MapId] = dbMap;
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Failed to load map {MapId}", map.MapId);
                }
            }

            var monsters = await context.MapMonster.ToListAsync();
            foreach (var monster in monsters)
            {
                if (!_monsters.ContainsKey(monster.MapId))
                    _monsters[monster.MapId] = new List<EntityPosition>();
                _monsters[monster.MapId].Add(new EntityPosition(monster.MapMonsterId, monster.MapX, monster.MapY, monster.VNum));
            }

            var npcs = await context.MapNpc.ToListAsync();
            foreach (var npc in npcs)
            {
                if (!_npcs.ContainsKey(npc.MapId))
                    _npcs[npc.MapId] = new List<EntityPosition>();
                _npcs[npc.MapId].Add(new EntityPosition(npc.MapNpcId, npc.MapX, npc.MapY, npc.VNum));
            }

            _databaseLoaded = true;
            _logger?.LogInformation("Loaded {MapCount} maps, {MonsterCount} monsters, {NpcCount} NPCs from database",
                _databaseMaps.Count, monsters.Count, npcs.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to connect to database - using sample maps only");
        }
    }

    public IMapGrid? GetMap(int mapId)
    {
        if (_databaseMaps.TryGetValue(mapId, out var dbMap))
            return dbMap;
        return _sampleMaps.GetValueOrDefault(mapId);
    }

    public List<EntityPosition> GetMonsters(int mapId) => _monsters.GetValueOrDefault(mapId) ?? new List<EntityPosition>();

    public List<EntityPosition> GetNpcs(int mapId) => _npcs.GetValueOrDefault(mapId) ?? new List<EntityPosition>();

    public IEnumerable<object> GetMapList()
    {
        var result = new List<object>();

        foreach (var (id, map) in _databaseMaps)
        {
            result.Add(new { Id = id, map.Width, map.Height, Source = "database" });
        }

        foreach (var (id, map) in _sampleMaps)
        {
            result.Add(new { Id = id, map.Width, map.Height, Source = "sample" });
        }

        return result.OrderBy(m => ((dynamic)m).Id);
    }

    public bool HasDatabaseMaps => _databaseMaps.Count > 0;

    private static byte[][] GenerateMaze(int width, int height)
    {
        var maze = new byte[height][];
        var random = new Random(42);

        for (var y = 0; y < height; y++)
        {
            maze[y] = new byte[width];
            for (var x = 0; x < width; x++)
            {
                if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                {
                    maze[y][x] = 1;
                }
                else if (x % 4 == 0 && y % 2 == 0 && random.NextDouble() > 0.3)
                {
                    maze[y][x] = 1;
                }
                else if (y % 4 == 0 && x % 2 == 0 && random.NextDouble() > 0.3)
                {
                    maze[y][x] = 1;
                }
                else
                {
                    maze[y][x] = 0;
                }
            }
        }

        return maze;
    }
}

public record EntityPosition(int Id, short X, short Y, short VNum);

public class SimpleMap : IMapGrid
{
    private readonly byte[][] _data;
    private byte[]? _walkabilityGrid;

    public SimpleMap(byte[][] data)
    {
        _data = data;
        Height = (short)data.Length;
        Width = (short)data[0].Length;
    }

    public short Width { get; }
    public short Height { get; }

    public byte this[short x, short y] => _data[y][x];

    public bool IsWalkable(short x, short y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return false;
        return _data[y][x] == 0;
    }

    public byte[] GetWalkabilityGrid()
    {
        if (_walkabilityGrid != null) return _walkabilityGrid;

        var size = Width * Height;
        var bytesNeeded = (size + 7) / 8;
        _walkabilityGrid = new byte[bytesNeeded];

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var idx = y * Width + x;
                if (!IsWalkable((short)x, (short)y))
                {
                    _walkabilityGrid[idx / 8] |= (byte)(1 << (idx % 8));
                }
            }
        }

        return _walkabilityGrid;
    }
}

public class DatabaseMap : IMapGrid
{
    private readonly byte[] _data;
    private readonly short _width;
    private readonly short _height;
    private byte[]? _walkabilityGrid;

    public DatabaseMap(byte[] data)
    {
        _data = data;
        _width = BitConverter.ToInt16(data, 0);
        _height = BitConverter.ToInt16(data, 2);
    }

    public short Width => _width;
    public short Height => _height;

    public byte this[short x, short y] => _data[4 + y * _width + x];

    public bool IsWalkable(short x, short y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return false;
        var value = this[x, y];
        return value == 0 || value == 2 || (value >= 16 && value <= 19);
    }

    public byte[] GetWalkabilityGrid()
    {
        if (_walkabilityGrid != null) return _walkabilityGrid;

        var size = _width * _height;
        var bytesNeeded = (size + 7) / 8;
        _walkabilityGrid = new byte[bytesNeeded];

        for (var y = 0; y < _height; y++)
        {
            for (var x = 0; x < _width; x++)
            {
                var idx = y * _width + x;
                if (!IsWalkable((short)x, (short)y))
                {
                    _walkabilityGrid[idx / 8] |= (byte)(1 << (idx % 8));
                }
            }
        }

        return _walkabilityGrid;
    }
}
