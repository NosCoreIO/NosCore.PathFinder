﻿//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// 
// Copyright (C) 2019 - NosCore
// 
// NosCore is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using NosCore.Dao.Interfaces;
using NosCore.PathFinder.Gui.Database;
using NosCore.PathFinder.Gui.Models;
using Serilog;
using NosCore.Dao;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace NosCore.PathFinder.Gui
{
    public class GuiWindow : GameWindow
    {
        private static readonly ILogger Logger = NosCore.Shared.I18N.Logger.GetLoggerConfiguration().CreateLogger();
        private readonly byte _gridsize;
        private readonly MapDto _map;

        private readonly List<MapMonsterDto> _monsters;
        private readonly List<MapNpcDto> _npcs;
        private readonly int _originalHeight;
        private readonly int _originalWidth;
        private readonly List<Tuple<short, short, byte>> _walls = new List<Tuple<short, short, byte>>();
        private double _gridsizeX;
        private double _gridsizeY;

        public GuiWindow(MapDto map, byte gridsize, int width, int height, string title, DataAccessHelper dbContextBuilder) : base(width * gridsize, height * gridsize, GraphicsMode.Default, title)
        {
            var dbContextBuilder1 = dbContextBuilder;
            var mapMonsterDao = new Dao<MapMonster, MapMonsterDto, int>(Logger, dbContextBuilder1);
            var mapNpcDao = new Dao<MapNpc, MapNpcDto, int>(Logger, dbContextBuilder1);
            _originalWidth = width * gridsize;
            _originalHeight = height * gridsize;
            _gridsizeX = gridsize;
            _gridsizeY = gridsize;
            _gridsize = gridsize;
            _monsters = mapMonsterDao.Where(s => s.MapId == map.MapId)?.Adapt<List<MapMonsterDto>>() ?? new List<MapMonsterDto>();
            _map = map;

            foreach (var mapMonster in _monsters)
            {
                mapMonster.PositionX = mapMonster.MapX;
                mapMonster.PositionY = mapMonster.MapY;
                mapMonster.Speed = 10;
                mapMonster.Map = _map;
            }

            _npcs = mapNpcDao.Where(s => s.MapId == map.MapId)?.Adapt<List<MapNpcDto>>() ?? new List<MapNpcDto>();
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

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(Color.LightSkyBlue.A, Color.LightSkyBlue.R, Color.LightSkyBlue.G, Color.LightSkyBlue.B);
            _gridsizeX = _gridsize * (ClientRectangle.Width / (double)_originalWidth);
            _gridsizeY = _gridsize * (ClientRectangle.Height / (double)_originalHeight);
            var world = Matrix4.CreateOrthographicOffCenter(0, ClientRectangle.Width, ClientRectangle.Height, 0, 0, 1);
            GL.LoadMatrix(ref world);
            foreach (var wall in _walls)
            {
                DrawPixel(wall.Item1, wall.Item2, Color.Blue); //TODO iswalkable
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
            for (short i = 0; i < _map.YLength; i++)
            {
                for (short t = 0; t < _map.XLength; t++)
                {
                    var value = _map[t, i];
                    if (_map[t, i] > 0)
                    {
                        _walls.Add(new Tuple<short, short, byte>(t, i, value));
                    }
                }
            }
        }

        private void DrawPixel(short x, short y, Color color)
        {
            GL.Begin(PrimitiveType.Quads);
            GL.Color3(color);
            GL.Vertex2(x * _gridsizeX, y * _gridsizeY);
            GL.Vertex2(_gridsizeX * (x + 1), y * _gridsizeY);
            GL.Vertex2(_gridsizeX * (x + 1), _gridsizeY * (y + 1));
            GL.Vertex2(x * _gridsizeX, _gridsizeY * (y + 1));
            GL.End();
        }

        private void DrawPixelCircle(short x, short y, Color color)
        {
            GL.Enable(EnableCap.Blend);
            GL.Begin(PrimitiveType.TriangleFan);
            GL.Color3(color);

            GL.Vertex2(x * _gridsizeX, y * _gridsizeY);
            for (var i = 0; i < 360; i++)
            {
                GL.Vertex2(x * _gridsizeX + Math.Cos(i) * _gridsizeX, y * _gridsizeY + Math.Sin(i) * _gridsizeY);
            }

            GL.End();
            GL.Disable(EnableCap.Blend);
        }
    }
}