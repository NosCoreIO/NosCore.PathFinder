//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NosCore.PathFinder.Brushfire;
using NosCore.PathFinder.Heuristic;

namespace NosCore.PathFinder.Tests
{
    [TestClass]
    public class FlowFieldTests
    {
        private readonly TestMap _map = TestHelper.SimpleMap;

        [TestMethod]
        public void Test_FlowField()
        {
            (short X, short Y) characterPosition = (6, 10);
            var brushFire = _map.LoadBrushFire(characterPosition, new OctileDistanceHeuristic());
            var flowField = brushFire.GetFlowField(_map);

            var bitmap = new Bitmap(_map.Width * TestHelper.Scale, _map.Height * TestHelper.Scale);
            var listPixel = new List<Color>();
            TestHelper.DrawMap(_map, TestHelper.Scale, listPixel, bitmap, (0, 0), characterPosition);
            using var graphics = Graphics.FromImage(bitmap);

            for (short y = 0; y < _map.Height; y++)
            {
                for (short x = 0; x < _map.Width; x++)
                {
                    var rectangle = new Rectangle(x * TestHelper.Scale, y * TestHelper.Scale, TestHelper.Scale, TestHelper.Scale);
                    if ((x, y) != characterPosition)
                    {
                        if (brushFire[x, y] != null)
                        {
                            graphics.FillRectangle(new Pen(Color.White).Brush, rectangle);
                            var color = Color.FromArgb((int)((brushFire[x, y] * 12 > 255 ? 255 : (brushFire[x, y] ?? 0) * 12)), 0, 0, 255);
                            graphics.FillRectangle(new Pen(color).Brush, rectangle);
                            listPixel.Add(color);

                            var vector = flowField[x, y];
                            if (vector != null)
                            {
                                TestHelper.DrawArrow(graphics, x, y, vector.Value.X, vector.Value.Y, TestHelper.Scale, Color.White);
                            }
                        }
                    }
                }
            }

            TestHelper.VerifyFile("flow-field.png", bitmap, listPixel, "Flow Field (Vector Field Pathfinding)");
        }

        [TestMethod]
        public void Test_FlowField_MonsterPath()
        {
            (short X, short Y) characterPosition = (6, 10);
            (short X, short Y) monsterPosition = (15, 16);

            var brushFire = _map.LoadBrushFire(characterPosition, new OctileDistanceHeuristic());
            var flowField = brushFire.GetFlowField(_map);

            var path = TraceFlowFieldPath(flowField, monsterPosition, characterPosition);

            var bitmap = new Bitmap(_map.Width * TestHelper.Scale, _map.Height * TestHelper.Scale);
            var listPixel = new List<Color>();
            TestHelper.DrawMap(_map, TestHelper.Scale, listPixel, bitmap, monsterPosition, characterPosition);
            using var graphics = Graphics.FromImage(bitmap);

            for (short y = 0; y < _map.Height; y++)
            {
                for (short x = 0; x < _map.Width; x++)
                {
                    var rectangle = new Rectangle(x * TestHelper.Scale, y * TestHelper.Scale, TestHelper.Scale, TestHelper.Scale);
                    if ((x, y) != characterPosition && (x, y) != monsterPosition)
                    {
                        if (brushFire[x, y] != null)
                        {
                            graphics.FillRectangle(new Pen(Color.White).Brush, rectangle);
                            var color = Color.FromArgb((int)((brushFire[x, y] * 12 > 255 ? 255 : (brushFire[x, y] ?? 0) * 12)), 0, 0, 255);
                            graphics.FillRectangle(new Pen(color).Brush, rectangle);
                            listPixel.Add(color);

                            var vector = flowField[x, y];
                            if (vector != null)
                            {
                                TestHelper.DrawArrow(graphics, x, y, vector.Value.X, vector.Value.Y, TestHelper.Scale, Color.White);
                            }
                        }
                    }
                }
            }

            var pathArray = path.ToArray();
            for (var i = 0; i < pathArray.Length; i++)
            {
                var (x, y) = pathArray[i];
                if ((x, y) != monsterPosition && (x, y) != characterPosition)
                {
                    var rectangle = new Rectangle(x * TestHelper.Scale, y * TestHelper.Scale, TestHelper.Scale, TestHelper.Scale);
                    var color = Color.LightPink;
                    graphics.FillRectangle(new Pen(color).Brush, rectangle);
                    graphics.DrawString(i.ToString(), new Font("Arial", 16), Brushes.Black, rectangle, TestHelper.StringFormat);
                    listPixel.Add(color);
                }
            }

            TestHelper.VerifyFile("flow-field-path.png", bitmap, listPixel, "Flow Field Path (Monster following vectors to Player)");
        }

        private static List<(short X, short Y)> TraceFlowFieldPath(FlowField flowField, (short X, short Y) start, (short X, short Y) target, int maxSteps = 100)
        {
            var path = new List<(short X, short Y)>();
            var current = start;

            for (var step = 0; step < maxSteps; step++)
            {
                var vector = flowField[current.X, current.Y];
                if (vector == null)
                    break;

                var nextX = (short)(current.X + Math.Sign(vector.Value.X));
                var nextY = (short)(current.Y + Math.Sign(vector.Value.Y));

                if ((nextX, nextY) == current)
                    break;

                current = (nextX, nextY);
                path.Add(current);

                if (current == target)
                    break;
            }

            return path;
        }
    }
}
