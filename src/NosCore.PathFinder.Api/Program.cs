//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NosCore.PathFinder.Api;
using NosCore.PathFinder.Api.Database;
using NosCore.PathFinder.Brushfire;
using NosCore.PathFinder.Heuristic;
using NosCore.PathFinder.Interfaces;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
builder.Services.AddSingleton<HeuristicProvider>();
builder.Services.AddSingleton<PerformanceTracker>();

var connectionString = LoadConnectionString(builder.Configuration);
if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<PathFinderContext>(options =>
        options.UseNpgsql(connectionString));
    builder.Services.AddSingleton<MapStore>(sp =>
        new MapStore(sp, sp.GetRequiredService<ILogger<MapStore>>()));
    Console.WriteLine("Database configured - will load maps from PostgreSQL");
}
else
{
    builder.Services.AddSingleton<MapStore>();
    Console.WriteLine("No database configured - using sample maps only");
}

var app = builder.Build();

var mapStore = app.Services.GetRequiredService<MapStore>();
await mapStore.LoadFromDatabaseAsync();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseWebSockets();

var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

app.MapGet("/api/maps", (MapStore store) =>
{
    return Results.Ok(store.GetMapList());
});

app.MapGet("/api/maps/{mapId}", (int mapId, MapStore store) =>
{
    var map = store.GetMap(mapId);
    if (map == null) return Results.NotFound();

    byte[] gridData;
    if (map is DatabaseMap dbMap)
        gridData = dbMap.GetWalkabilityGrid();
    else if (map is SimpleMap simpleMap)
        gridData = simpleMap.GetWalkabilityGrid();
    else
        gridData = Array.Empty<byte>();

    var monsters = store.GetMonsters(mapId).Select(m => new { id = m.Id, x = m.X, y = m.Y, vNum = m.VNum, type = "monster" });
    var npcs = store.GetNpcs(mapId).Select(n => new { id = n.Id, x = n.X, y = n.Y, vNum = n.VNum, type = "npc" });

    return Results.Ok(new
    {
        width = map.Width,
        height = map.Height,
        grid = Convert.ToBase64String(gridData),
        entities = monsters.Concat(npcs)
    });
});

app.MapGet("/api/maps/{mapId}/entities", (int mapId, MapStore store) =>
{
    var monsters = store.GetMonsters(mapId).Select(m => new { m.Id, m.X, m.Y, m.VNum, Type = "monster" });
    var npcs = store.GetNpcs(mapId).Select(n => new { n.Id, n.X, n.Y, n.VNum, Type = "npc" });
    return Results.Ok(new { Monsters = monsters, Npcs = npcs });
});

app.MapGet("/api/maps/{mapId}/brushfire", (int mapId, short x, short y, short maxDistance, string? heuristic, MapStore store, HeuristicProvider heuristicProvider, PerformanceTracker perf) =>
{
    var map = store.GetMap(mapId);
    if (map == null) return Results.NotFound();

    var h = heuristicProvider.Get(heuristic);
    var sw = Stopwatch.StartNew();
    var brushfire = map.LoadBrushFire((x, y), h, maxDistance);
    sw.Stop();

    perf.RecordBrushfire(sw.Elapsed, brushfire.Distances.Count);

    var cells = brushfire.Distances
        .Select(kvp => new { X = kvp.Key.X, Y = kvp.Key.Y, Distance = kvp.Value })
        .ToList();

    return Results.Ok(new
    {
        Origin = new { x, y },
        Cells = cells,
        Performance = new { ElapsedMs = sw.Elapsed.TotalMilliseconds, CellCount = cells.Count }
    });
});

app.MapGet("/api/maps/{mapId}/flowfield", (int mapId, short x, short y, short maxDistance, double stopDistance, string? heuristic, MapStore store, HeuristicProvider heuristicProvider, PerformanceTracker perf) =>
{
    var map = store.GetMap(mapId);
    if (map == null) return Results.NotFound();

    var h = heuristicProvider.Get(heuristic);
    var sw = Stopwatch.StartNew();
    var brushfire = map.LoadBrushFire((x, y), h, maxDistance);
    var flowfield = brushfire.GetFlowField(map, stopDistance);
    sw.Stop();

    perf.RecordFlowField(sw.Elapsed, flowfield.Vectors.Count);

    var vectors = flowfield.Vectors
        .Select(kvp => new { X = kvp.Key.X, Y = kvp.Key.Y, Dx = kvp.Value.X, Dy = kvp.Value.Y })
        .ToList();

    return Results.Ok(new
    {
        Origin = new { x, y },
        Vectors = vectors,
        Performance = new { ElapsedMs = sw.Elapsed.TotalMilliseconds, VectorCount = vectors.Count }
    });
});

app.MapGet("/api/performance", (PerformanceTracker perf) =>
{
    return Results.Ok(perf.GetStats());
});

