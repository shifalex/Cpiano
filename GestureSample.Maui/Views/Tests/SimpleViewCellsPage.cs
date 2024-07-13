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

    //TODO: Organizer for JSON
    //TODO: JSON file for the menu buttons
    public class SimpleViewCellsPage : ContentPage
    {
        //private readonly bool _isKeyboard = true;//, _isHistory=false;
        private readonly GameType _gameType;
        private readonly PianoKeyboard _pianoKeyboard = null;
        private readonly PPWGamePlay _gamePlay;

        #region view updating
        private readonly Label lblStatement;
        private readonly Label lblHistory;
        private readonly Entry txtaddend1;
        private readonly Entry txtaddend2;
        private readonly Entry txtSum;
        private readonly Label lblAction;
        private readonly Entry txtResult;
        private readonly PianoKeyboardReadOnly keyboardTask1;
        private readonly PianoKeyboardReadOnly keyboardTask2;
        //TODO: show arrows for patterns
        //TODO: Hand image and other images spaces.. To allow a fingu like scenario(just with no moving objects)
        private readonly Button btnNext = null;

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

            txtaddend1.Text = _gamePlay.addend1 == PPWGamePlay.NAN ? "" : _gamePlay.addend1.ToString();
            txtaddend2.Text = _gamePlay.addend2 == PPWGamePlay.NAN ? "" : _gamePlay.addend2.ToString();
            txtSum.Text = _gamePlay.Sum == PPWGamePlay.NAN ? "" : _gamePlay.Sum.ToString();
            if (newExercise)
            {
                erntryEnabled(txtaddend1, _gamePlay.addend1 == PPWGamePlay.NAN);
                erntryEnabled(txtaddend2, _gamePlay.addend2 == PPWGamePlay.NAN);
                erntryEnabled(txtSum, _gamePlay.Sum == PPWGamePlay.NAN);
            }
            if (btnNext != null) btnNext.IsEnabled = _gamePlay.GuessNumber > 0;
            //OnPropertyChanged(nameof(BtnNextEnabled));

            lblHistory.Text = GenerateHistoryString(_gamePlay.AllHistory.Where(item => item.Sum == _gamePlay.Sum).ToList());
        }

        private static string GenerateHistoryString(List<PPWObject> ppwHistoryArray)
        {
            String strHistory = "HISTORY:\n";
            foreach (PPWObject ppw in ppwHistoryArray)
                strHistory += ppw.addend1 + "\t" + ppw.addend2 + "\n";

            return strHistory;
        }

        #endregion

        private readonly GameConfig _config;

        public SimpleViewCellsPage(GameConfig config)
        {
            _config = config;
            _gameType = config.GameType;

            bool isDecomposeWithKeyboard = _gameType == GameType.DecompositionGameWithKeyboardHelp || _gameType == GameType.DecompositionGameFullWithKeyboardHelp;
            bool isKeyboard = config.KeyboardConfig != null;

            if (_gameType == GameType.DecompositionGameFullWithKeyboardHelp)
                _gamePlay = new PPWGamePlay(_gameType, this, config.IsHistory, 0, 0, 20, 20, VariableTypes.OneCanBeSum);
            else if (_gameType == GameType.DecompositionGameFull || config.KeyboardConfig.SyncType==SyncType.HalfSync)
                _gamePlay = new PPWGamePlay(_gameType, this, config.IsHistory, config.KeyboardConfig.WithoutZero ? 1 : 0, config.KeyboardConfig.WithoutZero ? 2 : 0, 10, 10);
            else
                _gamePlay = new PPWGamePlay(_gameType, this, config.IsHistory);

            Grid grid = new()
            {
                RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(40, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(isKeyboard ? 40 : 1, GridUnitType.Star) }
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
                Text = "Statement.Neutral" // Replace with the actual statement text
            };

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

            txtaddend1 = new Entry
            {
                Keyboard = Keyboard.Numeric,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Start,
                BackgroundColor = Colors.White,
                TextColor = Colors.Black,
                WidthRequest = 120,
                FontSize = 18,
                IsVisible = !isKeyboard || isDecomposeWithKeyboard
            };

            txtaddend2 = new Entry
            {
                Keyboard = Keyboard.Numeric,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Start,
                BackgroundColor = Colors.White,
                TextColor = Colors.Black,
                WidthRequest = 120,
                FontSize = 18,
                IsVisible = !isKeyboard || isDecomposeWithKeyboard
            };

            lblHistory = new Label
            {
                Text = "History:\n",
                HorizontalOptions = LayoutOptions.Center,
                IsVisible = config.IsHistory
            };
            
            if(isKeyboard)
            {
                if (isDecomposeWithKeyboard) { config.KeyboardConfig.Rows = 2; }
                if (_gameType == GameType.DecompositionGameFull) { config.KeyboardConfig.KeysInRow = 11; }
                switch (config.KeyboardConfig.SyncType)
                {
                    case SyncType.HalfSync:
                        _pianoKeyboard = new PianoKeyboardHalfSync(_gamePlay, lblStatement, config.KeyboardConfig);
                        break;
                    case SyncType.Sync:
                        _pianoKeyboard = new PianoKeyboardSync(_gamePlay, lblStatement, config.KeyboardConfig);
                        break;
                    default:
                        _pianoKeyboard = new PianoKeyboard(_gamePlay, lblStatement, config.KeyboardConfig);
                        break;
                }
            }
            HorizontalStackLayout hslBtns = new() { Padding = 20, Spacing = 10, HorizontalOptions = LayoutOptions.Center };
            if (isDecomposeWithKeyboard || (isKeyboard && config.KeyboardConfig.SyncType==SyncType.None))
            {
                Button btnCheck = new()
                {
                    Text = "Check",
                    Command = new Command(() =>
                    {
                        if (isKeyboard && !isDecomposeWithKeyboard)
                            _pianoKeyboard.IsEnabled = !_gamePlay.Check();
                        else
                            try
                            {
                                _gamePlay.Check(Convert.ToInt32(txtaddend1.Text), Convert.ToInt32(txtaddend2.Text), Convert.ToInt32(txtSum.Text));
                            }
                            catch
                            {
                                lblStatement.Text = "WrongInput"; // Replace with actual statement
                            }
                    }),
                    HorizontalOptions = LayoutOptions.Center
                };

                btnNext = new Button
                {
                    Text = "Next",
                    Command = new Command(() => { _gamePlay.GenerateExercise(); if (isKeyboard) _pianoKeyboard.PianoInit(); }),
                    HorizontalOptions = LayoutOptions.Center
                };

                hslBtns.Add(btnCheck);
                hslBtns.Add(btnNext);
            }

            VerticalStackLayout vslDecompositionDashboard = new() { Padding = 20, Spacing = 10, HorizontalOptions = LayoutOptions.Center };
            if (_gameType == GameType.DecompositionGame || _gameType == GameType.DecompositionGameWithKeyboardHelp)
            {
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
            }

            VerticalStackLayout vsl = new()
        {
            lblStatement,
            txtSum,
            new HorizontalStackLayout { txtaddend1, txtaddend2 },
            hslBtns,
            lblHistory,
            vslDecompositionDashboard
        };
            vsl.HorizontalOptions = LayoutOptions.Center;
            vsl.Padding = 15;
            vsl.Spacing = 10;
            grid.Add(vsl);

            Grid.SetRow(_pianoKeyboard, 2);
            if (isKeyboard) grid.Add(_pianoKeyboard);

            Content = grid;

            _gamePlay.GenerateExercise();
        }
    }
}