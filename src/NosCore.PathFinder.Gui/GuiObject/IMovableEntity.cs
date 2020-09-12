//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NosCore.PathFinder.Brushfire;
using NosCore.PathFinder.Gui.Dtos;
using NosCore.PathFinder.Interfaces;
using NosCore.PathFinder.Pathfinder;
using NosCore.Shared.Enumerations;
using NosCore.Shared.Helpers;

namespace NosCore.PathFinder.Gui.GuiObject
{
    public interface IMovableEntity : IAliveEntity
    {
        public const int RefreshRate = 200;
        DateTime NextMove { get; set; }
    }

    public interface IAliveEntity : IVisualEntity
    {
        long VisualId { get; }

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

    public static class MovableEntityExtension
    {
        public static async Task<int> MoveAsync(this IMovableEntity nonPlayableEntity, IHeuristic distanceCalculator)
        {
            var cellPerSec = 2.5 * nonPlayableEntity.Speed;
            var mapX = nonPlayableEntity.PositionX;
            var mapY = nonPlayableEntity.PositionY;

            if (nonPlayableEntity.TargetVisualId == null && nonPlayableEntity.TargetVisualType != VisualType.Map)
            {
                nonPlayableEntity.NextMove = DateTime.Now.AddMilliseconds(RandomHelper.Instance.RandomNumber(IMovableEntity.RefreshRate, 2500 + IMovableEntity.RefreshRate));
                if (!nonPlayableEntity.Map.GetFreePosition(ref mapX, ref mapY,
                    (byte)RandomHelper.Instance.RandomNumber(0, 3),
                    (byte)RandomHelper.Instance.RandomNumber(0, 3)))
                {
                    return 0;
                }
            }
            else
            {
                IPathfinder pathfinder = new JumpPointSearchPathfinder(nonPlayableEntity.Map, distanceCalculator);
                var target = nonPlayableEntity.Map.Players.FirstOrDefault(s => s.VisualId == nonPlayableEntity.TargetVisualId);
                List<(short X, short Y)>? path = null;
                if (target != null && distanceCalculator.GetDistance((target.PositionX, target.PositionY), (nonPlayableEntity.PositionX, nonPlayableEntity.PositionY)) < 20)
                {
                    path = pathfinder.FindPath((nonPlayableEntity.PositionX, nonPlayableEntity.PositionY),
                        (target.PositionX, target.PositionY)).ToList();

                    if (path.LastOrDefault() != (target.PositionX, target.PositionY))
                    {
                        var goalPathFinder = new GoalBasedPathfinder(nonPlayableEntity.Map, distanceCalculator);
                        path = goalPathFinder.FindPath((nonPlayableEntity.PositionX, nonPlayableEntity.PositionY),
                            (target.PositionX, target.PositionY)).ToList();
                    }
                }
                else if (nonPlayableEntity.TargetVisualType != VisualType.Map)
                {
                    var targetFound = false;
                    for (var i = 0; i < 10; i++)
                    {
                        target = nonPlayableEntity.Map.Players.FirstOrDefault(s => s.VisualId == nonPlayableEntity.TargetVisualId);
                        if (target != null && distanceCalculator.GetDistance((target.PositionX, target.PositionY),
                            (nonPlayableEntity.PositionX, nonPlayableEntity.PositionY)) < 20)
                        {
                            targetFound = true;
                            break;
                        }
                        await Task.Delay(500);
                    }

                    if (targetFound == false)
                    {

                        nonPlayableEntity.TargetVisualType = (nonPlayableEntity.MapX, nonPlayableEntity.MapY) !=
                                                             (nonPlayableEntity.PositionX, nonPlayableEntity.PositionY) ? VisualType.Map : (VisualType?)null;

                        nonPlayableEntity.TargetVisualId = null;
                    }
                }
                else
                {
                    path = pathfinder.FindPath((nonPlayableEntity.PositionX, nonPlayableEntity.PositionY),
                        (nonPlayableEntity.MapX, nonPlayableEntity.MapY)).ToList();

                    if (path.Count <= cellPerSec && path.LastOrDefault() == (nonPlayableEntity.MapX, nonPlayableEntity.MapY))
                    {
                        nonPlayableEntity.TargetVisualType = null;
                    }
                }


                if (path?.Count > 1)
                {
                    var refreshRate = TimeSpan.FromMilliseconds(IMovableEntity.RefreshRate).TotalSeconds;
                    var cellPerRefresh = (int)(cellPerSec * refreshRate);
                    var (x, y) = path.Count > cellPerRefresh ? path.Skip(cellPerRefresh).First() : path.SkipLast(1).Last();
                    mapX = x;
                    mapY = y;
                }
            }

            var distance = distanceCalculator.GetDistance((nonPlayableEntity.PositionX, nonPlayableEntity.PositionY), (mapX, mapY));
            nonPlayableEntity.NextMove = DateTime.Now.AddMilliseconds(distance / cellPerSec);
            await Task.Delay(TimeSpan.FromSeconds(distance / cellPerSec));
            nonPlayableEntity.PositionX = mapX;
            nonPlayableEntity.PositionY = mapY;
            return (int)TimeSpan.FromSeconds(distance / cellPerSec).TotalMilliseconds;
        }
    }
}