app.Map("/ws", async (HttpContext context, MapStore store, HeuristicProvider heuristicProvider, PerformanceTracker perf) =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        return;
    }

    using var ws = await context.WebSockets.AcceptWebSocketAsync();
    var buffer = new byte[4096];

    while (ws.State == WebSocketState.Open)
    {
        var result = await ws.ReceiveAsync(buffer, CancellationToken.None);
        if (result.MessageType == WebSocketMessageType.Close)
        {
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
            break;
        }

        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
        var request = JsonSerializer.Deserialize<WsRequest>(message, jsonOptions);

        if (request == null) continue;

        var map = store.GetMap(request.MapId);
        if (map == null) continue;

        var heuristic = heuristicProvider.Get(request.Heuristic);
        var sw = Stopwatch.StartNew();
        var brushfire = map.LoadBrushFire((request.X, request.Y), heuristic, request.MaxDistance);
        var flowfield = brushfire.GetFlowField(map, request.StopDistance);
        sw.Stop();

        perf.RecordFlowField(sw.Elapsed, flowfield.Vectors.Count);

        var monsters = store.GetMonsters(request.MapId);
        var npcs = store.GetNpcs(request.MapId);

        var response = new
        {
            Type = "flowfield",
            Origin = new { request.X, request.Y },
            Vectors = flowfield.Vectors.Select(kvp => new { X = kvp.Key.X, Y = kvp.Key.Y, Dx = kvp.Value.X, Dy = kvp.Value.Y }),
            Distances = brushfire.Distances.Select(kvp => new { X = kvp.Key.X, Y = kvp.Key.Y, D = kvp.Value }),
            Monsters = monsters.Select(m => new { m.X, m.Y }),
            Npcs = npcs.Select(n => new { n.X, n.Y }),
            Performance = new { ElapsedMs = sw.Elapsed.TotalMilliseconds, VectorCount = flowfield.Vectors.Count }
        };

        var responseJson = JsonSerializer.Serialize(response, jsonOptions);
        var responseBytes = Encoding.UTF8.GetBytes(responseJson);
        await ws.SendAsync(responseBytes, WebSocketMessageType.Text, true, CancellationToken.None);
    }
});

app.Run();

static string? LoadConnectionString(IConfiguration configuration)
{
    var baseDir = AppContext.BaseDirectory;
    var currentDir = Directory.GetCurrentDirectory();

    var yamlPaths = new[]
    {
        Path.Combine(baseDir, "..", "..", "configuration", "pathfinder.yml"),
        Path.Combine(baseDir, "configuration", "pathfinder.yml"),
        Path.Combine(currentDir, "configuration", "pathfinder.yml"),
        Path.Combine(currentDir, "..", "..", "configuration", "pathfinder.yml"),
        @"C:\dev\NosCore.PathFinder\configuration\pathfinder.yml"
    };

    Console.WriteLine($"Base directory: {baseDir}");
    Console.WriteLine($"Current directory: {currentDir}");
    Console.WriteLine("Searching for pathfinder.yml...");

    foreach (var yamlPath in yamlPaths)
    {
        var fullPath = Path.GetFullPath(yamlPath);
        Console.WriteLine($"  Checking: {fullPath}");
        if (File.Exists(fullPath))
        {
            try
            {
                var yaml = File.ReadAllText(fullPath);
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(PascalCaseNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();
                var config = deserializer.Deserialize<PathfinderConfig>(yaml);
                if (config?.Database != null)
                {
                    var db = config.Database;
                    var connStr = $"Host={db.Host};Port={db.Port};Database={db.Database};Username={db.Username};Password={db.Password}";
                    Console.WriteLine($"Loaded database config from: {fullPath}");
                    return connStr;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse {fullPath}: {ex.Message}");
            }
        }
    }

    var jsonConnStr = configuration.GetConnectionString("NosCore");
    if (!string.IsNullOrEmpty(jsonConnStr))
    {
        Console.WriteLine("Using connection string from appsettings.json");
        return jsonConnStr;
    }

    Console.WriteLine("No database configuration found");
    return null;
}

record WsRequest(int MapId, short X, short Y, short MaxDistance = 22, double StopDistance = 0, string? Heuristic = null);

class HeuristicProvider
{
    private readonly Dictionary<string, IHeuristic> _heuristics = new(StringComparer.OrdinalIgnoreCase)
    {
        ["octile"] = new OctileDistanceHeuristic(),
        ["manhattan"] = new ManhattanDistanceHeuristic(),
        ["euclidean"] = new EuclideanDistanceHeuristic(),
        ["chebyshev"] = new ChebyshevDistanceHeuristic()
    };

    public IHeuristic Get(string? name) =>
        string.IsNullOrEmpty(name) || !_heuristics.TryGetValue(name, out var h)
            ? _heuristics["octile"]
            : h;

    public IEnumerable<string> GetNames() => _heuristics.Keys;
}

class PathfinderConfig
{
    public string? Language { get; set; }
    public DatabaseConfig? Database { get; set; }
}

class DatabaseConfig
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5432;
    public string Database { get; set; } = "postgres";
    public string Username { get; set; } = "postgres";
    public string Password { get; set; } = "";
}
