using GestureSample.Maui;
using GestureSample.Maui.Models;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;

namespace GestureSample.Views.Tests
{

    public class SimpleViewCellsPage : ContentPage
    {
        private readonly GameConfig _config;

        private bool _isKeyboard { get { return _config.KeyboardConfig != null; } }
        private bool _isThreeTexts
        {
            get
            {
                return _config.UIQuestionType switch
                {
                    UIQuestionType.ThreeTexts => true,
                    UIQuestionType.SimpleEquation => true,
                    UIQuestionType.DecompositionGame => true,
                    _ => false
                };
            }
        }
        private PianoKeyboard _pianoKeyboard = null;
        private PPWGamePlay _gamePlay;

        private GraphicsView leftHandCanvas;
        private GraphicsView rightHandCanvas;


        private readonly int TASK_WIDTH = 120;//TODO: if phone then make smaller and make answer keyboard only notSync
        private Label _lblStatement;
        private Label _lblHistory;
        private Entry _txtAddend1;
        private Entry _txtAddend2;
        private Entry _txtSum;
        private Label _lblAction;
        private BoxView _hr;
        private Entry[] txt;
        private Entry _lastFocused;
        //private  Entry txtResult;
        private PianoKeyboardReadOnly _keyboardTask1;
        private PianoKeyboardReadOnly _keyboardTask2;
        //TODO: show arrows for patterns
        //TODO: Hand image and other images spaces.. To allow a fingu like scenario(just with no moving objects)
        private Button _btnNext = null;
        private Button _btnCheck = null;

        private Command _cmdNext = null;
        private Command _cmdCheck = null;
        private HorizontalStackLayout _hzlEquation;

