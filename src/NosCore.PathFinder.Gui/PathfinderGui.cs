//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// 
// Copyright (C) 2019 - NosCore
// 
// NosCore is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using OpenToolkit.Graphics.ES30;
using OpenToolkit.Windowing.GraphicsLibraryFramework;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NosCore.Dao.Interfaces;
using NosCore.PathFinder.Gui.Configuration;
using NosCore.PathFinder.Gui.Database;
using NosCore.PathFinder.Gui.I18N;
using NosCore.PathFinder.Gui.Models;
using Serilog;
using NosCore.Dao;

namespace NosCore.PathFinder.Gui
{
    public static class PathFinderGui
    {
        private const string ConfigurationPath = "\\Configuration";
        private const string Title = "NosCore - Pathfinder GUI";
        private const string ConsoleText = "PATHFINDER GUI - NosCoreIO";
        private static readonly PathfinderGuiConfiguration PathfinderGuiConfiguration = new PathfinderGuiConfiguration();
        private static readonly Dictionary<short, GuiWindow?> GuiWindows = new Dictionary<short, GuiWindow?>();
        private static readonly ILogger Logger = Shared.I18N.Logger.GetLoggerConfiguration().CreateLogger();
        private static readonly DataAccessHelper DbContextBuilder = new DataAccessHelper();
        private static IDao<MapDto, short> _mapDao = null!;

        private static void InitializeConfiguration()
        {
            var builder = new ConfigurationBuilder();
            builder
                .SetBasePath(Directory.GetCurrentDirectory() + ConfigurationPath)
                .AddYamlFile("pathfinder.yml", false)
                .Build()
                .Bind(PathfinderGuiConfiguration);
        }

        public static async Task Main()
        {
            try { Console.Title = Title; } catch (PlatformNotSupportedException) { }
            InitializeConfiguration();
            Shared.I18N.Logger.SetLoggerConfiguration(new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory() + ConfigurationPath)
                .AddYamlFile("logger.yml", false).Build());
            Shared.I18N.Logger.PrintHeader(ConsoleText);


            LogLanguage.Language = PathfinderGuiConfiguration.Language;
            var optionsBuilder = new DbContextOptionsBuilder<NosCoreContext>();
            optionsBuilder.UseNpgsql(PathfinderGuiConfiguration.Database!.ConnectionString);
            DbContextBuilder.Initialize(optionsBuilder.Options);
            _mapDao = new Dao<Map, MapDto, short>(Logger, DbContextBuilder);
            var distanceCalculator = new OctileDistanceCalculator();

            while (true)
            {
                Logger.Information(LogLanguage.Instance.GetMessageFromKey(LogLanguageKey.SELECT_MAPID));
                var input = Console.ReadLine();
                if ((input == null) || !int.TryParse(input, out var askMapId))
                {
                    Logger.Error(LogLanguage.Instance.GetMessageFromKey(LogLanguageKey.WRONG_SELECTED_MAPID));
                    continue;
                }

                var map = await _mapDao.FirstOrDefaultAsync(m => m.MapId == askMapId).ConfigureAwait(false);

                if ((!(map?.XLength > 0)) || (map.YLength <= 0))
                {
                    continue;
                }

                if (GuiWindows.ContainsKey(map.MapId) && GuiWindows[map.MapId]!.Exists)
                {
                    GuiWindows[map.MapId]!.Close();
                }

                GL.LoadBindings(new GLFWBindingsContext());
                GuiWindows[map.MapId] = new GuiWindow(map, 4, map.XLength, map.YLength,
                    $"NosCore Pathfinder GUI - Map {map.MapId}", DbContextBuilder);
                GuiWindows[map.MapId]!.Run();
            }
        }
    }
}