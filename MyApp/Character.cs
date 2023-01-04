using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Tmds.DBus;

namespace MyApp
{

    public class Player
    {
        private int _JumpSpeed { get; set; }
        private int _Gravity { get; set; }
        private int _score { get; set; }
        private bool _CanJump { get; set; }
        private bool _right { get; set; }
        private bool _left { get; set; }

        public Image image { get; set; }


        public double yPosition
        {
            get => Canvas.GetBottom(image);
            set
            {
                Canvas.SetBottom(image, value);
            }
        }
        public double xPosition
        {

            get => Canvas.GetLeft(image);
            set
            {
                Canvas.SetLeft(image, value);
            }
        }
        public int JumpSpeed
        {
            get => _JumpSpeed;
            set => _JumpSpeed = value;
        }
        public int Gravity
        {
            get => _Gravity;
            set => _Gravity = value;
        }

        public int score
        {
            get => _score;
            set => _score = value;
        }

        public bool CanJump
        {
            get => _CanJump;
            set => _CanJump = value;
        }public bool right
        {
            get => _right;
            set => _right = value;
        }public bool left
        {
            get => _left;
            set => _left = value;
        }

        public Player()
        {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            System.Uri uri = new System.Uri("avares://MyApp/Assets/running.gif");


            JumpSpeed = 10;
            Gravity = 12;
            CanJump = false;
            score = 0;
            image = new Image
            {
                Name = "MainCharacter",
               // Source = new Bitmap("../../../Assets/running.gif"),
                Source = new Bitmap(assets.Open(uri)),
                Height = 30,
                Width = 40,
            };
            xPosition = 10;
            yPosition = 0;

        }
    }

}

