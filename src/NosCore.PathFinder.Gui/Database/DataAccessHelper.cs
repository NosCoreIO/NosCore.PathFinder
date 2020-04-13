//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using Microsoft.EntityFrameworkCore;
using NosCore.Dao.Interfaces;
using NosCore.PathFinder.Gui.I18N;
using Serilog;

namespace NosCore.PathFinder.Gui.Database
{
    public class DataAccessHelper : IDbContextBuilder
    {
        private static readonly ILogger Logger = Shared.I18N.Logger.GetLoggerConfiguration().CreateLogger();

        private DbContextOptions? _option;

        /// <summary>
        ///     Creates new instance of database context.
        /// </summary>
        public DbContext CreateContext()
        {
            return new NosCoreContext(_option);
        }

        public void Initialize(DbContextOptions option)
        {
            _option = option;
            using var context = CreateContext();
            try
            {
                context.Database.Migrate();
                context.Database.GetDbConnection().Open();
                Logger.Information(LogLanguage.Instance.GetMessageFromKey(LogLanguageKey.DATABASE_INITIALIZED));
            }
            catch (Exception ex)
            {
                Logger.Error("Database Error", ex);
                Logger.Error(LogLanguage.Instance.GetMessageFromKey(LogLanguageKey.DATABASE_NOT_UPTODATE));
                throw;
            }
        }
    }
}