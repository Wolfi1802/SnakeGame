using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;

namespace SNAKE
{
    public class EllipseAnimation
    {
        const int FOOD_ANIMATION_SPEED = 100;

        public event EventHandler blinkedTick;

        private Thread blinkAnimatenSnakeFood;
        private Canvas gameArea; 
        private System.Windows.Threading.DispatcherTimer gameTickTimer = new System.Windows.Threading.DispatcherTimer();


        public void StartFoodAnimationThread(Canvas gameArea)
        {
            this.gameArea = gameArea;//aus alt wird neu....

            this.blinkAnimatenSnakeFood = new Thread(this.StartBlink);
            this.blinkAnimatenSnakeFood.Name = nameof(blinkAnimatenSnakeFood);
            this.blinkAnimatenSnakeFood.Start(gameArea);
        }

        public void CancelFoodAnimationThread()
        {
            try
            {
                this.gameTickTimer.Stop();
                this.blinkAnimatenSnakeFood.Abort();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }


        private void StartBlink(object gameArea)
        {
            this.gameTickTimer.Interval = TimeSpan.FromMilliseconds(FOOD_ANIMATION_SPEED);

            this.gameTickTimer.Tick += GameTickTimer_Tick;
            this.gameTickTimer.Start();
        }

        private void GameTickTimer_Tick(object sender, EventArgs e)
        {
            this.RaiseBlinkedTick();
        }

        private void RaiseBlinkedTick()
        {
            if (this.blinkedTick != null)
            {
                //Debug.WriteLine("Blink wurde ausgelöst!");
                this.blinkedTick(this, new EventArgs());
            }
            else
                Debug.WriteLine($"Aufruf von {nameof(this.blinkedTick)} fehlgeschlagen!");
        }
    }
}