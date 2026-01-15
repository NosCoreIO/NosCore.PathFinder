//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NosCore.PathFinder.Api.Database;

public class Map
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public short MapId { get; set; }

    public byte[] Data { get; set; } = null!;

    public virtual ICollection<MapMonster> MapMonster { get; set; } = new HashSet<MapMonster>();

    public virtual ICollection<MapNpc> MapNpc { get; set; } = new HashSet<MapNpc>();
}
