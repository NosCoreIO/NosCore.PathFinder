//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Linq;
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
    public interface IMovableEntity : IAliveEntity
    {
        DateTime LastMove { get; set; }
    }

    public interface IAliveEntity : IVisualEntity
    {
        short MapX { get; set; }

        short MapY { get; set; }

        int Speed { get; set; }

        long? TargetVisualId { get; set; }

        public VisualType? TargetVisualType { get; set; }
    }

    public interface IVisualEntity
    {
        short PositionX { get; set; }

        short PositionY { get; set; }

        MapDto Map { get; set; }
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
                var target = nonPlayableEntity.Map.Players.FirstOrDefault(s=>s.TargetVisualId == nonPlayableEntity.TargetVisualId);
                if (target != null && distanceCalculator.GetDistance((target.PositionX, target.PositionY), (nonPlayableEntity.PositionX, nonPlayableEntity.PositionY)) < 10)
                {
                    //todo pathfind
                }
                else
                {
                    var targetFound = false;
                    for (var i = 0; i < 10; i++)
                    {
                        //todo search the target or any target (npc / player) if aggresive
                        await Task.Delay(500);
                    }

                    if (targetFound == false)
                    {
                        nonPlayableEntity.TargetVisualType = null;
                        nonPlayableEntity.TargetVisualId = null;
                        //todo go to initial position
                    }
                }
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