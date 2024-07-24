using GestureSample.Maui.Models;
using GestureSample.Maui;
using System.Linq;
using System.Diagnostics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Skia;
using System;

namespace GestureSample.Views.Tests
{

    public class SimpleViewCellsPage : ContentPage
    {
        private readonly GameType _gameType;
        private readonly bool _isKeyboard;
        private PianoKeyboard _pianoKeyboard = null;
        private PPWGamePlay _gamePlay;

        private GraphicsView leftHandCanvas;
        private GraphicsView rightHandCanvas;

       
        private readonly int TASK_WIDTH = 240;//TODO: if phone then make smaller and make answer keyboard only notSync
        private Label lblStatement;
        private Label lblHistory;
        private Entry txtAddend1;
        private Entry txtAddend2;
        private Entry txtSum;
        private Label lblAction;
        //private  Entry txtResult;
        private PianoKeyboardReadOnly keyboardTask1;
        private PianoKeyboardReadOnly keyboardTask2;
        //TODO: show arrows for patterns
        //TODO: Hand image and other images spaces.. To allow a fingu like scenario(just with no moving objects)
        private Button btnNext = null;
        private Button btnCheck = null;

        private Command cmdNext = null;
        private Command cmdCheck = null;
        /*private bool _btnNextEnabled = false;
        public bool BtnNextEnabled { get => _btnNextEnabled; }*/
 #region view updating
        private static void EntryEnabled(Entry ent, bool enabled)
        {
            ent.IsEnabled = enabled;
            ent.TextColor = enabled ? Colors.Black : Colors.Gray;

        }
        public void UpdateView(bool newExercise = false)
        {
            lblStatement.Text = _gamePlay.Status;

            if (_config.UIQuestionType == UIQuestionType.ThreeTexts)
            {
                txtAddend1.Text = _gamePlay.addend1 == PPWGamePlay.NAN ? "" : _gamePlay.addend1.ToString();
                txtAddend2.Text = _gamePlay.addend2 == PPWGamePlay.NAN ? "" : _gamePlay.addend2.ToString();
                txtSum.Text = _gamePlay.Sum == PPWGamePlay.NAN ? "" : _gamePlay.Sum.ToString();

                if (newExercise)
                {
                    EntryEnabled(txtAddend1, _gamePlay.addend1 == PPWGamePlay.NAN);
                    EntryEnabled(txtAddend2, _gamePlay.addend2 == PPWGamePlay.NAN);
                    EntryEnabled(txtSum, _gamePlay.Sum == PPWGamePlay.NAN);
                }
            }
            if (btnNext != null) btnNext.IsEnabled = _gamePlay.GuessNumber > 0;

            if (_config.UIQuestionType == UIQuestionType.BitArrayQuestion && newExercise)
            {
                if (_config.ArrayQuestionTypes == ArrayQuestionTypes.Keyboard)
                {
                    keyboardTask1.PianoInit(((BitArrayGamePlay)_gamePlay).bitArrayQuestion);
                }
                if (_config.ArrayQuestionTypes == ArrayQuestionTypes.Hand)
                {
                    ((BitArrayGamePlay)_gamePlay).BitArrayforHands(((HandDrawable)leftHandCanvas.Drawable).Bits, ((HandDrawable)rightHandCanvas.Drawable).Bits);
                    leftHandCanvas.Invalidate();
                    rightHandCanvas.Invalidate();
                }
            }

            if (_isKeyboard && !_config.FromNumToNum && newExercise) _pianoKeyboard.PianoInit();
            if (_config.IsHistory) lblHistory.Text = GenerateHistoryString(_gamePlay.AllHistory.Where(item => item.Sum == _gamePlay.Sum).ToList());
        }

        private static string GenerateHistoryString(List<PPWObject> ppwHistoryArray)
        {
            String strHistory = "HISTORY:\n";
            foreach (PPWObject ppw in ppwHistoryArray)
                strHistory += ppw.Addend1 + "\t" + ppw.Addend2 + "\n";

            return strHistory;
        }

        #endregion

        private readonly GameConfig _config;

        public SimpleViewCellsPage(GameConfig config)
        {
            _config = config;
            _gameType = config.GameType;

            _isKeyboard = _config.KeyboardConfig != null;

            InitializeGamePlay();
            InitializeUI();

            _gamePlay.GenerateExercise();
        }

        private void InitializeGamePlay()
        {
            _gamePlay = new PPWGamePlay(_gameType, this, _config);
            cmdCheck = new Command(() =>
            {
                if (_isKeyboard && !_config.KeyboardConfig.KeyboardOnlyForHelp)
                    _pianoKeyboard.IsEnabled = !_gamePlay.Check(_pianoKeyboard);
                else
                    try
                    {
                        _gamePlay.Check(Convert.ToInt32(txtAddend1.Text), Convert.ToInt32(txtAddend2.Text), Convert.ToInt32(txtSum.Text));
                    }
                    catch
                    {
                        lblStatement.Text = Statement.WrongInput;
                    }
            });
            cmdNext = new Command(() =>
            {
                _gamePlay.GenerateExercise();
                if (_isKeyboard)
                    if (_config.FromNumToNum)
                        _pianoKeyboard.IsEnabled = true;
                    else
                        _pianoKeyboard.PianoInit();
            });
        }



