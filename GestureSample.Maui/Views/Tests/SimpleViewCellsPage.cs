using GestureSample.Debugging;
using GestureSample.Maui;
using GestureSample.Maui.Data;
using GestureSample.Maui.Data.SQLite;
using GestureSample.Maui.Handlers;
using GestureSample.Maui.Models;
using GestureSample.Maui.Views;
using GestureSample.Views;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Platform;
using Microsoft.Maui.ApplicationModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
#if IOS || MACCATALYST
using UIKit;
#endif
#if ANDROID
using Android.Content.Res;
#endif

namespace GestureSample.Views.Tests
{

    public class SimpleViewCellsPage : ContentPage
    {
        private Label _statusLight1Icon = new()
        {
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };
        private Label _statusLight2Icon = new()
        {
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };
        private Border _statusLight1 = new()
        {
            WidthRequest = 18,
            HeightRequest = 18,
            StrokeThickness = 0,
            BackgroundColor = Colors.Green,
            StrokeShape = new RoundRectangle { CornerRadius = 9 },
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        private Border _statusLight2 = new()
        {
            WidthRequest = 18,
            HeightRequest = 18,
            StrokeThickness = 0,
            BackgroundColor = Colors.Green,
            StrokeShape = new RoundRectangle { CornerRadius = 9 },
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        private PlayUiState _currentUiState = PlayUiState.ReadyForInput;

        private void ApplyUiState(PlayUiState state, string? text = null)
        {
            _currentUiState = state;

            switch (state)
            {
                case PlayUiState.Question:
                    _statusLight1.BackgroundColor = Colors.Red;
                    _statusLight1Icon.Text = "?";
                   // _lblStatement.Text = text ?? "Look";
                  //  _pianoPressProgress.Opacity = 0;
                    break;

                case PlayUiState.ReadyForInput:
                    _statusLight1.BackgroundColor = Colors.Green;
                    _statusLight1Icon.Text = string.Empty;
                   // _lblStatement.Text = text ?? "Your turn";
                  //  _pianoPressProgress.Opacity = 1;
                    break;

                case PlayUiState.Tutorial:
                    _statusLight1.BackgroundColor = Colors.Orange;
                    _statusLight1Icon.Text = "i";
                 //   _lblStatement.Text = text ?? "Tutorial";
                //    _pianoPressProgress.Opacity = 0;
                    break;

                case PlayUiState.FeedbackCorrect:
                    _statusLight1.BackgroundColor = Colors.LimeGreen;
                    _statusLight1Icon.Text = "✓";
               //     _lblStatement.Text = text ?? "✅";
                //    _pianoPressProgress.Opacity = 0;
                    break;

                case PlayUiState.FeedbackWrong:
                    _statusLight1.BackgroundColor = Colors.IndianRed;
                    _statusLight1Icon.Text = "✕";
                 //   _lblStatement.Text = text ?? "❌";
                 //   _pianoPressProgress.Opacity = 0;
                    break;

                case PlayUiState.Disabled:
                default:
                    _statusLight1.BackgroundColor = Colors.Gray;
                    _statusLight1Icon.Text = string.Empty;
               //     _lblStatement.Text = text ?? "";
                //    _pianoPressProgress.Opacity = 0;
                    break;
            }
            _statusLight2.BackgroundColor = _statusLight1.BackgroundColor;
            _statusLight2Icon.Text = _statusLight1Icon.Text;
            RefreshStatusActionSlot();
           // return Task.CompletedTask;
        }

        private void SetPlayUiState(PlayUiState state, string? text = null)
        {
            ApplyUiState(state, text);
        }

        private void SetKeyboardInteractionEnabled(bool enabled)
        {
            if (_pianoKeyboard == null)
                return;

            _pianoKeyboard.IsEnabled = true;
            _pianoKeyboard.InputTransparent = !enabled;
            if (_pianoKeyboard.BtnInit != null)
                _pianoKeyboard.BtnInit.IsEnabled = enabled;

            // Keep keyboard-driven submit actions aligned with keyboard availability.
            if (_isKeyboard && !_config.KeyboardConfig.KeyboardOnlyForHelp)
            {
                if (_btnAnswerTimePanel != null && _btnAnswerTimePanel.IsVisible)
                {
                    _btnAnswerTimePanel.IsEnabled = true;
                    _btnAnswerTimePanel.InputTransparent = false;
                    _btnAnswerTimePanel.Opacity = 1;
                    _btnAnswerTimePanel.BackgroundColor = GetAnswerTimePanelBackgroundColor();
                }

                if (_btnKeyboardSubmit != null && _btnKeyboardSubmit.IsVisible)
                    _btnKeyboardSubmit.IsEnabled = enabled;

                if (_btnImpossibleWeightedAnswer != null && _btnImpossibleWeightedAnswer.IsVisible)
                    _btnImpossibleWeightedAnswer.IsEnabled = enabled;

                if (_btnKeyboardCheckInline != null && _btnKeyboardCheckInline.IsVisible)
                    _btnKeyboardCheckInline.IsEnabled = enabled;
            }
        }

        private void SetPageInteractionEnabled(bool enabled)
        {
            _isPageInteractionEnabled = enabled;
            SetKeyboardInteractionEnabled(enabled);
            if (_numericKeypad != null)
                _numericKeypad.IsEnabled = enabled;

            if (_btnEquationHelp != null)
                _btnEquationHelp.IsEnabled = enabled;

            if (_btnHelp != null)
                _btnHelp.IsEnabled = true;

            if (_btnAnswerTimePanel != null)
            {
                _btnAnswerTimePanel.IsEnabled = true;
                _btnAnswerTimePanel.InputTransparent = false;
                _btnAnswerTimePanel.Opacity = 1;
                _btnAnswerTimePanel.BackgroundColor = GetAnswerTimePanelBackgroundColor();
            }

            if (_btnKeyboardSubmit != null)
                _btnKeyboardSubmit.IsEnabled = enabled;

            if (_btnImpossibleWeightedAnswer != null)
                _btnImpossibleWeightedAnswer.IsEnabled = enabled;

            if (_btnKeyboardCheckInline != null && _btnKeyboardCheckInline.IsVisible)
                _btnKeyboardCheckInline.IsEnabled = enabled;

            if (_btnThirdArrowVisibility != null)
                _btnThirdArrowVisibility.IsEnabled = enabled;

            if (_answerTimeEnabledSwitch != null)
                _answerTimeEnabledSwitch.IsEnabled = true;

            if (_answerTimeMinusButton != null)
                _answerTimeMinusButton.IsEnabled = true;

            if (_answerTimePlusButton != null)
                _answerTimePlusButton.IsEnabled = true;

            if (_answerTimeModeButton != null)
                _answerTimeModeButton.IsEnabled = true;

            if (_btnPrev != null)
                _btnPrev.IsEnabled = enabled && _previousPPW != null;

            if (_btnPrevBelow != null)
                _btnPrevBelow.IsEnabled = enabled && _previousPPW != null;

            if (_btnNext != null)
                _btnNext.IsEnabled = enabled && HasVisibleManualCheckButton() ? (_gamePlay.GuessNumber > 0) : false;

            if (_btnCheck != null && _btnCheck.IsVisible)
                _btnCheck.IsEnabled = enabled;
        }

        private PlayUiState GetExerciseUiState(bool newExercise)
        {
            if (_tutorialRunning)
                return PlayUiState.Tutorial;

            if (!newExercise)
                return PlayUiState.ReadyForInput;

            if (_config.SecondsTillAllowInput > 0 || _config.SecondsTillHideExercise > 0)
                return PlayUiState.Question;

            return PlayUiState.ReadyForInput;
        }

        private void ApplyExerciseUiState(bool newExercise)
        {
            SetPlayUiState(GetExerciseUiState(newExercise));
            SetInlineKeyboardCheckVisible(ShouldUseInlineKeyboardCheckButton());
        }

        private void ApplyFeedbackUiState(bool isCorrect)
        {
            SetPlayUiState(isCorrect ? PlayUiState.FeedbackCorrect : PlayUiState.FeedbackWrong);
            ApplyArrowLabelPromptFeedback(isCorrect);
            SetInlineKeyboardCheckVisible(false);
        }

        private void RestoreReadyForInputState()
        {
            if (_tutorialRunning)
                return;

            ResetArrowLabelPromptEntryColors();

            if (_config.SecondsTillAllowInput > 0 && _isKeyboard && _pianoKeyboard != null && !_pianoKeyboard.IsEnabled)
            {
                SetPlayUiState(PlayUiState.Question);
                return;
            }

            SetPlayUiState(PlayUiState.ReadyForInput);
            SetInlineKeyboardCheckVisible(ShouldUseInlineKeyboardCheckButton());
        }

        private void ResetStatusLineToNeutral()
        {
            _gamePlay.ResetStatusToNeutral();
            UpdateStatement();
        }

        private async Task RunTutorialAsync(KeyboardOverlayHost host)
        {
            if (_tutorialRunning || host == null)
                return;

            _tutorialRunning = true;
            SetPlayUiState(PlayUiState.Tutorial);
            host.SetTutorialMode(true);

            try
            {
                await Tutorial(host);
            }
            finally
            {
                ClearTutorialStepCounter();
                _pianoKeyboard?.ClearTutorialStepLabels();
                host.SetTutorialMode(false);
                _tutorialRunning = false;
                RestoreReadyForInputState();
            }
        }

        private Color[]? CaptureLiveKeyboardColors()
        {
            return _pianoKeyboard?.GetCurrentColors()?.ToArray();
        }

        private void ClearLiveKeyboardState()
        {
            if (_pianoKeyboard == null)
                return;

            Color[] clearedColors = Enumerable.Repeat(Colors.White, _pianoKeyboard.KeyCount).ToArray();
            _pianoKeyboard.PianoInit(clearedColors);
        }

        private void RestoreLiveKeyboardState(Color[]? colors)
        {
            if (_pianoKeyboard == null || colors == null || colors.Length == 0)
                return;

            _pianoKeyboard.PianoInit(colors);
        }

        private bool HasKeyboardGuidanceSupport()
        {
            return _pianoKeyboard is PianoKeyboardSync &&
                   _taskMainHost != null &&
                   _gamePlay is BitArrayGamePlay;
        }

        private bool HasDedicatedKeyboardTutorial()
        {
            return HasKeyboardGuidanceSupport() &&
                   ((_config.KeyboardConfig?.IsHelpNeeded ?? false) ||
                    _config.KeyboardConfig?.IsArrow == true ||
                    _config.KeyboardConfig?.ArrowLabelExerciseMode != ArrowLabelExerciseMode.None);
        }

        private async Task MarkCurrentKeyboardQuestionTutorialUsedAsync()
        {
            if (!HasKeyboardGuidanceSupport())
                return;

            if (_lastGeneratedExercise?.PersistenceTask != null)
                await _lastGeneratedExercise.PersistenceTask;

            await _keyboardQuestionRepository.MarkTutorialUsedAsync(_gamePlay.GameId.ToString(), _gamePlay._questionNumber);
        }

        private async Task RunRecordedKeyboardTutorialAsync(KeyboardOverlayHost host)
        {
            await MarkCurrentKeyboardQuestionTutorialUsedAsync();
            Color[]? keyboardSnapshot = CaptureLiveKeyboardColors();
            try
            {
                ClearLiveKeyboardState();
                host.SyncOverlay();
                await RunTutorialAsync(host);
            }
            finally
            {
                RestoreLiveKeyboardState(keyboardSnapshot);
                host.SyncOverlay();
            }
        }

        private async Task RunCorrectAnswerHintAsync(KeyboardOverlayHost host)
        {
            if (host == null || _gamePlay is not BitArrayGamePlay gp)
                return;

            await MarkCurrentKeyboardQuestionTutorialUsedAsync();
            host.SyncOverlay();
            await host.EnsureOverlaySyncedAsync();

            bool[] tutorialAnswer = gp.GetTutorialAnswerBits();
            if (tutorialAnswer.Length == 0)
                return;

            await host.FadeStaticOverlayAlphaAsync(0.18f, ScaleTutorialMs(220u), "TutStaticDimIn");
            try
            {
                await host.PulseBitsAsync(
                    tutorialAnswer,
                    fadeInMs: ScaleTutorialMs(280u),
                    holdMs: ScaleTutorialMs(2200u),
                    fadeOutMs: ScaleTutorialMs(380u),
                    animName: "TutCorrectPulse");
            }
            finally
            {
                await host.FadeStaticOverlayAlphaAsync(
                    KeyboardOverlayHost.DefaultStaticOverlayAlpha,
                    ScaleTutorialMs(220u),
                    "TutStaticDimOut");
            }
        }

        private readonly GameConfig _config;
        private readonly BackgroundSyncService _backgroundSyncService;
        private readonly SyncToolbarStatusController _syncToolbarStatusController;

        private bool _isKeyboard { get { return _config.KeyboardConfig != null; } }
        private bool UsesArrowPromptAnswerWithoutMainKeyboard =>
            _isKeyboard &&
            _config.KeyboardConfig != null &&
            _config.KeyboardConfig.KeyboardOnlyForHelp &&
            _config.KeyboardConfig.HideMainKeyboard &&
            ShouldShowKeyboardPromptLabel();

        private bool HasVisibleNumericInputs =>
            (_isThreeTexts && (!_isKeyboard || _config.KeyboardConfig.KeyboardOnlyForHelp)) ||
            UsesArrowPromptAnswerWithoutMainKeyboard;
        private NumericInputMode EffectiveNumericInputMode
        {
            get
            {
                if (_config.NumericInputMode == NumericInputMode.ChoiceKeyboard)
                    return NumericInputMode.ChoiceKeyboard;

                UserPreferenceService preferenceService = ServiceHelper.GetService<UserPreferenceService>();
                Guid? activeUserId = ServiceHelper.GetService<CurrentUserSession>().ActiveUser?.Id;
                NumericInputMode preferredMode = preferenceService.GetPreferredNumericInputMode(activeUserId);
                if (preferredMode != NumericInputMode.Auto)
                    return preferredMode;

                return _config.NumericInputMode == NumericInputMode.Auto
                    ? NumericInputMode.AppKeypad
                    : _config.NumericInputMode;
            }
        }
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
                    UIQuestionType.TwoLinesTwoAddends => true,
                    UIQuestionType.ThreeAddends => true,
                    _ => false
                };
            }
        }
        private bool UsesCustomNumericKeypad => HasVisibleNumericInputs && EffectiveNumericInputMode == NumericInputMode.AppKeypad;
        private bool UsesChoiceAnswerKeyboard => HasVisibleNumericInputs && EffectiveNumericInputMode == NumericInputMode.ChoiceKeyboard;
        private bool UsesManagedNumericInput => UsesCustomNumericKeypad || UsesChoiceAnswerKeyboard;
        public new bool IsEnabled
        {
            get => _pianoKeyboard?.IsEnabled ?? true;
            set
            {
                SetPageInteractionEnabled(value);
                if(value)
                    SetPlayUiState(PlayUiState.ReadyForInput);
                else
                    SetPlayUiState(PlayUiState.Disabled);

            }
        }

        private PianoKeyboard _pianoKeyboard = null;
        private PPWGamePlay _gamePlay;

        private GraphicsView leftHandCanvas;
        private GraphicsView rightHandCanvas;

        // Added field for tutorial hand drawable view
        private GraphicsView handGraphicsView;
        private HandDrawable _tutorialHandDrawable;
        private bool _tutorialHandIsLeft;
        private Grid _rootGrid;

        private readonly int FONT_SIZE_DEFAULT = 18;
        private readonly int TASK_WIDTH = 180;//TODO: if phone then make smaller and make answer keyboard only notSync
        private readonly int PIANO_HEIGHT1 = 90;
        private readonly int PIANO_HEIGHT2 = 60;
        private Label _lblStatement;
        private ProgressBar _pianoPressProgress;
        private string? _tutorialStepCounterText;
        private View _keyboardArrowPromptView;
        private Grid _customProgressHost;
        private Border _customProgressFill;
        private Button _btnKeyboardCheckInline = null;
        private Border _centerFeedbackBadge = null;
        private Label _centerFeedbackBadgeLabel = null;
        private Border _keyboardControlBar = null;
        private Button _btnThirdArrowVisibility = null;
        private bool _isPageInteractionEnabled = true;
        private Label _lblHistory;
        private Entry _txtAddend1;
        private Entry _txtAddend2;
        private Entry _txtSum;
        private Label _lblAction;
        private HorizontalStackLayout _logicalColorActionLayout;
        private Label _logicalColorLeftArrow;
        private Label _logicalColorRightArrow;
        private NumericKeypadView _numericKeypad;
        private ChoiceAnswerKeyboardView _choiceAnswerKeyboard;
        private BoxView _hr;
        private Entry[] txt;
        private Entry _lastFocused;
        //private  Entry txtResult;
        private PianoKeyboardReadOnly _keyboardTask1;
        private PianoKeyboardReadOnly _keyboardTask2;
        private KeyboardOverlayHost _task1Host;
        private KeyboardOverlayHost _task2Host;
        private KeyboardOverlayHost _taskMainHost;

        //TODO: show arrows for patterns
        //TODO: Hand image and other images spaces.. To allow a fingu like scenario(just with no moving objects)
        private Button _btnNext = null;
        private Button _btnCheck = null;
        private Button _btnPrev = null;
        private Button _btnPrevBelow = null;
        private Button _btnEquationHelp = null;
        private Microsoft.Maui.Controls.Switch _answerTimeEnabledSwitch = null;
        private Label _answerTimeValueLabel = null;
        private Label _answerTimeModeLabel = null;
        private Button _answerTimeMinusButton = null;
        private Button _answerTimePlusButton = null;
        private Button _answerTimeModeButton = null;
        private Button _btnAnswerTimePanel = null;
        private Button _btnKeyboardSubmit = null;
        private Button _btnImpossibleWeightedAnswer = null;
        private View _answerTimeTunerCard = null;
        private BoxView _answerTimeDismissShield = null;
        private bool _isAnswerTimeTunerVisible = false;
        private int _lastNonZeroAnswerTimeSetting = 0;
        private const int AnswerTimeStateMaxSeconds = 5;

        private PPWObject _currentPPW;
        private PPWObject _currentPPWEnabled;
        private PPWObject? _currentSecondaryPPW;
        private PPWObject? _currentSecondaryPPWEnabled;
        private PPWObject _previousPPW = null;
        private string _previousActionText = string.Empty;
        private ExerciseGenerationResult? _lastGeneratedExercise;
        private bool _hasLoadedInitialExercise = false;
        private int _consecutiveWrongAnswers = 0;
        private static readonly Color[] ArrowBackgroundCycle =
        {
            Colors.Black,
            Color.FromArgb("#1C2E4A"),
            Color.FromArgb("#3A3213"),
            Color.FromArgb("#3B1F2B")
        };
        private readonly List<Entry> _numericEntries = new();
        private Entry? _activeNumericEntry;

        private Command _cmdNext = null;
        private Command _cmdCheck = null;
        private HorizontalStackLayout _hzlEquation;
        private Label _lblEquationEquals;
        private bool _equationHelpRunning;
        private bool _showPreviousBelow;
        private View _previousBelowView;
        private View _questionInputsContainer;
        private Thickness _questionInputsBaseMargin;
        private bool _benchmarkAdvanceGestureRunning;
        private Entry _prevAddend1Entry;
        private Entry _prevAddend2Entry;
        private Entry _prevSumEntry;
        private Label _prevActionLabel;
        private Label _prevEqualsLabel;
        private const double EquationHelpRowSpacing = 16;
        private readonly KeyboardQuestionRepository _keyboardQuestionRepository;
        private readonly QuestionAnswerRepository _questionAnswerRepository;
        private readonly QuestionAnswerPartRepository _questionAnswerPartRepository;
        private readonly TimerChangeEventRepository _timerChangeEventRepository;
        private readonly GameRepository _gameRepository;
        private bool _isApplyingAutoTune;