        //VerticalStackLayout _vsl;
        protected IDispatcherTimer timer;
        protected virtual void TimerInit()
        {
            timer = Application.Current.Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await UpdateStatement();
                });
            };
        }
        private async Task UpdateStatement()
            {
        string text = _gamePlay.Status;
        TimeSpan ts = DateTime.Now.Subtract(_gamePlay.StartTime);
            if (_config.NumberOfTasksToWin > -1 && (_gamePlay.Status == Statement.Neutral || _gamePlay.Status == Statement.True))
            {
                text = string.Format("{0}\n{1} Remaining\n", ts.ToFormattedString("mm:ss"), (_config.NumberOfTasksToWin - _gamePlay._tasksMade).ToString().PadRight(2));
                if (_config.NumberOfMistakesToLose > -1 && _gamePlay._losesMade > 0)
                {
                    text += string.Format("{0} Mistakes left", (_config.NumberOfMistakesToLose - _gamePlay._losesMade).ToString().PadRight(3));
                }
    text += "\n";
            }
            else if (_config.NumberOfTasksToWin > -1)
{
    text += string.Format("\n{0} Remaining\n{1} Mistakes left", (_config.NumberOfTasksToWin - _gamePlay._tasksMade).ToString().PadRight(2),
        (_config.NumberOfMistakesToLose - _gamePlay._losesMade).ToString().PadRight(3));

    text += "\n";
}
_lblStatement.Text = text;

        }

        /*private bool _btnNextEnabled = false;
        public bool BtnNextEnabled { get => _btnNextEnabled; }*/
        #region view updating
        public async Task UpdateView(bool newExercise = false)
        {
               
            await UpdateStatement();
            
            List <Task> tasks = new();

            if (_btnNext != null) _btnNext.IsEnabled = _gamePlay.GuessNumber > 0;
            if (_config.IsHistory) _lblHistory.Text = GenerateHistoryString(_gamePlay.AllHistory.Where(item => item.Sum == _gamePlay.Sum).ToList());
            if (_isThreeTexts)
            {
                _txtAddend1.Text = _gamePlay.addend1 == PPWGamePlay.NAN ? "" : _gamePlay.addend1.ToString();
                _txtAddend2.Text = _gamePlay.addend2 == PPWGamePlay.NAN ? "" : _gamePlay.addend2.ToString();
                _txtSum.Text = _gamePlay.Sum == PPWGamePlay.NAN ? "" : _gamePlay.Sum.ToString();
                _hr.IsVisible = _gamePlay.CurrentOperation == Operation.Multiplication;
                
            }
            if (_config.UIQuestionType == UIQuestionType.CanvasesHands)
            {
                leftHandCanvas.IsVisible = true; rightHandCanvas.IsVisible = true;
                if (_config.SecondsTillHideExercise > 0)
                {
                    tasks.Add(HideGraphicsView(leftHandCanvas, _config.SecondsTillHideExercise));
                    tasks.Add(HideGraphicsView(rightHandCanvas, _config.SecondsTillHideExercise));
                }
            }

            if (_config.SecondsTillAllowInput > 0)
            {
                tasks.Add(DisableTemporeryKeyboard(_pianoKeyboard, _config.SecondsTillAllowInput));
            }

            if (newExercise)
            {
                if (_isThreeTexts)
                {
                    EntryEnabled(_txtAddend1, _gamePlay.addend1 == PPWGamePlay.NAN);
                    EntryEnabled(_txtAddend2, _gamePlay.addend2 == PPWGamePlay.NAN);
                    EntryEnabled(_txtSum, _gamePlay.Sum == PPWGamePlay.NAN);

                   
                }
                if (_config.isHelpEntries)
                    for (int i = 0; i < txt.Length; i++)
                        txt[i].Text = "";

                if (_config.UIQuestionType == UIQuestionType.SimpleEquation)
                    if (Operation.Divide == _gamePlay.CurrentOperation || Operation.Minus == _gamePlay.CurrentOperation)
                        OrderEntries(_hzlEquation, _txtSum, _txtAddend1);
                    else
                        OrderEntries(_hzlEquation, _txtAddend1, _txtSum);
                if (_config.UIQuestionType == UIQuestionType.LogicalKeyboards)
                {
                    _keyboardTask2.PianoInit(((BitArrayGamePlay)_gamePlay).BitArrayQuestion2);
                    _keyboardTask2.IsVisible = GameConfig.Operations.LogicalDual.Contains(((BitArrayGamePlay)_gamePlay).CurrentOperation);
                    _keyboardTask1.PianoInit(((BitArrayGamePlay)_gamePlay).BitArrayQuestion);
                }
                if (_config.UIQuestionType == UIQuestionType.CanvasesHands)
                {
                    ((BitArrayGamePlay)_gamePlay).BitArrayforHands(((HandDrawable)leftHandCanvas.Drawable).Bits, ((HandDrawable)rightHandCanvas.Drawable).Bits);
                    leftHandCanvas.Invalidate();
                    rightHandCanvas.Invalidate();
                }

                if (_config.KeyboardConfig != null && _config.KeyboardConfig.IsArrow)
                {
                    _pianoKeyboard.RemoveArrows();
                    _pianoKeyboard.AddArrow(((BitArrayGamePlay)_gamePlay).dir, ((BitArrayGamePlay)_gamePlay).aboveNumber, ((BitArrayGamePlay)_gamePlay).length);
                    //if (aboveNumber == 10) { _pianoKeyboard.AddArrow(dir, 0/*, _gamePlay.Sum*/); }

                }
                if (_lblAction != null) _lblAction.Text = _gamePlay.CurrentOperation.ToDString();
                if (_isKeyboard && !_config.FromNumToNum)
                {
                    _pianoKeyboard.PianoInit();
                }
                if (tasks.Count > 0) await Task.WhenAll(tasks);
                
            }
            if (_isThreeTexts && _config.KeyboardConfig == null)
            {
                if (_gamePlay.Status == Statement.False ||
         _gamePlay.Status == Statement.WrongInput ||
         _gamePlay.Status == Statement.New)
                    await ForceFocusAsync(_lastFocused);
                else
                {
                    _txtAddend1.ReturnCommand = null;

                    if (_gamePlay.Sum == PPWGamePlay.NAN)
                    {
                        await ForceFocusAsync(_txtSum);
                        _lastFocused = _txtSum;
                    }
                    else if (_gamePlay.addend1 == PPWGamePlay.NAN)
                    {
                        await ForceFocusAsync(_txtAddend1);
                        _lastFocused = _txtAddend1;
                    }
                    else
                    {
                        await ForceFocusAsync(_txtAddend2);
                        _lastFocused = _txtAddend2;
                    }
                }
            }
        }
        private async Task ForceFocusAsync(Entry entry, int delayMilliseconds = 50)
        {
            // Ensure we're on the UI thread
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (entry.IsFocused)
                {
                    entry.Unfocus();
                }
            });

            // Wait for the unfocus to register
            await Task.Delay(delayMilliseconds);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                entry.Focus();
            });
        }
        private static void EntryEnabled(Entry ent, bool enabled)
        {
            ent.IsEnabled = enabled;
            ent.TextColor = enabled ? Colors.Black : Colors.Gray;

        }

        private void OrderEntries(HorizontalStackLayout layout, Entry entFirst, Entry entSecond)
        {
            int index1 = -1, index2 = -1;
            //HorizontalStackLayout layout2;
            for (int i = 0; i < layout.Children.Count; i++)
            {
                if (layout.Children[i] == entFirst) index1 = i;

                if (layout.Children[i] == entSecond) index2 = i;

            }
            if (index1 < 0 || index2 < 0)
            {
                throw new ArgumentOutOfRangeException("Index is out of range.");
            }
            if (index2 > index1) return;


            _hzlEquation.Children.RemoveAt(index1);
            _hzlEquation.Children.RemoveAt(index2);
            _hzlEquation.Children.Insert(index2, entFirst);
            _hzlEquation.Children.Insert(index1, entSecond);
        }


        public static async Task HideGraphicsView(GraphicsView obj, int seconds)
        {

            await Task.Delay(seconds * 1000); // Simulate a 2-second operation
            obj.IsVisible = false;
        }

        public static async Task DisableTemporeryKeyboard(Grid pianoKeyboard, int seconds)
        {
            pianoKeyboard.IsEnabled = false;
            await Task.Delay(seconds * 1000);
            pianoKeyboard.IsEnabled = true;
        }


        private static string GenerateHistoryString(List<PPWObject> ppwHistoryArray)
        {
            String strHistory = "HISTORY:\n";
            foreach (PPWObject ppw in ppwHistoryArray)
                strHistory += ppw.Addend1 + "\t" + ppw.Addend2 + "\n";

            return strHistory;
        }

        #endregion


        public SimpleViewCellsPage(GameConfig config)
        {
            _config = config;
            if (_config.NumberOfTasksToWin > -1)
            {
                TimerInit();
                timer.Start();
            }
            InitializeGamePlay();
            InitializeUI();

            _gamePlay.GenerateExercise();
        }

        private void InitializeGamePlay()
        {
            _gamePlay = new PPWGamePlay(this, _config);
            if (_config.KeyboardConfig != null && _config.KeyboardConfig.IsArrow)
                _gamePlay = new BitArrayGamePlay(this, _config);
            _cmdCheck = new Command(CheckGamePlay);
            _cmdNext = new Command(GenerateNextExercise);
        }

        private async void CheckGamePlay()
        {
            if (_isKeyboard && !_config.KeyboardConfig.KeyboardOnlyForHelp)
            {
                bool isCorrect = await _gamePlay.CheckAsync(_pianoKeyboard);
                if(isCorrect) GenerateNextExercise();

            }
            else
            {
                try
                {
                    if (await _gamePlay.Check(Convert.ToInt32(_txtAddend1.Text), Convert.ToInt32(_txtAddend2.Text), Convert.ToInt32(_txtSum.Text)))
                    {
                        if (_config.NumberOfTasksToWin < 0)//TODO: Check if ok to remove or change condition
                        {
                            _txtAddend1.IsEnabled = false; _txtAddend2.IsEnabled = false; _txtSum.IsEnabled = false;
                            await Task.Delay(_config.SecondsTillNextExercise * 1000);
                            _txtAddend1.IsEnabled = true; _txtAddend2.IsEnabled = true; _txtSum.IsEnabled = true;
                        }
                        if (!_gamePlay.GameOver) GenerateNextExercise();
                    };
                }
                catch
                {
                    _lblStatement.Text = Statement.WrongInput;
                }
            }
        }

        private void GenerateNextExercise()
        {
            _gamePlay.GenerateExercise();
            if (_isKeyboard)
            {
                if (_config.FromNumToNum)
                {
                    _pianoKeyboard.IsEnabled = true;
                }
                else
                {
                    _pianoKeyboard.PianoInit();
                }
            }
        }



        private void InitializeUI()
        {
            bool isPianoHigh = _isKeyboard && _config.KeyboardConfig.SyncType!=SyncType.None && (_config.UIQuestionType == UIQuestionType.OnlyKeyboard || !_config.KeyboardConfig.KeyboardOnlyForHelp);
            Grid grid = new()
            {
                RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(40, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(_isKeyboard ? (isPianoHigh?240:40) : 1, GridUnitType.Star) }
            },
                ColumnDefinitions =
            {
                new ColumnDefinition()
            }
            };

            grid.Add(new BoxView
            {
                Color = Colors.AntiqueWhite
            });



            _lblStatement = new Label
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start,
                FontSize = 18,
                TextColor = Colors.Black,
                Text = Statement.Neutral
            };
            VerticalStackLayout vsl = new()
        {
            _lblStatement
            };

            vsl.HorizontalOptions = LayoutOptions.Center;
            vsl.Padding = 15;
            vsl.Spacing = 10;

            if (_isThreeTexts)
            {
                InitTextsUI();
                if (_config.UIQuestionType == UIQuestionType.SimpleEquation)
                {
                    _hzlEquation = InitEquationUI();
                    vsl.Add(_hzlEquation);
                }
                else
                {
                    txt = new Entry[6];
                    for (int i = 0; i < txt.Length; i++)
                    {
                        txt[i] = new Entry
                        {
                            HorizontalOptions = LayoutOptions.Center,
                            HorizontalTextAlignment = TextAlignment.Start,
                            BackgroundColor = Colors.White,
                            TextColor = Colors.Black,
                            WidthRequest = TASK_WIDTH / 4,
                            FontSize = 18,
                            IsVisible = !_isKeyboard || _config.KeyboardConfig.KeyboardOnlyForHelp
                        };
                        txt[i].Keyboard = Keyboard.Numeric;
                    }
                    if (_config.isHelpEntries)
                        vsl.Add(new HorizontalStackLayout { HorizontalOptions = LayoutOptions.Center, Children = { txt[0], txt[1] } });
                    vsl.Add(new HorizontalStackLayout { HorizontalOptions = LayoutOptions.Center, Children = { _txtSum } });

                    if (_config.OperationList.Contains(Operation.Multiplication))
                        vsl.Add(new HorizontalStackLayout { HorizontalOptions = LayoutOptions.Center, Children = { _hr } });
                    vsl.Add(new HorizontalStackLayout { HorizontalOptions = LayoutOptions.Center, Children = { _txtAddend1, _lblAction, _txtAddend2 } });
                    if (_config.isHelpEntries)
                        vsl.Add(new HorizontalStackLayout { HorizontalOptions = LayoutOptions.Center, Children = { txt[2], txt[3], txt[4], txt[5] } });
                }
            }

            if (!_isKeyboard ||
                _config.KeyboardConfig.SyncType == SyncType.None ||
                _config.KeyboardConfig.KeyboardOnlyForHelp)
            {
                vsl.Add(InitButtonsUI());
            }

            if (_config.IsHistory)
            {
                _lblHistory = new Label
                {
                    Text = "History:\n",
                    HorizontalOptions = LayoutOptions.Center
                };
                vsl.Add(_lblHistory);
            }

            if (_config.UIQuestionType == UIQuestionType.LogicalKeyboards)
            {
                _gamePlay = new BitArrayGamePlay(this, _config);
                vsl.Add(InitLogicalKeyboardsUI());
                vsl.Padding = 0;
                vsl.Spacing = 0;
                vsl.HorizontalOptions = LayoutOptions.Fill;
            }
            if (_config.UIQuestionType == UIQuestionType.CanvasesHands)
            {
                _gamePlay = new BitArrayGamePlay(this, _config);
                vsl.Add(InitCanvasComponentsUI());


            }


            if (_config.UIQuestionType == UIQuestionType.DecompositionGame)
            {
                vsl.Add(InitDecompositionGameUI());
            }


            grid.Add(vsl);


            if (_isKeyboard)
            {
                _pianoKeyboard = _config.KeyboardConfig.SyncType switch
                {
                    SyncType.HalfSync => new PianoKeyboardHalfSync(_gamePlay, _lblStatement, _config.KeyboardConfig),
                    SyncType.Sync or SyncType.Spatial => new PianoKeyboardSync(_gamePlay, _lblStatement, _config.KeyboardConfig),
                    _ => new PianoKeyboard(_gamePlay, _lblStatement, _config.KeyboardConfig)
                };
                grid.Add(_pianoKeyboard);
                Grid.SetRow(_pianoKeyboard, 2);
            }
            Content = grid;

        }

        private HorizontalStackLayout InitEquationUI()
        {
            _txtAddend1.WidthRequest = TASK_WIDTH / 2;
            _txtAddend2.WidthRequest = TASK_WIDTH / 2;
            _txtSum.WidthRequest = TASK_WIDTH / 2;
            _txtSum.BackgroundColor = Colors.White;
            _txtSum.FontSize = 18;
            HorizontalStackLayout hzlEquation = new HorizontalStackLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                Children ={ _txtAddend1, _lblAction, _txtAddend2,
                            new Label {FontSize=18, WidthRequest=20, HorizontalTextAlignment=TextAlignment.Center, VerticalTextAlignment=TextAlignment.Center, Text = "=" },
                            _txtSum }
            };
            return hzlEquation;
        }


        private VerticalStackLayout InitDecompositionGameUI()
        {
            VerticalStackLayout vslDecompositionDashboard = new() { };

            Label lblStats = new();
            Picker pc = new()
            {
                Title = "Level"
            };
            pc.Items.Add("Level 1");
            pc.Items.Add("Level 2");
            pc.Items.Add("Level 3");
            pc.Items.Add("Level 4");

            _gamePlay = new DecompositionGamePlay(this, _config, lblStats, pc);

            pc.SelectedIndex = 1;
            pc.SelectedIndexChanged += ((DecompositionGamePlay)_gamePlay).SelectedIndexChanged;


            vslDecompositionDashboard.Add(pc);
            vslDecompositionDashboard.Add(lblStats);

            return vslDecompositionDashboard;
        }


        private VerticalStackLayout InitLogicalKeyboardsUI()
        {
            VerticalStackLayout vsl = new();
            _gamePlay = new BitArrayGamePlay(this, _config);
            _keyboardTask2 = new PianoKeyboardReadOnly(_config.KeyboardConfig)
            {
                HorizontalOptions = LayoutOptions.Fill,
                HeightRequest = TASK_WIDTH / 2
            };
            _lblAction = new Label
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start,
                FontSize = 18,
                TextColor = Colors.Black
            };
            _keyboardTask1 = new PianoKeyboardReadOnly(_config.KeyboardConfig)
            {
                HorizontalOptions = LayoutOptions.Fill,
                HeightRequest = TASK_WIDTH / 2
            };
            vsl.Add(_keyboardTask2);
            vsl.Add(_lblAction);
            vsl.Add(_keyboardTask1);
            return vsl;
        }


        private StackLayout InitCanvasComponentsUI()
        {
            _lblAction = new Label
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start,
                FontSize = 18,
                TextColor = Colors.Black
            };
            leftHandCanvas = new()
            {
                HeightRequest = TASK_WIDTH,
                WidthRequest = TASK_WIDTH / 2,
                Drawable = new HandDrawable(isLeftHand: true)
            };

            rightHandCanvas = new()
            {
                HeightRequest = TASK_WIDTH,
                WidthRequest = TASK_WIDTH / 2,
                Drawable = new HandDrawable(isLeftHand: false)
            };




            StackLayout stackLayout = new(){
                new VerticalStackLayout{ _lblAction, new HorizontalStackLayout
            {
                Children = { leftHandCanvas, rightHandCanvas }
            } }
            };



            return stackLayout;
        }



        private HorizontalStackLayout InitButtonsUI()
        {
            HorizontalStackLayout hslBtns = new()
            {
                Padding = 20,
                Spacing = 10,
                HorizontalOptions = LayoutOptions.Center
            };

            _btnCheck = new()
            {
                Text = "Check",
                Command = _cmdCheck,
                HorizontalOptions = LayoutOptions.Center
            };

            _btnNext = new Button
            {
                Text = "Next",
                Command = _cmdNext,
                HorizontalOptions = LayoutOptions.Center
            };

            
            if (_config.NumberOfMistakesToLose < 0 && !_config.IsHistory)
            {   hslBtns.Add(_btnCheck);
                hslBtns.Add(_btnNext);
            }
            /*if(_config.NumberOfMistakesToLose >= 0 && OperatingSystem.IsIOS())
            {  
                hslBtns.Add(_btnCheck);
            }*/

            return hslBtns;
        }

        private void InitTextsUI()
        {
            bool isLblAction = _config.EnforceOperationLabel || _config.OperationList.Count > 1;

            _txtSum = new Entry
            {
                Keyboard = Keyboard.Numeric,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                BackgroundColor = Colors.White,
                TextColor = Colors.Black,
                WidthRequest = TASK_WIDTH,
                FontSize = 32,
                IsVisible = _config.UIQuestionType != UIQuestionType.OnlyKeyboard,
               
            };
            

                _txtAddend1 = new Entry
            {
                Keyboard = Keyboard.Numeric,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                BackgroundColor = Colors.White,
                TextColor = Colors.Black,
                WidthRequest = TASK_WIDTH / 2 - ((isLblAction) ? 10 : 0),
                FontSize = 18,
                IsVisible = !_isKeyboard || _config.KeyboardConfig.KeyboardOnlyForHelp
            };
            _lblAction = new Label
            {
                FontSize = 18,
                TextColor = Colors.Black,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                WidthRequest = 20,
                IsVisible = isLblAction
            };
            _hr = new BoxView
            {
                HeightRequest = 2,
                WidthRequest = TASK_WIDTH,
                BackgroundColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center,
                IsVisible = false
            };

            _txtAddend2 = new Entry
            {
                Keyboard = Keyboard.Numeric,

                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                BackgroundColor = Colors.White,
                TextColor = Colors.Black,
                WidthRequest = TASK_WIDTH / 2 - ((isLblAction) ? 10 : 0),
                FontSize = 18,
                IsVisible = !_isKeyboard || _config.KeyboardConfig.KeyboardOnlyForHelp
            };
            _txtAddend1.Keyboard = Keyboard.Numeric;
            _txtAddend2.Keyboard = Keyboard.Numeric;
            _txtSum.Keyboard = Keyboard.Numeric;

            _lastFocused = _txtSum;

            _txtSum.Completed += (sender, e) =>
            {
                CheckGamePlay();
            };
            _txtAddend1.Completed += (sender, e) =>
            {
                if (_config.VariableTypes != VariableTypes.TwoNoSum )
                    CheckGamePlay();
                else
                    _txtAddend2.Focus();
            };
            _txtAddend2.Completed += (sender, e) =>
            {
                CheckGamePlay();
            };
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            //_pianoKeyboard?.Dispose();
        }
    }
}
