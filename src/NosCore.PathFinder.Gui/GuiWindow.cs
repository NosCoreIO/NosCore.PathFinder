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

        private readonly List<Tuple<short, short, byte, Vector2[]>> _walls =
            new List<Tuple<short, short, byte, Vector2[]>>();

        private float _cellSize;
        private readonly CharacterGo _mouseCharacter;
        private readonly int _originalWidth;

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
            _mouseCharacter = new CharacterGo
            { BrushFire = new BrushFire((0, 0), 0, new Dictionary<(short X, short Y), BrushFireNode?>(), _map.XLength, _map.YLength) };
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

            Parallel.ForEach(_monsters.Where(s => s.Life == null), monster => monster.StartLife());
            Parallel.ForEach(_npcs.Where(s => s.Life == null), npc => npc.StartLife());

            GetMap();
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

            _mouseCharacter.MapX = (short)(e.X / _cellSize);
            _mouseCharacter.MapY = (short)(e.Y / _cellSize);
            _mouseCharacter.BrushFire = _map.LoadBrushFire((_mouseCharacter.MapX, _mouseCharacter.MapY),
                new OctileDistanceHeuristic());
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.ClearColor(Color.LightSkyBlue.A, Color.LightSkyBlue.R, Color.LightSkyBlue.G, Color.LightSkyBlue.B);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            var world = Matrix4.CreateOrthographicOffCenter(0, ClientRectangle.Width, ClientRectangle.Height, 0, 0, 1);
            GL.LoadMatrix(ref world);

            //draw brushfire
            for (short y = 0; y < _map.YLength; y++)
            {
                for (short x = 0; x < _map.XLength; x++)
                {
                    if (!Equals(_mouseCharacter.BrushFire[x, y] ?? 0d, 0d))
                    {
                        var alpha = 255 - ((_mouseCharacter.BrushFire[x, y] ?? 0) * 10);
                        if (alpha < 0)
                        {
                            alpha = 0;
                        }
                        DrawPixel(GeneratePixel(x, y), Color.FromArgb((int)(alpha), 0, 255, 0), PrimitiveType.Quads);
                    }
                }
            }

            DrawPixelCircle(_mouseCharacter.MapX, _mouseCharacter.MapY, Color.BlueViolet);

            foreach (var wall in _walls)
            {
                DrawPixel(wall.Item4, Color.Blue, PrimitiveType.Quads); //TODO iswalkable
            }

            foreach (var monster in _monsters)
            {
                DrawPixelCircle(monster.PositionX, monster.PositionY, Color.Red);
            }

            foreach (var npc in _npcs)
            {
                DrawPixelCircle(npc.PositionX, npc.PositionY, Color.Yellow);
            }

            SwapBuffers();
        }

        private void GetMap()
        {
            for (short y = 0; y < _map.YLength; y++)
            {
                for (short x = 0; x < _map.XLength; x++)
                {
                    var value = _map[x, y];
                    if (_map[x, y] > 0)
                    {
                        _walls.Add(new Tuple<short, short, byte, Vector2[]>(x, y, value, GeneratePixel(x, y)));
                    }
                }
            }
        }

        private Vector2[] GeneratePixel(short x, short y)
        {
            return new[]
            {
                new Vector2((float)(x * _cellSize), (float)(y * _cellSize)),
                new Vector2((float)(_cellSize * (x + 1)), (float)(y * _cellSize)),
                new Vector2((float)(_cellSize * (x + 1)), (float)(_cellSize * (y + 1))),
                new Vector2((float)(x * _cellSize),(float)( _cellSize * (y + 1)))
            };
        }

        private void DrawPixel(Vector2[] vector, Color color, PrimitiveType type)
        {
            var vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vector2.SizeInBytes * vector.Length), vector, BufferUsageHint.StaticDraw);

            GL.VertexPointer(2, VertexPointerType.Float, Vector2.SizeInBytes, 0);
            GL.Color4(color);
            GL.DrawArrays(type, 0, vector.Length);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(vbo);
        }

        private void DrawPixelCircle(short x, short y, Color color)
        {
            var pixelVertexBuffer =
                new[]
                {
                    new Vector2((float)(x * _cellSize), (float)(y * _cellSize))
                }.Concat(Enumerable.Range(0, 360).Select(i => new Vector2((float)(x * _cellSize + Math.Cos(i) * _cellSize),
                    (float)(y * _cellSize + Math.Sin(i) * _cellSize)))).ToArray();

            var vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vector2.SizeInBytes * pixelVertexBuffer.Length), pixelVertexBuffer, BufferUsageHint.StaticDraw);

            GL.VertexPointer(2, VertexPointerType.Float, Vector2.SizeInBytes, 0);
            GL.Color4(color);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, pixelVertexBuffer.Length);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(vbo);
        }
    }
}