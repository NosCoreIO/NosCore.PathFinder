﻿//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NosCore.PathFinder.Gui.Configuration;
using NosCore.PathFinder.Gui.Database;
using NosCore.PathFinder.Gui.I18N;
using NosCore.Dao;
using NosCore.PathFinder.Gui.Dtos;
using NosCore.Shared.Configuration;

namespace NosCore.PathFinder.Gui
{
    public static class PathFinderGui
    {
        private const string Title = "NosCore - Pathfinder GUI";
        private const string ConsoleText = "PATHFINDER GUI - NosCoreIO";
        private static readonly PathfinderGuiConfiguration PathfinderGuiConfiguration = new PathfinderGuiConfiguration();
        private static readonly Dictionary<short, GuiWindow?> GuiWindows = new Dictionary<short, GuiWindow?>();
        private static readonly DataAccessHelper DbContextBuilder = new DataAccessHelper();
   
        public static async Task Main(string[] args)
        {
            try { Console.Title = Title; } catch (PlatformNotSupportedException) { }
            ConfiguratorBuilder.InitializeConfiguration(args, new[] { "pathfinder.yml", "logger.yml" }).Bind(PathfinderGuiConfiguration);
            Shared.I18N.Logger.PrintHeader(ConsoleText);
            var logger = Shared.I18N.Logger.GetLoggerConfiguration().CreateLogger();
            LogLanguage.Language = PathfinderGuiConfiguration.Language;
            var optionsBuilder = new DbContextOptionsBuilder<NosCoreContext>();
            optionsBuilder.UseNpgsql(PathfinderGuiConfiguration.Database!.ConnectionString);
            DbContextBuilder.Initialize(optionsBuilder.Options);
            var  mapDao = new Dao<Map, MapDto, short>(logger, DbContextBuilder.CreateContext);

            while (true)
            {
                logger.Information(LogLanguage.Instance.GetMessageFromKey(LogLanguageKey.SELECT_MAPID));
                var input = Console.ReadLine();
                if ((input == null) || !short.TryParse(input, out var askMapId))
                {
                    logger.Error(LogLanguage.Instance.GetMessageFromKey(LogLanguageKey.WRONG_SELECTED_MAPID));
                    continue;
                }
                var map = await mapDao.FirstOrDefaultAsync(m => m.MapId == askMapId).ConfigureAwait(false);

                if ((!(map?.Width > 0)) || (map.Height <= 0))
                {
                    continue;
                }

                if (GuiWindows.ContainsKey(map.MapId) && GuiWindows[map.MapId]!.Exists)
                {
                    GuiWindows[map.MapId]!.Close();
                }

                GuiWindows[map.MapId] = new GuiWindow(map,1024, 768,
                    $"NosCore Pathfinder GUI - Map {map.MapId}", DbContextBuilder);
                GuiWindows[map.MapId]!.Run(30);
            }
        }
    }
}