//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using NosCore.PathFinder.Gui.Database;
using Serilog;
using NosCore.Dao;
using NosCore.PathFinder.Brushfire;
using NosCore.PathFinder.Gui.Dtos;
using NosCore.PathFinder.Gui.GuiObject;
using NosCore.PathFinder.Heuristic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Color = OpenTK.Color;

namespace NosCore.PathFinder.Gui
{
    public class GuiWindow : GameWindow
    {
        private static readonly ILogger Logger = NosCore.Shared.I18N.Logger.GetLoggerConfiguration().CreateLogger();
        private readonly MapDto _map;

        private readonly List<MapMonsterGo> _monsters;
        private readonly List<MapNpcGo> _npcs;
        private readonly int _originalCellSize;

        private float _cellSize;
        private readonly CharacterGo _mouseCharacter;
        private readonly int _originalWidth;

        private readonly Vector2[] _wallPixels;
        private readonly (int[] First, int[] Count, int ShapeCount) _wallShapeCount;
        private readonly (int[] First, int[] Count, int ShapeCount) _monstersShapeCount;
        private readonly (int[] First, int[] Count, int ShapeCount) _npcsShapeCount;

        private int _vertexBufferObject;
        private Dictionary<Color, Vector2[]>? _brushFirePixels;

        public GuiWindow(MapDto map, int width, int height, string title, DataAccessHelper dbContextBuilder)
            : base(width, (height < width / map.XLength * map.YLength) ? width / map.XLength * map.YLength : height,
                GraphicsMode.Default, title)
        {
            var dbContextBuilder1 = dbContextBuilder;
            var mapMonsterDao = new Dao<MapMonster, MapMonsterDto, int>(Logger, dbContextBuilder1);
            var mapNpcDao = new Dao<MapNpc, MapNpcDto, int>(Logger, dbContextBuilder1);
            _originalWidth = Width;
            _originalCellSize = Width / map.XLength;
            _cellSize = Width / map.XLength;

            _monsters = mapMonsterDao.Where(s => s.MapId == map.MapId)?.Adapt<List<MapMonsterGo>>() ??
                        new List<MapMonsterGo>();
            _map = map;
            _mouseCharacter = new CharacterGo();
            foreach (var mapMonster in _monsters)
            {
                mapMonster.PositionX = mapMonster.MapX;
                mapMonster.PositionY = mapMonster.MapY;
                mapMonster.Speed = 10;
                mapMonster.Map = _map;
            }

            _npcs = mapNpcDao.Where(s => s.MapId == map.MapId)?.Adapt<List<MapNpcGo>>() ?? new List<MapNpcGo>();
            foreach (var mapNpc in _npcs)
            {
                mapNpc.PositionX = mapNpc.MapX;
                mapNpc.PositionY = mapNpc.MapY;
                mapNpc.Speed = 10;
                mapNpc.Map = _map;
            }

            _npcsShapeCount = GetStartCount(_npcs.Count * 36, 36);
            _monstersShapeCount = GetStartCount(_monsters.Count * 36, 36);
            Parallel.ForEach(_monsters.Where(s => s.Life == null), monster => monster.StartLife());
            Parallel.ForEach(_npcs.Where(s => s.Life == null), npc => npc.StartLife());

            var wallpixels = new List<Vector2[]>();
            for (short y = 0; y < _map.YLength; y++)
            {
                for (short x = 0; x < _map.XLength; x++)
                {
                    if (_map[x, y] > 0)
                    {
                        wallpixels.Add(GenerateSquare(x, y));
                    }
                }
            }

            _wallPixels = wallpixels.SelectMany(s => s).ToArray();
            _wallShapeCount = GetStartCount(_wallPixels.Length, 4);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            var keyboardState = new KeyboardState();
            var lastKeyboardState = new KeyboardState();

            bool KeyPress(Key key)
            {
                return (keyboardState[key] && (keyboardState[key] != lastKeyboardState[key]));
            }

            if (KeyPress(Key.Escape))
            {
                Close();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            _cellSize = (float)_originalCellSize * Width / _originalWidth;
            GL.Viewport(0, 0, Width, Height);
            base.OnResize(e);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);

            var mapX = (short)(e.X / _cellSize);
            var mapY = (short)(e.Y / _cellSize);

            if (mapX != _mouseCharacter.MapX || _mouseCharacter.MapY != mapY)
            {
                _mouseCharacter.MapX = mapX;
                _mouseCharacter.MapY = mapY;
                _mouseCharacter.BrushFire = _map.LoadBrushFire((_mouseCharacter.MapX, _mouseCharacter.MapY),
                    new OctileDistanceHeuristic());

                _brushFirePixels = _mouseCharacter.BrushFire?.Grid.Values.Where(s => s?.Value != null).GroupBy(s => (int)s!.Value!)
                    .ToDictionary(s =>
                    {
                        var alpha = 255 - (s.Key * 10);
                        if (alpha < 0)
                        {
                            alpha = 0;
                        }
                        return Color.FromArgb((int)(alpha), 0, 255, 0);
                    }, s => s!.ToList().SelectMany(s => GenerateSquare(s!.Position.X, s.Position.Y)).ToArray());
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GL.ClearColor(Color.LightSkyBlue.A, Color.LightSkyBlue.R, Color.LightSkyBlue.G, Color.LightSkyBlue.B);
            var world = Matrix4.CreateOrthographicOffCenter(0, ClientRectangle.Width, ClientRectangle.Height, 0, 0, 1);
            GL.LoadMatrix(ref world);//deprecated
            GL.EnableVertexAttribArray(0);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            _vertexBufferObject = GL.GenBuffer();
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(_vertexBufferObject);
            base.OnUnload(e);
        }

        private (int[] First, int[] Count, int ShapeCount) GetStartCount(int vectorlength, int shapeSize)
        {
            var shapeCount = vectorlength / shapeSize;
            var first = new int[shapeCount];
            var count = new int[shapeCount];
            for (var i = 0; i < shapeCount; i++)
            {
                first[i] = i * shapeSize;
                count[i] = shapeSize;
            }

            return (first, count, shapeCount);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            DrawShapes(_wallPixels, Color.Blue, PrimitiveType.Quads, _wallShapeCount);
            foreach (var pixel in _brushFirePixels ?? new Dictionary<Color, Vector2[]>())
            {
                DrawShapes(pixel.Value, pixel.Key, PrimitiveType.Quads, GetStartCount(pixel.Value.Length, 4));
            }

            var circle = GenerateCircle(_mouseCharacter.MapX, _mouseCharacter.MapY);
            DrawShapes(circle, Color.BlueViolet, PrimitiveType.TriangleFan, (new[] { 0 }, new[] { 36 }, 1));

            var monstersCircle = _monsters.SelectMany(s => GenerateCircle(s.PositionX, s.PositionY)).ToArray();
            DrawShapes(monstersCircle, Color.Red, PrimitiveType.TriangleFan, _monstersShapeCount);

            var npcCircle = _npcs.SelectMany(s => GenerateCircle(s.PositionX, s.PositionY)).ToArray();
            DrawShapes(monstersCircle, Color.Yellow, PrimitiveType.TriangleFan, _npcsShapeCount);

            SwapBuffers();
        }

        private Vector2[] GenerateSquare(short x, short y)
        {
            return new[]
            {
                new Vector2((float)(x * _cellSize), (float)(y * _cellSize)),
                new Vector2((float)(_cellSize * (x + 1)), (float)(y * _cellSize)),
                new Vector2((float)(_cellSize * (x + 1)), (float)(_cellSize * (y + 1))),
                new Vector2((float)(x * _cellSize),(float)( _cellSize * (y + 1)))
            };
        }

        private Vector2[] GenerateCircle(short x, short y)
        {
            return Enumerable.Range(0, 36).Select(i => new Vector2((float)((x + Math.Cos(i)) * _cellSize),
                (float)((y + Math.Sin(i)) * _cellSize))).ToArray();
        }

        private void DrawShapes(Vector2[] vector, Color color, PrimitiveType type, (int[] First, int[] Count, int ShapeCount) shapeSize)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vector2.SizeInBytes * vector.Length), vector, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, Vector2.SizeInBytes, 0);
            GL.Color4(color); //deprecated
            GL.MultiDrawArrays(type, shapeSize.First, shapeSize.Count, shapeSize.ShapeCount);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
    }
}