//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NosCore.PathFinder.Gui.Database
{
    public class MapNpc
    {
        public virtual Map Map { get; set; } = null!;

        public short MapId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public int MapNpcId { get; set; }

        public short MapX { get; set; }

        public short MapY { get; set; }
    }
}
