using GestureSample.Maui;
using GestureSample.Maui.Models;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Graphics;
using System.Diagnostics;

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
                    UIQuestionType.OneText => true,
                    UIQuestionType.SimpleEquation => true,
                    UIQuestionType.DecompositionGame => true,
                    _ => false
                };
            }
        }
        public new bool IsEnabled
        {
            get => _pianoKeyboard?.IsEnabled ?? true;
            set
            {
                if(_pianoKeyboard!=null) _pianoKeyboard.IsEnabled = value;
                if (_btnNext != null && _btnCheck!=null && _btnCheck.IsVisible)
                {
                    _btnNext.IsEnabled = value ? (_gamePlay.GuessNumber > 0) : false;
                    _btnCheck.IsEnabled = value;
                    //if(_btnPrev!=null) 
                    //    _btnPrev.IsEnabled = value;
                    if (value) _lblStatement.Text = Statement.Neutral;
                }

            }
        }

        private PianoKeyboard _pianoKeyboard = null;
        private PPWGamePlay _gamePlay;

        private GraphicsView leftHandCanvas;
        private GraphicsView rightHandCanvas;

        // Added field for tutorial hand drawable view
        private GraphicsView handGraphicsView;

        private readonly int FONT_SIZE_DEFAULT = 18;
        private readonly int TASK_WIDTH = 180;//TODO: if phone then make smaller and make answer keyboard only notSync
        private readonly int PIANO_HEIGHT1 = 90;
        private readonly int PIANO_HEIGHT2 = 60;
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
        private Button _btnPrev = null;

        private PPWObject _currentPPW;
        private PPWObject _currentPPWEnabled;
        private PPWObject _previousPPW = null;

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

        public async Task AddToLblAction(string text)
        {
            _lblAction.Text += text;
        }

        public async Task UpdateView(bool newExercise = false)
        {

            await UpdateStatement();

            List<Task> tasks = new();

            if (_btnNext != null) _btnNext.IsEnabled = _gamePlay.GuessNumber > 0 && !newExercise;
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
                if (_btnNext != null) { _btnNext.IsEnabled = _gamePlay.GuessNumber > 0 && !newExercise; Console.WriteLine(" _gamePlay.GuessNumber: {0}", _gamePlay.GuessNumber); }

                tasks.Add(DisableTemporeryKeyboard(_pianoKeyboard, _config.SecondsTillAllowInput));
            }

            if (newExercise)
            {
                if (_isThreeTexts)
                {
                    EntryEnabled(_txtAddend1, _gamePlay.addend1 == PPWGamePlay.NAN && !(_isKeyboard && !_config.KeyboardConfig.KeyboardOnlyForHelp));
                    EntryEnabled(_txtAddend2, _gamePlay.addend2 == PPWGamePlay.NAN && !(_isKeyboard && !_config.KeyboardConfig.KeyboardOnlyForHelp));
                    EntryEnabled(_txtSum, _gamePlay.Sum == PPWGamePlay.NAN && !(_isKeyboard && !_config.KeyboardConfig.KeyboardOnlyForHelp));
                    _currentPPW = new PPWObject(_gamePlay.addend1, _gamePlay.addend2, _gamePlay.Sum);
                    _currentPPWEnabled = new PPWObject(
                        _txtAddend1.IsEnabled ? 1 : 0,
                        _txtAddend2.IsEnabled ? 1 : 0,
                        _txtSum.IsEnabled ? 1 : 0);

                }
                if (_config.isHelpEntries || _config.isHelpThroughTen)
                    for (int i = 0; i < txt.Length; i++)
                        txt[i].Text = "";
                if (_config.isHelpThroughTen)
                {
                    txt[1].IsEnabled = true;
                    //if (_gamePlay.addend1 != PPWGamePlay.NAN)
                        txt[0].Text = "10";//((_gamePlay.addend1 / 10 + 1) * 10).ToString();
                    /*txt[0].WidthRequest = 2 * TASK_WIDTH / 3;
                    txt[0].IsEnabled = false;
                    txt[1].WidthRequest = TASK_WIDTH / 3;
                    txt[1].IsEnabled = true;*/
                
                /* else if (_gamePlay.addend2 != PPWGamePlay.NAN)
                     {
                         txt[1].Text = ((_gamePlay.addend2 / 10 + 1) * 10).ToString();
                         txt[1].WidthRequest = 2* TASK_WIDTH / 3;
                         txt[1].IsEnabled = false;
                         txt[0].WidthRequest = TASK_WIDTH / 3;
                         txt[0].IsEnabled = true;
                     }*/
                    if (_gamePlay.Sum != PPWGamePlay.NAN)
                    {
                        txt[1].Text = (_gamePlay.Sum - 10).ToString();
                        txt[1].IsEnabled = false;

                    }
                    txt[4].Text = txt[1].Text;
                    txt[2].Text = _txtAddend1.Text;
                }

                if (_config.UIQuestionType == UIQuestionType.SimpleEquation)
                    if (Operation.Divide == _gamePlay.CurrentOperation || Operation.Minus == _gamePlay.CurrentOperation)
                        OrderEntries(_hzlEquation, _txtSum, _txtAddend1);
                    else
                        OrderEntries(_hzlEquation, _txtAddend1, _txtSum);
                if (_config.UIQuestionType == UIQuestionType.LogicalKeyboards)
                {
                    _keyboardTask2.PianoInit(((BitArrayGamePlay)_gamePlay).BitArrayQuestion2);
                    if (GameConfig.Operations.LogicalDual.Contains(((BitArrayGamePlay)_gamePlay).CurrentOperation))
                    {
                        _keyboardTask2.IsVisible = true;
                        _keyboardTask1.HeightRequest = PIANO_HEIGHT2;
                        _keyboardTask2.HeightRequest = PIANO_HEIGHT2;
                    } else
                    {
                        _keyboardTask2.IsVisible = false;
                        _keyboardTask1.HeightRequest = PIANO_HEIGHT1;
                        _keyboardTask2.HeightRequest = PIANO_HEIGHT1;
                    }
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
                    Console.WriteLine("aboveNumver: {0}, length: {1}", ((BitArrayGamePlay)_gamePlay).aboveNumber, ((BitArrayGamePlay)_gamePlay).length);
                    _pianoKeyboard.AddArrow(((BitArrayGamePlay)_gamePlay).dir, ((BitArrayGamePlay)_gamePlay).aboveNumber, ((BitArrayGamePlay)_gamePlay).length);
                    //if (aboveNumber == 10) { _pianoKeyboard.AddArrow(dir, 0/*, _gamePlay.Sum*/); }

                }
                if (_isKeyboard && _config.FromNumToNum)
                {
                    _pianoKeyboard.initColors = _pianoKeyboard.ToBitArray();
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
            Title = config.GameName;
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
            if (_btnNext != null) _btnNext.IsEnabled = false;
            //if (_btnPrev != null) _btnPrev.IsEnabled = false;

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
                        _previousPPW = new PPWObject(Convert.ToInt32(_txtAddend1.Text), Convert.ToInt32(_txtAddend2.Text), Convert.ToInt32(_txtSum.Text));
                        if (_config.NumberOfTasksToWin < 0)//TODO: Check if ok to remove or change condition
                        {
                            _txtAddend1.IsEnabled = false; _txtAddend2.IsEnabled = false; _txtSum.IsEnabled = false;
                            await Task.Delay(_config.SecondsTillNextExercise * 1000);
                            _txtAddend1.IsEnabled = true; _txtAddend2.IsEnabled = true; _txtSum.IsEnabled = true; 
                        }
                        if (!_gamePlay.GameOver) GenerateNextExercise();
                    }
                    else
                    {
                        Console.WriteLine("Wrong answer");
                        await Task.Delay(_config.SecondsTillNextExercise * 1000); 
                    }
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



        // Changed to async so we can await tutorial animation without changing constructor call site.
        private async void InitializeUI()
        {
            bool isPianoHigh = _isKeyboard && _config.KeyboardConfig.SyncType!=SyncType.None && (_config.UIQuestionType == UIQuestionType.OnlyKeyboard || !_config.KeyboardConfig.KeyboardOnlyForHelp);
            int pianoHeight = _isKeyboard ? (isPianoHigh ? 100 : 60) : 1;
            if (_isKeyboard && _config.KeyboardConfig.IsArrow) pianoHeight = 200;
            Grid grid = new()
            {
                RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(40, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(pianoHeight, GridUnitType.Star) }
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
                FontSize = _isKeyboard?((_config.UIQuestionType == UIQuestionType.LogicalKeyboards)?40:55): FONT_SIZE_DEFAULT,
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
                            HorizontalTextAlignment = TextAlignment.Center,
                            BackgroundColor = Colors.White,
                            TextColor = Colors.Black,
                            WidthRequest = TASK_WIDTH / 4,
                            FontSize = FONT_SIZE_DEFAULT,
                            IsVisible = !_isKeyboard || _config.KeyboardConfig.KeyboardOnlyForHelp
                        };
                        txt[i].Keyboard = Keyboard.Numeric;
                    }
                    Label lbl2 = new Label
                    {
                        Text = "",
                        FontSize = FONT_SIZE_DEFAULT,
                        WidthRequest = TASK_WIDTH / 2,
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center
                    };
                    Label lbl4 = new Label
                    {
                        Text = "",
                        FontSize = FONT_SIZE_DEFAULT,
                        WidthRequest = TASK_WIDTH / 4,  
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center
                    };
                    if (_config.isHelpThroughTen)
                    {
                        txt[0].IsEnabled = false;
                        txt[1].IsEnabled = false;
                        txt[0].WidthRequest = 3* TASK_WIDTH / 4;
                        txt[1].WidthRequest =  TASK_WIDTH / 4;
                        txt[2].WidthRequest =  TASK_WIDTH / 2;
                        txt[3].WidthRequest =  TASK_WIDTH / 4;
                        txt[4].WidthRequest = TASK_WIDTH / 4;
                    }

                    if (_config.isHelpEntries)
                        vsl.Add(new HorizontalStackLayout { HorizontalOptions = LayoutOptions.Center, Children = { txt[0], txt[1] } });
                    vsl.Add(new HorizontalStackLayout { HorizontalOptions = LayoutOptions.Center, Children = { _txtSum } });

                    if (_config.isHelpThroughTen)
                    {
                        vsl.Add(new HorizontalStackLayout { HorizontalOptions = LayoutOptions.Center, Children = { txt[0], txt[1] } });
                        //vsl.Add(new HorizontalStackLayout { HorizontalOptions = LayoutOptions.Center, Children = { lbl2, txt[3] , lbl4 } });
                    }
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

                if(_config.IncludeTutorials)
                {
                    Debug.WriteLine("Starting tutorial hand animation...");

                    // create the drawable
                    var hand = new HandDrawable(isLeftHand: false)
                    {
                        Bits = new[] { 1, 1, 1, 1, 1 },
                        Position = new PointF(0, 0),
                        Opacity = 0f
                    };

                    // create a full-screen overlay GraphicsView so hand coordinates are page-relative
                    handGraphicsView ??= new GraphicsView
                    {
                        Drawable = hand,
                        HorizontalOptions = LayoutOptions.Fill,
                        VerticalOptions = LayoutOptions.Fill,
                        BackgroundColor = Colors.Red.WithAlpha(0.30f),
                        InputTransparent = true, // let touches pass through
                        // ensure it renders above other content
                        ZIndex = 100
                    };

                    // add overlay to grid (span all rows/columns so it floats above everything)
                                if (!grid.Children.Contains(handGraphicsView))
                    {
                        grid.Add(handGraphicsView);
                        Grid.SetRowSpan(handGraphicsView, grid.RowDefinitions.Count);
                        Grid.SetColumnSpan(handGraphicsView, grid.ColumnDefinitions.Count);
                    }

                    // Wait for layout (size) then center the hand and force redraw
                    void OnSizeChanged(object? s, EventArgs e)
                    {
                        if (handGraphicsView.Width > 0 && handGraphicsView.Height > 0)
                        {
                            hand.Position = new PointF((float)(handGraphicsView.Width / 2.0), (float)(handGraphicsView.Height / 2.0));
                            handGraphicsView.Invalidate();
                            handGraphicsView.SizeChanged -= OnSizeChanged;
                        }
                    }
                    handGraphicsView.SizeChanged += OnSizeChanged;


                    // Wait a short time for layout to settle so overlay has a valid size, then animate to center
                    // await Task.Delay(50);
                    var target = new PointF((float)(handGraphicsView.Width / 2.0), (float)(handGraphicsView.Height / 2.0));
                 //   await hand.ShowMoveHideAsync(handGraphicsView, target, TimeSpan.FromMilliseconds(8000), TimeSpan.FromMilliseconds(2500));
                }
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
                //_lblStatement.FontSize = 55;
                _pianoKeyboard = _config.KeyboardConfig.SyncType switch
                {
                    SyncType.HalfSync => new PianoKeyboardHalfSync(_gamePlay, _lblStatement, _config.KeyboardConfig),
                    SyncType.Sync or SyncType.Spatial => new PianoKeyboardSync(_gamePlay, _lblStatement, _config.KeyboardConfig),
                    _ => new PianoKeyboard(_gamePlay, _lblStatement, _config.KeyboardConfig)
                };
                if (_config.KeyboardConfig.KeyboardAsAQuestion) {
                    _pianoKeyboard = (PianoKeyboard)new PianoKeyboardReadOnly(_config.KeyboardConfig);
                }
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
            _txtSum.FontSize = FONT_SIZE_DEFAULT;
            HorizontalStackLayout hzlEquation = new HorizontalStackLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                Children ={ _txtAddend1, _lblAction, _txtAddend2,
                            new Label {FontSize=FONT_SIZE_DEFAULT, WidthRequest=20, HorizontalTextAlignment=TextAlignment.Center, VerticalTextAlignment=TextAlignment.Center, Text = "=" },
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
                HeightRequest = PIANO_HEIGHT2
            };
            _lblAction = new Label
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start,
                FontSize = FONT_SIZE_DEFAULT,  
                TextColor = Colors.Black
            };
            _keyboardTask1 = new PianoKeyboardReadOnly(_config.KeyboardConfig)
            {
                HorizontalOptions = LayoutOptions.Fill,
                HeightRequest = PIANO_HEIGHT2
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
                FontSize = FONT_SIZE_DEFAULT,
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

            if (_config.ShowPrev)
            {
                _btnPrev = new Button
                {
                    Text = "Prev",
                    
                    HorizontalOptions = LayoutOptions.Center
                };
                _btnPrev.Pressed += (_, _) => {
                    if (_previousPPW != null)
                    {
                        _txtAddend1.Text = _previousPPW.Addend1.ToString(); _txtAddend1.IsEnabled = false;
                        _txtAddend2.Text = _previousPPW.Addend2.ToString(); _txtAddend2.IsEnabled = false;
                        _txtSum.Text = _previousPPW.Sum.ToString(); _txtSum.IsEnabled = false;
                    }

                };
                _btnPrev.Released += (_, _) =>
                {
                    _txtAddend1.IsEnabled = _currentPPWEnabled.Addend1 == 1;
                    _txtAddend2.IsEnabled = _currentPPWEnabled.Addend2 == 1;
                    _txtSum.IsEnabled = _currentPPWEnabled.Sum == 1;
                    _txtAddend1.Text = _currentPPW.Addend1 == PPWGamePlay.NAN ? "" : _currentPPW.Addend1.ToString();
                    _txtAddend2.Text = _currentPPW.Addend2 == PPWGamePlay.NAN ? "" : _currentPPW.Addend2.ToString();
                    _txtSum.Text = _currentPPW.Sum == PPWGamePlay.NAN ? "" : _currentPPW.Sum.ToString();
                    //ForceFocusAsync(_lastFocused);

                };

                hslBtns.Add(_btnPrev);
            }
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
                FontSize = FONT_SIZE_DEFAULT,
                IsVisible = _config.UIQuestionType == UIQuestionType.SimpleEquation || _config.UIQuestionType == UIQuestionType.ThreeTexts || _config.UIQuestionType == UIQuestionType.DecompositionGame
                };
            _lblAction = new Label
            {
                FontSize = FONT_SIZE_DEFAULT,
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
                FontSize = FONT_SIZE_DEFAULT,
                IsVisible = _config.UIQuestionType == UIQuestionType.SimpleEquation || _config.UIQuestionType == UIQuestionType.ThreeTexts || _config.UIQuestionType == UIQuestionType.DecompositionGame
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
