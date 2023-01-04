using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.ComponentModel;
using Avalonia.Input;
using System;
using System.IO;
using SkiaSharp;
using HarfBuzzSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Avalonia.Controls.Primitives;
using Microsoft.CodeAnalysis.Text;
using Avalonia.Threading;
using System.Reactive;
using System.Diagnostics;
using Avalonia.Styling;
using System.Collections.Generic;
using Avalonia.Controls.Shapes;
using System.Linq;
using Avalonia.Media;
using Avalonia.Platform;
using System.Xml.Linq;
using Avalonia.Rendering.SceneGraph;
using System.Numerics;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using NAudio.Wave;
using System.Threading;
using NAudio.Wave.SampleProviders;
using NAudio.CoreAudioApi;
using System.Threading.Tasks;
using FireAndForgetAudioSample;
using System.Reflection.PortableExecutable;

namespace MyApp
{
    public partial class MainWindow : Window
    {
        private static int elapsedTime;
        // Main Window Objects
        private static Canvas _canvas;
        private static List<Obstacle> _obstacles;
        private static TextBlock scoreText;
        // Game Objects
        private static Player player;
        private static double timesincelastobstacle = 100;
        private static int _score = 1;
        // Music Objects
        private static WaveOutEvent output;
        private static Task myTask;
        private static ManualResetEvent stopEvent;


        //make every button in a single thread
        public MainWindow()
        {

            InitializeComponent();
            DataContext = new MainWindowViewModel();
            Startgame();

        }

        private void Startgame()
        {
            player = new Player();
            // Main Window Content
            button1.Focusable = false;
            // scoreText = this.FindControl<TextBlock>("Score");
            // _canvas = this.FindControl<Canvas>("canvas");
            scoreText = Score;
            _canvas = canvas;
            _obstacles = new List<Obstacle>();

            // Add player image
            _canvas.Children.Add(player.image);
            // Creating task to play music without enterupting game and the closing event
            myTask = new Task(() => playsound());
            stopEvent = new ManualResetEvent(false);

            // Play game music
            myTask.Start();
        }
        // Play Background music
        public static void playsound()
        {

            output = new WaveOutEvent();
            using (var audio = new AudioFileReader("../../../Assets/main2.wav"))
            {
                audio.Volume = 0.1f;
                output.Init(audio);
                output.Play();

                while (!stopEvent.WaitOne(0))
                {
                    if (output.PlaybackState == PlaybackState.Stopped)
                    {
                        audio.Position = 0;
                    }
                    Thread.Sleep(100);

                }

            }

        }
        // Override The Main Windows Events
        protected override void OnClosed(EventArgs e)
        {
            // Release resources of sound
            output.Stop();
            AudioPlaybackEngine.Instance.Dispose();
            output.Dispose();
            stopEvent.Set();

            // Release resources of Task playing music
            myTask.Wait();
            myTask.Dispose();

        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            var press = new CachedSound("../../../Assets/buttonpress.wav");
            // Check for user input
            if (e.Key == Key.Space && !player.CanJump && player.yPosition == 0)
            {
                player.CanJump = true;
                // Sound of jumping
                AudioPlaybackEngine.Instance.PlaySound(press);
            }
            if (e.Key == Key.Down)
            {
                player.CanJump = false;
            }

            if (e.Key == Key.D)
            {
                if (player.xPosition >= 800)
                {
                    return;
                }
                else
                {
                    player.xPosition += +5;
                }
            }
            if (e.Key == Key.A)
            {
                if (player.xPosition <= 0)
                {
                    return;
                }
                else
                {
                    player.xPosition += -5;
                }
            }


        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            // Check when user release the key
            if (e.Key == Key.Space)
            {
                player.CanJump = false;
            }
        }


        // Main Game
        public class MainWindowViewModel : INotifyPropertyChanged
        {
            // Timer to run game event every tick
            private DispatcherTimer gameTimer;

            public static int PANEL_WIDTH = 800;
            public static int PANEL_HEIGHT = 450;

