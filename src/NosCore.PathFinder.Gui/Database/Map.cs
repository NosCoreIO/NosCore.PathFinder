using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace NosCore.PathFinder.Gui.Database
{
    public class Map
    {
        public virtual ICollection<MapMonster> MapMonster { get; set; }

        public virtual ICollection<MapNpc> MapNpc { get; set; }

        public Map()
        {
            MapMonster = new HashSet<MapMonster>();
            MapNpc = new HashSet<MapNpc>();
        }

        public byte[] Data { get; set; } = null!;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short MapId { get; set; }
    }
}
