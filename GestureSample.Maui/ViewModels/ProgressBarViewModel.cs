namespace GestureSample.ViewModels
{
    public class ProgressBarViewModel : ThreeDoublesViewModel
    {
        private int progress = 0;
        private void StartProgressTimer()
        {
            Application.Current?.Dispatcher.StartTimer(TimeSpan.FromMilliseconds(200), onTimer);
        }

        public ProgressBarViewModel()
        {
            StartProgressTimer();
        }

        private bool onTimer()
        {
            progress++;

            if (progress <= 300)
            {
                Value1 = progress / 100.0;
                Value2 = progress / 200.0;
                Value3 = progress / 300.0;

                return true;
            }
            else
            {
                AddText("All ProgressBars finished. Tap to start over.");
                return false;
            }
        }

        protected override void OnTapped(MR.Gestures.TapEventArgs e)
        {
            base.OnTapped(e);

            if (progress > 300)
            {
                progress = 0;
                AddText("Starting ProgressBars again.");
                StartProgressTimer();
            }
        }
    }
}
