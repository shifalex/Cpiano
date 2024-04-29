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
    public class SimpleViewCellsPage : ContentPage
    {
        //private readonly bool _isKeyboard = true;//, _isHistory=false;
        private readonly GameType _gameType;
        private readonly PianoKeyboard _pianoKeyboard = null;
        private readonly PPWGamePlay _gamePlay;

        #region view updating
        private readonly Label lblStatement;
        private readonly Label lblHistory;
        private readonly Entry txtAddent1;
        private readonly Entry txtAddent2;
        private readonly Entry txtSum;
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

            txtAddent1.Text = _gamePlay.Addent1 == PPWGamePlay.NAN ? "" : _gamePlay.Addent1.ToString();
            txtAddent2.Text = _gamePlay.Addent2 == PPWGamePlay.NAN ? "" : _gamePlay.Addent2.ToString();
            txtSum.Text = _gamePlay.Sum == PPWGamePlay.NAN ? "" : _gamePlay.Sum.ToString();
            if (newExercise)
            {
                erntryEnabled(txtAddent1, _gamePlay.Addent1 == PPWGamePlay.NAN);
                erntryEnabled(txtAddent2, _gamePlay.Addent2 == PPWGamePlay.NAN);
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
                strHistory += ppw.Addent1 + "\t" + ppw.Addent2 + "\n";

            return strHistory;
        }

        #endregion

        public SimpleViewCellsPage(GameType gameType = GameType.SimpleDecompositionGame, bool isHistory = false, bool isKeyboard = false, bool isSync = false, bool useKeyboardLabels = false, bool imposeEdges = false, bool fromNumToNum = false, bool halfSync = false)
        {
            _gameType = gameType; //_isKeyboard= isKeyboard;
            bool isDecomposeWithKeyboard = _gameType == GameType.DecompositionGameWithKeyboardHelp || _gameType == GameType.DecompositionGameFullWithKeyboardHelp;
            if(_gameType == GameType.DecompositionGameFullWithKeyboardHelp)
                _gamePlay = new PPWGamePlay(gameType, this, isHistory,20 ,20 , VariableTypes.OneCanBeSum);
            else if (_gameType == GameType.DecompositionGameFull)
                _gamePlay = new PPWGamePlay(gameType, this, isHistory, 10, 10);
            else
                _gamePlay = new PPWGamePlay(gameType, this, isHistory);

            Grid grid = new()
            {
                RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(40, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(isKeyboard?40:1, GridUnitType.Star) }
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

            lblStatement = new()
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start,
                FontSize = 18,
                TextColor = Colors.Black,
                Text = Statement.Neutral
            };
            txtSum = new()
            {
                Keyboard = Keyboard.Numeric,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Start,
                BackgroundColor = Colors.Yellow,
                TextColor = Colors.Black,
            WidthRequest = 240,
                FontSize = 32

                /* Text = "{Binding SSum}"   IsReadOnly = "{Binding IsReadOnly}"   IsEnabled = "{Binding SumEnabled}"*/
            };

            txtAddent1 = new()
            {

                Keyboard = Keyboard.Numeric,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Start,
                BackgroundColor = Colors.White,
                TextColor = Colors.Black,
                WidthRequest = 120,
                FontSize = 18,
                IsVisible = !isKeyboard || isDecomposeWithKeyboard

                /* Text = "{Binding SAddent1}"   IsReadOnly = "{Binding IsReadOnly}" IsVisible="{Binding IsNotBlind}"   IsEnabled = "{Binding Addent1Enabled}"*/
            };
            txtAddent2 = new()
            {

                Keyboard = Keyboard.Numeric,
                HorizontalOptions = LayoutOptions.Center,

                HorizontalTextAlignment = TextAlignment.Start,
                BackgroundColor = Colors.White,
                TextColor = Colors.Black,
                WidthRequest = 120,
                FontSize = 18,
                IsVisible = !isKeyboard || isDecomposeWithKeyboard

                /* Text = "{Binding SAddent2}"   IsReadOnly = "{Binding IsReadOnly}" IsVisible="{Binding HasTwoAddents}"   IsEnabled = "{Binding Addent2Enabled}"*/
            };

            txtSum.Keyboard = Keyboard.Numeric; txtAddent1.Keyboard = Keyboard.Numeric; txtAddent2.Keyboard = Keyboard.Numeric;
            lblHistory = new()
            {   /*Text = "{Binding History}",*/
                Text = "History:\n",
                HorizontalOptions = LayoutOptions.Center,
                IsVisible = isHistory
            };

            _pianoKeyboard = new(_gamePlay, lblStatement, isSync, useKeyboardLabels ? (_gameType == GameType.GuessOne ? 1 : 2) : 0, isDecomposeWithKeyboard ? 2 : 1, _gameType == GameType.DecompositionGameFull ? 11 : 10,imposeEdges,fromNumToNum,halfSync);

            HorizontalStackLayout hslBtns = new() { Padding = 20, Spacing = 10, HorizontalOptions = LayoutOptions.Center };
            if (!isSync || isDecomposeWithKeyboard)
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
                                _gamePlay.Check(Convert.ToInt32(this.txtAddent1.Text), Convert.ToInt32(this.txtAddent2.Text), Convert.ToInt32(this.txtSum.Text));
                            }
                            catch
                            {
                                lblStatement.Text = Statement.WrongInput;
                            }
                    }),
                    /*IsEnabled = "{Binding IsEnabledTotal}"*/
                    HorizontalOptions = LayoutOptions.Center
                    
                    
                };
                btnNext = new()
                {
                    Text = "Next",
                    Command = new Command(() => { _gamePlay.GenerateExercise(); if (isKeyboard) _pianoKeyboard.PianoInit(); }), /*IsEnabled = "{Binding IsNotFirstGuess}", IsVisible = "{Binding IsNotSync},"*/
                    HorizontalOptions = LayoutOptions.Center
                };
                hslBtns.Add(btnCheck);
                hslBtns.Add(btnNext);
                //btnNext.SetBinding(IsEnabledProperty, nameof(BtnNextEnabled));
                //btnNext.SetBinding(TextProperty, nameof(BtnNextEnabled));
            }

            VerticalStackLayout vslDecompositionDashboard = new() { Padding = 20, Spacing = 10, HorizontalOptions = LayoutOptions.Center };
            if (_gameType == GameType.DecompositionGame || _gameType == GameType.DecompositionGameWithKeyboardHelp)
            {
                //TODO: add first text label: Level
                Label lblStats = new();
                Picker pc = new()
                {
                    Title = "Level"
                };
                pc.Items.Add("1");

                pc.Items.Add("2");

                pc.Items.Add("3");
                _gamePlay = new DecompositionGamePlay(this, lblStats, pc);

                //TODO: add selected Index Cahnged to change level
                if (_gameType == GameType.DecompositionGame)
                {
                    vslDecompositionDashboard.Add(pc);
                    vslDecompositionDashboard.Add(lblStats);
                }
            }

            VerticalStackLayout vsl = new() {
                lblStatement,
                txtSum,
                new HorizontalStackLayout(){txtAddent1,txtAddent2},
                hslBtns,
                lblHistory,
                vslDecompositionDashboard
            };
            vsl.HorizontalOptions = LayoutOptions.Center;
            vsl.Padding = 15;
            vsl.Spacing = 10;
            grid.Add(vsl);

            Grid.SetRow(_pianoKeyboard, 2);
            if(isKeyboard) grid.Add(_pianoKeyboard);


            Content = grid;


            _gamePlay.GenerateExercise();

            //InitializeComponent();


        }
    }
}