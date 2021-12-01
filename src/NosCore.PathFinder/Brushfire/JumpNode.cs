//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;

namespace NosCore.PathFinder.Brushfire
{
    public class JumpNode : Node
    {
        public JumpNode((short X, short Y) position, double? value) : base(position, value)
        {
        }

        public double G { get; set; }

        public double? H { get; set; }

        public bool Opened { get; set; }
    }
}