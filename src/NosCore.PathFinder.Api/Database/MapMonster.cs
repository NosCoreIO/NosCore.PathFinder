//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NosCore.PathFinder.Api.Database;

public class MapMonster
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int MapMonsterId { get; set; }

    public short MapId { get; set; }

    public short MapX { get; set; }

    public short MapY { get; set; }

    public short VNum { get; set; }

    public virtual Map Map { get; set; } = null!;
}
