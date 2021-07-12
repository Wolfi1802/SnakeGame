using Microsoft.Win32;
using SNAKE.Registre;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SNAKE
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        const int SNAKE_SQUARE_SIZE = 20;
        const int SNAKE_START_LENGTH = 3;
        const int SNAKE_START_SPEED = 400;
        const int SNAKE_SPEED_INTERVAL = 100;

        #region ADONS
        const bool ANTI_WALL_CRASH_ADON_ACTIVATED = false;
        const int TO_DRAW_BARRIER_VALUE = 0;//x <- 0 == inaktiv , 1 >= aktiv
        #endregion

        public enum SnakeDirection { Left, Right, Up, Down };

        public string PlayerName { set; get; }

        private SolidColorBrush snakeBodyBrush = Brushes.Gray;
        private SolidColorBrush snakeHeadBrush = Brushes.Black;
        private List<SnakePart> snakeParts = new List<SnakePart>();
        private SnakeDirection snakeDirection = SnakeDirection.Right;
        private System.Windows.Threading.DispatcherTimer gameTickTimer = new System.Windows.Threading.DispatcherTimer();
        private Random rnd = new Random();
        private int snakeLength;
        private int currentScore = 0;
        private bool gameIsRunning = false;
        private GameEndWindow gameEndwindow;
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private UIElement snakeFood = null;
        private EllipseAnimation ellipseAnimation;
        private Point foodPosition;
        private Random randomizer = new Random();
        private List<Barrier> listOfBarrier;
        private double x = 0;
        private double y = 0;
        private double bestScore = 0;

        public MainWindow()
        {
            AddPlayerNameWindow addPlayerNameWindow = new AddPlayerNameWindow(this);
            addPlayerNameWindow.ShowDialog();

            if(string.IsNullOrEmpty(this.PlayerName))
            {
                this.PlayerName = "JaColaFanBoy";
            }

            InitializeComponent();
            this.ellipseAnimation = new EllipseAnimation();
            this.ResizeMode = ResizeMode.NoResize;
            this.gameTickTimer.Tick += this.GameTickTimer_Tick;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Loaded += this.OnWindowRendered;
            this.listOfBarrier = new List<Barrier>();
            this.InitEvents();
        }

        private void DrawSnakeFood()
        {
            this.foodPosition = GetNextFoodPosition();

            this.ellipseAnimation.StartFoodAnimationThread(this.GameArea);

            this.snakeFood = new Ellipse()
            {
                Width = SNAKE_SQUARE_SIZE,
                Height = SNAKE_SQUARE_SIZE,
                Fill = this.GetRandomBrush()
            };
            Canvas.SetTop(this.snakeFood, this.foodPosition.Y);
            Canvas.SetLeft(this.snakeFood, this.foodPosition.X);
            this.GameArea.Children.Add(snakeFood);
        }

        private void DrawGameArea()
        {
            bool doneDrawingBackground = false;
            int nextX = 0, nextY = 0;
            int rowCounter = 0;
            bool nextIsOdd = false;

            while (doneDrawingBackground == false)
            {
                Rectangle rect = new Rectangle
                {
                    Width = SNAKE_SQUARE_SIZE,
                    Height = SNAKE_SQUARE_SIZE,
                };
                Canvas.SetTop(rect, nextY);
                Canvas.SetLeft(rect, nextX);
                this.GameArea.Children.Add(rect);


                nextIsOdd = !nextIsOdd;
                nextX += SNAKE_SQUARE_SIZE;

                if (nextX >= this.GameArea.ActualWidth)
                {
                    nextX = 0;
                    nextY += SNAKE_SQUARE_SIZE;
                    rowCounter++;
                    nextIsOdd = (rowCounter % 2 != 0);
                }

                if (nextY >= this.GameArea.ActualHeight)
                    doneDrawingBackground = true;
            }

        }

        private void DrawSnake()
        {
            foreach (SnakePart snakePart in snakeParts)
            {
                if (snakePart.UiElement == null)
                {
                    snakePart.UiElement = new Rectangle()
                    {
                        Width = SNAKE_SQUARE_SIZE,
                        Height = SNAKE_SQUARE_SIZE,
                        Stroke = new SolidColorBrush(Colors.Gold),
                        StrokeThickness = 2,
                        Fill = snakePart.IsHead ? this.snakeHeadBrush : this.snakeBodyBrush
                    };
                    this.GameArea.Children.Add(snakePart.UiElement);
                    Canvas.SetTop(snakePart.UiElement, snakePart.Position.Y);
                    Canvas.SetLeft(snakePart.UiElement, snakePart.Position.X);
                }
            }
        }

        private void MoveSnake()
        {
            // Remove the last part of the snake, in preparation of the new part added below  
            while (this.snakeParts.Count >= this.snakeLength)
            {
                this.GameArea.Children.Remove(snakeParts[0].UiElement);
                this.snakeParts.RemoveAt(0);
            }

            foreach (SnakePart snakePart in snakeParts)
            {
                (snakePart.UiElement as Rectangle).Fill = this.snakeBodyBrush;
                snakePart.IsHead = false;
            }

            // Determine in which direction to expand the snake, based on the current direction  
            SnakePart snakeHead = this.snakeParts[this.snakeParts.Count - 1];
            double nextX = snakeHead.Position.X;
            double nextY = snakeHead.Position.Y;
            //Debug.WriteLine($"HEAD->  Position x {snakeHead.Position.X}, Position y {snakeHead.Position.Y}");

            switch (this.snakeDirection)
            {
                case SnakeDirection.Left:
                    nextX -= SNAKE_SQUARE_SIZE;
                    break;
                case SnakeDirection.Right:
                    nextX += SNAKE_SQUARE_SIZE;
                    break;
                case SnakeDirection.Up:
                    nextY -= SNAKE_SQUARE_SIZE;
                    break;
                case SnakeDirection.Down:
                    nextY += SNAKE_SQUARE_SIZE;
                    break;
            }

            // Now add the new head part to our list of snake parts...  
            this.snakeParts.Add(new SnakePart()
            {
                Position = new Point(nextX, nextY),
                IsHead = true
            });

            this.DrawSnake();
            this.CheckColission();
        }

        private void StartNewGame()
        {

            if (!this.gameIsRunning)
            {
                this.InitGameEndWindowEvents();
                this.snakeLength = SNAKE_START_LENGTH;
                this.snakeDirection = SnakeDirection.Right;
                this.snakeParts.Add(new SnakePart() { Position = new Point(SNAKE_SQUARE_SIZE * 5, SNAKE_SQUARE_SIZE * 5) });
                this.gameTickTimer.Interval = TimeSpan.FromMilliseconds(SNAKE_START_SPEED);

                // Draw the snake  
                this.DrawSnake();
                this.DrawSnakeFood();
                this.DrawBarrier(TO_DRAW_BARRIER_VALUE);

                // Update Status
                this.UpdateGameStatus();

                // Go!          
                this.gameTickTimer.IsEnabled = true;
                this.gameIsRunning = true;
            }
        }

        private void UpdateGameStatus()
        {
            this.Title = $"Snake - UserName: {this.PlayerName} | Score: {this.currentScore}";
        }

        private void EndGame()
        {
            this.gameIsRunning = false;
            this.gameTickTimer.IsEnabled = false;
            this.GameArea.Children.Clear();
            this.listOfBarrier.Clear();
            this.ellipseAnimation.CancelFoodAnimationThread();
            this.UpdateBestScore();
            this.gameEndwindow.GameResult.Text = $"Herzlichen Glückwunsch {this.PlayerName}.\nDu hast Ganze {this.currentScore} Punkte erreicht.\nBest Score: {this.bestScore}";
            this.gameEndwindow.ShowDialog();
            this.currentScore = 0;

            this.UpdateGameStatus();
        }

        private void UpdateBestScore()
        {
            if (this.currentScore > this.bestScore)
            {
                this.bestScore = this.currentScore;
            }
        }

        private void AntiWallCrashAdon(SnakePart snakeHead)
        {
            if (snakeHead.Position.X >= GameArea.ActualWidth)
            {
                Debug.WriteLine("head crash right");
                this.y = this.snakeParts[snakeParts.Count - 1].Position.Y;
                this.x = 0 - SNAKE_SQUARE_SIZE;
                this.snakeParts[snakeParts.Count - 1].Position = new Point(this.x, this.y);
            }
            else if (snakeHead.Position.X < 0)
            {
                Debug.WriteLine("head crash left");
                this.y = this.snakeParts[snakeParts.Count - 1].Position.Y;
                this.x = GameArea.ActualWidth;
                this.snakeParts[snakeParts.Count - 1].Position = new Point(this.x, this.y);
            }
            else if (snakeHead.Position.Y >= GameArea.ActualHeight)
            {
                Debug.WriteLine("head crash bot");
                this.x = this.snakeParts[snakeParts.Count - 1].Position.X;
                this.y = 0 - SNAKE_SQUARE_SIZE;
                this.snakeParts[snakeParts.Count - 1].Position = new Point(this.x, this.y);
            }
            else if (snakeHead.Position.Y < 0)
            {
                Debug.WriteLine("head crash top");
                this.x = this.snakeParts[snakeParts.Count - 1].Position.X;
                this.y = GameArea.ActualHeight;
                this.snakeParts[snakeParts.Count - 1].Position = new Point(this.x, this.y);
            }
        }

        private void CheckColission()
        {
            SnakePart snakeHead = snakeParts[snakeParts.Count - 1];

            if ((snakeHead.Position.X == Canvas.GetLeft(snakeFood)) && (snakeHead.Position.Y == Canvas.GetTop(snakeFood)))
            {
                this.EatSnakeFood();

                return;
            }

            //wenn schlange mit wand kolidiert
            if ((snakeHead.Position.Y < 0) || (snakeHead.Position.Y >= GameArea.ActualHeight) || (snakeHead.Position.X < 0) || (snakeHead.Position.X >= GameArea.ActualWidth))
            {
                if (ANTI_WALL_CRASH_ADON_ACTIVATED)
                {
                    this.AntiWallCrashAdon(snakeHead);
                }
                else
                {
                    this.EndGame();
                }

                return;
            }

            //Wenn schlange mit eigenem körper kolidiert
            foreach (SnakePart snakeBodyPart in snakeParts.Take(snakeParts.Count - 1))
            {
                if ((snakeHead.Position.X == snakeBodyPart.Position.X) && (snakeHead.Position.Y == snakeBodyPart.Position.Y))
                {
                    this.EndGame();

                    return;
                }
            }

            //Wenn schlange mit einer barrier kolidiert
            foreach (Barrier barrier in listOfBarrier)
            {
                if ((barrier.Position.X == snakeHead.Position.X) && (barrier.Position.Y == snakeHead.Position.Y))
                {
                    this.EndGame();

                    return;
                }
            }
        }

        private void EatSnakeFood()
        {
            this.snakeLength++;
            this.currentScore++;
            int timerInterval = Math.Max(SNAKE_SPEED_INTERVAL, (int)gameTickTimer.Interval.TotalMilliseconds - (currentScore * 2));
            this.gameTickTimer.Interval = TimeSpan.FromMilliseconds(timerInterval);
            this.ellipseAnimation.CancelFoodAnimationThread();
            this.GameArea.Children.Remove(this.snakeFood);
            this.DrawSnakeFood();
            this.UpdateGameStatus();
        }

        private void UserControllForMovingSnake(KeyEventArgs e)
        {
            SnakeDirection originalSnakeDirection = snakeDirection;
            switch (e.Key)
            {
                case Key.Up:
                    if (snakeDirection != SnakeDirection.Down)
                    {
                        snakeDirection = SnakeDirection.Up;
                    }
                    break;
                case Key.Down:
                    if (snakeDirection != SnakeDirection.Up)
                    {
                        snakeDirection = SnakeDirection.Down;
                    }
                    break;
                case Key.Left:
                    if (snakeDirection != SnakeDirection.Right)
                    {
                        snakeDirection = SnakeDirection.Left;
                    }
                    break;
                case Key.Right:
                    if (snakeDirection != SnakeDirection.Left)
                    {
                        snakeDirection = SnakeDirection.Right;
                    }
                    break;
                case Key.Space:
                    StartNewGame();
                    break;
            }

            if (snakeDirection != originalSnakeDirection)
                MoveSnake();
        }

        private void AnimateSnakeFood()
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(new Action(this.AnimateSnakeFood));
            }
            else
            {
                this.GameArea.Children.Remove(this.snakeFood);
                this.snakeFood = this.GetAnimatedFood(this.foodPosition);
                this.GameArea.Children.Add(this.snakeFood);
            }
        }

        private void DrawBarrier(int barrierToDraw)
        {
            for (int i = 0; i < barrierToDraw; i++)
            {
                Point point = this.GetNextFoodPosition();

                Barrier barrier = new Barrier();
                barrier.Position = point;
                barrier.UiElement = this.GetRandomBarrier(point);

                this.GameArea.Children.Add(barrier.UiElement);
                this.listOfBarrier.Add(barrier);
            }
        }

        private void InitGame()
        {
            string text = "<---Regeln--->\nDu verlierst, wenn du gegen eine Wand stößt.\nDu verlierst wenn du gegen den eigenen Körper stößt.\nDie Schlange wird mit jedem aufgenommenem Apfel um 1 Feld länger und etwas schneller.\n\nDu steuerst mit den Pfeiltasten.\n\nHAVE FUN";

            this.DrawGameArea();
            MessageBox.Show(text, "information");


            this.StartNewGame();
            Debug.WriteLine($"{nameof(MainWindow)},{nameof(OnWindowRendered)}, Game is started");
        }

        private Ellipse GetAnimatedFood(Point foodPosition)
        {
            Ellipse animatedEllipseAsFood = new Ellipse()
            {
                Width = SNAKE_SQUARE_SIZE,
                Height = SNAKE_SQUARE_SIZE,
                Fill = this.GetRandomBrush()
            };
            Canvas.SetTop(animatedEllipseAsFood, foodPosition.Y);
            Canvas.SetLeft(animatedEllipseAsFood, foodPosition.X);

            return animatedEllipseAsFood;
        }

        private Rectangle GetRandomBarrier(Point barrierPosition)
        {

            Rectangle barrier = new Rectangle()
            {
                Width = SNAKE_SQUARE_SIZE,
                Height = SNAKE_SQUARE_SIZE,
                Fill = GetBarrierBrush()
            };
            Canvas.SetTop(barrier, barrierPosition.Y);
            Canvas.SetLeft(barrier, barrierPosition.X);

            return barrier;
        }

        private Brush GetBarrierBrush()
        {
            var red = Convert.ToByte(0);
            var green = Convert.ToByte(0);
            var blue = Convert.ToByte(0);

            return new SolidColorBrush(Color.FromRgb(red, green, blue));
        }

        private Brush GetRandomBrush()
        {
            var red = Convert.ToByte(randomizer.Next(0, 255));
            var green = Convert.ToByte(randomizer.Next(0, 255));
            var blue = Convert.ToByte(randomizer.Next(0, 255));

            return new SolidColorBrush(Color.FromRgb(red, green, blue));
        }

        private LinearGradientBrush CreateLinearGradientBrush()
        {
            LinearGradientBrush linearGradientBrush = new LinearGradientBrush();
            linearGradientBrush.GradientStops.Add(new GradientStop(Colors.Red, 0));
            linearGradientBrush.GradientStops.Add(new GradientStop(Colors.Black, 1));
            linearGradientBrush.StartPoint = new Point(0, 1);
            linearGradientBrush.EndPoint = new Point(1, 0);

            return linearGradientBrush;
        }

        private Point GetNextFoodPosition()
        {
            int maxX = (int)(GameArea.ActualWidth / SNAKE_SQUARE_SIZE);
            int maxY = (int)(GameArea.ActualHeight / SNAKE_SQUARE_SIZE);
            int foodX = rnd.Next(0, maxX) * SNAKE_SQUARE_SIZE;
            int foodY = rnd.Next(0, maxY) * SNAKE_SQUARE_SIZE;

            foreach (SnakePart snakePart in snakeParts)
            {
                if ((snakePart.Position.X == foodX) && (snakePart.Position.Y == foodY))
                    return GetNextFoodPosition();
            }

            foreach (Barrier barrier in listOfBarrier)
            {
                if ((barrier.Position.X == foodX) && (barrier.Position.Y == foodY))
                    return GetNextFoodPosition();
            }

            return new Point(foodX, foodY);
        }

        #region Events
        private void InitGameEndWindowEvents()
        {
            this.gameEndwindow = new GameEndWindow();
            this.gameEndwindow.CloseClicked += this.OnGameWindowCloseButtonClicked;
            this.gameEndwindow.RestartClicked += this.OnGameWindowRestartButtonClicked;
        }

        private void InitEllipseanimationEvent()
        {
            this.ellipseAnimation.blinkedTick += OnEllipseAnimationBlinkedTick;
        }

        private void InitEvents()
        {
            this.InitEllipseanimationEvent();

            this.KeyUp += this.OnKeyUpTrigger;
            this.Closing += this.OnWindowClosing;
            this.InitGameEndWindowEvents();
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.gameTickTimer.IsEnabled)
            {
                this.gameTickTimer.Stop();
                Debug.WriteLine("Timer wurde gestoppt!");
            }
            else
                Debug.WriteLine("Der SpielTimer wurde schon angehalten.");

            this.ellipseAnimation.CancelFoodAnimationThread();

            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            Application.Current.Shutdown();
        }

        private void OnGameWindowRestartButtonClicked(object sender, EventArgs e)
        {
            try
            {
                this.UpdateGameStatus();
                this.gameEndwindow.Close();
                this.StartNewGame();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(MainWindow)},{nameof(OnGameWindowRestartButtonClicked)},Die excepion die auftritt wenn das Fenster geschlossen ist und nochmal geschlossen werden soll nicht beachten!\n" + ex);
            }
        }

        private void OnGameWindowCloseButtonClicked(object sender, EventArgs e)
        {
            try
            {
                this.Close();
                this.gameEndwindow.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(MainWindow)},{nameof(OnGameWindowCloseButtonClicked)}, Da das Fenster schon geschlossen wurde kann es nicht erneut geschlossen werden!\n" + ex);
            }
        }

        private void OnKeyUpTrigger(object sender, KeyEventArgs e)
        {
            this.UserControllForMovingSnake(e);
        }

        private void GameTickTimer_Tick(object sender, EventArgs e)
        {
            MoveSnake();
        }

        private void OnWindowRendered(object sender, EventArgs e)
        {
            this.InitGame();
        }

        private void OnEllipseAnimationBlinkedTick(object sender, EventArgs e)
        {
            this.AnimateSnakeFood();
        }
        #endregion
    }
}
