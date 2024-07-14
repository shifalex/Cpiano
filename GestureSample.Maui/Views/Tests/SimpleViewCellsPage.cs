using GestureSample.Maui.Models;
using GestureSample.Maui;
using Microsoft.Maui.Controls;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Input;
using MvvmCross.Binding.Extensions;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace GestureSample.Views.Tests
{

    //ODO: Organizer for JSON - DONE with Property Initialization instead
    //ODO: JSON file for the menu buttons -DONE
    public class SimpleViewCellsPage : ContentPage
    {
        private readonly GameType _gameType;
        private readonly bool _isDecomposeWithKeyboard;
        private readonly bool _isKeyboard;
        private  PianoKeyboard _pianoKeyboard = null;
        private  PPWGamePlay _gamePlay;

        #region view updating
        private  Label lblStatement = new Label
        {
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Start,
            FontSize = 18,
            TextColor = Colors.Black,
            Text = Statement.Neutral
        };
        private  Label lblHistory;
        private  Entry txtAddend1;
        private  Entry txtAddend2;
        private  Entry txtSum;
        private  Label lblAction;
        private  Entry txtResult;
        private  PianoKeyboardReadOnly keyboardTask1;
        private  PianoKeyboardReadOnly keyboardTask2;
        //TODO: show arrows for patterns
        //TODO: Hand image and other images spaces.. To allow a fingu like scenario(just with no moving objects)
        private  Button btnNext = null;
        private Button btnCheck = null;

        private Command cmdNext = null;
        private Command cmdCheck = null;
        /*private bool _btnNextEnabled = false;
        public bool BtnNextEnabled { get => _btnNextEnabled; }*/

        private void erntryEnabled(Entry ent, bool enabled)
        {
            ent.IsEnabled = enabled;
            ent.TextColor = enabled ? Colors.Black : Colors.Gray;

        }
        public void UpdateView(bool newExercise=false)
        {
            lblStatement.Text = _gamePlay.Status;

            txtAddend1.Text = _gamePlay.addend1 == PPWGamePlay.NAN ? "" : _gamePlay.addend1.ToString();
            txtAddend2.Text = _gamePlay.addend2 == PPWGamePlay.NAN ? "" : _gamePlay.addend2.ToString();
            txtSum.Text = _gamePlay.Sum == PPWGamePlay.NAN ? "" : _gamePlay.Sum.ToString();
            if (newExercise)
            {
                erntryEnabled(txtAddend1, _gamePlay.addend1 == PPWGamePlay.NAN);
                erntryEnabled(txtAddend2, _gamePlay.addend2 == PPWGamePlay.NAN);
                erntryEnabled(txtSum, _gamePlay.Sum == PPWGamePlay.NAN);
            }
            if (btnNext != null) btnNext.IsEnabled = _gamePlay.GuessNumber > 0;
            //OnPropertyChanged(nameof(BtnNextEnabled));

            if(_config.IsHistory)lblHistory.Text = GenerateHistoryString(_gamePlay.AllHistory.Where(item => item.Sum == _gamePlay.Sum).ToList());
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
            _isDecomposeWithKeyboard = _gameType == GameType.DecompositionGame && _isKeyboard;

            InitializeGamePlay();
            InitializeUI();
                        
            _gamePlay.GenerateExercise();
        }

        private void InitializeGamePlay()
        {
            _gamePlay = new PPWGamePlay(_gameType, this, _config);
            //if (_isKeyboard)
            //{
                //if (_isDecomposeWithKeyboard) { _config.KeyboardConfig.Rows = 2; }
                //if (_gameType == GameType.FullDecomposition) { _config.KeyboardConfig.KeysInRow = 11; }
                
            //}
            cmdCheck = new Command(() =>
            {
                if (_isKeyboard && !_isDecomposeWithKeyboard)
                    _pianoKeyboard.IsEnabled = !_gamePlay.Check();
                else
                    try
                    {
                        _gamePlay.Check(Convert.ToInt32(txtAddend1.Text), Convert.ToInt32(txtAddend2.Text), Convert.ToInt32(txtSum.Text));
                    }
                    catch
                    {
                        lblStatement.Text = "WrongInput"; // Replace with actual statement
                    }
            });
            cmdNext = new Command(() => 
            { 
                _gamePlay.GenerateExercise(); 
                if (_isKeyboard) _pianoKeyboard.PianoInit(); 
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

            

            txtSum = new Entry
            {
                Keyboard = Keyboard.Numeric,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Start,
                BackgroundColor = Colors.Yellow,
                TextColor = Colors.Black,
                WidthRequest = 240,
                FontSize = 32
            };

            txtAddend1 = new Entry
            {
                Keyboard = Keyboard.Numeric,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Start,
                BackgroundColor = Colors.White,
                TextColor = Colors.Black,
                WidthRequest = 120,
                FontSize = 18,
                IsVisible = !_isKeyboard || _isDecomposeWithKeyboard
            };

            txtAddend2 = new Entry
            {
                Keyboard = Keyboard.Numeric,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Start,
                BackgroundColor = Colors.White,
                TextColor = Colors.Black,
                WidthRequest = 120,
                FontSize = 18,
                IsVisible = !_isKeyboard || _isDecomposeWithKeyboard
            };


            VerticalStackLayout vsl = new()
        {
            lblStatement
            };
            vsl.Add(txtSum);
            vsl.Add(new HorizontalStackLayout { txtAddend1, txtAddend2 });




            if (_isDecomposeWithKeyboard || !_isKeyboard || _config.KeyboardConfig.SyncType == SyncType.None)
            {
                HorizontalStackLayout hslBtns = new() { Padding = 20, Spacing = 10, HorizontalOptions = LayoutOptions.Center };

                Button btnCheck = new()
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

                vsl.Add(hslBtns);
            }

            if (_config.IsHistory)
            {
                lblHistory = new Label
                {
                    Text = "History:\n",
                    HorizontalOptions = LayoutOptions.Center,
                    //IsVisible = _config.IsHistory
                };
                vsl.Add(lblHistory);
            }


            if (_gameType == GameType.DecompositionGame)
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

                if (_gameType == GameType.DecompositionGame)
                {
                    vslDecompositionDashboard.Add(pc);
                    vslDecompositionDashboard.Add(lblStats);
                }
                vsl.Add(vslDecompositionDashboard);

            }


            vsl.HorizontalOptions = LayoutOptions.Center;
            vsl.Padding = 15;
            vsl.Spacing = 10;
            grid.Add(vsl);


            if (_isKeyboard)
            {
                switch (_config.KeyboardConfig.SyncType)
                {
                    case SyncType.HalfSync:
                        _pianoKeyboard = new PianoKeyboardHalfSync(_gamePlay, lblStatement, _config.KeyboardConfig);
                        break;
                    case SyncType.Sync:
                        _pianoKeyboard = new PianoKeyboardSync(_gamePlay, lblStatement, _config.KeyboardConfig);
                        break;
                    default:
                        _pianoKeyboard = new PianoKeyboard(_gamePlay, lblStatement, _config.KeyboardConfig);
                        break;
                }
                grid.Add(_pianoKeyboard);
                Grid.SetRow(_pianoKeyboard, 2);
            }
            Content = grid;

        }
    }
}