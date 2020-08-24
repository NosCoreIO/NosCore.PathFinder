//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NosCore.PathFinder.Gui.Dtos;
using NosCore.PathFinder.Interfaces;
using NosCore.Shared.Enumerations;
using NosCore.Shared.Helpers;

namespace NosCore.PathFinder.Gui.GuiObject
{
    public interface IMovableEntity
    {
        DateTime LastMove { get; set; }

        short MapX { get; set; }

        short MapY { get; set; }

        short PositionX { get; set; }

        short PositionY { get; set; }

        int Speed { get; set; }

        MapDto Map { get; set; }

        long? TargetVisualId { get; set; }

        public VisualType? TargetVisualType { get; set; }
    }

    public static class IMovableEntityExtension
    {
        public static async Task MoveAsync(this IMovableEntity nonPlayableEntity, IHeuristic distanceCalculator)
        {
            var time = (DateTime.Now - nonPlayableEntity.LastMove).TotalMilliseconds;
            var mapX = nonPlayableEntity.MapX;
            var mapY = nonPlayableEntity.MapY;
            if (!nonPlayableEntity.Map.GetFreePosition(ref mapX, ref mapY,
                (byte)RandomHelper.Instance.RandomNumber(0, 3),
                (byte)RandomHelper.Instance.RandomNumber(0, 3)))
            {
                return;
            }

            if (nonPlayableEntity.TargetVisualId == null)
            {
                if (time < RandomHelper.Instance.RandomNumber(0, 2500))
                {
                    return;
                }
            }
            else
            {
                //if target arround
                //todo pathfind
                //else startpolling
                //    on exit reset target, go back
            }

            var distance = (int)distanceCalculator.GetDistance((nonPlayableEntity.PositionX, nonPlayableEntity.PositionY), (mapX, mapY));
            var value = 1000d * distance / (2 * nonPlayableEntity.Speed);
            await Task.Delay((int)value);
            nonPlayableEntity.PositionX = mapX;
            nonPlayableEntity.PositionY = mapY;
            nonPlayableEntity.LastMove = DateTime.Now.AddMilliseconds(value);
        }
    }
}