        private void InitializeUI()
        {

            Grid grid = new()
            {
                RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(40, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(_isKeyboard ? 40 : 1, GridUnitType.Star) }
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



            lblStatement = new Label
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start,
                FontSize = 18,
                TextColor = Colors.Black,
                Text = Statement.Neutral
            };
            VerticalStackLayout vsl = new()
        {
            lblStatement
            };


            if (_config.UIQuestionType == UIQuestionType.ThreeTexts)
            {
                GenerateTexts();
                vsl.Add(txtSum);
                vsl.Add(new HorizontalStackLayout { txtAddend1, txtAddend2 });
            }

            if (!_isKeyboard ||
                _config.KeyboardConfig.SyncType == SyncType.None ||
                _config.KeyboardConfig.KeyboardOnlyForHelp)
            {
                vsl.Add(GenerateButtons());
            }

            if (_config.IsHistory)
            {
                lblHistory = new Label
                {
                    Text = "History:\n",
                    HorizontalOptions = LayoutOptions.Center
                };
                vsl.Add(lblHistory);
            }

            if (_config.UIQuestionType == UIQuestionType.BitArrayQuestion)
            {
                _gamePlay = new BitArrayGamePlay(_gameType, this, _config);
                if (_config.ArrayQuestionTypes == ArrayQuestionTypes.Keyboard)
                {
                    keyboardTask1 = new PianoKeyboardReadOnly(_config.KeyboardConfig.Rows, _config.KeyboardConfig.KeysInRow);
                    keyboardTask1.WidthRequest = TASK_WIDTH;
                    keyboardTask1.HeightRequest = TASK_WIDTH / 2;
                    vsl.Add(keyboardTask1);
                }
                if (_config.ArrayQuestionTypes == ArrayQuestionTypes.Hand)
                {
                    vsl.Add(InitializeCanvasComponents());

                }
            }


            if (_gameType == GameType.DecompositionGame)
            {
                vsl.Add(GenerateDecompositionGameUI());
            }


            vsl.HorizontalOptions = LayoutOptions.Center;
            vsl.Padding = 15;
            vsl.Spacing = 10;
            grid.Add(vsl);


            if (_isKeyboard)
            {
                _pianoKeyboard = _config.KeyboardConfig.SyncType switch
                {
                    SyncType.HalfSync => new PianoKeyboardHalfSync(_gamePlay, lblStatement, _config.KeyboardConfig),
                    SyncType.Sync => new PianoKeyboardSync(_gamePlay, lblStatement, _config.KeyboardConfig),
                    _ => new PianoKeyboard(_gamePlay, lblStatement, _config.KeyboardConfig)
                };
                grid.Add(_pianoKeyboard);
                Grid.SetRow(_pianoKeyboard, 2);
            }
            Content = grid;

        }

        private VerticalStackLayout GenerateDecompositionGameUI()
        {
            VerticalStackLayout vslDecompositionDashboard = new() { Padding = 20, Spacing = 10, HorizontalOptions = LayoutOptions.Center };

            Label lblStats = new();
            Picker pc = new()
            {
                Title = "Level"
            };
            pc.Items.Add("1");
            pc.Items.Add("2");
            pc.Items.Add("3");
            _gamePlay = new DecompositionGamePlay(this, lblStats, pc);

            vslDecompositionDashboard.Add(pc);
            vslDecompositionDashboard.Add(lblStats);

            return vslDecompositionDashboard;
        }


        private HorizontalStackLayout InitializeCanvasComponents()
        {
            leftHandCanvas = new ()
            {
                HeightRequest = TASK_WIDTH,
                WidthRequest = TASK_WIDTH/2,
                Drawable = new HandDrawable(isLeftHand: true)
            };

            rightHandCanvas = new ()
            {
                HeightRequest = TASK_WIDTH,
                WidthRequest = TASK_WIDTH/2,
                Drawable = new HandDrawable(isLeftHand: false)
            };

                
            

            var stackLayout = new HorizontalStackLayout
            {
                Children = { leftHandCanvas, rightHandCanvas }
            };



            return stackLayout;
        }

        

        private HorizontalStackLayout GenerateButtons()
        {
            HorizontalStackLayout hslBtns = new() { Padding = 20, Spacing = 10, HorizontalOptions = LayoutOptions.Center };

            btnCheck = new()
            {
                Text = "Check",
                Command = cmdCheck,
                HorizontalOptions = LayoutOptions.Center
            };

            btnNext = new Button
            {
                Text = "Next",
                Command = cmdNext,
                HorizontalOptions = LayoutOptions.Center
            };

            hslBtns.Add(btnCheck);
            hslBtns.Add(btnNext);

            return hslBtns;
        }

        private void GenerateTexts()
        {

            txtSum = new Entry
            {
                Keyboard = Keyboard.Numeric,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Start,
                BackgroundColor = Colors.Yellow,
                TextColor = Colors.Black,
                WidthRequest = TASK_WIDTH,
                FontSize = 32
            };

            txtAddend1 = new Entry
            {
                Keyboard = Keyboard.Numeric,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Start,
                BackgroundColor = Colors.White,
                TextColor = Colors.Black,
                WidthRequest = TASK_WIDTH/2,
                FontSize = 18,
                IsVisible = !_isKeyboard || _config.KeyboardConfig.KeyboardOnlyForHelp
            };

            txtAddend2 = new Entry
            {
                Keyboard = Keyboard.Numeric,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Start,
                BackgroundColor = Colors.White,
                TextColor = Colors.Black,
                WidthRequest = TASK_WIDTH/2,
                FontSize = 18,
                IsVisible = !_isKeyboard || _config.KeyboardConfig.KeyboardOnlyForHelp
            };

        }
    }
}
