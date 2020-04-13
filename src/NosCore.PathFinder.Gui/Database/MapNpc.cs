using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

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
