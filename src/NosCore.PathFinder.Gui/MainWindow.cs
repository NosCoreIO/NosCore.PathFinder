// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace NosCore.PathFinder.Gui
{
    public partial class MainWindow : Window
    {
        private int _currentTick;
        private double _elapsed;
        private int _frameCount;
        private double _frameCountTime;
        private int _frameRate;
        private int _lastTick;

        public MainWindow()
        {
            InitializeComponent();

            var frameTimer = new DispatcherTimer();
            frameTimer.Tick += OnFrame;
            frameTimer.Interval = TimeSpan.FromSeconds(1.0 / 60.0);
            frameTimer.Start();

            _lastTick = Environment.TickCount;
            KeyDown += Window1_KeyDown;
            Title = "NosCore Pathfinder GUI";
        }

        private void Window1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        private void OnFrame(object sender, EventArgs e)
        {
            _currentTick = Environment.TickCount;
            _elapsed = (_currentTick - _lastTick) / 1000.0;
            _lastTick = _currentTick;

            _frameCount++;
            _frameCountTime += _elapsed; 
            var position = Mouse.GetPosition(this);
            if (_frameCountTime >= 1.0)
            {
                _frameCountTime -= 1.0;
                _frameRate = _frameCount;
                _frameCount = 0;
                FrameRateLabel.Content = "FPS: " + _frameRate + "  Monsters: " + 0 + "  Npcs: " + 0 + " MouseX:" + position.X + " MouseY:" + position.Y;
                
            }

            //var originWidth = ActualWidth / 2;
            //var originHeight = ActualHeight / 2;
            //var c = Math.Cos(_totalElapsed);
            //var s = Math.Sin(_totalElapsed);
            //_spawnPoint = new Point3D(position.X, position.Y, 0.0);
            //_spawnPoint = new Point3D(-originWidth + position.X, originHeight - position.Y, 0.0);
        }


    }
}