        //VerticalStackLayout _vsl;
        protected IDispatcherTimer timer;
        protected virtual void TimerInit()
        {
            timer = Application.Current.Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) =>
            {
                MainThread.BeginInvokeOnMainThread(UpdateStatement);
            };
        }
        private void UpdateStatement()
        {
            //Console.WriteLine("Updating statement. Current status: {0}", _gamePlay.Status);
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

        private bool ShouldShowKeyboardSubmitButton()
        {
            return UsesManualKeyboardCheckMode() &&
                   !ShouldUseInlineKeyboardCheckButton();
        }

        private bool ShouldUseInlineKeyboardCheckButton()
        {
            return UsesManualKeyboardCheckMode();
        }

        private bool ShouldShowPpwCheckButton()
        {
            return (!_isKeyboard || _config.KeyboardConfig.KeyboardOnlyForHelp) &&
                   !UsesBenchmarkPickerPreview();
        }

        private bool UsesBenchmarkPickerPreview()
        {
            string gameName = _config.GameName ?? string.Empty;
            return _config.ShowPrev &&
                   _isThreeTexts &&
                   !_isKeyboard &&
                   (gameName.Contains("Benchmark", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(gameName, "Level 3.2", StringComparison.OrdinalIgnoreCase));
        }

        private bool IsGroupByColorKeyboardStage()
        {
            return _gamePlay is BitArrayGamePlay bitArrayGamePlay &&
                   bitArrayGamePlay.CurrentOperation == Operation.GroupByColor;
        }

        private bool UsesManualKeyboardCheckMode()
        {
            return _isKeyboard &&
                   !_config.KeyboardConfig.KeyboardOnlyForHelp &&
                   (_config.KeyboardConfig.SyncType == SyncType.None || IsGroupByColorKeyboardStage());
        }

        private bool UsesSyncKeyboardSubmissionMode()
        {
            return _isKeyboard &&
                   !_config.KeyboardConfig.KeyboardOnlyForHelp &&
                   !UsesManualKeyboardCheckMode();
        }

        private bool UsesThreeColorGroupByColorStage()
        {
            return IsGroupByColorKeyboardStage() &&
                   (_config.KeyboardConfig.GroupByColorColorCount >= 3 ||
                    (_config.KeyboardConfig.GroupByColorCounts?.Length ?? 0) >= 3);
        }

        private bool ShouldShowNextButton()
        {
            return !IsGroupByColorKeyboardStage() &&
                   !UsesBenchmarkPickerPreview();
        }

        private bool ShouldShowImpossibleWeightedAnswerButton()
        {
            return _isKeyboard &&
                   !_config.KeyboardConfig.KeyboardOnlyForHelp &&
                   _gamePlay.SupportsImpossibleWeightedAnswer;
        }

        private bool HasVisibleManualCheckButton()
        {
            return (_btnCheck?.IsVisible ?? false) ||
                   (_btnKeyboardCheckInline?.IsVisible ?? false) ||
                   (_btnKeyboardSubmit?.IsVisible ?? false) ||
                   (_btnImpossibleWeightedAnswer?.IsVisible ?? false);
        }

        private void SetInlineKeyboardCheckVisible(bool isVisible)
        {
            if (_btnKeyboardCheckInline == null)
                return;

            bool shouldShow = isVisible && ShouldUseInlineKeyboardCheckButton();
            _btnKeyboardCheckInline.IsVisible = shouldShow;
            _btnKeyboardCheckInline.IsEnabled = shouldShow && _isPageInteractionEnabled;
            RefreshStatusActionSlot();
        }

        private void EnsureKeyboardInlineCheckButton()
        {
            if (_btnKeyboardCheckInline != null || !ShouldUseInlineKeyboardCheckButton())
                return;

            _btnKeyboardCheckInline = new Button
            {
                Text = "Check",
                Command = _cmdCheck,
                WidthRequest = 220,
                HeightRequest = 55,
                CornerRadius = 12,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center
            };
        }

        private void EnsureCenterFeedbackBadge()
        {
            if (_centerFeedbackBadge != null)
                return;

            _centerFeedbackBadgeLabel = new Label
            {
                FontSize = 55,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                TextColor = Colors.White
            };

            _centerFeedbackBadge = new Border
            {
                WidthRequest = 220,
                HeightRequest = 55,
                Padding = 0,
                StrokeThickness = 0,
                BackgroundColor = Colors.Transparent,
                IsVisible = false,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Content = _centerFeedbackBadgeLabel
            };
        }

        private void EnsureCustomProgressVisual()
        {
            if (_customProgressHost != null)
                return;

            _customProgressFill = new Border
            {
                WidthRequest = 0,
                HeightRequest = 55,
                Padding = 0,
                StrokeThickness = 0,
                BackgroundColor = Color.FromArgb("#5A42D0"),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Fill,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                IsVisible = false,
                InputTransparent = true
            };

            _customProgressHost = new Grid
            {
                WidthRequest = 220,
                HeightRequest = 55,
                BackgroundColor = Colors.Transparent,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center,
                IsVisible = false,
                InputTransparent = true
            };

            _customProgressHost.Add(_customProgressFill);
            _customProgressHost.SizeChanged += (_, _) => RefreshCustomProgressVisual();
        }

        private void RefreshCustomProgressVisual()
        {
            if (_customProgressHost == null || _customProgressFill == null || _pianoPressProgress == null)
                return;

            double hostWidth = _customProgressHost.Width > 0
                ? _customProgressHost.Width
                : _customProgressHost.WidthRequest;

            double progress = Math.Clamp(_pianoPressProgress.Progress, 0, 1);
            _customProgressFill.BackgroundColor = _pianoPressProgress.ProgressColor;
            _customProgressFill.WidthRequest = hostWidth * progress;
            _customProgressFill.IsVisible = _customProgressHost.IsVisible && progress > 0;
        }

        private void RefreshStatusActionSlot()
        {
            bool showTutorialStepCounter = !string.IsNullOrWhiteSpace(_tutorialStepCounterText);
            bool isFeedbackState = _currentUiState == PlayUiState.FeedbackCorrect ||
                                   _currentUiState == PlayUiState.FeedbackWrong;
            bool usePromptEntryFeedback = UsesArrowCorrectResponseFeedback() && isFeedbackState;
            bool showFeedbackBadge = isFeedbackState && !usePromptEntryFeedback && !showTutorialStepCounter;
            bool usesInlineCheck = ShouldUseInlineKeyboardCheckButton();
            bool showInlineCheck = usesInlineCheck &&
                                   _btnKeyboardCheckInline != null &&
                                   _btnKeyboardCheckInline.IsVisible &&
                                   !isFeedbackState &&
                                   !showTutorialStepCounter;
            bool showProgress = _pianoPressProgress != null &&
                                !usesInlineCheck &&
                                !isFeedbackState &&
                                !showTutorialStepCounter;

            if (_pianoPressProgress != null)
                _pianoPressProgress.IsVisible = showProgress;

            if (_customProgressHost != null)
            {
                _customProgressHost.IsVisible = showProgress;
                RefreshCustomProgressVisual();
            }

            if (_btnKeyboardCheckInline != null)
            {
                _btnKeyboardCheckInline.IsVisible = showInlineCheck;
                _btnKeyboardCheckInline.IsEnabled = showInlineCheck && _isPageInteractionEnabled;
            }

            if (_centerFeedbackBadge == null || _centerFeedbackBadgeLabel == null)
                return;

            if (showTutorialStepCounter)
            {
                _centerFeedbackBadge.BackgroundColor = Colors.Yellow;
                _centerFeedbackBadgeLabel.TextColor = Colors.Black;
                _centerFeedbackBadgeLabel.Text = _tutorialStepCounterText;
            }
            else if (showFeedbackBadge)
            {
                bool isCorrect = _currentUiState == PlayUiState.FeedbackCorrect;
                _centerFeedbackBadge.BackgroundColor = Colors.Transparent;
                _centerFeedbackBadgeLabel.TextColor = Colors.White;
                _centerFeedbackBadgeLabel.Text = isCorrect ? "💪" : "🤔";
            }

            _centerFeedbackBadge.IsVisible = showTutorialStepCounter || showFeedbackBadge;
        }

        private void SetTutorialStepCounter(int currentStep, int totalSteps)
        {
            _tutorialStepCounterText = currentStep.ToString();
            RefreshStatusActionSlot();
        }

        private void ClearTutorialStepCounter()
        {
            if (_tutorialStepCounterText == null)
                return;

            _tutorialStepCounterText = null;
            RefreshStatusActionSlot();
        }

        private string GetKeyboardTypeText()
        {
            if (_config.KeyboardConfig?.KeyboardAsAQuestion == true)
                return "Read only";

            return _config.KeyboardConfig?.SyncType switch
            {
                SyncType.Sync => "Sync keyboard",
                SyncType.HalfSync => "Half sync",
                SyncType.Spatial => "Spatial keyboard",
                _ => "Piano keyboard"
            };
        }

        private bool SupportsThirdArrowVisibilityToggle()
        {
            return _config.KeyboardConfig?.IsArrow == true &&
                   _gamePlay is BitArrayGamePlay arrowGamePlay &&
                   arrowGamePlay.SupportsThirdArrowVisibilityControl();
        }

        private async Task ToggleThirdArrowVisibilityAsync()
        {
            if (_gamePlay is not BitArrayGamePlay arrowGamePlay)
                return;

            arrowGamePlay.ForceShowMaskedThirdArrow = !arrowGamePlay.ForceShowMaskedThirdArrow;
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                RefreshArrowVisuals();
                RefreshKeyboardControlBar();
            });
        }

        private void RefreshArrowVisuals()
        {
            if (!_isKeyboard || _pianoKeyboard == null)
                return;

            if (_config.KeyboardConfig?.IsArrow == true && _gamePlay is BitArrayGamePlay arrowGamePlay)
            {
                _pianoKeyboard.RemoveArrows();
                _pianoKeyboard.BackgroundColor = ShouldCycleArrowBackground()
                    ? GetArrowBackgroundColor()
                    : Colors.Black;
                _pianoKeyboard.AddArrow(
                    arrowGamePlay.dir,
                    arrowGamePlay.aboveNumber,
                    arrowGamePlay.length,
                    labelTextOverride: arrowGamePlay.GetCurrentArrowLabelText());
            }
            else
            {
                _pianoKeyboard.BackgroundColor = Colors.Black;
            }
        }

        private View BuildInlineAnswerTimeControls()
        {
            _answerTimeEnabledSwitch = new Microsoft.Maui.Controls.Switch
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                OnColor = Color.FromArgb("#6D4AFF"),
                Scale = 0.85
            };

            _answerTimeEnabledSwitch.Toggled += async (_, e) =>
            {
                if (_pianoKeyboard is not PianoKeyboardSync syncKeyboard)
                    return;

                if (e.Value)
                {
                    int seconds = GetEffectiveAnswerTimeMagnitude(syncKeyboard);
                    int sign = _lastNonZeroAnswerTimeSetting < 0 ? -1 : (syncKeyboard.AnswerTimeSetting < 0 ? -1 : 1);
                    await ApplyAnswerTimeSettingAsync(seconds * sign, "AnswerTimeEnabled");
                }
                else
                {
                    if (syncKeyboard.AnswerTimeSetting != 0)
                        _lastNonZeroAnswerTimeSetting = syncKeyboard.AnswerTimeSetting;

                    await ApplyAnswerTimeSettingAsync(0, "AnswerTimeDisabled");
                }
            };

            _answerTimeMinusButton = new Button
            {
                Text = "-",
                FontSize = 18,
                WidthRequest = 34,
                HeightRequest = 34,
                CornerRadius = 10,
                Padding = 0,
                BackgroundColor = Colors.White,
                TextColor = Colors.Black
            };
            _answerTimeMinusButton.Clicked += async (_, _) =>
            {
                if (_pianoKeyboard is not PianoKeyboardSync syncKeyboard)
                    return;

                int seconds = Math.Max(1, GetEffectiveAnswerTimeMagnitude(syncKeyboard) - 1);
                int sign = UsesWholeAnswerTimer(syncKeyboard) ? -1 : 1;
                await ApplyAnswerTimeSettingAsync(seconds * sign, "AnswerTimeMinus");
            };

            _answerTimePlusButton = new Button
            {
                Text = "+",
                FontSize = 18,
                WidthRequest = 34,
                HeightRequest = 34,
                CornerRadius = 10,
                Padding = 0,
                BackgroundColor = Colors.White,
                TextColor = Colors.Black
            };
            _answerTimePlusButton.Clicked += async (_, _) =>
            {
                if (_pianoKeyboard is not PianoKeyboardSync syncKeyboard)
                    return;

                int seconds = Math.Min(AnswerTimeStateMaxSeconds, GetEffectiveAnswerTimeMagnitude(syncKeyboard) + 1);
                int sign = UsesWholeAnswerTimer(syncKeyboard) ? -1 : 1;
                await ApplyAnswerTimeSettingAsync(seconds * sign, "AnswerTimePlus");
            };

            _answerTimeValueLabel = new Label
            {
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                MinimumWidthRequest = 44
            };

            _answerTimeModeLabel = new Label
            {
                FontSize = 10,
                TextColor = Colors.White.WithAlpha(0.85f),
                HorizontalTextAlignment = TextAlignment.End,
                VerticalTextAlignment = TextAlignment.Center
            };

            _answerTimeModeButton = new Button
            {
                FontSize = 11,
                Padding = new Thickness(10, 4),
                CornerRadius = 10,
                BackgroundColor = Color.FromArgb("#F4F0FF"),
                TextColor = Color.FromArgb("#5A42D0"),
                VerticalOptions = LayoutOptions.Center
            };
            _answerTimeModeButton.Clicked += async (_, _) => await ToggleAnswerTimeModeAsync();

            return new HorizontalStackLayout
            {
                Spacing = 6,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label
                    {
                        Text = "Timer",
                        FontSize = 11,
                        TextColor = Colors.White.WithAlpha(0.85f),
                        VerticalTextAlignment = TextAlignment.Center
                    },
                    _answerTimeEnabledSwitch,
                    _answerTimeMinusButton,
                    _answerTimeValueLabel,
                    _answerTimePlusButton,
                    _answerTimeModeLabel,
                    _answerTimeModeButton
                }
            };
        }

        private View BuildKeyboardControlBar()
        {
            EnsureKeyboardInlineCheckButton();
            EnsureCenterFeedbackBadge();
            EnsureCustomProgressVisual();

            Grid statusActionHost = new()
            {
                WidthRequest = 220,
                HeightRequest = 55,
                BackgroundColor = Colors.Transparent,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 6, 0, 6),
                InputTransparent = false
            };
            if (_customProgressHost != null)
                statusActionHost.Add(_customProgressHost);
            if (_centerFeedbackBadge != null)
                statusActionHost.Add(_centerFeedbackBadge);
            if (_btnKeyboardCheckInline != null)
                statusActionHost.Add(_btnKeyboardCheckInline);
            _keyboardControlBar = new Border
            {
                Padding = 0,
                Margin = new Thickness(0, 0, 0, 0),
                StrokeThickness = 0,
                Stroke = Colors.Transparent,
                BackgroundColor = Colors.Transparent,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                ZIndex = 10,
                Content = statusActionHost
            };

            RefreshKeyboardControlBar();
            return _keyboardControlBar;
        }

        private void RefreshKeyboardControlBar()
        {
            if (_pianoPressProgress != null)
                _pianoPressProgress.HeightRequest = 55;

            if (_customProgressHost != null)
                _customProgressHost.HeightRequest = 55;

            if (_customProgressFill != null)
                _customProgressFill.HeightRequest = 55;

            RefreshStatusActionSlot();
            RefreshAnswerTimeTuner();
        }

        /*private bool _btnNextEnabled = false;
        public bool BtnNextEnabled { get => _btnNextEnabled; }*/
        #region view updating

        public void AddToLblAction(string text)
        {
            _lblAction.Text += text;
        }

        private void CapturePreviousAnswer(PPWObject submittedAnswer)
        {
            _previousPPW = submittedAnswer;
            _previousActionText = _lblAction?.Text ?? _gamePlay.CurrentOperation.ToDString();
            RefreshPreviousPreview();
        }

        private static string FormatPreviousValue(int value)
        {
            return value == PPWGamePlay.NAN ? string.Empty : value.ToString();
        }

        private static int ParseEntryValueOrNan(Entry entry)
        {
            return int.TryParse(entry?.Text, out int value) ? value : PPWGamePlay.NAN;
        }

        private void CapturePreviousExerciseSnapshot()
        {
            if (!_isThreeTexts || _txtAddend1 == null || _txtAddend2 == null || _txtSum == null)
                return;

            _previousPPW = new PPWObject(
                ParseEntryValueOrNan(_txtAddend1),
                ParseEntryValueOrNan(_txtAddend2),
                ParseEntryValueOrNan(_txtSum));
            _previousActionText = _lblAction?.Text ?? _gamePlay.CurrentOperation.ToDString();
            RefreshPreviousPreview();
        }

        private void ShowPreviousInline()
        {
            if (_previousPPW == null || !_isThreeTexts)
                return;

            _txtAddend1.Text = FormatPreviousValue(_previousPPW.Addend1);
            _txtAddend2.Text = FormatPreviousValue(_previousPPW.Addend2);
            _txtSum.Text = FormatPreviousValue(_previousPPW.Sum);
            _txtAddend1.IsEnabled = false;
            _txtAddend2.IsEnabled = false;
            _txtSum.IsEnabled = false;
            RefreshNumericEntryAppearance();
        }

        private void RestoreCurrentInlinePreview()
        {
            if (_currentPPW == null || !_isThreeTexts)
                return;

            _txtAddend1.IsEnabled = _currentPPWEnabled.Addend1 == 1;
            _txtAddend2.IsEnabled = _currentPPWEnabled.Addend2 == 1;
            _txtSum.IsEnabled = _currentPPWEnabled.Sum == 1;
            _txtAddend1.Text = _currentPPW.Addend1 == PPWGamePlay.NAN ? "" : _currentPPW.Addend1.ToString();
            _txtAddend2.Text = _currentPPW.Addend2 == PPWGamePlay.NAN ? "" : _currentPPW.Addend2.ToString();
            _txtSum.Text = _currentPPW.Sum == PPWGamePlay.NAN ? "" : _currentPPW.Sum.ToString();
            RefreshNumericEntryAppearance();
        }

        private void TogglePreviousBelow()
        {
            _showPreviousBelow = !_showPreviousBelow;
            RefreshPreviousPreview();
        }

        private Entry CreatePreviousEntry(double widthRequest)
        {
            return new Entry
            {
                IsReadOnly = true,
                IsEnabled = true,
                InputTransparent = true,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                BackgroundColor = Colors.White,
                TextColor = Colors.Black,
                WidthRequest = widthRequest,
                FontSize = FONT_SIZE_DEFAULT
            };
        }

        private View BuildEquationPreviousLayout()
        {
            double addendWidth = TASK_WIDTH / 2;

            _prevSumEntry = CreatePreviousEntry(TASK_WIDTH);
            _prevSumEntry.FontSize = _txtSum?.FontSize > 0 ? _txtSum.FontSize : 32;

            _prevAddend1Entry = CreatePreviousEntry(addendWidth);
            _prevAddend2Entry = CreatePreviousEntry(addendWidth);

            _prevActionLabel = new Label
            {
                FontSize = FONT_SIZE_DEFAULT,
                TextColor = Colors.Gray,
                Opacity = 0,
                IsVisible = false,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                WidthRequest = 0
            };

            _prevEqualsLabel = new Label
            {
                FontSize = FONT_SIZE_DEFAULT,
                TextColor = Colors.Gray,
                Opacity = 0,
                IsVisible = false,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                WidthRequest = 0,
                Text = "="
            };

            return new VerticalStackLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                Spacing = EquationHelpRowSpacing,
                Children =
                {
                    new HorizontalStackLayout
                    {
                        HorizontalOptions = LayoutOptions.Center,
                        Children = { _prevSumEntry }
                    },
                    new HorizontalStackLayout
                    {
                        HorizontalOptions = LayoutOptions.Center,
                        Spacing = 0,
                        WidthRequest = TASK_WIDTH,
                        Children = { _prevAddend1Entry, _prevAddend2Entry }
                    }
                }
            };
        }

        private View BuildStandardPreviousLayout()
        {
            double equationWidth = TASK_WIDTH / 2;
            _prevSumEntry = CreatePreviousEntry(TASK_WIDTH);
            _prevAddend1Entry = CreatePreviousEntry(equationWidth);
            _prevAddend2Entry = CreatePreviousEntry(equationWidth);
            _prevActionLabel = new Label
            {
                FontSize = FONT_SIZE_DEFAULT,
                TextColor = Colors.Gray,
                Opacity = 0.2,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                WidthRequest = 20
            };
            _prevEqualsLabel = new Label
            {
                FontSize = FONT_SIZE_DEFAULT,
                TextColor = Colors.Gray,
                Opacity = 0.15,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                WidthRequest = 20,
                Text = "="
            };

            return new VerticalStackLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                Spacing = 6,
                Children =
                {
                    new HorizontalStackLayout
                    {
                        HorizontalOptions = LayoutOptions.Center,
                        Children = { _prevSumEntry }
                    },
                    new HorizontalStackLayout
                    {
                        HorizontalOptions = LayoutOptions.Center,
                        Children = { _prevAddend1Entry, _prevActionLabel, _prevAddend2Entry }
                    }
                }
            };
        }

        private View BuildPreviousBelowView()
        {
            View previousView = _config.UIQuestionType == UIQuestionType.SimpleEquation
                ? BuildEquationPreviousLayout()
                : BuildStandardPreviousLayout();

            if (UsesBenchmarkPickerPreview())
            {
                previousView.Opacity = 0.42;
                previousView.Scale = 0.96;

                _previousBelowView = new VerticalStackLayout
                {
                    HorizontalOptions = LayoutOptions.Center,
                    Spacing = 2,
                    IsVisible = false,
                    Margin = new Thickness(0, 0, 0, 2),
                    InputTransparent = true,
                    Children = { previousView }
                };

                RefreshPreviousPreview();
                return _previousBelowView;
            }

            _previousBelowView = new VerticalStackLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                Spacing = 4,
                IsVisible = false,
                Children =
                {
                    new Label
                    {
                        Text = "Previous",
                        FontSize = FONT_SIZE_DEFAULT - 2,
                        TextColor = Colors.Gray,
                        HorizontalOptions = LayoutOptions.Center
                    },
                    previousView
                }
            };

            RefreshPreviousPreview();
            return _previousBelowView;
        }

        private void RefreshPreviousPreview()
        {
            bool hasPrevious = _previousPPW != null;

            if (_btnPrev != null)
                _btnPrev.IsEnabled = hasPrevious;

            if (_btnPrevBelow != null)
            {
                _btnPrevBelow.IsEnabled = hasPrevious;
                _btnPrevBelow.Text = _showPreviousBelow ? "Hide Prev" : "Show Prev";
            }

            if (_previousBelowView == null)
                return;

            if (!hasPrevious)
            {
                _previousBelowView.IsVisible = false;
                return;
            }

            _prevAddend1Entry.Text = FormatPreviousValue(_previousPPW.Addend1);
            _prevAddend2Entry.Text = FormatPreviousValue(_previousPPW.Addend2);
            _prevSumEntry.Text = FormatPreviousValue(_previousPPW.Sum);
            if (_config.UIQuestionType != UIQuestionType.SimpleEquation)
                _prevActionLabel.Text = _previousActionText;

            if (UsesBenchmarkPickerPreview())
            {
                _previousBelowView.Margin = hasPrevious
                    ? new Thickness(0, -42, 0, -10)
                    : new Thickness(0, 0, 0, 2);

                if (_questionInputsContainer != null)
                {
                    _questionInputsContainer.Margin = hasPrevious
                        ? new Thickness(_questionInputsBaseMargin.Left, -6, _questionInputsBaseMargin.Right, _questionInputsBaseMargin.Bottom)
                        : _questionInputsBaseMargin;
                }
            }

            _previousBelowView.IsVisible = UsesBenchmarkPickerPreview() || _showPreviousBelow;
        }

        private async Task AnimateBenchmarkQuestionAdvanceInAsync()
        {
            if (!UsesBenchmarkPickerPreview() || _questionInputsContainer == null)
                return;

            List<Task> animations = new();

            if (_previousBelowView?.IsVisible == true)
            {
                _previousBelowView.Opacity = 0.14;
                _previousBelowView.TranslationY = 20;
                _previousBelowView.Scale = 1.01;
                animations.Add(_previousBelowView.TranslateTo(0, 0, 210, Easing.CubicOut));
                animations.Add(_previousBelowView.FadeTo(1, 210, Easing.CubicOut));
                animations.Add(_previousBelowView.ScaleTo(0.96, 210, Easing.CubicOut));
            }

            _questionInputsContainer.Opacity = 0.1;
            _questionInputsContainer.TranslationY = 52;
            _questionInputsContainer.Scale = 0.98;
            animations.Add(_questionInputsContainer.TranslateTo(0, 0, 220, Easing.CubicOut));
            animations.Add(_questionInputsContainer.FadeTo(1, 220, Easing.CubicOut));
            animations.Add(_questionInputsContainer.ScaleTo(1, 220, Easing.CubicOut));

            await Task.WhenAll(animations);
        }

        private async Task AnimateBenchmarkQuestionAdvanceOutAsync()
        {
            if (!UsesBenchmarkPickerPreview() || _questionInputsContainer == null)
                return;

            List<Task> animations = new()
            {
                _questionInputsContainer.TranslateTo(0, -34, 130, Easing.CubicIn),
                _questionInputsContainer.FadeTo(0.18, 130, Easing.CubicIn),
                _questionInputsContainer.ScaleTo(0.97, 130, Easing.CubicIn)
            };

            if (_previousBelowView?.IsVisible == true)
            {
                animations.Add(_previousBelowView.FadeTo(0.32, 110, Easing.CubicIn));
            }

            await Task.WhenAll(animations);
        }

        private async Task TryAdvanceBenchmarkPickerAsync()
        {
            if (!UsesBenchmarkPickerPreview() ||
                _benchmarkAdvanceGestureRunning ||
                !_isPageInteractionEnabled ||
                _tutorialRunning ||
                _gamePlay.GameOver)
            {
                return;
            }

            _benchmarkAdvanceGestureRunning = true;
            try
            {
                CapturePreviousExerciseSnapshot();
                await GenerateNextExerciseAsync();
            }
            finally
            {
                _benchmarkAdvanceGestureRunning = false;
            }
        }

        private void AttachBenchmarkPickerGesture(View target)
        {
            if (!UsesBenchmarkPickerPreview() || target == null)
                return;

            SwipeGestureRecognizer swipeUp = new()
            {
                Direction = SwipeDirection.Up
            };
            swipeUp.Swiped += async (_, _) => await TryAdvanceBenchmarkPickerAsync();
            target.GestureRecognizers.Add(swipeUp);
        }

        private async Task RunEquationHelpAsync()
        {
            if (_equationHelpRunning || _config.UIQuestionType != UIQuestionType.SimpleEquation || _hzlEquation == null)
                return;

            _equationHelpRunning = true;
            SetPageInteractionEnabled(false);

            double addend1Width = _txtAddend1.Width > 0 ? _txtAddend1.Width : (TASK_WIDTH / 2);
            double addend2Width = _txtAddend2.Width > 0 ? _txtAddend2.Width : (TASK_WIDTH / 2);
            double sumWidth = _txtSum.Width > 0 ? _txtSum.Width : (TASK_WIDTH / 2);
            double addendHeight = _txtAddend1.Height > 0 ? _txtAddend1.Height : 48;
            double sumHeight = _txtSum.Height > 0 ? _txtSum.Height : 48;
            double centerShiftY = (((sumHeight + addendHeight) / 2) + EquationHelpRowSpacing) / 2;
            double centerX = _hzlEquation.Width > 0 ? _hzlEquation.Width / 2 : TASK_WIDTH;
            double sumTargetCenterX = centerX;
            double targetAddendWidth = TASK_WIDTH / 2;
            double addendRowWidth = TASK_WIDTH;
            double addendRowLeft = centerX - (addendRowWidth / 2);
            double addend1TargetCenterX = addendRowLeft + (targetAddendWidth / 2);
            double addend2TargetCenterX = addendRowLeft + targetAddendWidth + (targetAddendWidth / 2);

            double sumShiftX = sumTargetCenterX - (_txtSum.X + (_txtSum.Width / 2));
            double addend1ShiftX = addend1TargetCenterX - (_txtAddend1.X + (_txtAddend1.Width / 2));
            double addend2ShiftX = addend2TargetCenterX - (_txtAddend2.X + (_txtAddend2.Width / 2));
            double sumScaleX = sumWidth > 0 ? TASK_WIDTH / sumWidth : 1;
            double addend1ScaleX = addend1Width > 0 ? targetAddendWidth / addend1Width : 1;
            double addend2ScaleX = addend2Width > 0 ? targetAddendWidth / addend2Width : 1;

            try
            {
                await Task.WhenAll(
                    _txtSum.TranslateTo(sumShiftX, -centerShiftY, 360, Easing.CubicInOut),
                    _txtSum.ScaleXTo(sumScaleX, 360, Easing.CubicInOut),
                    _txtAddend1.TranslateTo(addend1ShiftX, centerShiftY, 360, Easing.CubicInOut),
                    _txtAddend1.ScaleXTo(addend1ScaleX, 360, Easing.CubicInOut),
                    _txtAddend2.TranslateTo(addend2ShiftX, centerShiftY, 360, Easing.CubicInOut),
                    _txtAddend2.ScaleXTo(addend2ScaleX, 360, Easing.CubicInOut),
                    _lblAction.FadeTo(0, 220, Easing.CubicInOut),
                    _lblEquationEquals.FadeTo(0, 220, Easing.CubicInOut)
                );

                await Task.Delay(3000);

                await Task.WhenAll(
                    _txtSum.TranslateTo(0, 0, 320, Easing.CubicInOut),
                    _txtSum.ScaleXTo(1, 320, Easing.CubicInOut),
                    _txtAddend1.TranslateTo(0, 0, 320, Easing.CubicInOut),
                    _txtAddend1.ScaleXTo(1, 320, Easing.CubicInOut),
                    _txtAddend2.TranslateTo(0, 0, 320, Easing.CubicInOut),
                    _txtAddend2.ScaleXTo(1, 320, Easing.CubicInOut),
                    _lblAction.FadeTo(1, 220, Easing.CubicInOut),
                    _lblEquationEquals.FadeTo(1, 220, Easing.CubicInOut)
                );
            }
            finally
            {
                _equationHelpRunning = false;
                SetPageInteractionEnabled(true);
                RestoreReadyForInputState();
            }
        }

        public async Task UpdateView(bool newExercise = false, bool applyUiState = true, ExerciseGenerationResult? generatedExercise = null, bool allowInputFocus = true)
        {
            if (_tutorialRunning) return;

            UpdateStatement();
            RefreshKeyboardArrowPromptView();
            if (applyUiState)
                ApplyExerciseUiState(newExercise);

            if (generatedExercise != null)
                _lastGeneratedExercise = generatedExercise;

            List<Task> tasks = new();

            if (_btnNext != null) _btnNext.IsEnabled = _gamePlay.GuessNumber > 0 && !newExercise;
            if (_config.IsHistory) _lblHistory.Text = GenerateHistoryString(_gamePlay.AllHistory.Where(item => item.Sum == _gamePlay.Sum).ToList());
            if (_isThreeTexts && (_config.UIQuestionType != UIQuestionType.ThreeAddends || newExercise))
            {
                _txtAddend1.Text = _gamePlay.addend1 == PPWGamePlay.NAN ? "" : _gamePlay.addend1.ToString();
                _txtAddend2.Text = _gamePlay.addend2 == PPWGamePlay.NAN ? "" : _gamePlay.addend2.ToString();
                _txtSum.Text = _gamePlay.Sum == PPWGamePlay.NAN ? "" : _gamePlay.Sum.ToString();
                _hr.IsVisible = _gamePlay.CurrentOperation == Operation.Multiplication;

            }
            if(_config.UIQuestionType  == UIQuestionType.ThreeAddends && !newExercise)
            {
                if (_gamePlay.addend1 == PPWGamePlay.NAN) _txtAddend1.Text = "";
                if(_gamePlay.addend2 == PPWGamePlay.NAN) _txtAddend2.Text = "";
                if(_gamePlay.Sum == PPWGamePlay.NAN) _txtSum.Text = "";
            }
            if (newExercise && _config.UIQuestionType == UIQuestionType.CanvasesHands)
            {
                leftHandCanvas.IsVisible = true; rightHandCanvas.IsVisible = true;
                if (_config.SecondsTillHideExercise > 0)
                {
                    tasks.Add(HideGraphicsView(leftHandCanvas, _config.SecondsTillHideExercise));
                    tasks.Add(HideGraphicsView(rightHandCanvas, _config.SecondsTillHideExercise));
                }
            }

            if (newExercise && _config.SecondsTillAllowInput > 0)
            {
                if (_btnNext != null) { _btnNext.IsEnabled = _gamePlay.GuessNumber > 0 && !newExercise; Console.WriteLine(" _gamePlay.GuessNumber: {0}", _gamePlay.GuessNumber); }

                tasks.Add(DelayKeyboardInputAsync(_config.SecondsTillAllowInput));
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
                    _currentSecondaryPPW = null;
                    _currentSecondaryPPWEnabled = null;

                }
                if (_config.HelpEntries || _config.HelpThroughTen)
                    for (int i = 0; i < txt.Length; i++)
                        txt[i].Text = "";
                if (_config.UIQuestionType == UIQuestionType.TwoLinesTwoAddends)
                {
                    //TODO:what if null?
                    PPWObject secondary = _gamePlay.GenerateSecondaryTriad(_gamePlay.Sum);
                    if(_config.MaxSum>10)
                        secondary = _gamePlay.GenerateSecondaryTriad(_gamePlay.Sum, 10, 10);
                    txt[0].Text = secondary.Addend1.ToString();
                    txt[1].Text = secondary.Addend2.ToString();
                    _currentSecondaryPPW = new PPWObject(secondary.Addend1, secondary.Addend2, _gamePlay.Sum);
                    _currentSecondaryPPWEnabled = new PPWObject(
                        txt.ElementAtOrDefault(0)?.IsEnabled == true ? 1 : 0,
                        txt.ElementAtOrDefault(1)?.IsEnabled == true ? 1 : 0,
                        0);
                }
                if (_config.UIQuestionType == UIQuestionType.ThreeAddends)
                {
                    if (_gamePlay.addend1 == PPWGamePlay.NAN)
                    {
                        PPWObject secondary = _gamePlay.GenerateTriadBySum(_gamePlay.addend2);
                        txt[0].Text = secondary.Addend1.ToString();
                        _txtAddend2.Text = secondary.Addend2.ToString();
                        _currentSecondaryPPW = new PPWObject(secondary.Addend1, secondary.Addend2, _gamePlay.addend2);
                        _currentSecondaryPPWEnabled = new PPWObject(
                            txt.ElementAtOrDefault(0)?.IsEnabled == true ? 1 : 0,
                            _txtAddend2.IsEnabled ? 1 : 0,
                            0);
                    }
                    else
                    {
                        PPWObject secondary = _gamePlay.GenerateTriadBySum(_gamePlay.addend1);
                        _txtAddend1.Text = secondary.Addend1.ToString();
                        txt[0].Text = secondary.Addend2.ToString();
                        _currentSecondaryPPW = new PPWObject(secondary.Addend1, secondary.Addend2, _gamePlay.addend1);
                        _currentSecondaryPPWEnabled = new PPWObject(
                            _txtAddend1.IsEnabled ? 1 : 0,
                            txt.ElementAtOrDefault(0)?.IsEnabled == true ? 1 : 0,
                            0);
                    }
                }
                    if (_config.HelpThroughTen)
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
                    UpdateLogicalActionVisual((BitArrayGamePlay)_gamePlay);
                    if (_config.UsesCombinedLogicalKeyboard)
                    {

                        _keyboardTask1.PianoInit(((BitArrayGamePlay)_gamePlay).BitArrayQuestion
    .Concat(((BitArrayGamePlay)_gamePlay).BitArrayQuestion2).ToArray());
                        if (_config.UsesSpecialLogicalKeyboardColors)
                            _keyboardTask1.SpecialColors();
                        _keyboardTask1.SetNoBorderBetweenRows();
                        _keyboardTask2.IsVisible = false;
                    }
                    else
                    {
                        if (((BitArrayGamePlay)_gamePlay).CurrentOperation == Operation.GroupByColor)
                        {
                            BitArrayGamePlay colorGamePlay = (BitArrayGamePlay)_gamePlay;
                            _keyboardTask2.IsVisible = false;
                            _keyboardTask1.HeightRequest = PIANO_HEIGHT2;
                            _keyboardTask1.PianoInit(colorGamePlay.GetGroupByColorQuestionColors());
                        }
                        else
                        {
                        _keyboardTask2.PianoInit(((BitArrayGamePlay)_gamePlay).BitArrayQuestion2);
                        if (GameConfig.Operations.LogicalDual.Contains(((BitArrayGamePlay)_gamePlay).CurrentOperation))
                        {
                            _keyboardTask2.IsVisible = true;
                            _keyboardTask1.HeightRequest = PIANO_HEIGHT2;
                            _keyboardTask2.HeightRequest = PIANO_HEIGHT2;
                        }
                        else
                        {
                            _keyboardTask2.IsVisible = false;
                            _keyboardTask1.HeightRequest = PIANO_HEIGHT1;
                            _keyboardTask2.HeightRequest = PIANO_HEIGHT1;
                        }
                        _keyboardTask1.PianoInit(((BitArrayGamePlay)_gamePlay).BitArrayQuestion);
                        if (_config.KeyboardOnly)//TODO: move to init method
                            _keyboardTask1.IsVisible = false;
                        if (_taskMainHost != null && (_config.IncludeTutorials || _config.KeyboardOnly))
                        {
                            if (_config.KeyboardOnly)
                                _taskMainHost.SetStaticBits(((BitArrayGamePlay)_gamePlay).BitArrayQuestion);
                            if (_config.IncludeTutorials)
                            {
                                await RunRecordedKeyboardTutorialAsync(_taskMainHost);
                            }
                        }
                        }
                    }
                }
                if (_config.UIQuestionType == UIQuestionType.CanvasesHands)
                {
                    ((BitArrayGamePlay)_gamePlay).BitArrayforHands(((HandDrawable)leftHandCanvas.Drawable).Bits, ((HandDrawable)rightHandCanvas.Drawable).Bits);
                    leftHandCanvas.Invalidate();
                    rightHandCanvas.Invalidate();
                }

                if (_config.KeyboardConfig != null && _config.KeyboardConfig.IsArrow)
                {
                    BitArrayGamePlay arrowGamePlay = (BitArrayGamePlay)_gamePlay;
                    Console.WriteLine("aboveNumver: {0}, length: {1}", arrowGamePlay.aboveNumber, arrowGamePlay.length);
                    RefreshArrowVisuals();
                    //if (aboveNumber == 10) { _pianoKeyboard.AddArrow(dir, 0/*, _gamePlay.Sum*/); }

                }
                else if (_isKeyboard)
                {
                    _pianoKeyboard.BackgroundColor = Colors.Black;
                    _pianoKeyboard.ClearTraceOverlay();
                }
                if (_isKeyboard && _config.FromNumToNum)
                {
                    _pianoKeyboard.initColors = _pianoKeyboard.ToBitArray();
                }
                if (_lblAction != null) _lblAction.Text = _lastGeneratedExercise?.ActionText ?? _gamePlay.CurrentOperation.ToDString();
                RefreshPreviousPreview();
                if (_isKeyboard && !_config.FromNumToNum)
                {
                    if (!ApplyConfiguredKeyboardSeedState())
                        _pianoKeyboard.PianoInit();

                    if (_config.KeyboardConfig?.IsArrow == true && _gamePlay is BitArrayGamePlay arrowGamePlay)
                        _pianoKeyboard.SetTraceOverlayColors(
                            arrowGamePlay.GetStagedArrowTraceOverlayColors(),
                            arrowGamePlay.GetStagedArrowSecondaryTraceOverlayColors());
                    else
                        _pianoKeyboard.ClearTraceOverlay();
                }
                if (tasks.Count > 0) _ = Task.WhenAll(tasks);

            }
            RefreshKeyboardControlBar();
            if (HasVisibleNumericInputs && allowInputFocus)
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

                RefreshNumericEntryAppearance();
            }
        }
        private async Task ForceFocusAsync(Entry entry, int delayMilliseconds = 50)
        {
            if (entry == null)
                return;

            int effectiveDelay = Math.Max(0, delayMilliseconds);
            if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
                effectiveDelay = Math.Max(effectiveDelay, 120);

            await Task.Delay(effectiveDelay);

            if (UsesManagedNumericInput)
            {
                SelectNumericEntry(entry);
                return;
            }

            if (entry != null && entry.IsVisible && entry.IsEnabled)
            {
                _lastFocused = entry;
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    entry.Unfocus();
                    entry.Focus();
                });

                if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
                {
                    await Task.Delay(80);
                    if (!entry.IsFocused && entry.IsVisible && entry.IsEnabled)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() => entry.Focus());
                    }
                }
            }
        }

        private View InitNumericKeypadUI()
        {
            if (UsesChoiceAnswerKeyboard)
            {
                double choiceKeyboardWidth = GetChoiceKeyboardWidth();
                _choiceAnswerKeyboard = new ChoiceAnswerKeyboardView
                {
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Center,
                    WidthRequest = choiceKeyboardWidth,
                    MaximumWidthRequest = choiceKeyboardWidth,
                    Margin = new Thickness(0, 2, 0, 0),
                    IsVisible = true
                };

                _choiceAnswerKeyboard.ChoicePressed += OnChoiceAnswerPressed;
                return _choiceAnswerKeyboard;
            }

            _numericKeypad = new NumericKeypadView
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center,
                WidthRequest = TASK_WIDTH,
                MaximumWidthRequest = TASK_WIDTH,
                Margin = new Thickness(0, 2, 0, 0),
                IsVisible = UsesCustomNumericKeypad
            };

            _numericKeypad.DigitPressed += OnNumericDigitPressed;
            _numericKeypad.BackspacePressed += OnNumericBackspacePressed;
            _numericKeypad.ClearPressed += OnNumericClearPressed;
            _numericKeypad.SubmitPressed += OnNumericSubmitPressed;

            return _numericKeypad;
        }

        private double GetChoiceKeyboardWidth()
        {
            DisplayInfo info = DeviceDisplay.Current.MainDisplayInfo;
            double width = info.Density > 0 ? info.Width / info.Density : info.Width;
            double height = info.Density > 0 ? info.Height / info.Density : info.Height;
            double shortSide = Math.Min(width, height);
            double availableWidth = shortSide > 0 ? shortSide - 28 : TASK_WIDTH * 1.65;

            return Math.Max(TASK_WIDTH * 1.4, Math.Min(availableWidth, TASK_WIDTH * 1.8));
        }

        private bool IsLargeEnoughForExpandedNumericLayout()
        {
            DisplayInfo info = DeviceDisplay.Current.MainDisplayInfo;
            double width = info.Density > 0 ? info.Width / info.Density : info.Width;
            double height = info.Density > 0 ? info.Height / info.Density : info.Height;
            double longSide = Math.Max(width, height);
            double shortSide = Math.Min(width, height);
            return longSide >= 1000 && shortSide >= 700;
        }

        private bool ShouldPlaceNumericKeypadBesideEntriesForHelp()
        {
            return UsesCustomNumericKeypad &&
                   _isKeyboard &&
                   _config.KeyboardConfig.KeyboardOnlyForHelp;
        }

        private bool ShouldPlaceNumericKeypadBelowPreviousPreview()
        {
            return UsesCustomNumericKeypad &&
                   _config.ShowPrev &&
                   !ShouldPlaceNumericKeypadBesideEntriesForHelp() &&
                   IsLargeEnoughForExpandedNumericLayout();
        }

        private double GetQuestionLayoutWidth()
        {
            if (!UsesManagedNumericInput)
                return TASK_WIDTH;

            DisplayInfo info = DeviceDisplay.Current.MainDisplayInfo;
            double width = info.Density > 0 ? info.Width / info.Density : info.Width;
            double height = info.Density > 0 ? info.Height / info.Density : info.Height;
            double shortSide = Math.Min(width, height);
            double availableWidth = shortSide > 0 ? shortSide - 44 : TASK_WIDTH;

            if (ShouldPlaceNumericKeypadBesideEntriesForHelp())
                return Math.Max(150, Math.Min(availableWidth * 0.42, 210));

            if (UsesChoiceAnswerKeyboard)
                return Math.Max(150, Math.Min(availableWidth * 0.56, 190));

            return Math.Max(220, Math.Min(availableWidth * 0.78, 320));
        }

        private double GetQuestionActionWidth()
        {
            return UsesManagedNumericInput ? 28 : 20;
        }

        private double GetQuestionHalfWidth(bool reserveActionLabel)
        {
            double halfWidth = GetQuestionLayoutWidth() / 2;
            if (reserveActionLabel)
                halfWidth -= GetQuestionActionWidth() / 2;

            return Math.Max(72, halfWidth);
        }

        private double GetQuestionQuarterWidth()
        {
            return Math.Max(54, GetQuestionLayoutWidth() / 4);
        }

        private void ConfigureNumericEntry(Entry entry)
        {
            DesignResources.ApplyStyle(entry, "GameNumericEntryStyle");
            entry.IsReadOnly = UsesManagedNumericInput;
            entry.IsSpellCheckEnabled = false;
            entry.IsTextPredictionEnabled = false;

            if (!_numericEntries.Contains(entry))
                _numericEntries.Add(entry);

            if (UsesManagedNumericInput)
            {
                TapGestureRecognizer tapGesture = new();
                tapGesture.Tapped += (_, _) => SelectNumericEntry(entry);
                entry.GestureRecognizers.Add(tapGesture);

                entry.Focused += (_, _) =>
                {
                    SelectNumericEntry(entry);
                    entry.Unfocus();
                };
            }

            RefreshNumericEntryAppearance();
        }

        private void ResetNumericEntryTransform(Entry entry)
        {
            entry.AbortAnimation("NumericEntryY");
            entry.AbortAnimation("NumericEntryScale");
            entry.TranslationY = 0;
            entry.Scale = 1;
        }

        private bool IsEntryEditable(Entry? entry)
        {
            return entry != null && entry.IsVisible && entry.IsEnabled;
        }

        private void SelectNumericEntry(Entry? entry)
        {
            if (!UsesManagedNumericInput)
                return;

            if (!IsEntryEditable(entry))
            {
                RefreshNumericEntryAppearance();
                return;
            }

            _activeNumericEntry = entry;
            _lastFocused = entry;
            RefreshNumericEntryAppearance();
        }

        private Entry? EnsureNumericEntrySelection()
        {
            if (UsesArrowLabelPromptStage())
            {
                Entry? arrowLabelMissingEntry = GetArrowLabelMissingEntry();
                if (IsEntryEditable(arrowLabelMissingEntry))
                {
                    if (_activeNumericEntry != arrowLabelMissingEntry)
                        SelectNumericEntry(arrowLabelMissingEntry);

                    return arrowLabelMissingEntry;
                }
            }

            if (IsEntryEditable(_activeNumericEntry))
                return _activeNumericEntry;

            Entry? preferredEntry = GetPreferredNumericEntry();
            if (preferredEntry != null)
                SelectNumericEntry(preferredEntry);

            return preferredEntry;
        }

        private Entry? GetPreferredNumericEntry()
        {
            Entry? arrowLabelMissingEntry = GetArrowLabelMissingEntry();
            if (IsEntryEditable(arrowLabelMissingEntry))
                return arrowLabelMissingEntry;

            if (IsEntryEditable(_lastFocused))
                return _lastFocused;

            if (IsEntryEditable(_txtSum) && _gamePlay.Sum == PPWGamePlay.NAN)
                return _txtSum;

            if (IsEntryEditable(_txtAddend1) && _gamePlay.addend1 == PPWGamePlay.NAN)
                return _txtAddend1;

            if (IsEntryEditable(_txtAddend2) && _gamePlay.addend2 == PPWGamePlay.NAN)
                return _txtAddend2;

            return _numericEntries.FirstOrDefault(IsEntryEditable);
        }

        private Entry? GetNextEditableEntry(Entry currentEntry)
        {
            List<Entry> editableEntries = _numericEntries.Where(IsEntryEditable).ToList();
            if (editableEntries.Count == 0)
                return null;

            int currentIndex = editableEntries.IndexOf(currentEntry);
            if (currentIndex < 0)
                return editableEntries[0];

            return currentIndex + 1 < editableEntries.Count ? editableEntries[currentIndex + 1] : null;
        }

        private void RefreshNumericEntryAppearance()
        {
            foreach (Entry numericEntry in _numericEntries)
            {
                if (!numericEntry.IsEnabled)
                {
                    numericEntry.BackgroundColor = DesignResources.GetColor("GameNumericEntryBackgroundColor", Colors.White);
                    numericEntry.TextColor = DesignResources.GetColor("GameNumericEntryDisabledTextColor", Colors.Gray);
                    ResetNumericEntryTransform(numericEntry);
                }
                else if (numericEntry == _activeNumericEntry && UsesManagedNumericInput)
                {
                    numericEntry.BackgroundColor = DesignResources.GetColor("GameNumericEntryActiveBackgroundColor", Color.FromArgb("#FFF9D6"));
                    numericEntry.TextColor = DesignResources.GetColor("GameNumericEntryTextColor", Colors.Black);
                    ResetNumericEntryTransform(numericEntry);
                }
                else
                {
                    numericEntry.BackgroundColor = DesignResources.GetColor("GameNumericEntryBackgroundColor", Colors.White);
                    numericEntry.TextColor = DesignResources.GetColor("GameNumericEntryTextColor", Colors.Black);
                    ResetNumericEntryTransform(numericEntry);
                }
            }
        }

        private void OnNumericDigitPressed(string digit)
        {
            Entry? targetEntry = EnsureNumericEntrySelection();
            if (targetEntry == null)
                return;

            string currentText = targetEntry.Text ?? string.Empty;
            if (digit == "-")
            {
                targetEntry.Text = currentText.StartsWith("-")
                    ? currentText[1..]
                    : "-" + currentText.TrimStart('+');
                _lastFocused = targetEntry;
                RefreshNumericEntryAppearance();
                return;
            }

            if (currentText == "0")
                currentText = string.Empty;

            targetEntry.Text = currentText + digit;
            _lastFocused = targetEntry;
            RefreshNumericEntryAppearance();
        }

        private void OnChoiceAnswerPressed(int value)
        {
            Entry? targetEntry = EnsureNumericEntrySelection();
            if (targetEntry == null)
                return;

            targetEntry.Text = value.ToString();
            _lastFocused = targetEntry;
            RefreshNumericEntryAppearance();

            if (UsesArrowLabelPromptStage())
            {
                SubmitArrowLabelPromptAnswer();
                return;
            }

            OnNumericSubmitPressed();
        }

        private void OnNumericBackspacePressed()
        {
            Entry? targetEntry = EnsureNumericEntrySelection();
            if (targetEntry == null || string.IsNullOrEmpty(targetEntry.Text))
                return;

            targetEntry.Text = targetEntry.Text[..^1];
            _lastFocused = targetEntry;
            RefreshNumericEntryAppearance();
        }

        private void OnNumericClearPressed()
        {
            Entry? targetEntry = EnsureNumericEntrySelection();
            if (targetEntry == null)
                return;

            targetEntry.Text = string.Empty;
            _lastFocused = targetEntry;
            RefreshNumericEntryAppearance();
        }

        private void OnNumericSubmitPressed()
        {
            Entry? targetEntry = EnsureNumericEntrySelection();
            if (targetEntry == null)
                return;

            if (UsesArrowLabelPromptStage())
            {
                SubmitArrowLabelPromptAnswer();
                return;
            }

            if (targetEntry == _txtAddend1 && _config.RequiresBothAddendsInput && IsEntryEditable(_txtAddend2))
            {
                SelectNumericEntry(_txtAddend2);
                return;
            }

            if (targetEntry == _txtSum || targetEntry == _txtAddend2 || !_config.RequiresBothAddendsInput)
            {
                CheckGamePlay();
                return;
            }

            Entry? nextEntry = GetNextEditableEntry(targetEntry);
            if (nextEntry != null)
            {
                SelectNumericEntry(nextEntry);
                return;
            }

            CheckGamePlay();
        }

        private async void SubmitArrowLabelPromptAnswer()
        {
            if (_gamePlay is not BitArrayGamePlay arrowGamePlay || !arrowGamePlay.HasArrowLabelPrompt)
                return;

            int addend1 = arrowGamePlay.ArrowLabelAddend1Value;
            int addend2 = arrowGamePlay.ArrowLabelAddend2Value ?? 0;
            int sum = arrowGamePlay.ArrowLabelSumValue;
            Entry? missingEntry = GetArrowLabelMissingEntry();

            if (missingEntry == null || !int.TryParse(missingEntry.Text, out int submittedValue))
            {
                ApplyFeedbackUiState(false);
                if (missingEntry != null)
                    missingEntry.BackgroundColor = Colors.IndianRed;

                _lblStatement.Text = Statement.WrongInput;
                return;
            }

            switch (arrowGamePlay.CurrentArrowLabelExerciseMode)
            {
                case ArrowLabelExerciseMode.StartAndLength:
                case ArrowLabelExerciseMode.OrdinalStartAndLength:
                    sum = submittedValue;
                    break;
                case ArrowLabelExerciseMode.StartAndEndWithMissingLength:
                    addend2 = submittedValue;
                    break;
                case ArrowLabelExerciseMode.EndAndLengthWithMissingStart:
                    addend1 = submittedValue;
                    break;
            }

            ExerciseCheckResult checkResult = await _gamePlay.EvaluateAsync(addend1, addend2, sum);
            ApplyFeedbackUiState(checkResult.IsCorrect);
            missingEntry.BackgroundColor = checkResult.IsCorrect ? Colors.LightGreen : Colors.IndianRed;

            if (checkResult.Completion != null)
            {
                await Task.Delay(450);
                await HandleGameCompletionAsync(checkResult.Completion);
                return;
            }

            if (checkResult.IsCorrect && !_gamePlay.GameOver)
            {
                await Task.Delay(450);
                await GenerateNextExerciseAsync();
                return;
            }

            if (!checkResult.IsCorrect)
                return;

            ResetStatusLineToNeutral();
            RestoreReadyForInputState();
        }

        private bool UsesCyclicalTutorial()
        {
            return _config.QuestionOrder == QuestionOrder.CyclicalLeft ||
                   _config.QuestionOrder == QuestionOrder.CyclicalRight ||
                   _config.QuestionOrder == QuestionOrder.CyclicalMixed;
        }

        private bool UsesFullHandTutorial()
        {
            return _config.KeyboardConfig?.UseFullHandTutorial == true &&
                   _pianoKeyboard?.KeyButtons?.Count >= 5;
        }

        private void EnsureTutorialHandOverlay(bool isLeftHand)
        {
            if (_rootGrid == null)
                return;

            if (handGraphicsView == null || _tutorialHandDrawable == null || _tutorialHandIsLeft != isLeftHand)
            {
                _tutorialHandIsLeft = isLeftHand;
                _tutorialHandDrawable = new HandDrawable(isLeftHand)
                {
                    Bits = new[] { 1, 1, 1, 1, 1 },
                    Position = new PointF(0, 0),
                    Opacity = 0f
                };

                handGraphicsView = new GraphicsView
                {
                    Drawable = _tutorialHandDrawable,
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill,
                    InputTransparent = true,
                    ZIndex = 100
                };
            }
            else
            {
                handGraphicsView.Drawable = _tutorialHandDrawable;
            }

            if (!_rootGrid.Children.Contains(handGraphicsView))
            {
                _rootGrid.Add(handGraphicsView);
                Grid.SetRowSpan(handGraphicsView, _rootGrid.RowDefinitions.Count);
                Grid.SetColumnSpan(handGraphicsView, _rootGrid.ColumnDefinitions.Count);
            }
        }

        private PointF GetPointRelativeToRoot(VisualElement element, double xOffset = 0, double yOffset = 0)
        {
            double x = xOffset;
            double y = yOffset;

            VisualElement current = element;
            while (current != null && current != _rootGrid)
            {
                x += current.X + current.TranslationX;
                y += current.Y + current.TranslationY;
                current = current.Parent as VisualElement;
            }

            return new PointF((float)x, (float)y);
        }

        private async Task RunFullHandTutorialAsync(bool[] questionBits)
        {
            if (_pianoKeyboard == null || _rootGrid == null || questionBits == null || questionBits.Length == 0)
                return;

            var activeIndices = questionBits
                .Select((value, index) => new { value, index })
                .Where(x => x.value)
                .Select(x => x.index)
                .ToList();

            if (activeIndices.Count == 0)
            {
                if (handGraphicsView != null && _tutorialHandDrawable != null)
                {
                    _tutorialHandDrawable.Opacity = 1f;
                    PointF downPoint = new((float)(_tutorialHandDrawable.Position.X), (float)(_rootGrid.Height + 40));
                    await _tutorialHandDrawable.AnimateMoveAsync(handGraphicsView, downPoint, TimeSpan.FromMilliseconds(ScaleTutorialMs(260)));
                    await _tutorialHandDrawable.HideAsync(handGraphicsView, TimeSpan.FromMilliseconds(ScaleTutorialMs(180)));
                }
                return;
            }

            int keyCount = _pianoKeyboard.KeyButtons.Count;
            bool isLeftHand = activeIndices.Average() < ((keyCount - 1) / 2.0);
            EnsureTutorialHandOverlay(isLeftHand);
            if (handGraphicsView == null || _tutorialHandDrawable == null)
                return;

            await Task.Delay(ScaleTutorialMs(40));

            int windowSize = Math.Min(5, keyCount);
            int centerIndex = (activeIndices.First() + activeIndices.Last()) / 2;
            int windowStart = Math.Clamp(centerIndex - (windowSize / 2), 0, Math.Max(0, keyCount - windowSize));
            int windowEnd = Math.Min(keyCount - 1, windowStart + windowSize - 1);

            int[] targetBits = new int[5];
            for (int i = 0; i < windowSize; i++)
            {
                int sourceIndex = windowStart + i;
                bool isActive = sourceIndex < questionBits.Length && questionBits[sourceIndex];
                int handIndex = isLeftHand ? (windowSize - 1 - i) : i;
                targetBits[handIndex] = isActive ? 1 : 0;
            }

            var startButton = _pianoKeyboard.KeyButtons[windowStart];
            var endButton = _pianoKeyboard.KeyButtons[windowEnd];
            PointF startPoint = GetPointRelativeToRoot(startButton, startButton.Width / 2, 0);
            PointF endPoint = GetPointRelativeToRoot(endButton, endButton.Width / 2, 0);
            PointF keyboardTop = GetPointRelativeToRoot(startButton, 0, 0);

            float desiredWidth = Math.Max(150f, endPoint.X - startPoint.X + (float)startButton.Width * 1.2f);
            float desiredHeight = Math.Max(120f, desiredWidth * 0.7f);
            float targetX = ((startPoint.X + endPoint.X) / 2f) - (desiredWidth / 2f);
            float targetY = keyboardTop.Y - desiredHeight * 0.12f;

            _tutorialHandDrawable.DesiredWidth = desiredWidth;
            _tutorialHandDrawable.DesiredHeight = desiredHeight;
            _tutorialHandDrawable.Bits = new[] { 1, 1, 1, 1, 1 };
            _tutorialHandDrawable.Opacity = 1f;

            PointF riseStart = new(targetX, (float)(_rootGrid.Height + desiredHeight));
            PointF target = new(targetX, targetY);
            _tutorialHandDrawable.Position = riseStart;
            handGraphicsView.Invalidate();

            await _tutorialHandDrawable.AnimateMoveAsync(handGraphicsView, target, TimeSpan.FromMilliseconds(ScaleTutorialMs(520)));
            await Task.Delay(ScaleTutorialMs(120));
            _tutorialHandDrawable.Bits = targetBits;
            handGraphicsView.Invalidate();
            await Task.Delay(ScaleTutorialMs(900));
            await _tutorialHandDrawable.AnimateMoveAsync(handGraphicsView, riseStart, TimeSpan.FromMilliseconds(ScaleTutorialMs(320)));
            await _tutorialHandDrawable.HideAsync(handGraphicsView, TimeSpan.FromMilliseconds(ScaleTutorialMs(160)));
        }

        async Task Tutorial(KeyboardOverlayHost koh)
        {
            var gp = (BitArrayGamePlay)_gamePlay;
            bool[] tutorialAnswer = gp.GetTutorialAnswerBits();

            await Task.Delay(
                gp.UsesArrowDirectionTutorial()
                    ? ScaleArrowTutorialMs(150)
                    : ScaleTutorialMs(1000));

            koh.SyncOverlay();
            await koh.EnsureOverlaySyncedAsync();

            if (UsesFullHandTutorial())
            {
                await koh.FadeStaticOverlayAlphaAsync(0.22f, ScaleTutorialMs(180u), "TutStaticDimIn");
                try
                {
                    await RunFullHandTutorialAsync(gp.GetTutorialQuestionBits());
                }
                finally
                {
                    await koh.FadeStaticOverlayAlphaAsync(KeyboardOverlayHost.DefaultStaticOverlayAlpha, ScaleTutorialMs(180u), "TutStaticDimOut");
                }
                return;
            }

            if (gp.UsesArrowDirectionTutorial())
            {
                IReadOnlyList<int> tutorialIndices = gp.GetArrowTutorialStepIndices();
                if (tutorialIndices.Count > 0 && _pianoKeyboard != null)
                {
                    int keyCount = _pianoKeyboard.KeyCount;
                    bool isOrdinalArrow = gp.IsOrdinalArrowTutorial();

                    await koh.FadeStaticOverlayAlphaAsync(0.18f, ScaleTutorialMs(220u), "TutStaticDimIn");
                    try
                    {
                        for (int step = 0; step < tutorialIndices.Count; step++)
                        {
                            bool[] stepBits = new bool[keyCount];
                            if (isOrdinalArrow)
                            {
                                int idx = tutorialIndices[step];
                                if (idx >= 0 && idx < stepBits.Length)
                                {
                                    stepBits[idx] = true;
                                    _pianoKeyboard.SetTutorialStepLabels(new Dictionary<int, int> { [idx] = step + 1 });
                                }

                                koh.ShowHighlightedBits(stepBits, Colors.Yellow, 0.58f);
                                await Task.Delay(ScaleArrowTutorialMs(420));
                                _pianoKeyboard.ClearTutorialStepLabels();
                                await koh.FadeOutHighlightedBitsAsync(ScaleArrowTutorialMs(180u), $"ArrowTutOrdinal_{step}");
                                await Task.Delay(ScaleArrowTutorialMs(90));
                            }
                            else
                            {
                                Dictionary<int, int> stepNumbers = new();
                                for (int i = 0; i <= step && i < tutorialIndices.Count; i++)
                                {
                                    int idx = tutorialIndices[i];
                                    if (idx >= 0 && idx < stepBits.Length)
                                    {
                                        stepBits[idx] = true;
                                        stepNumbers[idx] = i + 1;
                                    }
                                }

                                _pianoKeyboard.SetTutorialStepLabels(stepNumbers);
                                koh.ShowHighlightedBits(stepBits, Colors.Yellow, 0.58f);
                                await Task.Delay(ScaleArrowTutorialMs(440));
                            }
                        }

                        if (!isOrdinalArrow)
                        {
                            _pianoKeyboard.ClearTutorialStepLabels();
                            await koh.FadeOutHighlightedBitsAsync(ScaleArrowTutorialMs(220u), "ArrowTutCardinalEnd");
                        }
                    }
                    finally
                    {
                        _pianoKeyboard.ClearTutorialStepLabels();
                        await koh.FadeStaticOverlayAlphaAsync(KeyboardOverlayHost.DefaultStaticOverlayAlpha, ScaleTutorialMs(220u), "TutStaticDimOut");
                    }

                    return;
                }
            }

            if (UsesCyclicalTutorial())
            {
                bool[] tutorialQuestion = gp.GetTutorialQuestionBits();
                if (tutorialQuestion.Length > 0)
                {
                    int shiftBy1 = _config.QuestionOrder switch
                    {
                        QuestionOrder.CyclicalLeft => -1,
                        QuestionOrder.CyclicalRight => 1,
                        _ => gp.CurrentOperation == Operation.MoveBy
                            ? (gp.moveBydir == Direction.Left ? -1 : 1)
                            : (gp.dir == Direction.Left ? -1 : 1)
                    };
                    int tutorialStepsPerRound = gp.CurrentOperation == Operation.MoveBy
                        ? Math.Max(1, gp.moveByLength)
                        : Math.Max(1, gp.length);
                    int tutorialRounds = gp.CurrentOperation == Operation.MoveBy ? 1 : 2;
                    uint tutorialFadeMs = gp.CurrentOperation == Operation.MoveBy ? ScaleTutorialMs(1800u) : ScaleTutorialMs(120u);
                    uint tutorialHoldMs = gp.CurrentOperation == Operation.MoveBy ? ScaleTutorialMs(450u) : ScaleTutorialMs(150u);

                    await koh.AnimateCyclicalStatesAsync(
                        tutorialQuestion,
                        shiftBy1,
                        rounds: tutorialRounds,
                        stepsPerRound: tutorialStepsPerRound,
                        fadeMs: tutorialFadeMs,
                        holdMs: tutorialHoldMs);
                    return;
                }
            }

            if (gp.CurrentOperation is Operation.Not or Operation.Mirror or Operation.Copy)
            {
                await koh.FadeStaticOverlayAlphaAsync(0.18f, ScaleTutorialMs(220u), "TutStaticDimIn");
                try
                {
                    await koh.PulseBitsAsync(tutorialAnswer, fadeInMs: ScaleTutorialMs(280u), holdMs: ScaleTutorialMs(2200u), fadeOutMs: ScaleTutorialMs(380u));
                }
                finally
                {
                    await koh.FadeStaticOverlayAlphaAsync(KeyboardOverlayHost.DefaultStaticOverlayAlpha, ScaleTutorialMs(220u), "TutStaticDimOut");
                }
                return;
            }

            if (gp.CurrentOperation == Operation.GroupByColor)
            {
                await koh.FadeStaticOverlayAlphaAsync(0.18f, ScaleTutorialMs(220u), "TutStaticDimIn");
                try
                {
                    var tutorialGroups = gp.GetGroupByColorTutorialSteps()
                        .Select(step => (
                            step.Bits,
                            step.TargetBits,
                            step.Color))
                        .ToList();

                    await koh.AnimatePackedGroupsAsync(
                        tutorialGroups,
                        moveMs: ScaleTutorialMs(2900u),
                        holdMs: ScaleTutorialMs(1800u),
                        fadeOutMs: ScaleTutorialMs(520u),
                        animName: "TutColorParallel");
                }
                finally
                {
                    await koh.FadeStaticOverlayAlphaAsync(KeyboardOverlayHost.DefaultStaticOverlayAlpha, ScaleTutorialMs(220u), "TutStaticDimOut");
                }
                return;
            }

            int move = gp.moveByLength * (gp.moveBydir == Direction.Right ? 1 : -1);
            await koh.Animate(gp.GetTutorialQuestionBits(), gp.CurrentOperation, move, ScaleTutorialMs(4000u));
        }

        private static bool[] BuildPackedTutorialBits(bool[] bits, Direction direction)
        {
            bool[] packed = new bool[bits.Length];
            int count = bits.Count(bit => bit);

            if (direction == Direction.Left)
            {
                for (int i = 0; i < count && i < packed.Length; i++)
                    packed[i] = true;
            }
            else
            {
                for (int i = 0; i < count && i < packed.Length; i++)
                    packed[packed.Length - 1 - i] = true;
            }

            return packed;
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

        private async Task DelayKeyboardInputAsync(int seconds)
        {
            SetKeyboardInteractionEnabled(false);
            try
            {
                await Task.Delay(seconds * 1000);
            }
            finally
            {
                SetKeyboardInteractionEnabled(true);
                RestoreReadyForInputState();
            }
        }

        private void UpdateLogicalActionVisual(BitArrayGamePlay gp)
        {
            if (_lblAction == null || _logicalColorActionLayout == null || gp == null)
                return;

            if (gp.CurrentOperation != Operation.GroupByColor)
            {
                _lblAction.IsVisible = true;
                _logicalColorActionLayout.IsVisible = false;
                _lblAction.Text = gp.CurrentOperation.ToDString();
                return;
            }

            _lblAction.IsVisible = false;
            _logicalColorActionLayout.IsVisible = true;
            _logicalColorActionLayout.Children.Clear();
            foreach (GroupByColorStep step in gp.GetGroupByColorMissionArrows())
            {
                Color cardColor = step.Color.WithAlpha(0.95f);
                Label arrowLabel = new Label
                {
                    Text = step.Direction == Direction.Left ? "\u2190" : "\u2192",
                    FontSize = 54,
                    FontAttributes = FontAttributes.Bold,
                    FontFamily = DeviceInfo.Platform == DevicePlatform.iOS ? "HelveticaNeue-Bold" : null,
                    TextColor = GetReadableInstructionTextColor(step.Color),
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center
                };

                _logicalColorActionLayout.Children.Add(new Border
                {
                    Padding = new Thickness(18, 8),
                    Margin = new Thickness(4, 0),
                    Stroke = Colors.White,
                    StrokeThickness = 3,
                    BackgroundColor = cardColor,
                    MinimumWidthRequest = 74,
                    StrokeShape = new RoundRectangle { CornerRadius = 18 },
                    Shadow = new Shadow
                    {
                        Brush = Colors.Black.WithAlpha(0.35f),
                        Offset = new Point(0, 4),
                        Radius = 10
                    },
                    Content = arrowLabel
                });
            }
        }

        private static Color GetReadableInstructionTextColor(Color background)
        {
            double luminance = (0.299 * background.Red) + (0.587 * background.Green) + (0.114 * background.Blue);
            return luminance > 0.62 ? Colors.Black : Colors.White;
        }

        private Color GetArrowBackgroundColor()
        {
            int groupIndex = Math.Max(0, (_gamePlay._questionNumber - 1) / 3);
            return ArrowBackgroundCycle[groupIndex % ArrowBackgroundCycle.Length];
        }

        private bool ShouldCycleArrowBackground()
        {
            return _config.KeyboardConfig?.IsArrow == true &&
                   _config.KeyboardConfig.IsArrowLengthDynamic == true;
        }


        private static string GenerateHistoryString(List<PPWObject> ppwHistoryArray)
        {
            String strHistory = "HISTORY:\n";
            foreach (PPWObject ppw in ppwHistoryArray)
                strHistory += ppw.Addend1 + "\t" + ppw.Addend2 + "\n";

            return strHistory;
        }

        #endregion

       
        private Button _btnHelp = null;
        public SimpleViewCellsPage(GameConfig config)
        {
            Title = config.GameName;
            _config = config;
            _backgroundSyncService = ServiceHelper.GetService<BackgroundSyncService>();
            _syncToolbarStatusController = new SyncToolbarStatusController(this, _backgroundSyncService);
            _keyboardQuestionRepository = ServiceHelper.GetService<KeyboardQuestionRepository>();
            _questionAnswerRepository = ServiceHelper.GetService<QuestionAnswerRepository>();
            _questionAnswerPartRepository = ServiceHelper.GetService<QuestionAnswerPartRepository>();
            _timerChangeEventRepository = ServiceHelper.GetService<TimerChangeEventRepository>();
            _gameRepository = ServiceHelper.GetService<GameRepository>();
            if (_config.NumberOfTasksToWin > -1)
            {
                TimerInit();
                timer.Start();
            }
            InitializeStatusLightIcons();
            InitializeGamePlay();
            InitializeUI();
        }

        private void InitializeStatusLightIcons()
        {
            _statusLight1.Content = _statusLight1Icon;
            _statusLight2.Content = _statusLight2Icon;
            _statusLight1.IsVisible = false;
            _statusLight2.IsVisible = false;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _syncToolbarStatusController.Attach();

            if (!_hasLoadedInitialExercise)
            {
                _hasLoadedInitialExercise = true;
                await GenerateNextExerciseAsync();
            }

           /* await ApplyUiStateAsync(PlayUiState.Question);
            await Task.Delay(1000);
            await ApplyUiStateAsync(PlayUiState.ReadyForInput);
            await Task.Delay(1000);
            await ApplyUiStateAsync(PlayUiState.FeedbackCorrect);
            await Task.Delay(1000);
            await ApplyUiStateAsync(PlayUiState.FeedbackWrong);*/
        }

        protected override void OnDisappearing()
        {
            _syncToolbarStatusController.Detach();
            base.OnDisappearing();
        }
        private void InitializeGamePlay()
        {
            _gamePlay = CreateGamePlay();
            _cmdCheck = new Command(CheckGamePlay);
            _cmdNext = new Command(async () => await GenerateNextExerciseAsync());
        }

        private PPWGamePlay CreateGamePlay()
        {
            return _config.UIQuestionType switch
            {
                UIQuestionType.LogicalKeyboards => new BitArrayGamePlay(_config),
                UIQuestionType.CanvasesHands => new BitArrayGamePlay(_config),
                UIQuestionType.DecompositionGame => new DecompositionGamePlay(_config),
                _ when _config.KeyboardConfig != null &&
                       (_config.KeyboardConfig.IsArrow || _config.KeyboardConfig.ArrowLabelExerciseMode != ArrowLabelExerciseMode.None) => new BitArrayGamePlay(_config),
                _ => new PPWGamePlay(_config)
            };
        }

        private async void CheckGamePlay()
        {
            if (_btnNext != null) _btnNext.IsEnabled = false;
            //if (_btnPrev != null) _btnPrev.IsEnabled = false;

            if (_isKeyboard && !_config.KeyboardConfig.KeyboardOnlyForHelp)
            {
                SetInlineKeyboardCheckVisible(false);
                ExerciseCheckResult checkResult = await _gamePlay.EvaluateAsync(_pianoKeyboard);
                await HandleCheckResultAsync(checkResult, isKeyboardSubmission: true);

            }
            else
            {
                try
                {
                    PPWObject submittedAnswer = new(
                        Convert.ToInt32(_txtAddend1.Text),
                        Convert.ToInt32(_txtAddend2.Text),
                        Convert.ToInt32(_txtSum.Text));

                    ExerciseCheckResult checkResult = await _gamePlay.EvaluateAsync(submittedAnswer.Addend1, submittedAnswer.Addend2, submittedAnswer.Sum);
                    await HandleCheckResultAsync(checkResult, isKeyboardSubmission: false, onCorrect: () => CapturePreviousAnswer(submittedAnswer));
                }
                catch
                {
                    _lblStatement.Text = Statement.WrongInput;
                }
            }
        }

        private async Task HandleImpossibleWeightedAnswerAsync()
        {
            if (_btnNext != null)
                _btnNext.IsEnabled = false;

            if (!_gamePlay.SupportsImpossibleWeightedAnswer)
                return;

            SetInlineKeyboardCheckVisible(false);
            ExerciseCheckResult checkResult = await _gamePlay.EvaluateImpossibleWeightedAnswerAsync();
            await HandleCheckResultAsync(checkResult, isKeyboardSubmission: true);
        }

        private async Task HandleCheckResultAsync(ExerciseCheckResult checkResult, bool isKeyboardSubmission, Action? onCorrect = null)
        {
            bool willAdvanceToNextExercise = checkResult.Completion == null && checkResult.IsCorrect && !_gamePlay.GameOver;
            await UpdateView(applyUiState: false, allowInputFocus: !willAdvanceToNextExercise);
            bool shouldAutoShowTutorial = UpdateAutoTutorialState(checkResult);

            if (checkResult.IsCorrect)
            {
                onCorrect?.Invoke();
            }

            if (!checkResult.IsWrongInput)
            {
                ApplyFeedbackUiState(checkResult.IsCorrect);
            }
            else
            {
                RestoreReadyForInputState();
            }

            await ApplyPostCheckDelayAsync(checkResult, isKeyboardSubmission, willAdvanceToNextExercise);

            if (checkResult.Completion != null)
            {
                await HandleGameCompletionAsync(checkResult.Completion);
            }
            else if (willAdvanceToNextExercise)
            {
                await GenerateNextExerciseAsync();
            }
            else if (isKeyboardSubmission)
            {
                await ApplyAutoAnswerTimeTuningAsync("AutoTuneRetry");
                _pianoKeyboard.PianoInit();
                SetPageInteractionEnabled(true);
                ResetStatusLineToNeutral();
                RestoreReadyForInputState();
            }
            else
            {
                ResetStatusLineToNeutral();
                RestoreReadyForInputState();
            }

            if (shouldAutoShowTutorial && checkResult.Completion == null)
            {
                await RunAutoTutorialIfAvailableAsync();
            }
        }

        private bool HasAvailableTutorial()
        {
            return HasKeyboardGuidanceSupport();
        }

        private bool UpdateAutoTutorialState(ExerciseCheckResult checkResult)
        {
            if (!HasAvailableTutorial() || checkResult.IsWrongInput)
                return false;

            if (checkResult.IsCorrect)
            {
                _consecutiveWrongAnswers = 0;
                return false;
            }

            _consecutiveWrongAnswers++;
            if (_consecutiveWrongAnswers < 3)
                return false;

            _consecutiveWrongAnswers = 0;
            return true;
        }

        private async Task RunAutoTutorialIfAvailableAsync()
        {
            if (!HasAvailableTutorial() || _taskMainHost == null)
                return;

            _consecutiveWrongAnswers = 0;

            if (HasDedicatedKeyboardTutorial())
                await RunRecordedKeyboardTutorialAsync(_taskMainHost);
            else
                await RunCorrectAnswerHintAsync(_taskMainHost);
        }

        private async Task ApplyPostCheckDelayAsync(ExerciseCheckResult checkResult, bool isKeyboardSubmission, bool keepDisabledForNextExercise)
        {
            if (ShouldSkipPostCheckDelay(checkResult, isKeyboardSubmission))
                return;

            if (!checkResult.ShouldDelayFeedback || _config.SecondsTillNextExercise <= 0)
                return;

            if (isKeyboardSubmission)
            {
                SetPageInteractionEnabled(false);
                try
                {
                    await Task.Delay(_config.SecondsTillNextExercise * 1000);
                }
                finally
                {
                    if (!keepDisabledForNextExercise)
                        SetPageInteractionEnabled(true);
                }
                return;
            }

            SetPageInteractionEnabled(false);

            try
            {
                await Task.Delay(_config.SecondsTillNextExercise * 1000);
            }
            finally
            {
                if (!keepDisabledForNextExercise)
                    SetPageInteractionEnabled(true);
            }
        }

        private bool ShouldSkipPostCheckDelay(ExerciseCheckResult checkResult, bool isKeyboardSubmission)
        {
            if (checkResult.Completion != null || _gamePlay.GameOver)
                return true;

            if (isKeyboardSubmission || !checkResult.IsCorrect)
                return false;

            return IsInstantArithmeticExerciseFlow();
        }

        private bool IsInstantArithmeticExerciseFlow()
        {
            if (_config.OperationList == null || _config.OperationList.Count == 0)
                return false;

            if (_config.UIQuestionType != UIQuestionType.ThreeTexts &&
                _config.UIQuestionType != UIQuestionType.ThreeAddends &&
                _config.UIQuestionType != UIQuestionType.SimpleEquation &&
                _config.UIQuestionType != UIQuestionType.TwoLinesTwoAddends)
            {
                return false;
            }

            return _config.OperationList.All(IsArithmeticOperation);
        }

        private static bool IsArithmeticOperation(Operation operation)
        {
            return operation == Operation.Sum ||
                   operation == Operation.Minus ||
                   operation == Operation.Multiplication ||
                   operation == Operation.Divide;
        }

        private async Task HandleGameCompletionAsync(GameCompletionResult completion)
        {
            SetPageInteractionEnabled(false);
            SetPlayUiState(PlayUiState.Disabled);

            Page dataPage = ShowDataRoutingHelper.CreateChooserPage(completion.GameId);
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Debug.WriteLine($"[GameCompletion] Navigating to Games chooser for completed game {completion.GameId}");
                Application.Current.MainPage = new NavigationPage(dataPage);
            });

            _ = MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Task.Delay(100);

                try
                {
                    if (completion.IsWin)
                        await dataPage.DisplayAlert("Win", "🎉😊🏅\n" + completion.Duration.ToFormattedString("mm: ss"), "OK");
                    else
                        await dataPage.DisplayAlert("Lose", "🤷", "OK");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[GameCompletion] Completion alert failed: {ex}");
                }
            });
        }

        private void SetTextInputsEnabled(bool enabled)
        {
            if (_txtAddend1 != null) EntryEnabled(_txtAddend1, enabled);
            if (_txtAddend2 != null) EntryEnabled(_txtAddend2, enabled);
            if (_txtSum != null) EntryEnabled(_txtSum, enabled);
            RefreshNumericEntryAppearance();
        }

        private async Task GenerateNextExerciseAsync()
        {
            await AnimateBenchmarkQuestionAdvanceOutAsync();
            ExerciseGenerationResult generatedExercise = await _gamePlay.GenerateExerciseAsync();
            _pianoKeyboard?.RefreshKeyCaptions();
            await UpdateView(true, generatedExercise: generatedExercise);
            await AnimateBenchmarkQuestionAdvanceInAsync();
            await ApplyAutoAnswerTimeTuningAsync("AutoTuneNextQuestion");
            await EnsureInitialTimerSettingSavedAsync();
            await PersistVisibleQuestionPartsAsync();
            await PersistSecondaryPpwAsync();
            if (generatedExercise.PersistenceTask != null)
                await generatedExercise.PersistenceTask;
            await PersistKeyboardQuestionDisplayMetadataAsync();
            if (_isKeyboard)
            {
                SetKeyboardInteractionEnabled(true);

                if (_config.FromNumToNum)
                {
                    _pianoKeyboard.IsEnabled = true;
                }
            }
        }

        private async Task EnsureInitialTimerSettingSavedAsync()
        {
            if (_timerChangeEventRepository == null ||
                _gamePlay == null ||
                _gamePlay._questionNumber != 1 ||
                _pianoKeyboard is not PianoKeyboardSync syncKeyboard)
            {
                return;
            }

            await _timerChangeEventRepository.EnsureInitialEventAsync(
                _gamePlay.GameId.ToString(),
                syncKeyboard.AnswerTimeSetting,
                DateTime.Now,
                "InitialConfig");
        }

        private async Task ApplyAutoAnswerTimeTuningAsync(string source)
        {
            if (_isApplyingAutoTune ||
                _gameRepository == null ||
                _keyboardQuestionRepository == null ||
                _pianoKeyboard is not PianoKeyboardSync syncKeyboard)
            {
                return;
            }

            var activeUser = ServiceHelper.GetService<CurrentUserSession>()?.ActiveUser;
            if (activeUser == null)
                return;

            _isApplyingAutoTune = true;
            try
            {
                List<Game> userGames = await _gameRepository.GetAllByUserAsync(activeUser.Id);
                List<Game> relevantGames = userGames
                    .Where(game => game.Config?.KeyboardConfig != null)
                    .Where(game => game.Config.KeyboardConfig.SyncType == SyncType.Sync ||
                                   game.Config.KeyboardConfig.SyncType == SyncType.HalfSync ||
                                   game.Config.KeyboardConfig.SyncType == SyncType.Spatial)
                    .Where(game => string.Equals(game.GameName, _config.GameName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (relevantGames.Count == 0)
                    return;

                List<KeyboardQuestion> questions = await _keyboardQuestionRepository.GetByGameIdsAsync(relevantGames.Select(game => game.Id));
                KeyboardTimingRecommendation? recommendation = KeyboardTimingAnalyzer.BuildRecommendation(questions);
                if (recommendation == null)
                    return;

                bool wholeAnswerMode = UsesWholeAnswerTimer(syncKeyboard);
                int desiredMagnitude = wholeAnswerMode
                    ? recommendation.RecommendedWholeAnswerSeconds
                    : recommendation.RecommendedAfterLastKeySeconds;
                int desiredSetting = desiredMagnitude * (wholeAnswerMode ? -1 : 1);

                if (syncKeyboard.AnswerTimeSetting == 0)
                {
                    _lastNonZeroAnswerTimeSetting = desiredSetting;
                    RefreshAnswerTimeTuner();
                    return;
                }

                if (syncKeyboard.AnswerTimeSetting == desiredSetting)
                    return;

                await ApplyAnswerTimeSettingAsync(desiredSetting, source);
            }
            finally
            {
                _isApplyingAutoTune = false;
            }
        }

        private bool ApplyConfiguredKeyboardSeedState()
        {
            if (!_isKeyboard || _pianoKeyboard == null)
                return false;

            Color[]? initialColors = _gamePlay.GetInitialKeyboardColors();
            if (initialColors == null || initialColors.Length == 0)
                return false;

            _pianoKeyboard.PianoInit(initialColors);
            return true;
        }

        private async Task PersistVisibleQuestionPartsAsync()
        {
            if (_questionAnswerPartRepository == null)
                return;

            await _questionAnswerPartRepository.ReplaceForQuestionAsync(
                _gamePlay.GameId.ToString(),
                _gamePlay._questionNumber,
                BuildVisibleQuestionParts());
        }

        private async Task PersistSecondaryPpwAsync()
        {
            if (_questionAnswerRepository == null || !_isThreeTexts)
                return;

            PPWObject? secondary = _currentSecondaryPPW;
            PPWObject? enabled = _currentSecondaryPPWEnabled;

            if ((secondary == null || enabled == null) &&
                _config.ShowPrev &&
                _previousPPW != null &&
                _config.UIQuestionType != UIQuestionType.TwoLinesTwoAddends &&
                _config.UIQuestionType != UIQuestionType.ThreeAddends)
            {
                secondary = _previousPPW;
                enabled = new PPWObject(0, 0, 0);
            }

            if (secondary == null || enabled == null)
                return;

            await _questionAnswerRepository.UpdateSecondaryPpwAsync(
                _gamePlay.GameId.ToString(),
                _gamePlay._questionNumber,
                secondary.Addend1,
                secondary.Addend2,
                secondary.Sum,
                enabled.Addend1 == 1,
                enabled.Addend2 == 1,
                enabled.Sum == 1);
        }

        private async Task PersistKeyboardQuestionDisplayMetadataAsync()
        {
            if (_keyboardQuestionRepository == null ||
                !_isKeyboard ||
                _config.KeyboardConfig == null ||
                _config.KeyboardConfig.KeyboardOnlyForHelp ||
                _gamePlay is BitArrayGamePlay ||
                _pianoKeyboard == null)
            {
                return;
            }

            bool[]? initialKeyboardState = TryGetInitialKeyboardStateForData();
            bool[]? questionKeyboard = _config.FromNumToNum ? initialKeyboardState?.ToArray() : null;

            await _keyboardQuestionRepository.UpdatePendingDisplayMetadataAsync(
                _gamePlay.GameId.ToString(),
                _gamePlay._questionNumber,
                BuildCurrentKeyboardQuestionPromptText(),
                _config.KeyboardConfig.ShowNumbersOnKeys,
                _config.KeyboardConfig.WeightsArray?.ToArray(),
                initialKeyboardState,
                initialKeyboardColors: null,
                questionKeyboard: questionKeyboard,
                questionKeyboardColors: null,
                keyboardRows: _config.KeyboardConfig.Rows,
                keyboardKeysInRow: _config.KeyboardConfig.KeysInRow);
        }

        private bool[]? TryGetInitialKeyboardStateForData()
        {
            if (_pianoKeyboard == null || _config.KeyboardConfig == null)
                return null;

            if (_pianoKeyboard.initColors != null &&
                _pianoKeyboard.initColors.Length > 0 &&
                _pianoKeyboard.initColors.Any(bit => bit))
            {
                return _pianoKeyboard.initColors.ToArray();
            }

            if (_config.FromNumToNum || _config.KeyboardConfig.PpwKeyboardSeedMode != PpwKeyboardSeedMode.None)
            {
                bool[] currentBits = _pianoKeyboard.ToBitArray();
                if (currentBits.Any(bit => bit))
                    return currentBits;
            }

            return null;
        }

        private string? BuildCurrentKeyboardQuestionPromptText()
        {
            string[] candidates =
            {
                _txtSum?.Text ?? string.Empty,
                _txtAddend1?.Text ?? string.Empty,
                _txtAddend2?.Text ?? string.Empty
            };

            string? text = candidates.FirstOrDefault(candidate => !string.IsNullOrWhiteSpace(candidate));
            if (!string.IsNullOrWhiteSpace(text))
                return text.Trim();

            return _gamePlay.GetKeyboardQuestionPromptText();
        }

        private bool ShouldShowKeyboardPromptLabel()
        {
            return _isKeyboard &&
                   _config.KeyboardConfig != null &&
                   (_config.KeyboardConfig.ArrowLabelExerciseMode != ArrowLabelExerciseMode.None ||
                    _config.KeyboardConfig.AllowedArrowPromptKinds.HasFlag(ArrowPromptKindFlags.SpecialPrompt));
        }

        private bool UsesArrowLabelPromptStage()
        {
            return _gamePlay is BitArrayGamePlay arrowGamePlay && arrowGamePlay.HasArrowLabelPrompt;
        }

        private bool UsesArrowCorrectResponseFeedback()
        {
            if (!UsesArrowLabelPromptStage() || _config.KeyboardConfig == null)
                return false;

            bool usesComposedArrowSettings =
                _config.KeyboardConfig.AllowedArrowPromptKinds != ArrowPromptKindFlags.None ||
                _config.KeyboardConfig.AllowedArrowRouteKinds != ArrowRouteKindFlags.None ||
                _config.KeyboardConfig.SpecialArrowMissingTargets != MissingValueTargetFlags.None;

            if (!usesComposedArrowSettings &&
                _config.KeyboardConfig.ArrowLabelExerciseMode != ArrowLabelExerciseMode.None)
            {
                return true;
            }

            return _config.KeyboardConfig.ArrowFeedbackMode == ArrowFeedbackMode.CorrectResponse;
        }

        private ArrowLabelExerciseMode GetDisplayedArrowLabelExerciseMode()
        {
            if (_gamePlay is BitArrayGamePlay arrowGamePlay)
                return arrowGamePlay.CurrentArrowLabelExerciseMode;

            return _config.KeyboardConfig?.ArrowLabelExerciseMode ?? ArrowLabelExerciseMode.None;
        }

        private Entry? GetArrowLabelMissingEntry()
        {
            if (!UsesArrowLabelPromptStage() || _config.KeyboardConfig == null)
                return null;

            return GetDisplayedArrowLabelExerciseMode() switch
            {
                ArrowLabelExerciseMode.StartAndLength => _txtSum,
                ArrowLabelExerciseMode.StartAndEndWithMissingLength => _txtAddend2,
                ArrowLabelExerciseMode.EndAndLengthWithMissingStart => _txtAddend1,
                ArrowLabelExerciseMode.OrdinalStartAndLength => _txtSum,
                _ => null
            };
        }

        private void ResetArrowLabelPromptEntryColors()
        {
            if (!UsesArrowLabelPromptStage())
                return;

            if (_txtAddend1 != null) _txtAddend1.BackgroundColor = Colors.White;
            if (_txtAddend2 != null) _txtAddend2.BackgroundColor = Colors.White;
            if (_txtSum != null) _txtSum.BackgroundColor = Colors.White;
        }

        private void ApplyArrowLabelPromptFeedback(bool isCorrect)
        {
            if (!UsesArrowCorrectResponseFeedback())
                return;

            Entry? missingEntry = GetArrowLabelMissingEntry();
            if (missingEntry == null)
                return;

            ResetArrowLabelPromptEntryColors();
            missingEntry.BackgroundColor = isCorrect ? Colors.LightGreen : Colors.IndianRed;
        }

        private View BuildKeyboardArrowPromptView()
        {
            _txtAddend1 ??= CreateKeyboardArrowPromptEntry();
            _txtAddend2 ??= CreateKeyboardArrowPromptEntry();
            _txtSum ??= CreateKeyboardArrowPromptEntry();

            string pathData = GetArrowLabelPromptPathData();
            var layout = GetArrowLabelPromptLayout();

            var arrowPath = new Microsoft.Maui.Controls.Shapes.Path
            {
                Data = (Geometry)new PathGeometryConverter().ConvertFromInvariantString(pathData),
                Fill = Colors.Transparent,
                Stroke = Colors.Black,
                StrokeThickness = 5,
                WidthRequest = 220,
                HeightRequest = 88,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                StrokeLineCap = layout.UseRoundedStroke ? PenLineCap.Round : PenLineCap.Flat,
                StrokeLineJoin = layout.UseRoundedStroke ? PenLineJoin.Round : PenLineJoin.Miter
            };

            var promptSurface = new AbsoluteLayout
            {
                WidthRequest = 280,
                HeightRequest = 112,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start
            };

            AbsoluteLayout.SetLayoutBounds(arrowPath, layout.PathBounds);
            promptSurface.Children.Add(arrowPath);

            AbsoluteLayout.SetLayoutBounds(_txtAddend2, layout.Addend2Bounds);
            promptSurface.Children.Add(_txtAddend2);

            AbsoluteLayout.SetLayoutBounds(_txtAddend1, layout.Addend1Bounds);
            promptSurface.Children.Add(_txtAddend1);

            AbsoluteLayout.SetLayoutBounds(_txtSum, layout.SumBounds);
            promptSurface.Children.Add(_txtSum);

            return new VerticalStackLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start,
                Spacing = 0,
                Margin = new Thickness(0, 4, 0, 0),
                Children = { promptSurface }
            };
        }

        private static Entry CreateKeyboardArrowPromptEntry()
        {
            return new Entry
            {
                IsReadOnly = true,
                InputTransparent = true,
                BackgroundColor = Colors.White,
                TextColor = Colors.Black,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                FontSize = 18,
                HeightRequest = 25,
                WidthRequest = 50
            };
        }

        private void RefreshKeyboardArrowPromptView()
        {
            if (_keyboardArrowPromptView == null || _gamePlay is not BitArrayGamePlay arrowPromptGamePlay)
                return;

            _keyboardArrowPromptView.IsVisible = arrowPromptGamePlay.HasArrowLabelPrompt;
            if (!arrowPromptGamePlay.HasArrowLabelPrompt)
                return;

            ResetArrowLabelPromptEntryColors();

            ArrowLabelExerciseMode mode = arrowPromptGamePlay.CurrentArrowLabelExerciseMode;
            bool revealCorrectResponse = _gamePlay.Status == Statement.True && UsesArrowCorrectResponseFeedback();

            _txtAddend1.Text = mode == ArrowLabelExerciseMode.EndAndLengthWithMissingStart && !revealCorrectResponse
                ? string.Empty
                : arrowPromptGamePlay.ArrowLabelAddend1Value.ToString();

            _txtAddend2.Text = mode == ArrowLabelExerciseMode.StartAndEndWithMissingLength && !revealCorrectResponse
                ? string.Empty
                : arrowPromptGamePlay.ArrowLabelAddend2Value?.ToString() ?? string.Empty;

            _txtSum.Text = (mode == ArrowLabelExerciseMode.StartAndLength || mode == ArrowLabelExerciseMode.OrdinalStartAndLength) && !revealCorrectResponse
                ? string.Empty
                : arrowPromptGamePlay.ArrowLabelSumValue.ToString();
        }

        private string GetArrowLabelPromptPathData()
        {
            ArrowLabelExerciseMode mode = GetDisplayedArrowLabelExerciseMode();

            return mode switch
            {
                ArrowLabelExerciseMode.EndAndLengthWithMissingStart =>
                    "M 195,76 L 195,28 L 37,28 M 55,14 L 37,28 L 55,42",
                ArrowLabelExerciseMode.OrdinalStartAndLength =>
                    "M 60,76 L 60,52 A 22,22 0 0 1 80,30 L 118,30 L 100,14 M 118,30 L 100,46",
                _ =>
                    "M 72,72 L 72,28 L 182,28 M 166,14 L 182,28 L 166,42"
            };
        }

        private (Rect PathBounds, Rect Addend1Bounds, Rect Addend2Bounds, Rect SumBounds, bool UseRoundedStroke) GetArrowLabelPromptLayout()
        {
            ArrowLabelExerciseMode mode = GetDisplayedArrowLabelExerciseMode();

            return mode switch
            {
                ArrowLabelExerciseMode.EndAndLengthWithMissingStart => (
                    new Rect(27, 10, 220, 88),
                    new Rect(55, 64, 50, 25),
                    new Rect(130, 0, 50, 25),
                    new Rect(175, 64, 50, 25),
                    false),
                ArrowLabelExerciseMode.OrdinalStartAndLength => (
                    new Rect(12, 8, 220, 88),
                    new Rect(37, 64, 50, 25),
                    new Rect(62, 0, 50, 25),
                    new Rect(130, 64, 50, 25),
                    true),
                _ => (
                    new Rect(16, 10, 220, 88),
                    new Rect(41, 64, 50, 25),
                    new Rect(110, 0, 50, 25),
                    new Rect(154, 64, 50, 25),
                    false)
            };
        }

        private List<QuestionAnswerPart> BuildVisibleQuestionParts()
        {
            List<QuestionAnswerPart> parts = new();
            if (!_isThreeTexts || txt == null || txt.Length == 0)
                return parts;

            void AddPart(Entry? entry, int rowIndex, int columnIndex)
            {
                if (entry == null || !entry.IsVisible)
                    return;

                string valueText = entry.Text?.Trim() ?? string.Empty;
                if (string.IsNullOrEmpty(valueText))
                    return;

                parts.Add(new QuestionAnswerPart
                {
                    RowIndex = rowIndex,
                    ColumnIndex = columnIndex,
                    ValueText = valueText,
                    IsEnabled = entry.IsEnabled
                });
            }

            if (_config.UIQuestionType == UIQuestionType.TwoLinesTwoAddends)
            {
                AddPart(txt.ElementAtOrDefault(0), 0, 0);
                AddPart(txt.ElementAtOrDefault(1), 0, 1);
                return parts;
            }

            if (_config.UIQuestionType == UIQuestionType.ThreeAddends)
            {
                AddPart(txt.ElementAtOrDefault(0), 0, 0);
                return parts;
            }

            if (_config.HelpEntries || _config.HelpThroughTen)
            {
                AddPart(txt.ElementAtOrDefault(0), 0, 0);
                AddPart(txt.ElementAtOrDefault(1), 0, 1);
            }

            if (_config.HelpEntries || _config.HelpThroughTen)
            {
                AddPart(txt.ElementAtOrDefault(2), 1, 0);
                AddPart(txt.ElementAtOrDefault(3), 1, 1);
                AddPart(txt.ElementAtOrDefault(4), 1, 2);
                AddPart(txt.ElementAtOrDefault(5), 1, 3);
            }

            return parts;
        }


        private volatile bool _tutorialRunning = false;

        private bool CanShowAnswerTimeTuner()
        {
            return _pianoKeyboard is PianoKeyboardSync syncKeyboard &&
                   syncKeyboard.SupportsAnswerTimeTuner &&
                   !_config.KeyboardConfig.KeyboardOnlyForHelp;
        }

        private int GetEffectiveAnswerTimeMagnitude(PianoKeyboardSync syncKeyboard)
        {
            int currentMagnitude = Math.Min(AnswerTimeStateMaxSeconds, Math.Abs(syncKeyboard.AnswerTimeSetting));
            if (currentMagnitude > 0)
                return currentMagnitude;

            int fallbackMagnitude = Math.Min(AnswerTimeStateMaxSeconds, Math.Abs(_lastNonZeroAnswerTimeSetting));
            return fallbackMagnitude > 0 ? fallbackMagnitude : 3;
        }

        private string GetAnswerTimePanelIcon()
        {
            if (_pianoKeyboard is not PianoKeyboardSync syncKeyboard)
                return "⏱";

            if (syncKeyboard.AnswerTimeSetting == 0)
                return "⏱";

            int seconds = GetEffectiveAnswerTimeMagnitude(syncKeyboard);
            return UsesWholeAnswerTimer(syncKeyboard)
                ? $"Σ{seconds}"
                : $"↺{seconds}";
        }

        private static Color GetWholeAnswerFireColor() => Color.FromArgb("#FF7A00");

        private static Color GetAfterLastKeyAccentColor() => Color.FromArgb("#5A42D0");

        private Color GetAnswerTimeActiveColor(PianoKeyboardSync syncKeyboard)
        {
            return UsesWholeAnswerTimer(syncKeyboard)
                ? GetWholeAnswerFireColor()
                : GetAfterLastKeyAccentColor();
        }

        private Color GetAnswerTimePanelBackgroundColor()
        {
            if (_pianoKeyboard is not PianoKeyboardSync syncKeyboard)
                return Colors.Black.WithAlpha(0.25f);

            if (syncKeyboard.AnswerTimeSetting == 0)
                return Colors.Black.WithAlpha(0.25f);

            return GetAnswerTimeActiveColor(syncKeyboard).WithAlpha(0.55f);
        }

        private void RefreshAnswerTimePanelIcon()
        {
            if (_btnAnswerTimePanel == null)
                return;

            string icon = GetAnswerTimePanelIcon();
            _btnAnswerTimePanel.Text = icon;
            _btnAnswerTimePanel.FontSize = icon.Length > 1 ? 11 : 14;
            _btnAnswerTimePanel.BackgroundColor = GetAnswerTimePanelBackgroundColor();
            _btnAnswerTimePanel.Opacity = 1;
        }

        private void ApplyTransparentProgressBarTrack()
        {
            if (_pianoPressProgress?.Handler?.PlatformView == null)
                return;

#if IOS || MACCATALYST
            if (_pianoPressProgress.Handler.PlatformView is UIProgressView iosProgress)
            {
                iosProgress.TrackTintColor = UIColor.Clear;
                iosProgress.BackgroundColor = UIColor.Clear;
            }
#endif
#if ANDROID
            if (_pianoPressProgress.Handler.PlatformView is Android.Widget.ProgressBar androidProgress)
            {
                androidProgress.ProgressBackgroundTintList = ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
                androidProgress.SetBackgroundColor(Android.Graphics.Color.Transparent);
            }
#endif
#if WINDOWS
            if (_pianoPressProgress.Handler.PlatformView is Microsoft.UI.Xaml.Controls.ProgressBar windowsProgress)
            {
                windowsProgress.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            }
#endif
        }

        private bool UsesWholeAnswerTimer(PianoKeyboardSync syncKeyboard)
        {
            if (syncKeyboard.AnswerTimeSetting != 0)
                return syncKeyboard.AnswerTimeSetting < 0;

            if (_lastNonZeroAnswerTimeSetting != 0)
                return _lastNonZeroAnswerTimeSetting < 0;

            return _config.KeyboardConfig.SecondsPressingToAnswer < 0;
        }

        private void RefreshAnswerTimeTuner()
        {
            if (_pianoKeyboard is not PianoKeyboardSync syncKeyboard)
                return;

            RefreshAnswerTimePanelIcon();

            if (_answerTimeEnabledSwitch == null ||
                _answerTimeValueLabel == null ||
                _answerTimeModeLabel == null ||
                _answerTimeMinusButton == null ||
                _answerTimePlusButton == null ||
                _answerTimeModeButton == null)
            {
                return;
            }

            int answerTimeSetting = syncKeyboard.AnswerTimeSetting;
            bool isEnabled = answerTimeSetting != 0;
            int seconds = GetEffectiveAnswerTimeMagnitude(syncKeyboard);
            bool isWholeTimer = UsesWholeAnswerTimer(syncKeyboard);

            _answerTimeEnabledSwitch.IsToggled = isEnabled;
            _answerTimeValueLabel.Text = $"{seconds}s";
            _answerTimeModeLabel.Text = isWholeTimer
                ? "Counts across whole answer"
                : "Resets after each key press";
            _answerTimeModeButton.Text = isWholeTimer ? "Whole Answer" : "After Last Key";
            Color modeAccentColor = GetAnswerTimeActiveColor(syncKeyboard);
            _answerTimeModeButton.BackgroundColor = modeAccentColor;
            _answerTimeModeButton.TextColor = Colors.White;
            _answerTimeEnabledSwitch.IsEnabled = true;
            _answerTimeMinusButton.IsEnabled = seconds > 1;
            _answerTimePlusButton.IsEnabled = seconds < AnswerTimeStateMaxSeconds;
            _answerTimeModeButton.IsEnabled = true;
            if (_answerTimeTunerCard != null)
                _answerTimeTunerCard.IsVisible = _isAnswerTimeTunerVisible;
        }

        private async Task RecordTimerChangeAsync(int oldSetting, int newSetting, string source)
        {
            if (_timerChangeEventRepository == null || oldSetting == newSetting)
                return;

            TimerChangeEvent timerChangeEvent = new()
            {
                GameId = _gamePlay.GameId.ToString(),
                QuestionNumber = _gamePlay._questionNumber,
                EventTime = DateTime.Now,
                OldSetting = oldSetting,
                NewSetting = newSetting,
                Source = source
            };

            await _timerChangeEventRepository.SaveAsync(timerChangeEvent);
        }

        private async Task ApplyAnswerTimeSettingAsync(int newSetting, string source)
        {
            if (_pianoKeyboard is not PianoKeyboardSync syncKeyboard)
                return;

            int oldSetting = syncKeyboard.AnswerTimeSetting;
            syncKeyboard.UpdateAnswerTimeSetting(newSetting);
            if (newSetting != 0)
                _lastNonZeroAnswerTimeSetting = newSetting;

            await RecordTimerChangeAsync(oldSetting, newSetting, source);
            RefreshAnswerTimeTuner();
            RefreshCustomProgressVisual();
        }

        private async Task ToggleAnswerTimeModeAsync()
        {
            if (_pianoKeyboard is not PianoKeyboardSync syncKeyboard)
                return;

            int seconds = GetEffectiveAnswerTimeMagnitude(syncKeyboard);
            bool currentWhole = UsesWholeAnswerTimer(syncKeyboard);
            int signedValue = seconds * (currentWhole ? 1 : -1);

            if (syncKeyboard.AnswerTimeSetting == 0)
            {
                int oldDesiredSetting = _lastNonZeroAnswerTimeSetting;
                _lastNonZeroAnswerTimeSetting = signedValue;
                await RecordTimerChangeAsync(oldDesiredSetting, signedValue, "AnswerTimeModeWhileOff");
                RefreshAnswerTimeTuner();
                return;
            }

            await ApplyAnswerTimeSettingAsync(signedValue, "AnswerTimeMode");
        }

        private void SetAnswerTimeTunerVisibility(bool isVisible)
        {
            _isAnswerTimeTunerVisible = isVisible;
            if (_answerTimeTunerCard != null)
                _answerTimeTunerCard.IsVisible = isVisible;

            if (_answerTimeDismissShield != null)
                _answerTimeDismissShield.IsVisible = isVisible;

            if (isVisible)
                PositionAnswerTimeTunerCard();

            RefreshAnswerTimePanelIcon();
        }

        private void ToggleAnswerTimeTunerVisibility()
        {
            SetAnswerTimeTunerVisibility(!_isAnswerTimeTunerVisible);
        }

        private void HideAnswerTimeTuner()
        {
            if (!_isAnswerTimeTunerVisible)
                return;

            SetAnswerTimeTunerVisibility(false);
        }

        private void PositionAnswerTimeTunerCard(int remainingAttempts = 4)
        {
            if (!_isAnswerTimeTunerVisible ||
                _answerTimeTunerCard is not VisualElement tunerCard ||
                _btnAnswerTimePanel == null ||
                _rootGrid == null)
            {
                return;
            }

            void ApplyPosition()
            {
                if (!_isAnswerTimeTunerVisible ||
                    _answerTimeTunerCard is not VisualElement visibleCard ||
                    _btnAnswerTimePanel == null ||
                    _rootGrid == null)
                {
                    return;
                }

                double cardWidth = visibleCard.Width > 0 ? visibleCard.Width : visibleCard.WidthRequest;
                double cardHeight = visibleCard.Height > 0 ? visibleCard.Height : visibleCard.HeightRequest;
                double buttonWidth = _btnAnswerTimePanel.Width > 0 ? _btnAnswerTimePanel.Width : _btnAnswerTimePanel.WidthRequest;
                double buttonHeight = _btnAnswerTimePanel.Height > 0 ? _btnAnswerTimePanel.Height : _btnAnswerTimePanel.HeightRequest;

                if ((cardWidth <= 0 || cardHeight <= 0 || buttonWidth <= 0 || buttonHeight <= 0 || _rootGrid.Width <= 0 || _rootGrid.Height <= 0) &&
                    remainingAttempts > 0)
                {
                    _rootGrid.Dispatcher?.Dispatch(() => PositionAnswerTimeTunerCard(remainingAttempts - 1));
                    return;
                }

                visibleCard.TranslationX = 0;
                visibleCard.TranslationY = 0;

                PointF buttonPoint = GetPointRelativeToRoot(_btnAnswerTimePanel);
                PointF cardBasePoint = GetPointRelativeToRoot(visibleCard);

                double targetX = buttonPoint.X + (buttonWidth / 2d) - (cardWidth / 2d);
                double targetY = buttonPoint.Y + buttonHeight + 8;

                targetX = Math.Clamp(targetX, 8, Math.Max(8, _rootGrid.Width - cardWidth - 8));
                targetY = Math.Clamp(targetY, 8, Math.Max(8, _rootGrid.Height - cardHeight - 8));

                visibleCard.TranslationX = targetX - cardBasePoint.X;
                visibleCard.TranslationY = targetY - cardBasePoint.Y;
            }

            if (_rootGrid.Dispatcher?.IsDispatchRequired == true)
            {
                _rootGrid.Dispatcher.Dispatch(ApplyPosition);
                return;
            }

            ApplyPosition();
        }

        private View BuildAnswerTimeTuner()
        {
            _answerTimeEnabledSwitch = new Microsoft.Maui.Controls.Switch
            {
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center,
                OnColor = Color.FromArgb("#6D4AFF")
            };

            _answerTimeEnabledSwitch.Toggled += async (_, e) =>
            {
                if (_pianoKeyboard is not PianoKeyboardSync syncKeyboard)
                    return;

                if (e.Value)
                {
                    int seconds = GetEffectiveAnswerTimeMagnitude(syncKeyboard);
                    int sign = _lastNonZeroAnswerTimeSetting < 0 ? -1 : (syncKeyboard.AnswerTimeSetting < 0 ? -1 : 1);
                    await ApplyAnswerTimeSettingAsync(seconds * sign, "AnswerTimeEnabled");
                }
                else
                {
                    if (syncKeyboard.AnswerTimeSetting != 0)
                        _lastNonZeroAnswerTimeSetting = syncKeyboard.AnswerTimeSetting;

                    await ApplyAnswerTimeSettingAsync(0, "AnswerTimeDisabled");
                }
            };

            _answerTimeMinusButton = new Button
            {
                Text = "-",
                FontSize = 22,
                WidthRequest = 44,
                HeightRequest = 44,
                CornerRadius = 12,
                BackgroundColor = Colors.White,
                TextColor = Colors.Black
            };
            _answerTimeMinusButton.Clicked += async (_, _) =>
            {
                if (_pianoKeyboard is not PianoKeyboardSync syncKeyboard)
                    return;

                int seconds = Math.Max(1, GetEffectiveAnswerTimeMagnitude(syncKeyboard) - 1);
                int sign = UsesWholeAnswerTimer(syncKeyboard) ? -1 : 1;
                await ApplyAnswerTimeSettingAsync(seconds * sign, "AnswerTimeMinus");
            };

            _answerTimePlusButton = new Button
            {
                Text = "+",
                FontSize = 22,
                WidthRequest = 44,
                HeightRequest = 44,
                CornerRadius = 12,
                BackgroundColor = Colors.White,
                TextColor = Colors.Black
            };
            _answerTimePlusButton.Clicked += async (_, _) =>
            {
                if (_pianoKeyboard is not PianoKeyboardSync syncKeyboard)
                    return;

                int seconds = Math.Min(AnswerTimeStateMaxSeconds, GetEffectiveAnswerTimeMagnitude(syncKeyboard) + 1);
                int sign = UsesWholeAnswerTimer(syncKeyboard) ? -1 : 1;
                await ApplyAnswerTimeSettingAsync(seconds * sign, "AnswerTimePlus");
            };

            _answerTimeModeButton = new Button
            {
                FontSize = 13,
                CornerRadius = 12,
                Padding = new Thickness(12, 6),
                BackgroundColor = Color.FromArgb("#F4F0FF"),
                TextColor = Color.FromArgb("#5A42D0"),
                HorizontalOptions = LayoutOptions.Center
            };
            _answerTimeModeButton.Clicked += async (_, _) => await ToggleAnswerTimeModeAsync();

            _answerTimeValueLabel = new Label
            {
                FontSize = 28,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = Color.FromArgb("#6D4AFF")
            };

            _answerTimeModeLabel = new Label
            {
                FontSize = 12,
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = Colors.Gray
            };

            Border centerValue = new()
            {
                Padding = new Thickness(18, 10),
                StrokeThickness = 1,
                Stroke = Color.FromArgb("#DDD5FF"),
                BackgroundColor = Colors.White,
                StrokeShape = new RoundRectangle { CornerRadius = 24 },
                Shadow = new Shadow
                {
                    Brush = Colors.Black.WithAlpha(0.08f),
                    Offset = new Point(0, 3),
                    Radius = 10
                },
                Content = _answerTimeValueLabel
            };

            Border card = new()
            {
                Padding = new Thickness(16, 12),
                Margin = new Thickness(8, 52, 0, 0),
                StrokeThickness = 1,
                Stroke = Color.FromArgb("#E8E0FF"),
                BackgroundColor = Colors.White.WithAlpha(0.92f),
                WidthRequest = 210,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                ZIndex = 998,
                StrokeShape = new RoundRectangle { CornerRadius = 18 },
                Content = new VerticalStackLayout
                {
                    Spacing = 12,
                    Children =
                    {
                        new Grid
                        {
                            ColumnDefinitions =
                            {
                                new ColumnDefinition { Width = GridLength.Star },
                                new ColumnDefinition { Width = GridLength.Auto }
                            },
                            Children =
                            {
                                new Label
                                {
                                    Text = "Answer time",
                                    FontSize = 16,
                                    FontAttributes = FontAttributes.Bold,
                                    TextColor = Color.FromArgb("#5A42D0"),
                                    VerticalTextAlignment = TextAlignment.Center
                                },
                                _answerTimeEnabledSwitch
                            }
                        },
                        new HorizontalStackLayout
                        {
                            HorizontalOptions = LayoutOptions.Center,
                            Spacing = 10,
                            Children =
                            {
                                _answerTimeMinusButton,
                                centerValue,
                                _answerTimePlusButton
                            }
                        },
                        _answerTimeModeButton,
                        _answerTimeModeLabel
                    }
                }
            };

            Grid.SetColumn(_answerTimeEnabledSwitch, 1);
            card.IsVisible = _isAnswerTimeTunerVisible;
            _answerTimeTunerCard = card;
            RefreshAnswerTimeTuner();
            return card;
        }

        // Changed to async so we can await tutorial animation without changing constructor call site.
        private void InitializeUI()
        {
            bool isPianoHigh = UsesSyncKeyboardSubmissionMode() &&
                               (_config.UIQuestionType == UIQuestionType.OnlyKeyboard || !_config.KeyboardConfig.KeyboardOnlyForHelp);
            int pianoHeight = _isKeyboard ? (isPianoHigh ? 120 : 80) : 1;
            if (_isKeyboard && _config.KeyboardConfig.IsArrow) pianoHeight = 220;
            Grid grid = new()
            {
                BackgroundColor = Colors.AntiqueWhite,
                RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(40, GridUnitType.Star) },
                new RowDefinition { Height = _isKeyboard && !_config.KeyboardConfig.KeyboardOnlyForHelp ? GridLength.Auto : new GridLength(0) },
                new RowDefinition { Height = new GridLength(pianoHeight, GridUnitType.Star) }
            },
                ColumnDefinitions =
            {
                new ColumnDefinition()
            }
            };
            _rootGrid = grid;

            BoxView pageBackground = new()
            {
                Color = Colors.AntiqueWhite
            };
            grid.Add(pageBackground);
            Grid.SetRowSpan(pageBackground, grid.RowDefinitions.Count);



            double statementFontSize = _isKeyboard
                ? ((_config.UIQuestionType == UIQuestionType.LogicalKeyboards) ? 40 : 55)
                : 24;
            double statementMinHeight = _isKeyboard ? 60 : 120;

            _lblStatement = new Label
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Start,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                FontSize = statementFontSize,
                MinimumHeightRequest = statementMinHeight,
                HeightRequest = statementMinHeight,
                MaxLines = 4,
                LineBreakMode = LineBreakMode.WordWrap,
                TextColor = Colors.Black,
                Text = Statement.Neutral
            };

            if (ShouldShowKeyboardPromptLabel())
            {
                _keyboardArrowPromptView = BuildKeyboardArrowPromptView();
            }

            _pianoPressProgress = new ProgressBar
            {
                Progress = 0,
                Opacity = 1,
                BackgroundColor = Colors.Transparent,
                HeightRequest = 55,
                WidthRequest = 220,
                IsVisible = false,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center
            };
            _pianoPressProgress.HandlerChanged += (_, _) => ApplyTransparentProgressBarTrack();
            _pianoPressProgress.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == ProgressBar.ProgressProperty.PropertyName ||
                    e.PropertyName == VisualElement.IsVisibleProperty.PropertyName)
                {
                    RefreshCustomProgressVisual();
                }
            };

           /* HorizontalStackLayout statusRow = new()
            {
                Spacing = 10,
                HorizontalOptions = LayoutOptions.Center,
                Children =
    {
        
        
    }
            };*/

            VerticalStackLayout vsl = new()
        {
           // statusRow,
              
            };

            if (_keyboardArrowPromptView != null)
                vsl.Add(_keyboardArrowPromptView);
            else if (!_isKeyboard || _config.KeyboardConfig.KeyboardOnlyForHelp)
                vsl.Add(_lblStatement);

            vsl.HorizontalOptions = (!_isKeyboard || _config.KeyboardConfig.KeyboardOnlyForHelp)
                ? LayoutOptions.Fill
                : LayoutOptions.Center;
            vsl.Padding = _keyboardArrowPromptView != null
                ? new Thickness(15, 15, 15, 0)
                : new Thickness(15);
            vsl.Spacing = _keyboardArrowPromptView != null ? 0 : 10;

            if (_isThreeTexts)
            {
                InitTextsUI();
                View? numericKeypadView = UsesManagedNumericInput ? InitNumericKeypadUI() : null;
                VerticalStackLayout questionInputsLayout = new()
                {
                    HorizontalOptions = LayoutOptions.Center,
                    Spacing = _config.UIQuestionType == UIQuestionType.SimpleEquation ? 0 : 6,
                    Margin = _config.UIQuestionType == UIQuestionType.SimpleEquation
                        ? new Thickness(0, 0, 0, 12)
                        : Thickness.Zero
                };

                if (_config.UIQuestionType == UIQuestionType.SimpleEquation)
                {
                    _hzlEquation = InitEquationUI();
                    questionInputsLayout.Add(_hzlEquation);
                }
                else
                {
                    txt = new Entry[6];
                    double questionWidth = GetQuestionLayoutWidth();
                    double quarterWidth = GetQuestionQuarterWidth();
                    double halfWidth = GetQuestionHalfWidth(false);
                    for (int i = 0; i < txt.Length; i++)
                    {
                        txt[i] = new Entry
                        {
                            HorizontalOptions = LayoutOptions.Center,
                            HorizontalTextAlignment = TextAlignment.Center,
                            BackgroundColor = Colors.White,
                            TextColor = Colors.Black,
                            WidthRequest = quarterWidth,
                            FontSize = FONT_SIZE_DEFAULT,
                            IsVisible = !_isKeyboard || _config.KeyboardConfig.KeyboardOnlyForHelp
                        };
                        txt[i].Keyboard = Keyboard.Numeric;
                        ConfigureNumericEntry(txt[i]);
                    }
                    Label lbl2 = new Label
                    {
                        Text = "",
                        FontSize = FONT_SIZE_DEFAULT,
                        WidthRequest = halfWidth,
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center
                    };
                    Label lbl4 = new Label
                    {
                        Text = "",
                        FontSize = FONT_SIZE_DEFAULT,
                        WidthRequest = quarterWidth,
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center
                    };
                    if (_config.HelpThroughTen)
                    {
                        txt[0].IsEnabled = false;
                        txt[1].IsEnabled = false;
                        txt[0].WidthRequest = 3 * questionWidth / 4;
                        txt[1].WidthRequest = quarterWidth;
                        txt[2].WidthRequest = halfWidth;
                        txt[3].WidthRequest = quarterWidth;
                        txt[4].WidthRequest = quarterWidth;
                    }
                    if(_config.UIQuestionType == UIQuestionType.TwoLinesTwoAddends)
                    {
                        //_txtSum.IsVisible = false;
                        txt[0].IsEnabled = false;
                        txt[1].IsEnabled = false;
                        txt[0].WidthRequest = halfWidth;
                        txt[1].WidthRequest = halfWidth;
                    }
                    if (_config.UIQuestionType == UIQuestionType.ThreeAddends)
                    {
                        txt[0].IsEnabled = false;
                        txt[0].WidthRequest = questionWidth / 3;
                        _txtAddend1.WidthRequest = questionWidth / 3;
                        _txtAddend2.WidthRequest = questionWidth / 3;
                    }

                    if (_config.HelpEntries || _config.UIQuestionType == UIQuestionType.TwoLinesTwoAddends)
                        questionInputsLayout.Add(new HorizontalStackLayout { HorizontalOptions = LayoutOptions.Center, Children = { txt[0], txt[1] } });
                    if (_config.UIQuestionType != UIQuestionType.TwoLinesTwoAddends)
                        questionInputsLayout.Add(new HorizontalStackLayout { HorizontalOptions = LayoutOptions.Center, Children = { _txtSum } });

                    if (_config.HelpThroughTen)
                    {
                        questionInputsLayout.Add(new HorizontalStackLayout { HorizontalOptions = LayoutOptions.Center, Children = { txt[0], txt[1] } });
                        //vsl.Add(new HorizontalStackLayout { HorizontalOptions = LayoutOptions.Center, Children = { lbl2, txt[3] , lbl4 } });
                    }
                    if (_config.OperationList.Contains(Operation.Multiplication))
                        questionInputsLayout.Add(new HorizontalStackLayout { HorizontalOptions = LayoutOptions.Center, Children = { _hr } });
                    if(_config.UIQuestionType == UIQuestionType.ThreeAddends)
                    {
                        questionInputsLayout.Add(new HorizontalStackLayout
                        {
                            HorizontalOptions = LayoutOptions.Center,
                            Children = { _txtAddend1, txt[0], _txtAddend2 }
                        });
                    }
                    else
                    {
                        Grid centeredOperationRow = new()
                        {
                            HorizontalOptions = LayoutOptions.Center,
                            ColumnSpacing = 0,
                            ColumnDefinitions =
                            {
                                new ColumnDefinition { Width = new GridLength(_txtAddend1.WidthRequest) },
                                new ColumnDefinition { Width = new GridLength(_lblAction.WidthRequest) },
                                new ColumnDefinition { Width = new GridLength(_txtAddend2.WidthRequest) }
                            }
                        };

                        Grid.SetColumn(_txtAddend1, 0);
                        Grid.SetColumn(_lblAction, 1);
                        Grid.SetColumn(_txtAddend2, 2);

                        centeredOperationRow.Children.Add(_txtAddend1);
                        centeredOperationRow.Children.Add(_lblAction);
                        centeredOperationRow.Children.Add(_txtAddend2);

                        questionInputsLayout.Add(centeredOperationRow);
                    }
                    if (_config.HelpEntries)
                        questionInputsLayout.Add(new HorizontalStackLayout { HorizontalOptions = LayoutOptions.Center, Children = { txt[2], txt[3], txt[4], txt[5] } });
                }

                _questionInputsContainer = questionInputsLayout;
                _questionInputsBaseMargin = questionInputsLayout.Margin;
                AttachBenchmarkPickerGesture(questionInputsLayout);

                if (UsesBenchmarkPickerPreview() && _config.ShowPrev)
                {
                    View benchmarkPreviousPeekView = BuildPreviousBelowView();
                    vsl.Add(benchmarkPreviousPeekView);
                }

                if (numericKeypadView != null && ShouldPlaceNumericKeypadBesideEntriesForHelp())
                {
                    if (_numericKeypad != null)
                    {
                        _numericKeypad.WidthRequest = TASK_WIDTH * 0.72;
                        _numericKeypad.MaximumWidthRequest = TASK_WIDTH * 0.72;
                        _numericKeypad.HorizontalOptions = LayoutOptions.Start;
                        _numericKeypad.VerticalOptions = LayoutOptions.Center;
                        _numericKeypad.Margin = new Thickness(0, -8, 0, 0);
                    }

                    questionInputsLayout.VerticalOptions = LayoutOptions.Center;

                    vsl.Add(new HorizontalStackLayout
                    {
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        Spacing = 14,
                        Children =
                        {
                            questionInputsLayout,
                            numericKeypadView
                        }
                    });
                }
                else
                {
                    vsl.Add(questionInputsLayout);

                    if (numericKeypadView != null && !ShouldPlaceNumericKeypadBelowPreviousPreview())
                    {
                        vsl.Add(numericKeypadView);
                    }
                }
            }
            else if (UsesArrowPromptAnswerWithoutMainKeyboard)
            {
                View? numericKeypadView = UsesManagedNumericInput ? InitNumericKeypadUI() : null;
                if (numericKeypadView != null)
                    vsl.Add(numericKeypadView);
            }

            if (!_isKeyboard ||
                UsesManualKeyboardCheckMode() ||
                _config.KeyboardConfig.KeyboardOnlyForHelp)
            {
                View? previousBelowView = null;
                if (_config.ShowPrev && _isThreeTexts && !UsesBenchmarkPickerPreview())
                {
                    previousBelowView = BuildPreviousBelowView();
                    vsl.Add(previousBelowView);
                }
                if (_numericKeypad != null && ShouldPlaceNumericKeypadBelowPreviousPreview())
                    vsl.Add(_numericKeypad);
                HorizontalStackLayout buttonsRow = InitButtonsUI();
                if (buttonsRow.Children.Count > 0)
                    vsl.Add(buttonsRow);
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
                vsl.Add(InitLogicalKeyboardsUI());
                vsl.Padding = 0;
                vsl.Spacing = 0;
                vsl.HorizontalOptions = LayoutOptions.Fill;

                if (_config.IncludeTutorials)
                {
                    Debug.WriteLine("Starting tutorial hand animation...");
                    EnsureTutorialHandOverlay(isLeftHand: false);
                }
            }
            if (_config.UIQuestionType == UIQuestionType.CanvasesHands)
            {
                vsl.Add(InitCanvasComponentsUI());


            }


            if (_config.UIQuestionType == UIQuestionType.DecompositionGame)
            {
                vsl.Add(InitDecompositionGameUI());
            }

            if (_isKeyboard && !_config.KeyboardConfig.KeyboardOnlyForHelp)
            {
                EnsureKeyboardInlineCheckButton();
                vsl.VerticalOptions = LayoutOptions.End;
            }
            grid.Add(vsl);


            if (_isKeyboard)
            {
                if (UsesManualKeyboardCheckMode())
                {
                    _pianoKeyboard = new PianoKeyboard(_gamePlay, _lblStatement, _config.KeyboardConfig);
                }
                else
                {
                    _pianoKeyboard = _config.KeyboardConfig.SyncType switch
                    {
                        SyncType.HalfSync => new PianoKeyboardHalfSync(_gamePlay, _lblStatement, _pianoPressProgress, _config.KeyboardConfig),
                        SyncType.Sync or SyncType.Spatial => new PianoKeyboardSync(_gamePlay, _lblStatement, _pianoPressProgress, _config.KeyboardConfig),
                        _ => new PianoKeyboard(_gamePlay, _lblStatement, _config.KeyboardConfig)
                    };
                }

                if (_config.KeyboardConfig.KeyboardAsAQuestion)
                {
                    _pianoKeyboard = (PianoKeyboard)new PianoKeyboardReadOnly(_config.KeyboardConfig);
                }

                if (_pianoKeyboard is PianoKeyboardSync syncKeyboard)
                {
                    _lastNonZeroAnswerTimeSetting = syncKeyboard.AnswerTimeSetting != 0
                        ? syncKeyboard.AnswerTimeSetting
                        : _config.KeyboardConfig.SecondsPressingToAnswer;
                    syncKeyboard.CheckCompletedAsync = checkResult => HandleCheckResultAsync(checkResult, isKeyboardSubmission: true);
                }

                // Always host the keyboard inside an overlay host so we can run tutorials on-demand
                if (!_config.KeyboardConfig.HideMainKeyboard)
                {
                    _taskMainHost = new KeyboardOverlayHost(_pianoKeyboard);
                    grid.Add(_taskMainHost);
                    Grid.SetRow(_taskMainHost, 2);
                    View keyboardControlBar = BuildKeyboardControlBar();
                    grid.Add(keyboardControlBar);
                    Grid.SetRow(keyboardControlBar, 1);
                }

                if (CanShowAnswerTimeTuner())
                {
                    _answerTimeDismissShield = new BoxView
                    {
                        BackgroundColor = Colors.Transparent,
                        IsVisible = false,
                        InputTransparent = false,
                        ZIndex = 997
                    };
                    _answerTimeDismissShield.GestureRecognizers.Add(new TapGestureRecognizer
                    {
                        Command = new Command(HideAnswerTimeTuner)
                    });

                    View answerTimeTuner = BuildAnswerTimeTuner();

                    grid.Add(_answerTimeDismissShield);
                    Grid.SetRow(_answerTimeDismissShield, 1);
                    Grid.SetRowSpan(_answerTimeDismissShield, 2);

                    grid.Add(answerTimeTuner);
                    Grid.SetRow(answerTimeTuner, 1);
                    Grid.SetRowSpan(answerTimeTuner, 2);
                }

                if (_taskMainHost != null)
                {
                    // Help button (top-right over the keyboard)
                    _btnHelp = new Button
                    {
                        Text = "?",
                        FontSize = 16,
                        WidthRequest = 34,
                        HeightRequest = 34,
                        Padding = 0,
                        CornerRadius = 17,
                        BackgroundColor = Colors.Black.WithAlpha(0.25f),
                        TextColor = Colors.White,
                        HorizontalOptions = LayoutOptions.End,
                        VerticalOptions = LayoutOptions.Center,
                        Margin = Thickness.Zero,
                        ZIndex = 999,
                    };

                    _btnHelp.Clicked += async (_, __) =>
                    {
                        if(_tutorialRunning) return; // prevent multiple simultaneous tutorials
                        await MarkCurrentKeyboardQuestionTutorialUsedAsync();
                        
                        // make sure rects are synced before animating
                        _taskMainHost.SyncOverlay();

                    if (HasDedicatedKeyboardTutorial())
                        await RunRecordedKeyboardTutorialAsync(_taskMainHost);
                    else if (HasAvailableTutorial())
                        await RunCorrectAnswerHintAsync(_taskMainHost);
                };

                Grid overlayButtons = new()
                {
                    ColumnDefinitions =
    {
        new ColumnDefinition { Width = GridLength.Auto },
        new ColumnDefinition { Width = GridLength.Star },
        new ColumnDefinition { Width = GridLength.Auto }
    },
                    VerticalOptions = LayoutOptions.Start,
                    HeightRequest = 34,
                    Margin = new Thickness(8, 10, 8, 0),
                    ZIndex = 999
                };

                HorizontalStackLayout leftOverlayButtons = new()
                {
                    Spacing = 8,
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Center
                };

                // init button comes from the keyboard now
                if (_pianoKeyboard is PianoKeyboard pk && pk.BtnInit != null)
                {
                    pk.BtnInit.WidthRequest = 34;
                    pk.BtnInit.HeightRequest = 34;
                    pk.BtnInit.Padding = 0;
                    pk.BtnInit.CornerRadius = 17;
                    pk.BtnInit.BackgroundColor = Colors.Black.WithAlpha(0.25f);
                    pk.BtnInit.HorizontalOptions = LayoutOptions.Start;
                    pk.BtnInit.VerticalOptions = LayoutOptions.Center;
                    pk.BtnInit.Margin = Thickness.Zero;
                    pk.BtnInit.TranslationY = 0;

                    leftOverlayButtons.Add(pk.BtnInit);
                }

                if (CanShowAnswerTimeTuner())
                {
                    _btnAnswerTimePanel = new Button
                    {
                        Text = GetAnswerTimePanelIcon(),
                        FontSize = 14,
                        WidthRequest = 40,
                        HeightRequest = 34,
                        Padding = 0,
                        CornerRadius = 17,
                        BackgroundColor = Colors.Black.WithAlpha(0.25f),
                        TextColor = Colors.White,
                        HorizontalOptions = LayoutOptions.Start,
                        VerticalOptions = LayoutOptions.Center,
                        Margin = Thickness.Zero
                    };
                    _btnAnswerTimePanel.Clicked += (_, _) =>
                    {
                        ToggleAnswerTimeTunerVisibility();
                    };
                    leftOverlayButtons.Add(_btnAnswerTimePanel);
                    RefreshAnswerTimePanelIcon();
                }

                overlayButtons.Add(leftOverlayButtons, 0, 0);

                HorizontalStackLayout rightOverlayButtons = new()
                {
                    Spacing = 8,
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Center
                };

                HorizontalStackLayout? colorLegend = null;
                if (_pianoKeyboard?.Config?.IsMulticolor == true)
                {
                    colorLegend = new HorizontalStackLayout()
                    {
                        Spacing = 4,
                        HorizontalOptions = LayoutOptions.End,
                        VerticalOptions = LayoutOptions.Center
                    };

                    List<Color> legendColors = new() { Colors.Yellow, Colors.LightGreen };
                    if (UsesThreeColorGroupByColorStage())
                        legendColors.Add(Colors.Blue);

                    foreach (Color legendColor in legendColors)
                    {
                        colorLegend.Children.Add(new Border
                        {
                            WidthRequest = 12,
                            HeightRequest = 12,
                            Padding = 0,
                            StrokeThickness = 1,
                            Stroke = Colors.White.WithAlpha(0.85f),
                            BackgroundColor = legendColor,
                            StrokeShape = new RoundRectangle { CornerRadius = 3 },
                            HorizontalOptions = LayoutOptions.Center,
                            VerticalOptions = LayoutOptions.Center
                        });
                    }

                }

                if (HasDedicatedKeyboardTutorial())
                    rightOverlayButtons.Add(_btnHelp);

                if (ShouldShowKeyboardSubmitButton())
                {
                    _btnKeyboardSubmit = new Button
                    {
                        Text = "V",
                        FontSize = 15,
                        WidthRequest = 34,
                        HeightRequest = 34,
                        Padding = 0,
                        CornerRadius = 17,
                        BackgroundColor = Colors.Black.WithAlpha(0.25f),
                        TextColor = Colors.White,
                        HorizontalOptions = LayoutOptions.End,
                        VerticalOptions = LayoutOptions.Center,
                        Margin = Thickness.Zero
                    };
                    _btnKeyboardSubmit.Clicked += (_, _) => CheckGamePlay();
                    rightOverlayButtons.Add(_btnKeyboardSubmit);
                }

                if (ShouldShowImpossibleWeightedAnswerButton())
                {
                    _btnImpossibleWeightedAnswer = new Button
                    {
                        Text = "XXX",
                        FontSize = 13,
                        WidthRequest = 48,
                        HeightRequest = 34,
                        Padding = 0,
                        CornerRadius = 17,
                        BackgroundColor = Colors.Black.WithAlpha(0.25f),
                        TextColor = Colors.White,
                        HorizontalOptions = LayoutOptions.End,
                        VerticalOptions = LayoutOptions.Center,
                        Margin = Thickness.Zero
                    };
                    _btnImpossibleWeightedAnswer.Clicked += async (_, _) => await HandleImpossibleWeightedAnswerAsync();
                    rightOverlayButtons.Add(_btnImpossibleWeightedAnswer);
                }

                if (_pianoKeyboard is PianoKeyboard keyboardWithToggle &&
                    keyboardWithToggle.SupportsExternalHeaderResultVisibilityToggle)
                {
                    Button btnToggleSumVisibility = new()
                    {
                        Text = "◐",
                        FontSize = 14,
                        WidthRequest = 34,
                        HeightRequest = 34,
                        Padding = 0,
                        CornerRadius = 17,
                        BackgroundColor = Colors.Black.WithAlpha(0.25f),
                        TextColor = Colors.White,
                        HorizontalOptions = LayoutOptions.End,
                        VerticalOptions = LayoutOptions.Center,
                        Margin = Thickness.Zero
                    };
                    btnToggleSumVisibility.Clicked += async (_, _) =>
                    {
                        await keyboardWithToggle.ToggleHeaderResultVisibilityFromExternalButtonAsync();
                    };
                    rightOverlayButtons.Add(btnToggleSumVisibility);
                }

                if (colorLegend != null)
                    rightOverlayButtons.Add(colorLegend);

                if (rightOverlayButtons.Children.Count > 0)
                    overlayButtons.Add(rightOverlayButtons, 2, 0);

                    _taskMainHost.Children.Add(overlayButtons);
                }

                
                   
            }
            Content = grid;
