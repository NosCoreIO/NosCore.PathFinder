using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NosCore.PathFinder.Interfaces;

namespace NosCore.PathFinder.Gui.Models
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
    }

    public static class IMovableEntityExtension
    {
        public static Task MoveAsync(this IMovableEntity nonPlayableEntity, IDistanceCalculator distanceCalculator)
        {
            var time = (DateTime.Now - nonPlayableEntity.LastMove).TotalMilliseconds;

            if (!(time > RandomHelper.Instance.RandomNumber(400, 3200)))
            {
                return Task.CompletedTask;
            }

            var mapX = nonPlayableEntity.MapX;
            var mapY = nonPlayableEntity.MapY;
            if (!nonPlayableEntity.Map.GetFreePosition(ref mapX, ref mapY,
                (byte)RandomHelper.Instance.RandomNumber(0, 3),
                (byte)RandomHelper.Instance.RandomNumber(0, 3)))
            {
                return Task.CompletedTask;
            }

            var distance = (int)distanceCalculator.GetDistance(new MapCell { X = nonPlayableEntity.PositionX, Y = nonPlayableEntity.PositionY }, new MapCell { X = mapX, Y = mapY });
            var value = 1000d * distance / (2 * nonPlayableEntity.Speed);
            Observable.Timer(TimeSpan.FromMilliseconds(value))
                .Subscribe(
                    _ =>
                    {
                        nonPlayableEntity.PositionX = mapX;
                        nonPlayableEntity.PositionY = mapY;
                    });

            nonPlayableEntity.LastMove = DateTime.Now.AddMilliseconds(value);
            return Task.CompletedTask;
        }
    }
}