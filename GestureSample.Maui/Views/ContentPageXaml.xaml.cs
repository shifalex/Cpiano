
//using Foundation;
using MR.Gestures;
using System.Windows.Input;

namespace GestureSample.Views
{
    public partial class ContentPageXaml : MR.Gestures.ContentPage
    {
        public MR.Gestures.Button[] Keys;
        readonly int FINGER_SEPERATOR = 5;

        private readonly bool _isSync = false;

        private int _addent1 = 0;
        private int _addent2 = 0;
        //private int _sum = 0;
        
        
        private bool _waiting_check = false;
        private bool _isTimerWorking = false;
        private IDispatcherTimer timer;
        private int _seconds_pressed = 0;
        public string SecondsToEnd
        {
            get
            {
                return string.Format("{0}", 3 - _seconds_pressed);
            }
        }

        public ContentPageXaml()
        {

            MR.Gestures.Grid grid = new()
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(3, GridUnitType.Star) },
                    new RowDefinition{Height = new GridLength(30)},
                    new RowDefinition { Height = new GridLength(2, GridUnitType.Star) }
                }

            };
            for (int i = 0; i < 11; i++)
                grid.ColumnDefinitions.Add((i == FINGER_SEPERATOR) ? new ColumnDefinition { Width = new GridLength(5) } : new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            int numberOfKeys = 10;
            Keys = new MR.Gestures.Button[numberOfKeys];

            for (int i = 0; i < Keys.Length; i++)
            {
                Keys[i] = new()
                {
                    Text = "Button " + (i + 1).ToString(),
                    BackgroundColor = Colors.White,
                    CommandParameter = i + 1,
                    DownCommand = new Command<MR.Gestures.DownUpEventArgs>(OnDown),
                    UpCommand = new Command<MR.Gestures.DownUpEventArgs>(OnUp)
                };
                grid.SetRow(Keys[i], 2);
                grid.SetColumn(Keys[i], (i < FINGER_SEPERATOR) ? i + 1 : i + 2);

            }
            TimerInit();


            Content = grid;
            

            //InitializeComponent();

            
        }

        private void TimerInit()
        {
            timer = Application.Current.Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => {
                MainThread.BeginInvokeOnMainThread(async () =>
                {

                    _seconds_pressed++;
                    //NotifyPropertyChanged(nameof(ShowSecondsToEnd));
                    if (_seconds_pressed >= 3 && _waiting_check)
                    {
                        _isTimerWorking = false; _waiting_check = false; _seconds_pressed = 0;
                        //Check();
                        timer.Stop();

                        //if (!IsEnabledTotal)
                        {
                            await Task.Delay(3000);
                            //GenerateExercise();
                        }

                    }
                    //NotifyPropertyChanged(nameof(SecondsToEnd));
                    //NotifyPropertyChanged(nameof(TrueStatement));
                });
            };
        }

        protected void OnDown(MR.Gestures.DownUpEventArgs e)
        {
            MR.Gestures.Button sender = (MR.Gestures.Button)e.Sender;
            if (_isSync)
            {
                sender.BackgroundColor = Colors.Yellow;
                if (Convert.ToInt32(sender.CommandParameter) > 4)
                    _addent2++;
                else
                    _addent1++;
                _waiting_check = true;
                _seconds_pressed = 0;
                if (!_isTimerWorking)
                {
                    timer.Start();
                    _isTimerWorking = true;
                }
            }
            else
            {
                sender.BackgroundColor = (sender.BackgroundColor != Colors.Yellow) ? Colors.Yellow : Colors.White;
            }

            //NotifyPropertyChanged(nameof(SAddent1)); NotifyPropertyChanged(nameof(SAddent2)); NotifyPropertyChanged(nameof(SecondsToEnd));
            //SaveState();
        }

        protected void OnUp(MR.Gestures.DownUpEventArgs e)
        {
            MR.Gestures.Button sender = (MR.Gestures.Button)e.Sender;
            if (_isSync)
            {
                _seconds_pressed = 0; //NotifyPropertyChanged(nameof(SecondsToEnd));
                sender.BackgroundColor = Colors.White;
                if (Convert.ToInt32(sender.CommandParameter) > 4)
                    _addent2--;
                else
                    _addent1--;
                if (_addent1 == 0 && _addent2 == 0) { 
                    _isTimerWorking = false; _waiting_check = false; _seconds_pressed = 0; timer.Stop(); 
                    //NotifyPropertyChanged(nameof(TrueStatement)); 
                }

            }
            else
            {

                if (Convert.ToInt32(sender.CommandParameter) > 4)
                    _addent2= (sender.BackgroundColor != Colors.Yellow)? _addent2 - 1 : _addent2 + 1;
                else
                    _addent1 = (sender.BackgroundColor != Colors.Yellow) ? _addent1 - 1 : _addent1 + 1;
            }
            if (_addent1 < 0) _addent1 = 0;
            if (_addent2 < 0) _addent2 = 0;
            //AddText2("{0} {1}", _addent1, _addent2);
            //SaveState();
            //NotifyPropertyChanged(nameof(SAddent1)); NotifyPropertyChanged(nameof(SAddent2));
        }
    }
}
