using GestureSample.Maui.Models;
using GestureSample.Maui;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System.Linq;


namespace GestureSample.Views.Tests
{

    //ODO: Organizer for JSON - DONE with Property Initialization instead
    //ODO: JSON file for the menu buttons -DONE
    public class SimpleViewCellsPage : ContentPage
    {
        private readonly GameType _gameType;
        private readonly bool _isKeyboard;
        private PianoKeyboard _pianoKeyboard = null;
        private PPWGamePlay _gamePlay;

        private SKCanvasView leftHandCanvas;
        private SKCanvasView rightHandCanvas;
        private int[] leftHandBits;
        private int[] rightHandBits;

        #region view updating
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

        private void erntryEnabled(Entry ent, bool enabled)
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
                    erntryEnabled(txtAddend1, _gamePlay.addend1 == PPWGamePlay.NAN);
                    erntryEnabled(txtAddend2, _gamePlay.addend2 == PPWGamePlay.NAN);
                    erntryEnabled(txtSum, _gamePlay.Sum == PPWGamePlay.NAN);
                }
            }
            if (btnNext != null) btnNext.IsEnabled = _gamePlay.GuessNumber > 0;

            if (_config.UIQuestionType == UIQuestionType.BitArrayQuestion && newExercise)
            {
                if (_config.ArrayQuestionTypes == ArrayQuestionTypes.Keyboard)
                {
                    keyboardTask1.Random();
                    ((BitArrayGamePlay)_gamePlay).bitArrayQuestion = keyboardTask1.ToBitArray();
                }
                if (_config.ArrayQuestionTypes == ArrayQuestionTypes.Hand)
                {
                    GenerateHandsImage();
                    ((BitArrayGamePlay)_gamePlay).bitArrayQuestion = BoolArrayFromHands(leftHandBits, rightHandBits);
                }

                if (_isKeyboard && !_config.FromNumToNum) _pianoKeyboard.PianoInit();
                if (_config.IsHistory) lblHistory.Text = GenerateHistoryString(_gamePlay.AllHistory.Where(item => item.Sum == _gamePlay.Sum).ToList());
            }
        }

        private static string GenerateHistoryString(List<PPWObject> ppwHistoryArray)
        {
            String strHistory = "HISTORY:\n";
            foreach (PPWObject ppw in ppwHistoryArray)
                strHistory += ppw.Addend1 + "\t" + ppw.Addend2 + "\n";

            return strHistory;
        }

        private static bool[] BoolArrayFromHands(int[] leftHandBits, int[] rightHandBits)
        {
            int[] array = leftHandBits.Concat(rightHandBits).ToArray();
            bool[] result = new bool[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                result[i] = array[i] > 0;
            }
            return result;
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


        private StackLayout InitializeCanvasComponents()
        {
            leftHandCanvas = new SKCanvasView
            {
                IsVisible = false,
                HeightRequest = 200,
                WidthRequest = 200
            };
            leftHandCanvas.PaintSurface += OnLeftHandCanvasPaintSurface;

            rightHandCanvas = new SKCanvasView
            {
                IsVisible = false,
                HeightRequest = 200,
                WidthRequest = 200
            };
            rightHandCanvas.PaintSurface += OnRightHandCanvasPaintSurface;


            StackLayout stackLayout = new()
            {
                Children = { leftHandCanvas, rightHandCanvas }
            };

            Content = stackLayout;

            return stackLayout;
        }

        private void GenerateHandsImage()
        {
            Random r = new Random();
            leftHandBits = new int[] { r.Next(2), r.Next(2), r.Next(2), r.Next(2), r.Next(2) };
            rightHandBits = new int[] { r.Next(2), r.Next(2), r.Next(2), r.Next(2), r.Next(2) };

            leftHandCanvas.IsVisible = true;
            rightHandCanvas.IsVisible = true;

            leftHandCanvas.InvalidateSurface();
            rightHandCanvas.InvalidateSurface();

            // Wait for 2 seconds
            //await Task.Delay(2000);

            leftHandCanvas.IsVisible = false;
            rightHandCanvas.IsVisible = false;
        }

        private void OnLeftHandCanvasPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.White);
            DrawHand(canvas, leftHandBits);
        }

        private void OnRightHandCanvasPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.White);
            DrawHand(canvas, rightHandBits);
        }

        private void DrawHand(SKCanvas canvas, int[] bits)
        {
            var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                StrokeWidth = 5
            };

            // Draw palm
            var palmPath = new SKPath();
            palmPath.MoveTo(50, 100);
            palmPath.LineTo(150, 100);
            palmPath.LineTo(150, 150);
            palmPath.LineTo(50, 150);
            palmPath.Close();
            canvas.DrawPath(palmPath, paint);

            // Coordinates for fingers
            var fingers = new[]
            {
                new[] { new SKPoint(60, 50), new SKPoint(80, 100) },  // Thumb
                new[] { new SKPoint(90, 30), new SKPoint(110, 100) }, // Index
                new[] { new SKPoint(120, 20), new SKPoint(140, 100) },// Middle
                new[] { new SKPoint(150, 30), new SKPoint(170, 100) },// Ring
                new[] { new SKPoint(180, 50), new SKPoint(200, 100) } // Pinky
            };

            // Draw fingers based on the bit array
            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i] == 1)
                {
                    canvas.DrawLine(fingers[i][0], fingers[i][1], paint);
                }
            }
        }

        private HorizontalStackLayout GenerateButtons()
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
                IsVisible = !_isKeyboard || _config.KeyboardConfig.KeyboardOnlyForHelp
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
                IsVisible = !_isKeyboard || _config.KeyboardConfig.KeyboardOnlyForHelp
            };

        }
    }
}
