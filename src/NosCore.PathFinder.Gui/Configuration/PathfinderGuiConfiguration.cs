//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.ComponentModel.DataAnnotations;
using NosCore.Shared.Configuration;

namespace NosCore.PathFinder.Gui.Configuration
{
    public class PathfinderGuiConfiguration : LanguageConfiguration
    {
        [Required]
        public SqlConnectionConfiguration? Database { get; set; }
    }
}