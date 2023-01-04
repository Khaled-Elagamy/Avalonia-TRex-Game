using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp
{
    public class Obstacle
    {

        public readonly int SpawnRate; // in milliseconds
        private int _Speed = 5;
    
        public Image image { get; set; }

        public Obstacle()
        {
            
            image = new Image
            {
                Width = 40,
                Height = 30,
               
            };
            Canvas.SetBottom(image, 0);
            var rnd = new Random();
            if (rnd.Next(0, 20) % 2 == 0)
            {
                image.Source = new Bitmap("../../../Assets/obstacle-1.gif");
            }
            else
            {
                image.Source = new Bitmap("../../../Assets/obstacle-2.gif");
            }

            SpawnRate = 300;
            
        }

        public double xPosition
        {

            get => Canvas.GetLeft(image);
            set
            {
                Canvas.SetLeft(image, value);
            }
        }
        public int Speed
        {
            get => _Speed;
            set => _Speed = value;
        }
    }

}