            private string buttonText = "Click Me!";
            public string ButtonText
            {
                get => buttonText;
                set
                {
                    buttonText = value;
                    resetGame();
                    UpdateScore(0);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonText)));
                }
            }
            public void ButtonClicked() => ButtonText = "Welcome to my game!";

            //Main game fucntions
            private void MainGame(object sender, EventArgs e)
            {
                elapsedTime++;
                timesincelastobstacle++;

                UpdatePLayer();
                UpdateObstacles();
                CheckObstacleCollisions();
            }


            // Check player status 
            private void UpdatePLayer()
            {
                // Assgin the speed
                player.yPosition += player.JumpSpeed;

                // If the player is at the maximum hieght
                if ((player.CanJump && (player.Gravity < 0 || player.Gravity < 4)))
                {
                    player.CanJump = false;
                }
                // Make the player jump
                if (player.CanJump)
                {
                    player.JumpSpeed = 10;
                    player.Gravity -= 1;
                }
                // Make the player fall down
                else
                {
                    player.JumpSpeed = -6;
                }
                // If the player is at the minimum height 
                if (player.yPosition <= -1 && !player.CanJump)
                {
                    player.Gravity = 12;
                    player.yPosition = 0;
                    player.JumpSpeed = 0;
                }
            }


            public void UpdateObstacles()
            {

                var rnd = new Random();
                var obstacle = new Obstacle();

                // Spawning the Obstacles
                if (rnd.Next(0, (int)(timesincelastobstacle + 50)) > obstacle.SpawnRate)
                {
                    string filePath;
                    if (rnd.Next(0, 20) % 2 == 0)
                    {
                        filePath = "../../../Assets/obstacle-1.gif";
                    }
                    else
                    {
                        filePath = "../../../Assets/obstacle-2.gif";
                    }
                    var place = new Bitmap(filePath);
                    obstacle.xPosition = _canvas.Width;
                    _obstacles.Add(obstacle);
                    _canvas.Children.Add(obstacle.image);
                    timesincelastobstacle = 100;
                }
                // Removing obstacles after reaching the end of the window
                foreach (var cactus in _obstacles.ToList())
                {
                    cactus.xPosition = cactus.xPosition - cactus.Speed;
                    if (cactus.xPosition + cactus.image.Width < 0)
                    {
                        _obstacles.Remove(cactus);
                        _canvas.Children.Remove(cactus.image);
                        UpdateScore(_score++);
                    }

                }
            }


            private void CheckObstacleCollisions()
            {
                // Iterate through the list of obstacles
                foreach (var obstacle in _obstacles.ToList())
                {
                    // Check if the player character's bounding box intersects with the bounding box of the obstacle
                    if (player.image.Bounds.Intersects(obstacle.image.Bounds))
                    {
                        // Sound of dying
                        var died = new CachedSound("../../../Assets/hit.wav");
                        AudioPlaybackEngine.Instance.PlaySound(died);
                        // Trigger game over
                        GameOver(_score - 1);
                    }
                }
            }





            // Change the score
            private void UpdateScore(int score)
            {
                scoreText.Text = "Score: " + score;
            }
            // Reset the game when the player touch the obstacles
            public void resetGame()
            {
                output.Play();
                // Reset the player and score
                player.Gravity = 12;
                player.yPosition = 0;
                player.xPosition = 10;
                player.CanJump = false;
                player.score = 0;
                UpdateScore(0);
                // Reset the Obstacles
                foreach (var obstacle in _obstacles.ToList()) { _canvas.Children.Remove(obstacle.image); _obstacles.Remove(obstacle); }
                // Restart the game (Sometimes it glitches)

                // Create the timer and run it 
                gameTimer = new DispatcherTimer();
                gameTimer.Interval = TimeSpan.FromMilliseconds(30);
                gameTimer.Tick += MainGame;
                gameTimer.Start();

            }


            private void PlayAgainButton_Click(object sender, RoutedEventArgs e)
            {
                // Close the score window
                ((Window)sender).Close();

                // Restart the game
                resetGame();

            }


            private void GameOver(int score)
            {

                // Stop the timer
                gameTimer.Stop();

                //Stop the music 
                output.Stop();


                // Create a new window to display the score
                var window = new Window
                {

                    CanResize = false,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Width = 200,
                    Height = 200,
                    Title = "Score"

                };

                // Create a stack panel to hold the controls
                var stackPanel = new StackPanel();

                // Create a label to display the score
                var scoreLabel = new Label
                {
                    Content = $"Your score: {score}"
                };

                // Add the label to the stack panel
                stackPanel.Children.Add(scoreLabel);

                // Create a label to display the highest score
                var highestScoreLabel = new Label
                {
                    Content = $"Highest score: {score}"
                };

                // Add the label to the stack panel
                stackPanel.Children.Add(highestScoreLabel);


                // Create a button to play again
                var playAgainButton = new Button
                {
                    Content = "Play again"
                };

                //var window = new GameOverWindow();
                // playAgainButton.Click += PlayAgainButton_Click;




                // Closing window event
                window.Closing += Window_Closing;
                // Add a click event handler to the button
                playAgainButton.Click += (sender, args) =>
                {
                    window.Close();

                };

                // Add the button to the stack panel
                stackPanel.Children.Add(playAgainButton);

                // Set the content of the window to the stack panel
                window.Content = stackPanel;


                // Show the window
                window.Show();


            }

            private void Window_Closing(object sender, CancelEventArgs e)
            {
                // Window is being closed by the user or by the button
                if (!e.Cancel)
                {

                    resetGame();

                }
            }


            /*public void Nojump()
            {
                if (player.Jump)
                {
                    player.Jump = false;

                    player.JumpSpeed = -12;
                    player.Force -= 1;

                }
                else
                {
                    player.JumpSpeed = 12;
                }
            }
            */

            /*To be removed
             * public double playerjump
            {
                get => Canvas.GetBottom(player.image);
                set
                {
                    Canvas.SetBottom(player.image, value);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(playerjump)));
                }
            }
            */

            /*To be removed
             * public int playerforce
            {
                get => player.Force;
                set
                {
                    player.Force = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(playerforce)));
                }
            }*/
            public int playerscore
            {
                get => player.score;
                set
                {
                    player.score = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(playerscore)));
                }
            }


            public double CharMove
            {
                get => player.xPosition;
                set
                {
                    player.xPosition = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharMove)));
                }
            }


            public event PropertyChangedEventHandler PropertyChanged;


        }
    }
}