#if DEBUG
            //AttachHudOverlay();
#endif
        }

        private HorizontalStackLayout InitEquationUI()
        {
            double equationHalfWidth = GetQuestionHalfWidth(false);
            _txtAddend1.WidthRequest = equationHalfWidth;
            _txtAddend2.WidthRequest = equationHalfWidth;
            _txtSum.WidthRequest = equationHalfWidth;
            _txtSum.BackgroundColor = Colors.White;
            _txtSum.FontSize = FONT_SIZE_DEFAULT;
            _lblEquationEquals = new Label
            {
                FontSize = FONT_SIZE_DEFAULT,
                WidthRequest = GetQuestionActionWidth(),
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                Text = "="
            };
            HorizontalStackLayout hzlEquation = new HorizontalStackLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                Children ={ _txtAddend1, _lblAction, _txtAddend2,
                            _lblEquationEquals,
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

            if (_gamePlay is DecompositionGamePlay decompositionGamePlay)
            {
                decompositionGamePlay.AttachDashboard(lblStats, pc);
                pc.SelectedIndexChanged += async (_, __) =>
                {
                    ExerciseGenerationResult generatedExercise = await decompositionGamePlay.OnLevelSelectedAsync(pc.SelectedIndex);
                    await UpdateView(true, generatedExercise: generatedExercise);
                    if (generatedExercise.PersistenceTask != null)
                        await generatedExercise.PersistenceTask;
                };
            }


            vslDecompositionDashboard.Add(pc);
            vslDecompositionDashboard.Add(lblStats);

            return vslDecompositionDashboard;
        }


        private VerticalStackLayout InitLogicalKeyboardsUI()
        {
            VerticalStackLayout vsl = new();
            _keyboardTask2 = new PianoKeyboardReadOnly(_config.KeyboardConfig)
            {
                HorizontalOptions = LayoutOptions.Fill,
                HeightRequest = PIANO_HEIGHT2
            };
            _lblAction = new Label
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Start,
                FontSize = 40,
                TextColor = Colors.Black,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };
            _logicalColorLeftArrow = new Label
            {
                Text = "\u2190",
                FontSize = 34,
                TextColor = Colors.Yellow,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };
            _logicalColorRightArrow = new Label
            {
                Text = "\u2192",
                FontSize = 34,
                TextColor = Colors.LightGreen,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };
            _logicalColorActionLayout = new HorizontalStackLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                Spacing = 26,
                IsVisible = false,
                Children = { _logicalColorLeftArrow, _logicalColorRightArrow }
            };
            KeyboardConfig config1 = new KeyboardConfig
            {
                Rows = 1,
                KeysInRow = _config.KeyboardConfig.KeysInRow
            };
            if (_config.UsesCombinedLogicalKeyboard) config1.Rows = 2;
            _keyboardTask1 = new PianoKeyboardReadOnly(config1)
            {
                HorizontalOptions = LayoutOptions.Fill,
                HeightRequest = (_config.UsesCombinedLogicalKeyboard) ? (PIANO_HEIGHT2 * 2) + 5 : PIANO_HEIGHT2
            };

            _task2Host = new KeyboardOverlayHost(_keyboardTask2);
            _task1Host = new KeyboardOverlayHost(_keyboardTask1);


            //vsl.Add(_keyboardTask2);
            vsl.Add(_task2Host);
            vsl.Add(new VerticalStackLayout
            {
                HorizontalOptions = LayoutOptions.Fill,
                Spacing = 2,
                Children = { _lblAction, _logicalColorActionLayout }
            });
            vsl.Add(_task1Host);//vsl.Add(_keyboardTask1);
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

            if (_config.ShowPrev && !UsesBenchmarkPickerPreview())
            {
                _btnPrev = new Button
                {
                    Text = "Prev",

                    HorizontalOptions = LayoutOptions.Center
                };
                _btnPrev.Pressed += (_, _) => ShowPreviousInline();
                _btnPrev.Released += (_, _) => RestoreCurrentInlinePreview();

                hslBtns.Add(_btnPrev);

                _btnPrevBelow = new Button
                {
                    Text = "Show Prev",
                    HorizontalOptions = LayoutOptions.Center
                };
                _btnPrevBelow.Clicked += (_, _) => TogglePreviousBelow();
                hslBtns.Add(_btnPrevBelow);
            }

            if (_config.UIQuestionType == UIQuestionType.SimpleEquation)
            {
                _btnEquationHelp = new Button
                {
                    Text = "Help",
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = Thickness.Zero
                };
                _btnEquationHelp.Clicked += async (_, _) => await RunEquationHelpAsync();
                hslBtns.Add(_btnEquationHelp);
            }
            bool showCheckButton = ShouldShowPpwCheckButton();

            if (_config.NumberOfMistakesToLose < 0 && !_config.IsHistory)
            {
                if (showCheckButton)
                    hslBtns.Add(_btnCheck);
                if (ShouldShowNextButton())
                    hslBtns.Add(_btnNext);
            }
            /*if(_config.NumberOfMistakesToLose >= 0 && OperatingSystem.IsIOS())
            {  
                hslBtns.Add(_btnCheck);
            }*/

            RefreshPreviousPreview();
            return hslBtns;
        }

        private void InitTextsUI()
        {
            bool isLblAction = _config.EnforceOperationLabel || _config.OperationList.Count > 1;
            double questionWidth = GetQuestionLayoutWidth();
            double halfWidth = GetQuestionHalfWidth(isLblAction);

            _txtSum = new Entry
            {
                Keyboard = Keyboard.Numeric,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                BackgroundColor = Colors.White,
                TextColor = Colors.Black,
                WidthRequest = questionWidth,
                FontSize = 32,
                IsVisible = _config.UIQuestionType != UIQuestionType.OnlyKeyboard && _config.UIQuestionType != UIQuestionType.TwoLinesTwoAddends

            };


            _txtAddend1 = new Entry
            {
                Keyboard = Keyboard.Numeric,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                BackgroundColor = Colors.White,
                TextColor = Colors.Black,
                WidthRequest = halfWidth,
                FontSize = FONT_SIZE_DEFAULT,
                IsVisible = _config.UIQuestionType == UIQuestionType.SimpleEquation || _config.UIQuestionType == UIQuestionType.ThreeTexts || _config.UIQuestionType == UIQuestionType.DecompositionGame || _config.UIQuestionType == UIQuestionType.TwoLinesTwoAddends || _config.UIQuestionType == UIQuestionType.ThreeAddends
            };
            _lblAction = new Label
            {
                FontSize = FONT_SIZE_DEFAULT,
                TextColor = Colors.Black,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                WidthRequest = GetQuestionActionWidth(),
                IsVisible = isLblAction
            };
            _hr = new BoxView
            {
                HeightRequest = 2,
                WidthRequest = questionWidth,
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
                WidthRequest = halfWidth,
                FontSize = FONT_SIZE_DEFAULT,
                IsVisible = _config.UIQuestionType == UIQuestionType.SimpleEquation || _config.UIQuestionType == UIQuestionType.ThreeTexts || _config.UIQuestionType == UIQuestionType.DecompositionGame || _config.UIQuestionType == UIQuestionType.TwoLinesTwoAddends || _config.UIQuestionType == UIQuestionType.ThreeAddends
            };
            _txtAddend1.Keyboard = Keyboard.Numeric;
            _txtAddend2.Keyboard = Keyboard.Numeric;
            _txtSum.Keyboard = Keyboard.Numeric;
            ConfigureNumericEntry(_txtAddend1);
            ConfigureNumericEntry(_txtAddend2);
            ConfigureNumericEntry(_txtSum);

            _lastFocused = _txtSum;

            _txtSum.Completed += (sender, e) =>
            {
                CheckGamePlay();
            };
            _txtAddend1.Completed += (sender, e) =>
            {
                if (!_config.RequiresBothAddendsInput)
                    CheckGamePlay();
                else
                    _txtAddend2.Focus();
            };
            _txtAddend2.Completed += (sender, e) =>
            {
                CheckGamePlay();
            };
        }

        private void AttachHudOverlay()
        {
            var original = Content;
            if (original == null) return;

            var root = new AbsoluteLayout();

            // Main content fills screen
            AbsoluteLayout.SetLayoutBounds(original, new Rect(0, 0, 1, 1));
            AbsoluteLayout.SetLayoutFlags(original, AbsoluteLayoutFlags.All);
            root.Children.Add(original);

            var hud = new DebugHudView
            {
                WidthRequest = 340,
                HeightRequest = 240
            };

            // Top-right corner
            AbsoluteLayout.SetLayoutBounds(hud, new Rect(1, 0, hud.WidthRequest, hud.HeightRequest));
            AbsoluteLayout.SetLayoutFlags(hud, AbsoluteLayoutFlags.PositionProportional);

            root.Children.Add(hud);

            Content = root;
        }

        private const double TutorialSpeedFactor = 2.0;
        private const double ArrowTutorialSpeedFactor = 1.0;

        private static int ScaleTutorialMs(int milliseconds)
            => (int)Math.Round(milliseconds * TutorialSpeedFactor);

        private static uint ScaleTutorialMs(uint milliseconds)
            => (uint)Math.Round(milliseconds * TutorialSpeedFactor);

        private static int ScaleArrowTutorialMs(int milliseconds)
            => (int)Math.Round(milliseconds * ArrowTutorialSpeedFactor);

        private static uint ScaleArrowTutorialMs(uint milliseconds)
            => (uint)Math.Round(milliseconds * ArrowTutorialSpeedFactor);
    }

}
