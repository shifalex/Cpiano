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

            if (_btnArrowLabelRetryHelp != null)
                _btnArrowLabelRetryHelp.IsEnabled = enabled &&
                    ShouldUseArrowLabelRetryButtons() &&
                    (!_isArrowLabelRetryHelpUsed || CanUseSecondHelpForLearnerMiddle()) &&
                    _gamePlay.Status != Statement.True;

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
                _btnNext.IsEnabled = ShouldUseArrowLabelRetryButtons()
                    ? enabled && ShouldEnableArrowLabelRetryNextButton()
                    : enabled && HasVisibleManualCheckButton() ? (_gamePlay.GuessNumber > 0) : false;

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

        private async void ShowSequenceFirstProgressFeedback(double progress)
        {
            if (!_isPageVisible || !_config.KeyboardConfig.IsPrecisionPinchSequenceMemorize)
                return;

            int version = ++_sequenceFeedbackChangeVersion;
            if (progress == 0)
            {
                await Task.Delay(800);
                if (version != _sequenceFeedbackChangeVersion || !_isPageVisible)
                    return;
            }

            _sequenceFirstFeedbackProgress = Math.Clamp(progress, -1, 1);
            RefreshStatusActionSlot();
        }

        private void RestoreReadyForInputState()
        {
            if (_tutorialRunning)
                return;

            ResetArrowLabelPromptEntryColors();
            _choiceAnswerKeyboard?.ResetFeedback();

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
            bool useAndroidPinchInputBlock =
                DeviceInfo.Platform == DevicePlatform.Android &&
                _config.KeyboardConfig?.IsPrecisionPinchExercise == true;
            if (useAndroidPinchInputBlock)
                _pianoKeyboard.InputTransparent = true;
            else
                host.SetTutorialMode(true);

            try
            {
                await Tutorial(host);
            }
            finally
            {
                ClearTutorialStepCounter();
                _pianoKeyboard?.ClearTutorialStepLabels();
                if (useAndroidPinchInputBlock)
                {
                    if (_pianoKeyboard is PianoKeyboardSync syncKeyboard)
                        syncKeyboard.NotifyQuestionReadyForInput();
                    else if (_pianoKeyboard != null)
                        _pianoKeyboard.InputTransparent = false;
                }
                else
                {
                    host.SetTutorialMode(false);
                }
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

            if (_config.KeyboardConfig?.PrecisionPinchMemorizeDelaySeconds > 0 &&
                _gamePlay is BitArrayGamePlay memorizeGamePlay)
            {
                await RunMemorizeHelpAsync(host, memorizeGamePlay);
                return;
            }

            bool preserveQuestionColors =
                _config.KeyboardConfig?.IsPrecisionSignLearningExercise == true ||
                (DeviceInfo.Platform == DevicePlatform.Android &&
                 _config.KeyboardConfig?.IsPrecisionPinchExercise == true);
            Color[]? keyboardSnapshot = CaptureLiveKeyboardColors();
            try
            {
                if (!preserveQuestionColors)
                    ClearLiveKeyboardState();
                host.SyncOverlay();
                await RunTutorialAsync(host);
            }
            finally
            {
                if (!preserveQuestionColors)
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
            ShouldShowKeyboardPromptLabel() &&
            !IsActiveArrowKeyboardQuestion;

        private bool IsActiveArrowKeyboardQuestion =>
            _gamePlay is BitArrayGamePlay arrowGamePlay &&
            arrowGamePlay.IsActiveOnKeyboardArrowQuestion;

        private bool UsesArrowLabelRetryStage =>
            _isKeyboard &&
            _config.KeyboardConfig != null &&
            (_config.KeyboardConfig.EnableArrowLabelRetry ||
             _config.KeyboardConfig.ArrowLabelRetryMode != ArrowLabelRetryMode.None);

        private bool ShouldHostMainKeyboard =>
            _isKeyboard &&
            (!_config.KeyboardConfig.HideMainKeyboard || UsesArrowLabelRetryStage);

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
        private Border _calmAttemptIndicator;
        private Label _calmAttemptIndicatorLabel;
        private Button _btnKeyboardCheckInline = null;
        private Border _centerFeedbackBadge = null;
        private Label _centerFeedbackBadgeLabel = null;
        private double _sequenceFirstFeedbackProgress;
        private int _sequenceFeedbackChangeVersion;
        private Label _correctExpressionLabel = null;
        private Border _keyboardControlBar = null;
        private Button _btnThirdArrowVisibility = null;
        private bool _isPageInteractionEnabled = true;
        private bool _isArrowLabelEquationIntroVisible;
        private bool _isArrowLabelRetryHelpVisible;
        private bool _isComplexThroughTenBreakdownVisible;
        private readonly Dictionary<Entry, bool> _complexPromptEntryValidationStates = new();
        private Label _lblHistory;
        private Entry _txtAddend1;
        private Entry _txtAddend2;
        private Entry _txtSum;
        private Entry _txtComplexAddend3;
        private Entry _txtComplexSum2;
        private Entry _txtComplexTotalDistance;
        private Microsoft.Maui.Controls.Shapes.Path _keyboardArrowPromptPath;
        private Microsoft.Maui.Controls.Shapes.Path _complexFirstArrowPath;
        private Microsoft.Maui.Controls.Shapes.Path _complexSecondArrowPath;
        private Microsoft.Maui.Controls.Shapes.Path _complexTotalArrowPath;
        private View _arrowEquationPromptView;
        private Label _arrowEquationLeftLabel;
        private Label _arrowEquationMiddleLabel;
        private Label _arrowEquationRightLabel;
        private Entry _arrowEquationAnswerEntry;
        private Label _lblAction;
        private GraphicsView _verticalLeftShiftInstruction;
        private GraphicsView _verticalRightShiftInstruction;
        private PrecisionShiftInstructionDrawable _verticalLeftShiftDrawable;
        private PrecisionShiftInstructionDrawable _verticalRightShiftDrawable;
        private GraphicsView _legacyShiftInstructionView;
        private PrecisionShiftInstructionDrawable _legacyShiftInstructionDrawable;
        private Action<double>? _applyPrecisionHandGap;
        private Action? _togglePrecisionHandGapSlider;
        private View? _precisionHandGapButton;
        private double _precisionHandGap = 2;
        private HorizontalStackLayout _logicalColorActionLayout;
        private Label _logicalColorLeftArrow;
        private Label _logicalColorRightArrow;
        private NumericKeypadView _numericKeypad;
        private ChoiceAnswerKeyboardView _choiceAnswerKeyboard;
        private int? _lastChoiceAnswerValue;
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
        private Button _btnArrowLabelRetryHelp = null;
        private bool _isArrowLabelRetryHelpUsed;
        private bool _isComplexMiddleFilledByHelp;
        private bool _isCorrectExpressionLabelVisibleForCurrentExercise;
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
        private TapGestureRecognizer _answerTimeOutsideTap = null;
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
        private bool _isPageVisible;
        private int _consecutiveWrongAnswers = 0;
        private static readonly Color[] ArrowBackgroundCycle =
        {
            Colors.Black,
            Color.FromArgb("#1C2E4A"),
            Color.FromArgb("#3A3213"),
            Color.FromArgb("#3B1F2B")
        };
        private const bool EnableNumericInputDebug = false;
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
        private bool _hasManualAnswerTimeOverride;

        //VerticalStackLayout _vsl;
        protected IDispatcherTimer timer;
        protected virtual void TimerInit()
        {
            timer = Application.Current.Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) =>
            {
                if (_isPageVisible)
                    UpdateStatement();
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
            if (ShouldHideCheckAndNextButtons())
                return false;

            return UsesManualKeyboardCheckMode() &&
                   !ShouldUseInlineKeyboardCheckButton();
        }

        private bool ShouldUseInlineKeyboardCheckButton()
        {
            if (ShouldHideCheckAndNextButtons())
                return false;

            return UsesManualKeyboardCheckMode();
        }

        private bool ShouldHideCheckAndNextButtons()
        {
            bool isArrowLabelStage =
                _config.KeyboardConfig?.ArrowLabelExerciseMode != ArrowLabelExerciseMode.None ||
                (_config.GameName?.Contains("Arrow Label", StringComparison.OrdinalIgnoreCase) ?? false);

            return _config.HideCheckAndNextButtons ||
                   (isArrowLabelStage && !ShouldUseArrowLabelRetryButtons());
        }

        private bool ShouldShowPpwCheckButton()
        {
            if (ShouldUseArrowLabelRetryButtons())
                return false;

            if (ShouldHideCheckAndNextButtons())
                return false;

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
            if (ShouldUseArrowLabelRetryButtons())
                return true;

            if (ShouldHideCheckAndNextButtons())
                return false;

            return !IsGroupByColorKeyboardStage() &&
                   !UsesBenchmarkPickerPreview();
        }

        private bool ShouldShowImpossibleWeightedAnswerButton()
        {
            if (ShouldHideCheckAndNextButtons())
                return false;

            return _isKeyboard &&
                   !_config.KeyboardConfig.KeyboardOnlyForHelp &&
                   _gamePlay.SupportsImpossibleWeightedAnswer;
        }

        private bool ShouldUseArrowLabelRetryButtons()
        {
            return UsesArrowLabelRetryStage &&
                   ShouldShowKeyboardPromptLabel() &&
                   _config.KeyboardConfig?.KeyboardOnlyForHelp == true &&
                   _config.KeyboardConfig?.HideMainKeyboard == true &&
                   !IsActiveArrowKeyboardQuestion;
        }

        private bool ShouldEnableArrowLabelRetryNextButton()
        {
            return ShouldUseArrowLabelRetryButtons() &&
                   _gamePlay.Status == Statement.True;
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

        private void EnsureCalmAttemptIndicator()
        {
            if (_calmAttemptIndicator != null)
                return;

            _calmAttemptIndicatorLabel = new Label
            {
                Text = "•",
                FontSize = 34,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };

            _calmAttemptIndicator = new Border
            {
                WidthRequest = 55,
                HeightRequest = 55,
                Padding = 0,
                StrokeThickness = 0,
                BackgroundColor = Color.FromArgb("#5A42D0").WithAlpha(0.78f),
                StrokeShape = new RoundRectangle { CornerRadius = 28 },
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                IsVisible = false,
                InputTransparent = true,
                Content = _calmAttemptIndicatorLabel
            };
        }

        private bool UsesCalmAttemptIndicator()
        {
            return _config.KeyboardConfig?.AllowedArrowMovementModes != ArrowMovementModeFlags.None;
        }

        private bool IsCurrentKeyboardPressAlreadyCorrect()
        {
            if (!UsesCalmAttemptIndicator() ||
                _pianoKeyboard == null ||
                _gamePlay is not BitArrayGamePlay bitArrayGamePlay)
                return false;

            try
            {
                return bitArrayGamePlay.CheckOnly(_pianoKeyboard.ToBitArray());
            }
            catch
            {
                return false;
            }
        }

        private void RefreshCustomProgressVisual()
        {
            if (_customProgressHost == null || _customProgressFill == null || _pianoPressProgress == null)
                return;

            bool useCalmAttemptIndicator = UsesCalmAttemptIndicator();
            bool currentPressIsCorrect = IsCurrentKeyboardPressAlreadyCorrect();
            if (useCalmAttemptIndicator && !currentPressIsCorrect && _pianoPressProgress.IsVisible)
                _pianoPressProgress.IsVisible = false;

            bool isFeedbackState = _currentUiState == PlayUiState.FeedbackCorrect ||
                                   _currentUiState == PlayUiState.FeedbackWrong;
            bool showTutorialStepCounter = !string.IsNullOrWhiteSpace(_tutorialStepCounterText);
            if (useCalmAttemptIndicator)
            {
                _customProgressHost.IsVisible = currentPressIsCorrect &&
                                                !isFeedbackState &&
                                                !showTutorialStepCounter &&
                                                (!_isArrowLabelRetryHelpVisible || IsActiveArrowKeyboardQuestion);
            }

            double hostWidth = _customProgressHost.Width > 0
                ? _customProgressHost.Width
                : _customProgressHost.WidthRequest;

            double progress = Math.Clamp(_pianoPressProgress.Progress, 0, 1);
            _customProgressFill.BackgroundColor = _pianoPressProgress.ProgressColor;
            _customProgressFill.WidthRequest = hostWidth * progress;
            _customProgressFill.IsVisible = _customProgressHost.IsVisible && progress > 0;

            if (_calmAttemptIndicator != null && useCalmAttemptIndicator)
            {
                bool showAttemptIndicator = progress > 0 &&
                                            !currentPressIsCorrect &&
                                            _currentUiState is not PlayUiState.FeedbackCorrect and not PlayUiState.FeedbackWrong &&
                                            string.IsNullOrWhiteSpace(_tutorialStepCounterText);
                _calmAttemptIndicator.IsVisible = showAttemptIndicator;
                _calmAttemptIndicator.Opacity = showAttemptIndicator
                    ? 0.55 + (0.45 * Math.Min(1, progress))
                    : 0;
                _calmAttemptIndicator.Scale = showAttemptIndicator
                    ? 0.92 + (0.12 * Math.Min(1, progress))
                    : 1;
            }
        }

        private void RefreshStatusActionSlot()
        {
            bool showTutorialStepCounter = !string.IsNullOrWhiteSpace(_tutorialStepCounterText);
            bool isFeedbackState = _currentUiState == PlayUiState.FeedbackCorrect ||
                                   _currentUiState == PlayUiState.FeedbackWrong;
            bool usePromptEntryFeedback = UsesArrowCorrectResponseFeedback() && isFeedbackState;
            bool showFeedbackBadge = isFeedbackState && !usePromptEntryFeedback && !showTutorialStepCounter;
            bool showSequenceFirstFeedback = _sequenceFirstFeedbackProgress != 0 &&
                                             !isFeedbackState &&
                                             !showTutorialStepCounter;
            bool usesInlineCheck = ShouldUseInlineKeyboardCheckButton();
            bool currentPressIsCorrect = IsCurrentKeyboardPressAlreadyCorrect();
            bool showInlineCheck = usesInlineCheck &&
                                   _btnKeyboardCheckInline != null &&
                                   _btnKeyboardCheckInline.IsVisible &&
                                   !isFeedbackState &&
                                   !showTutorialStepCounter;
            bool showProgress = _pianoPressProgress != null &&
                                (!UsesCalmAttemptIndicator() || currentPressIsCorrect) &&
                                !usesInlineCheck &&
                                !isFeedbackState &&
                                !showTutorialStepCounter &&
                                (!_isArrowLabelRetryHelpVisible || IsActiveArrowKeyboardQuestion);

            if (_pianoPressProgress != null)
                _pianoPressProgress.IsVisible = showProgress;

            if (_customProgressHost != null)
            {
                _customProgressHost.IsVisible = showProgress;
                RefreshCustomProgressVisual();
            }

            if (_calmAttemptIndicator != null && !UsesCalmAttemptIndicator())
                _calmAttemptIndicator.IsVisible = false;

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
            else if (showFeedbackBadge || showSequenceFirstFeedback)
            {
                bool isCorrect = _currentUiState == PlayUiState.FeedbackCorrect;
                _centerFeedbackBadge.BackgroundColor = Colors.Transparent;
                _centerFeedbackBadgeLabel.TextColor = Colors.White;
                _centerFeedbackBadgeLabel.Text = showSequenceFirstFeedback
                    ? (_sequenceFirstFeedbackProgress > 0 ? "💪" : "🤔")
                    : (isCorrect ? "💪" : "🤔");
                _centerFeedbackBadgeLabel.FontSize = showSequenceFirstFeedback
                    ? 28
                    : (_config.KeyboardConfig?.IsVerticalPrecisionPinchExercise == true ? 42 : 55);
            }

            _centerFeedbackBadge.Scale = showSequenceFirstFeedback ? 0.75 : 1;
            _centerFeedbackBadge.Opacity = showSequenceFirstFeedback ? 0.78 : 1;
            _centerFeedbackBadge.IsVisible = showTutorialStepCounter || showFeedbackBadge || showSequenceFirstFeedback;
        }

        private void EnsureCorrectExpressionLabel()
        {
            if (_correctExpressionLabel != null)
                return;

            _correctExpressionLabel = new Label
            {
                IsVisible = false,
                BackgroundColor = Colors.White.WithAlpha(0.92f),
                TextColor = Colors.Black,
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                Padding = new Thickness(14, 6),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(16, 8, 16, 0),
                ZIndex = 900
            };
        }

        private bool ShouldShowCorrectExpressionFeedback(ExerciseCheckResult checkResult)
        {
            return checkResult.IsCorrect &&
                   !checkResult.IsWrongInput &&
                   checkResult.Completion == null &&
                   UsesArrowLabelRetryStage &&
                   _gamePlay is BitArrayGamePlay;
        }

        private string? BuildCorrectExpressionFeedbackText()
        {
            if (_gamePlay is not BitArrayGamePlay arrowGamePlay)
                return null;

            int start = arrowGamePlay.ArrowLabelAddend1Value;
            int end = arrowGamePlay.ArrowLabelSumValue;
            int distance = arrowGamePlay.ArrowLabelDistanceValue;
            int? middle = arrowGamePlay.ArrowLabelAddend2Value;
            MissingValueTargetFlags missingTarget = arrowGamePlay.CurrentArrowLabelMissingTarget;

            if (_config.KeyboardConfig?.AllowLearnerChosenComplexMiddle == true &&
                IsComplexArrowLabelPromptMode(arrowGamePlay.CurrentArrowLabelExerciseMode))
            {
                string directExpression = $"{start}+{distance}";
                if (TryGetValidLearnerChosenSplit(arrowGamePlay, out int learnerDistance1, out int learnerMiddle, out int learnerDistance2))
                {
                    string decomposedExpression = $"{start}{FormatSignedTerm(learnerDistance1)}{FormatSignedTerm(learnerDistance2)}";
                    return missingTarget is MissingValueTargetFlags.Addend2 or MissingValueTargetFlags.TotalDistance
                        ? $"{end}={decomposedExpression}={directExpression}"
                        : $"{directExpression}={decomposedExpression}={end}";
                }

                return missingTarget is MissingValueTargetFlags.Addend2 or MissingValueTargetFlags.TotalDistance
                    ? $"{end}={directExpression}"
                    : $"{directExpression}={end}";
            }

            if (IsComplexArrowLabelPromptMode(arrowGamePlay.CurrentArrowLabelExerciseMode) &&
                middle.HasValue &&
                middle.Value != start &&
                middle.Value != end)
            {
                if (UsesRtlComplexThroughTenPrompt())
                {
                    int leftDistance = middle.Value - start;
                    int rightDistance = end - middle.Value;
                    string distanceExpression = $"{end}-{start}";
                    string distanceBreakdown = $"{rightDistance}+{leftDistance}";
                    string startExpression = $"{end}-{distance}";
                    string decomposedSubtraction = $"{end}-{rightDistance}-{leftDistance}";
                    return missingTarget is MissingValueTargetFlags.Addend2 or MissingValueTargetFlags.TotalDistance
                        ? $"{distanceExpression}={distanceBreakdown}={distance}"
                        : $"{startExpression}={decomposedSubtraction}={start}";
                }

                int firstDistance = middle.Value - start;
                int secondDistance = end - middle.Value;
                string directExpression = $"{start}+{distance}";
                string decomposedExpression = $"{start}+{firstDistance}+{secondDistance}";
                return missingTarget is MissingValueTargetFlags.Addend2 or MissingValueTargetFlags.TotalDistance
                    ? $"{end}={decomposedExpression}={directExpression}"
                    : $"{directExpression}={decomposedExpression}={end}";
            }

            return $"{start}+{distance}={end}";
        }

        private static string FormatSignedTerm(int value)
        {
            return value < 0 ? value.ToString() : $"+{value}";
        }

        private bool TryGetValidLearnerChosenSplit(
            BitArrayGamePlay arrowGamePlay,
            out int distance1,
            out int middle,
            out int distance2)
        {
            distance1 = 0;
            middle = 0;
            distance2 = 0;

            if (_config.KeyboardConfig?.AllowLearnerChosenComplexMiddle != true ||
                !int.TryParse(_txtAddend2?.Text, out distance1) ||
                !int.TryParse(_txtSum?.Text, out middle) ||
                !int.TryParse(_txtComplexAddend3?.Text, out distance2))
            {
                return false;
            }

            int start = arrowGamePlay.ArrowLabelAddend1Value;
            int end = arrowGamePlay.ArrowLabelSumValue;
            int totalDistance = arrowGamePlay.ArrowLabelDistanceValue;

            return start + distance1 == middle &&
                   middle + distance2 == end &&
                   distance1 + distance2 == totalDistance;
        }

        private Task ShowCorrectExpressionFeedbackAsync()
        {
            EnsureCorrectExpressionLabel();
            string? expressionText = BuildCorrectExpressionFeedbackText();
            if (string.IsNullOrWhiteSpace(expressionText))
                return Task.CompletedTask;

            _correctExpressionLabel.Text = expressionText;
            _correctExpressionLabel.IsVisible = true;
            _isCorrectExpressionLabelVisibleForCurrentExercise = true;
            return Task.CompletedTask;
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
                    labelTextOverride: arrowGamePlay.GetCurrentArrowLabelText(),
                    movementMode: arrowGamePlay.CurrentArrowMovementMode);
                RefreshArrowMovementDebugText();
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
            EnsureCalmAttemptIndicator();

            bool compactVerticalPrecision = _config.KeyboardConfig?.IsVerticalPrecisionPinchExercise == true;
            double progressRowHeight = compactVerticalPrecision ? 50 : 55;
            double statusWidth = compactVerticalPrecision ? 160 : 220;
            if (compactVerticalPrecision && _centerFeedbackBadge != null && _centerFeedbackBadgeLabel != null)
            {
                _centerFeedbackBadge.WidthRequest = statusWidth;
                _centerFeedbackBadge.HeightRequest = progressRowHeight;
                _centerFeedbackBadgeLabel.FontSize = 42;
            }
            Grid statusActionHost = new()
            {
                WidthRequest = statusWidth,
                HeightRequest = progressRowHeight,
                BackgroundColor = Colors.Transparent,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Margin = compactVerticalPrecision ? new Thickness(0, 2) : new Thickness(0, 6),
                InputTransparent = false
            };
            if (_customProgressHost != null)
            {
                _customProgressHost.WidthRequest = statusWidth;
                _customProgressHost.HeightRequest = progressRowHeight;
                statusActionHost.Add(_customProgressHost);
            }
            if (_pianoPressProgress != null)
                _pianoPressProgress.WidthRequest = statusWidth;
            if (_customProgressFill != null)
                _customProgressFill.HeightRequest = progressRowHeight;
            if (_calmAttemptIndicator != null)
                statusActionHost.Add(_calmAttemptIndicator);
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
            bool shouldShowHostedKeyboard = !_isKeyboard ||
                                            !_config.KeyboardConfig.HideMainKeyboard ||
                                            _isArrowLabelRetryHelpVisible ||
                                            IsActiveArrowKeyboardQuestion;
            if (_taskMainHost != null)
                _taskMainHost.IsVisible = shouldShowHostedKeyboard;
            if (_keyboardControlBar != null)
                _keyboardControlBar.IsVisible = shouldShowHostedKeyboard;

            double progressRowHeight = _config.KeyboardConfig?.IsVerticalPrecisionPinchExercise == true ? 50 : 55;
            if (_pianoPressProgress != null)
                _pianoPressProgress.HeightRequest = progressRowHeight;

            if (_customProgressHost != null)
                _customProgressHost.HeightRequest = progressRowHeight;

            if (_customProgressFill != null)
                _customProgressFill.HeightRequest = progressRowHeight;

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

            // Sign learning must first publish the mission and its side direction arrow.
            // Its automatic animation is deferred until both have been refreshed below.
            bool deferPrecisionSignLearningTutorial =
                newExercise &&
                _config.IncludeTutorials &&
                _config.KeyboardConfig?.IsPrecisionSignLearningExercise == true &&
                _gamePlay._questionNumber <= 3;

            // Reset the answer keyboard before publishing the new prompt. Previously the
            // prompt could become visible first, so a fast initial press was accepted and
            // then erased by the later PianoInit call.
            bool preparedAnswerKeyboardEarly = newExercise && _isKeyboard && !_config.FromNumToNum;
            if (preparedAnswerKeyboardEarly)
            {
                PrepareAnswerKeyboardForCurrentExercise();
                SetKeyboardInteractionEnabled(false);
            }

            UpdateStatement();
            RefreshKeyboardArrowPromptView();
            RefreshManagedNumericInputVisibility();
            if (UsesManagedNumericInput || UsesArrowLabelPromptStage())
                LogNumericInputDebug("UpdateView:Begin", $"newExercise={newExercise}, allowInputFocus={allowInputFocus}");
            if (applyUiState)
                ApplyExerciseUiState(newExercise);

            if (generatedExercise != null)
                _lastGeneratedExercise = generatedExercise;

            List<Task> tasks = new();

            if (_btnNext != null)
                _btnNext.IsEnabled = ShouldUseArrowLabelRetryButtons()
                    ? _isPageInteractionEnabled && ShouldEnableArrowLabelRetryNextButton()
                    : _gamePlay.GuessNumber > 0 && !newExercise;
            if (_config.IsHistory) _lblHistory.Text = GenerateHistoryString(_gamePlay.AllHistory.Where(item => item.Sum == _gamePlay.Sum).ToList());
            if (_isThreeTexts && !UsesArrowLabelPromptStage() && (_config.UIQuestionType != UIQuestionType.ThreeAddends || newExercise))
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
                if (_btnNext != null)
                {
                    _btnNext.IsEnabled = ShouldUseArrowLabelRetryButtons()
                        ? _isPageInteractionEnabled && ShouldEnableArrowLabelRetryNextButton()
                        : _gamePlay.GuessNumber > 0 && !newExercise;
                    Console.WriteLine(" _gamePlay.GuessNumber: {0}", _gamePlay.GuessNumber);
                }

                tasks.Add(DelayKeyboardInputAsync(_config.SecondsTillAllowInput));
            }

            if (_isThreeTexts || UsesArrowLabelPromptStage())
            {
                SyncPrimaryEntryEnabledState();
            }

            if (newExercise)
            {
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
                            {
                                _taskMainHost.SetStaticBits(((BitArrayGamePlay)_gamePlay).BitArrayQuestion);
                                _taskMainHost.SetPrecisionPinchGuideVisible(
                                    _config.KeyboardConfig.IsPrecisionPinchExercise &&
                                    _config.KeyboardConfig.ShowPrecisionPinchGuideLine);

                                int memorizeDelay = _config.KeyboardConfig.PrecisionPinchMemorizeDelaySeconds;
                                if (memorizeDelay > 0)
                                {
                                    _pianoKeyboard.InputTransparent = true;
                                    BitArrayGamePlay memorizeGamePlay = (BitArrayGamePlay)_gamePlay;
                                    bool showFullSequence =
                                        _config.KeyboardConfig.IsPrecisionPinchSequenceMemorize &&
                                        memorizeGamePlay.IsSequenceMemorizeFirstResponse();

                                    if (showFullSequence)
                                    {
                                        await Task.Delay(TimeSpan.FromSeconds(memorizeDelay));
                                        if (memorizeGamePlay.ShouldAnimateTwoHandCombinationTransition())
                                        {
                                            bool[] firstPreview = memorizeGamePlay.GetSequenceMemorizeFirstPreview();
                                            _taskMainHost.SetStaticBits(Array.Empty<bool>());
                                            if (memorizeGamePlay.IsTwoHandCombinationFlip())
                                            {
                                                _taskMainHost.SetStaticBits(
                                                    memorizeGamePlay.GetTwoHandCombinationFlipFixedBits());
                                                await _taskMainHost.AnimateFlipAcrossAxisAsync(
                                                    memorizeGamePlay.GetTwoHandCombinationFlipMovingBits(),
                                                    memorizeGamePlay.GetTwoHandCombinationAnimationTargets(),
                                                    memorizeGamePlay.GetTwoHandCombinationFlipAxisSourceIndex(),
                                                    240u,
                                                    memorizeGamePlay.IsTwoHandCombinationFlipAxisAboveSource(),
                                                    Colors.DarkOrange,
                                                    settleMs: 0,
                                                    showLeadIn: false);
                                            }
                                            else
                                            {
                                                await _taskMainHost.AnimateToTargetsAsync(
                                                    firstPreview,
                                                    memorizeGamePlay.GetTwoHandCombinationAnimationTargets(),
                                                    240u,
                                                    Colors.DarkOrange,
                                                    settleMs: 0);
                                            }
                                        }
                                        _taskMainHost.SetStaticBits(memorizeGamePlay.GetSequenceMemorizeSecondPreview());
                                        await Task.Delay(TimeSpan.FromSeconds(memorizeDelay));
                                    }
                                    else if (!_config.KeyboardConfig.IsPrecisionPinchSequenceMemorize)
                                    {
                                        await Task.Delay(TimeSpan.FromSeconds(memorizeDelay));
                                    }

                                    _taskMainHost.SetStaticBits(Array.Empty<bool>());
                                    // Sequence presentation is not a tutorial overlay.
                                    // Explicitly clear every possible blocker before
                                    // enabling multi-touch input on Android tablets.
                                    _taskMainHost.SetTutorialMode(false);
                                    SetKeyboardInteractionEnabled(true);
                                    if (_pianoKeyboard is PianoKeyboardSync memorizeKeyboard)
                                        memorizeKeyboard.NotifyQuestionReadyForInput();
                                    else
                                        _pianoKeyboard.InputTransparent = false;
                                }
                            }
                            if (_config.IncludeTutorials &&
                                _config.KeyboardConfig?.IsPrecisionSignLearningExercise != true &&
                                !deferPrecisionSignLearningTutorial)
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
                if (_lblAction != null) _lblAction.Text = BuildActionTextWithDebug(_lastGeneratedExercise?.ActionText ?? _gamePlay.CurrentOperation.ToDString());
                if (_gamePlay is BitArrayGamePlay shiftGamePlay && _legacyShiftInstructionView != null)
                {
                    bool useVectorShift = shiftGamePlay.CurrentOperation == Operation.MoveBy &&
                                          _config.KeyboardConfig?.IsPrecisionShiftExercise != true;
                    _legacyShiftInstructionView.IsVisible = useVectorShift;
                    if (_lblAction != null)
                        _lblAction.IsVisible = true;
                    if (useVectorShift && _legacyShiftInstructionDrawable != null)
                    {
                        _legacyShiftInstructionDrawable.Delta = shiftGamePlay.moveBydir == Direction.Right
                            ? Math.Max(1, shiftGamePlay.moveByLength)
                            : -Math.Max(1, shiftGamePlay.moveByLength);
                        _legacyShiftInstructionView.Invalidate();
                    }
                }
                if (_gamePlay is BitArrayGamePlay verticalShiftGamePlay &&
                    _config.KeyboardConfig?.IsVerticalPrecisionPinchExercise == true)
                {
                    if (_verticalLeftShiftDrawable != null && _verticalLeftShiftInstruction != null)
                    {
                        _verticalLeftShiftDrawable.Delta = verticalShiftGamePlay.GetPrecisionShiftSideDelta(leftSide: true);
                        _verticalLeftShiftDrawable.BaseAtTop = verticalShiftGamePlay.GetPrecisionShiftSideBaseAtTop(leftSide: true);
                        _verticalLeftShiftDrawable.IsShift = verticalShiftGamePlay.IsPrecisionShiftSideGenericShift(leftSide: true);
                        _verticalLeftShiftInstruction.Invalidate();
                    }
                    if (_verticalRightShiftDrawable != null && _verticalRightShiftInstruction != null)
                    {
                        _verticalRightShiftDrawable.Delta = verticalShiftGamePlay.GetPrecisionShiftSideDelta(leftSide: false);
                        _verticalRightShiftDrawable.BaseAtTop = verticalShiftGamePlay.GetPrecisionShiftSideBaseAtTop(leftSide: false);
                        _verticalRightShiftDrawable.IsShift = verticalShiftGamePlay.IsPrecisionShiftSideGenericShift(leftSide: false);
                        _verticalRightShiftInstruction.Invalidate();
                    }
                }
                if (deferPrecisionSignLearningTutorial && _taskMainHost != null)
                {
                    // Yield once so the mission and side arrow reach the screen before
                    // the tutorial's introductory pause and movement explanation.
                    await Task.Yield();
                    await RunRecordedKeyboardTutorialAsync(_taskMainHost);
                }
                if (preparedAnswerKeyboardEarly &&
                    _isPageVisible &&
                    _config.SecondsTillAllowInput <= 0)
                {
                    // At this point the prompt, direction and question keyboard all match.
                    // No later initialization is allowed to clear this first response.
                    SetPageInteractionEnabled(true);
                    if (_pianoKeyboard is PianoKeyboardSync readySyncKeyboard)
                        readySyncKeyboard.NotifyQuestionReadyForInput();
                }
                RefreshPreviousPreview();
                if (_isKeyboard && !_config.FromNumToNum && !preparedAnswerKeyboardEarly)
                {
                    PrepareAnswerKeyboardForCurrentExercise();
                }
                if (tasks.Count > 0) _ = Task.WhenAll(tasks);

            }
            RefreshKeyboardControlBar();
            if (HasVisibleNumericInputs && allowInputFocus)
            {
                if (UsesManagedNumericInput)
                {
                    // A new two-multiplier exercise must not inherit the second
                    // multiplier selection from the previous exercise. This path is
                    // used by the in-app keypad, so native-focus handling below does
                    // not protect it.
                    bool resetToFirstMultiplier =
                        newExercise &&
                        _config.RequiresBothAddendsInput &&
                        IsEntryEditable(_txtAddend1);
                    Entry? preferredManagedEntry = resetToFirstMultiplier
                        ? _txtAddend1
                        : GetPreferredNumericEntry();
                    if (preferredManagedEntry != null)
                    {
                        LogNumericInputDebug("UpdateView:ManagedFocus", $"target={GetNumericEntryName(preferredManagedEntry)}");
                        if (resetToFirstMultiplier)
                        {
                            // Apply immediately. ForceFocusAsync intentionally delays
                            // normal focus changes, which left a short window where a
                            // fast first keypad press still went to multiplier 2.
                            SelectNumericEntry(preferredManagedEntry);
                            Debug.Assert(ReferenceEquals(_activeNumericEntry, _txtAddend1));
                        }
                        else
                        {
                            await ForceFocusAsync(preferredManagedEntry);
                        }
                    }
                    else
                    {
                        LogNumericInputDebug("UpdateView:ManagedFocus", "target=null");
                    }

                    RefreshNumericEntryAppearance();
                    return;
                }

                if (newExercise && _config.RequiresBothAddendsInput && IsEntryEditable(_txtAddend1))
                {
                    // A fresh two-multiplier question must always begin at multiplier 1.
                    // Keeping _lastFocused from the previous question could reopen the
                    // native keyboard on multiplier 2 and submit an empty first value.
                    _lastFocused = _txtAddend1;
                    await ForceFocusAsync(_txtAddend1, delayMilliseconds: 0);
                }
                else if (_gamePlay.Status == Statement.False ||
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
                if (UsesArrowLabelPromptStage())
                    _config.ArrowLabelFocusIndex = GetArrowLabelFocusIndex(entry);
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

        private void SyncPrimaryEntryEnabledState()
        {
            if (UsesArrowLabelPromptStage())
            {
                Entry? missingEntry = GetArrowLabelMissingEntry();
                LogNumericInputDebug("SyncPrimaryEntryEnabledState:ArrowPrompt", $"missing={GetNumericEntryName(missingEntry)}");
                _config.ArrowLabelFocusIndex = GetArrowLabelFocusIndex(missingEntry);

                bool requireComplexBreakdownEntries =
                    _config.KeyboardConfig.ArrowLabelRetryMode == ArrowLabelRetryMode.RevealComplexThroughTen &&
                    _isComplexThroughTenBreakdownVisible;
                bool requireLearnerChosenMiddle =
                    requireComplexBreakdownEntries &&
                    _config.KeyboardConfig.AllowLearnerChosenComplexMiddle;
                bool requireComplexThroughTenFillOrder =
                    _config.KeyboardConfig.ArrowLabelRetryMode == ArrowLabelRetryMode.None &&
                    UsesComplexThroughTenDistanceInput();
                bool requireComplexNextTenFillOrder =
                    _config.KeyboardConfig.ArrowLabelRetryMode == ArrowLabelRetryMode.None &&
                    UsesComplexNextTenDistanceInput();

                EntryEnabled(_txtAddend1, missingEntry == _txtAddend1);
                EntryEnabled(_txtAddend2, missingEntry == _txtAddend2 || requireComplexBreakdownEntries || requireComplexThroughTenFillOrder || requireComplexNextTenFillOrder);
                EntryEnabled(_txtSum, missingEntry == _txtSum ||
                    requireComplexNextTenFillOrder ||
                    (requireLearnerChosenMiddle && !_isComplexMiddleFilledByHelp));
                if (_txtComplexAddend3 != null) EntryEnabled(_txtComplexAddend3, missingEntry == _txtComplexAddend3);
                if (_txtComplexSum2 != null) EntryEnabled(_txtComplexSum2, missingEntry == _txtComplexSum2);
                if (_txtComplexTotalDistance != null) EntryEnabled(_txtComplexTotalDistance, missingEntry == _txtComplexTotalDistance);
                if (_arrowEquationAnswerEntry != null) EntryEnabled(_arrowEquationAnswerEntry, IsArrowLabelEquationIntroVisible());
                if (_txtComplexAddend3 != null && (requireComplexBreakdownEntries || requireComplexThroughTenFillOrder || requireComplexNextTenFillOrder))
                    EntryEnabled(_txtComplexAddend3, true);

                Entry? preferredEntry = requireComplexBreakdownEntries || requireComplexThroughTenFillOrder || requireComplexNextTenFillOrder
                    ? (UsesRtlComplexThroughTenPrompt() ? _txtComplexAddend3 : _txtAddend2)
                    : missingEntry;

                if (UsesManagedNumericInput && IsEntryEditable(preferredEntry))
                    SelectNumericEntry(preferredEntry);

                LogNumericInputDebug("SyncPrimaryEntryEnabledState:ArrowPromptApplied", $"missing={GetNumericEntryName(missingEntry)}");

                _currentPPW = new PPWObject(_gamePlay.addend1, _gamePlay.addend2, _gamePlay.Sum);
                _currentPPWEnabled = new PPWObject(
                    _txtAddend1.IsEnabled ? 1 : 0,
                    _txtAddend2.IsEnabled ? 1 : 0,
                    _txtSum.IsEnabled ? 1 : 0);
                _currentSecondaryPPW = null;
                _currentSecondaryPPWEnabled = null;
                return;
            }

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

            LogNumericInputDebug("SyncPrimaryEntryEnabledState:DefaultApplied");
        }

        private View InitNumericKeypadUI()
        {
            if (UsesChoiceAnswerKeyboard)
            {
                _choiceAnswerKeyboard = new ChoiceAnswerKeyboardView
                {
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Center,
                    MaxValue = Math.Max(10, _config.MaxSum),
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

        private void RefreshManagedNumericInputVisibility()
        {
            bool showNumericInput = HasVisibleNumericInputs;

            if (_choiceAnswerKeyboard != null)
                _choiceAnswerKeyboard.IsVisible = showNumericInput && EffectiveNumericInputMode == NumericInputMode.ChoiceKeyboard;

            if (_numericKeypad != null)
                _numericKeypad.IsVisible = showNumericInput && EffectiveNumericInputMode == NumericInputMode.AppKeypad;
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
            double compactQuestionWidth = _config.UIQuestionType == UIQuestionType.ThreeTexts
                ? 120
                : TASK_WIDTH;

            DisplayInfo info = DeviceDisplay.Current.MainDisplayInfo;
            double width = info.Density > 0 ? info.Width / info.Density : info.Width;
            double height = info.Density > 0 ? info.Height / info.Density : info.Height;
            double shortSide = Math.Min(width, height);

            if (ShouldPlaceNumericKeypadBesideEntriesForHelp())
            {
                double availableWidth = shortSide > 0 ? shortSide - 44 : compactQuestionWidth;
                return Math.Max(150, Math.Min(availableWidth * 0.42, compactQuestionWidth));
            }

            return compactQuestionWidth;
        }

        private double GetQuestionActionWidth()
        {
            return 20;
        }

        private double GetQuestionHalfWidth(bool reserveActionLabel)
        {
            double halfWidth = GetQuestionLayoutWidth() / 2;
            if (reserveActionLabel)
                halfWidth -= GetQuestionActionWidth() / 2;

            double minimumWidth = _config.UIQuestionType == UIQuestionType.ThreeTexts ? 54 : 72;
            return Math.Max(minimumWidth, halfWidth);
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

        private string GetNumericEntryName(Entry? entry)
        {
            if (entry == null)
                return "null";
            if (ReferenceEquals(entry, _txtAddend1))
                return "Addend1";
            if (ReferenceEquals(entry, _txtAddend2))
                return "Addend2";
            if (ReferenceEquals(entry, _txtSum))
                return "Sum";
            if (ReferenceEquals(entry, _txtComplexAddend3))
                return "ComplexAddend3";
            if (ReferenceEquals(entry, _txtComplexSum2))
                return "ComplexSum2";
            if (ReferenceEquals(entry, _txtComplexTotalDistance))
                return "ComplexTotalDistance";

            int helperIndex = Array.IndexOf(txt ?? Array.Empty<Entry>(), entry);
            if (helperIndex >= 0)
                return $"Helper[{helperIndex}]";

            int index = _numericEntries.IndexOf(entry);
            return index >= 0 ? $"Entry[{index}]" : "UnknownEntry";
        }

        private int GetArrowLabelFocusIndex(Entry? entry)
        {
            if (entry == null)
                return -1;
            if (ReferenceEquals(entry, _txtAddend1))
                return 0;
            if (ReferenceEquals(entry, _txtAddend2) || ReferenceEquals(entry, _txtComplexTotalDistance))
                return 1;
            if (ReferenceEquals(entry, _txtSum) || ReferenceEquals(entry, _txtComplexSum2))
                return 2;
            if (ReferenceEquals(entry, _txtComplexAddend3))
                return 3;

            return _numericEntries.IndexOf(entry);
        }

        private string FormatNumericEntryState(Entry? entry)
        {
            if (entry == null)
                return "null";

            string text = entry.Text ?? string.Empty;
            return $"{GetNumericEntryName(entry)}(enabled={entry.IsEnabled},visible={entry.IsVisible},text='{text}')";
        }

        private void LogNumericInputDebug(string eventName, string details = "")
        {
            if (!EnableNumericInputDebug)
                return;

            ArrowLabelExerciseMode mode = GetDisplayedArrowLabelExerciseMode();
            Entry? missingEntry = GetArrowLabelMissingEntry();
            string message =
                $"[NUMPADDBG] {eventName} | mode={mode} | missing={GetNumericEntryName(missingEntry)} | " +
                $"active={GetNumericEntryName(_activeNumericEntry)} | last={GetNumericEntryName(_lastFocused)} | " +
                $"A1={FormatNumericEntryState(_txtAddend1)} | A2={FormatNumericEntryState(_txtAddend2)} | Sum={FormatNumericEntryState(_txtSum)}";

            if (!string.IsNullOrWhiteSpace(details))
                message += $" | {details}";

            Debug.WriteLine(message);
            Console.WriteLine(message);
        }

        private string BuildActionTextWithDebug(string actionText)
        {
            if (_gamePlay is not BitArrayGamePlay arrowGamePlay ||
                _config.KeyboardConfig?.AllowedArrowMovementModes == ArrowMovementModeFlags.None)
            {
                return actionText;
            }

            List<string> debugLines = new();
            if (!string.IsNullOrWhiteSpace(arrowGamePlay.LastArrowMovementDebugText))
                debugLines.Add(arrowGamePlay.LastArrowMovementDebugText);
            if (!string.IsNullOrWhiteSpace(_pianoKeyboard?.LastArrowDrawingDebugText))
                debugLines.Add(_pianoKeyboard.LastArrowDrawingDebugText);

            return debugLines.Count == 0
                ? actionText
                : $"{actionText}\n{string.Join("\n", debugLines)}";
        }

        private void RefreshArrowMovementDebugText()
        {
            if (_lblAction == null)
                return;

            string baseActionText = _lastGeneratedExercise?.ActionText ?? _gamePlay.CurrentOperation.ToDString();
            _lblAction.Text = BuildActionTextWithDebug(baseActionText);
        }

        private bool IsEntryEditable(Entry? entry)
        {
            return entry != null && entry.IsVisible && entry.IsEnabled;
        }

        private void SelectNumericEntry(Entry? entry)
        {
            if (!UsesManagedNumericInput)
                return;

            LogNumericInputDebug("SelectNumericEntry:Requested", $"target={GetNumericEntryName(entry)}");

            if (!IsEntryEditable(entry))
            {
                LogNumericInputDebug("SelectNumericEntry:Rejected", $"target={GetNumericEntryName(entry)} not editable");
                RefreshNumericEntryAppearance();
                return;
            }

            _activeNumericEntry = entry;
            _lastFocused = entry;
            if (UsesArrowLabelPromptStage())
                _config.ArrowLabelFocusIndex = GetArrowLabelFocusIndex(entry);
            LogNumericInputDebug("SelectNumericEntry:Applied", $"target={GetNumericEntryName(entry)}");
            RefreshNumericEntryAppearance();
        }

        private Entry? EnsureNumericEntrySelection()
        {
            if (UsesComplexThroughTenBreakdownInput() || UsesComplexThroughTenDistanceInput())
            {
                if ((ReferenceEquals(_activeNumericEntry, _txtAddend2) ||
                     (UsesLearnerChosenComplexMiddle() && ReferenceEquals(_activeNumericEntry, _txtSum)) ||
                     ReferenceEquals(_activeNumericEntry, _txtComplexAddend3)) &&
                    IsEntryEditable(_activeNumericEntry))
                {
                    LogNumericInputDebug("EnsureNumericEntrySelection:UseActiveComplexThroughTen", $"target={GetNumericEntryName(_activeNumericEntry)}");
                    return _activeNumericEntry;
                }

                Entry? complexThroughTenEntry = GetCurrentComplexThroughTenEntry();
                if (IsEntryEditable(complexThroughTenEntry) && !ReferenceEquals(_activeNumericEntry, complexThroughTenEntry))
                {
                    LogNumericInputDebug("EnsureNumericEntrySelection:UseComplexThroughTen", $"target={GetNumericEntryName(complexThroughTenEntry)}");
                    SelectNumericEntry(complexThroughTenEntry);
                    return complexThroughTenEntry;
                }
            }

            if (IsEntryEditable(_activeNumericEntry))
            {
                LogNumericInputDebug("EnsureNumericEntrySelection:UseActive", $"target={GetNumericEntryName(_activeNumericEntry)}");
                return _activeNumericEntry;
            }

            Entry? preferredEntry = GetPreferredNumericEntry();
            if (preferredEntry != null)
            {
                LogNumericInputDebug("EnsureNumericEntrySelection:UsePreferred", $"target={GetNumericEntryName(preferredEntry)}");
                SelectNumericEntry(preferredEntry);
            }
            else
            {
                LogNumericInputDebug("EnsureNumericEntrySelection:NoTarget");
            }

            return preferredEntry;
        }

        private Entry? GetPreferredNumericEntry()
        {
            if (UsesComplexThroughTenBreakdownInput() || UsesComplexThroughTenDistanceInput())
            {
                Entry? complexThroughTenEntry = GetCurrentComplexThroughTenEntry();
                if (IsEntryEditable(complexThroughTenEntry))
                {
                    LogNumericInputDebug("GetPreferredNumericEntry:ComplexThroughTen", $"target={GetNumericEntryName(complexThroughTenEntry)}");
                    return complexThroughTenEntry;
                }
            }

            if (UsesComplexNextTenDistanceInput())
            {
                Entry? complexNextEntry = GetCurrentComplexNextTenEntry();
                if (IsEntryEditable(complexNextEntry))
                {
                    LogNumericInputDebug("GetPreferredNumericEntry:ComplexNextTen", $"target={GetNumericEntryName(complexNextEntry)}");
                    return complexNextEntry;
                }
            }

            Entry? arrowLabelMissingEntry = GetArrowLabelMissingEntry();
            if (IsEntryEditable(arrowLabelMissingEntry))
            {
                LogNumericInputDebug("GetPreferredNumericEntry:ArrowMissing", $"target={GetNumericEntryName(arrowLabelMissingEntry)}");
                return arrowLabelMissingEntry;
            }

            if (IsEntryEditable(_lastFocused))
            {
                LogNumericInputDebug("GetPreferredNumericEntry:LastFocused", $"target={GetNumericEntryName(_lastFocused)}");
                return _lastFocused;
            }

            if (IsEntryEditable(_txtSum) && _gamePlay.Sum == PPWGamePlay.NAN)
            {
                LogNumericInputDebug("GetPreferredNumericEntry:Sum", $"target={GetNumericEntryName(_txtSum)}");
                return _txtSum;
            }

            if (IsEntryEditable(_txtAddend1) && _gamePlay.addend1 == PPWGamePlay.NAN)
            {
                LogNumericInputDebug("GetPreferredNumericEntry:Addend1", $"target={GetNumericEntryName(_txtAddend1)}");
                return _txtAddend1;
            }

            if (IsEntryEditable(_txtAddend2) && _gamePlay.addend2 == PPWGamePlay.NAN)
            {
                LogNumericInputDebug("GetPreferredNumericEntry:Addend2", $"target={GetNumericEntryName(_txtAddend2)}");
                return _txtAddend2;
            }

            Entry? fallback = _numericEntries.FirstOrDefault(IsEntryEditable);
            LogNumericInputDebug("GetPreferredNumericEntry:Fallback", $"target={GetNumericEntryName(fallback)}");
            return fallback;
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
            bool shouldOnlyColorMissingCorrectAnswer =
                ShouldUseArrowLabelRetryButtons() &&
                _gamePlay.Status == Statement.True;
            Entry? correctAnswerEntry = shouldOnlyColorMissingCorrectAnswer
                ? GetArrowLabelMissingEntry()
                : null;

            foreach (Entry numericEntry in _numericEntries)
            {
                if (shouldOnlyColorMissingCorrectAnswer && ReferenceEquals(numericEntry, correctAnswerEntry))
                {
                    numericEntry.BackgroundColor = Colors.LightGreen;
                    numericEntry.TextColor = Colors.Black;
                    ResetNumericEntryTransform(numericEntry);
                }
                else if (!numericEntry.IsEnabled)
                {
                    numericEntry.BackgroundColor = DesignResources.GetColor("GameNumericEntryBackgroundColor", Colors.White);
                    numericEntry.TextColor = DesignResources.GetColor("GameNumericEntryDisabledTextColor", Colors.Gray);
                    ResetNumericEntryTransform(numericEntry);
                }
                else if (_complexPromptEntryValidationStates.TryGetValue(numericEntry, out bool isValid) && !isValid)
                {
                    numericEntry.BackgroundColor = Colors.IndianRed;
                    numericEntry.TextColor = Colors.Black;
                    ResetNumericEntryTransform(numericEntry);
                }
                else if (numericEntry == _activeNumericEntry && UsesManagedNumericInput)
                {
                    numericEntry.BackgroundColor = DesignResources.GetColor("GameNumericEntryActiveBackgroundColor", Color.FromArgb("#FFF9D6"));
                    numericEntry.TextColor = DesignResources.GetColor("GameNumericEntryTextColor", Colors.Black);
                    ResetNumericEntryTransform(numericEntry);
                }
                else if (_complexPromptEntryValidationStates.TryGetValue(numericEntry, out isValid))
                {
                    numericEntry.BackgroundColor = shouldOnlyColorMissingCorrectAnswer && !ReferenceEquals(numericEntry, correctAnswerEntry)
                        ? DesignResources.GetColor("GameNumericEntryBackgroundColor", Colors.White)
                        : Colors.LightGreen;
                    numericEntry.TextColor = Colors.Black;
                    ResetNumericEntryTransform(numericEntry);
                }
                else
                {
                    numericEntry.BackgroundColor = DesignResources.GetColor("GameNumericEntryBackgroundColor", Colors.White);
                    numericEntry.TextColor = DesignResources.GetColor("GameNumericEntryTextColor", Colors.Black);
                    ResetNumericEntryTransform(numericEntry);
                }
            }

            RefreshArrowPromptActiveEntryAppearance();
        }

        private void RefreshArrowPromptActiveEntryAppearance()
        {
            if (!UsesArrowLabelPromptStage() || !UsesManagedNumericInput)
                return;

            bool shouldOnlyColorMissingCorrectAnswer =
                ShouldUseArrowLabelRetryButtons() &&
                _gamePlay.Status == Statement.True;
            Entry? correctAnswerEntry = shouldOnlyColorMissingCorrectAnswer
                ? GetArrowLabelMissingEntry()
                : null;

            Entry?[] promptEntries =
            {
                _txtAddend1,
                _txtAddend2,
                _txtSum,
                _txtComplexAddend3,
                _txtComplexSum2,
                _txtComplexTotalDistance
            };

            foreach (Entry? promptEntry in promptEntries)
            {
                if (promptEntry == null || _numericEntries.Contains(promptEntry))
                    continue;

                if (shouldOnlyColorMissingCorrectAnswer && ReferenceEquals(promptEntry, correctAnswerEntry))
                {
                    promptEntry.BackgroundColor = Colors.LightGreen;
                    promptEntry.TextColor = Colors.Black;
                }
                else if (!promptEntry.IsEnabled)
                {
                    promptEntry.BackgroundColor = DesignResources.GetColor("GameNumericEntryBackgroundColor", Colors.White);
                    promptEntry.TextColor = DesignResources.GetColor("GameNumericEntryDisabledTextColor", Colors.Gray);
                }
                else if (_complexPromptEntryValidationStates.TryGetValue(promptEntry, out bool isValid) && !isValid)
                {
                    promptEntry.BackgroundColor = Colors.IndianRed;
                    promptEntry.TextColor = Colors.Black;
                }
                else if (promptEntry == _activeNumericEntry)
                {
                    promptEntry.BackgroundColor = DesignResources.GetColor("GameNumericEntryActiveBackgroundColor", Color.FromArgb("#FFF9D6"));
                    promptEntry.TextColor = DesignResources.GetColor("GameNumericEntryTextColor", Colors.Black);
                }
                else if (_complexPromptEntryValidationStates.TryGetValue(promptEntry, out isValid))
                {
                    promptEntry.BackgroundColor = shouldOnlyColorMissingCorrectAnswer && !ReferenceEquals(promptEntry, correctAnswerEntry)
                        ? DesignResources.GetColor("GameNumericEntryBackgroundColor", Colors.White)
                        : Colors.LightGreen;
                    promptEntry.TextColor = Colors.Black;
                }
                else
                {
                    promptEntry.BackgroundColor = DesignResources.GetColor("GameNumericEntryBackgroundColor", Colors.White);
                    promptEntry.TextColor = DesignResources.GetColor("GameNumericEntryTextColor", Colors.Black);
                }
            }
        }

        private void OnNumericDigitPressed(string digit)
        {
            Entry? targetEntry = EnsureNumericEntrySelection();
            if (targetEntry == null)
                return;

            _complexPromptEntryValidationStates.Remove(targetEntry);
            LogNumericInputDebug("OnNumericDigitPressed", $"digit='{digit}' target={GetNumericEntryName(targetEntry)}");
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

            _complexPromptEntryValidationStates.Remove(targetEntry);
            LogNumericInputDebug("OnChoiceAnswerPressed", $"value={value} target={GetNumericEntryName(targetEntry)}");
            _lastChoiceAnswerValue = value;
            targetEntry.Text = value.ToString();
            _lastFocused = targetEntry;
            RefreshNumericEntryAppearance();
            OnNumericSubmitPressed();
        }

        private void OnNumericBackspacePressed()
        {
            Entry? targetEntry = EnsureNumericEntrySelection();
            if (targetEntry == null || string.IsNullOrEmpty(targetEntry.Text))
                return;

            _complexPromptEntryValidationStates.Remove(targetEntry);
            LogNumericInputDebug("OnNumericBackspacePressed", $"target={GetNumericEntryName(targetEntry)}");
            targetEntry.Text = targetEntry.Text[..^1];
            _lastFocused = targetEntry;
            RefreshNumericEntryAppearance();
        }

        private void OnNumericClearPressed()
        {
            Entry? targetEntry = EnsureNumericEntrySelection();
            if (targetEntry == null)
                return;

            _complexPromptEntryValidationStates.Remove(targetEntry);
            LogNumericInputDebug("OnNumericClearPressed", $"target={GetNumericEntryName(targetEntry)}");
            targetEntry.Text = string.Empty;
            _lastFocused = targetEntry;
            RefreshNumericEntryAppearance();
        }

        private void OnNumericSubmitPressed()
        {
            Entry? targetEntry = EnsureNumericEntrySelection();
            if (targetEntry == null)
                return;

            LogNumericInputDebug("OnNumericSubmitPressed", $"target={GetNumericEntryName(targetEntry)}");
            if (UsesComplexNextTenDistanceInput())
            {
                if (!ValidateComplexPromptEntry(targetEntry))
                    return;

                Entry? nextComplexEntry = GetNextComplexNextTenEntry(targetEntry);
                if (nextComplexEntry != null)
                {
                    SelectNumericEntry(nextComplexEntry);
                    return;
                }

                CheckGamePlay();
                return;
            }

            if (UsesComplexThroughTenBreakdownInput() || UsesComplexThroughTenDistanceInput())
            {
                if (!ValidateComplexPromptEntry(targetEntry))
                    return;

                Entry? nextComplexEntry = GetNextComplexThroughTenEntry(targetEntry);
                if (nextComplexEntry != null)
                {
                    SelectNumericEntry(nextComplexEntry);
                    return;
                }

                CheckGamePlay();
                return;
            }

            if (UsesArrowLabelPromptStage() && targetEntry == GetArrowLabelMissingEntry())
            {
                CheckGamePlay();
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

        private bool ValidateComplexPromptEntry(Entry entry)
        {
            if (_gamePlay is not BitArrayGamePlay arrowPromptGamePlay ||
                !IsComplexArrowLabelPromptMode(GetDisplayedArrowLabelExerciseMode()))
            {
                return true;
            }

            if (UsesLearnerChosenComplexMiddle())
                return ValidateLearnerChosenComplexMiddleEntry(entry, arrowPromptGamePlay);

            int start = arrowPromptGamePlay.ArrowLabelAddend1Value;
            int end = arrowPromptGamePlay.ArrowLabelSumValue;
            int middle = arrowPromptGamePlay.ArrowLabelAddend2Value ?? 10;
            bool isRtl = UsesRtlComplexThroughTenPrompt();
            int? expectedValue = null;

            if (ReferenceEquals(entry, _txtAddend1))
                expectedValue = start;
            else if (ReferenceEquals(entry, _txtAddend2))
                expectedValue = middle - start;
            else if (ReferenceEquals(entry, _txtSum))
                expectedValue = middle;
            else if (ReferenceEquals(entry, _txtComplexAddend3))
                expectedValue = end - middle;
            else if (ReferenceEquals(entry, _txtComplexSum2))
                expectedValue = end;
            else if (ReferenceEquals(entry, _txtComplexTotalDistance))
                expectedValue = arrowPromptGamePlay.ArrowLabelDistanceValue;

            if (!expectedValue.HasValue)
                return true;

            bool isCorrect = int.TryParse(entry.Text, out int value) && value == expectedValue.Value;
            _complexPromptEntryValidationStates[entry] = isCorrect;
            _ = PersistComplexArrowAttemptAsync(entry, isCorrect);
            SelectNumericEntry(entry);
            RefreshNumericEntryAppearance();
            if (!isCorrect)
            {
                entry.BackgroundColor = Colors.IndianRed;
                entry.TextColor = Colors.Black;
            }

            return isCorrect;
        }

        private bool ValidateLearnerChosenComplexMiddleEntry(Entry entry, BitArrayGamePlay arrowPromptGamePlay)
        {
            int start = arrowPromptGamePlay.ArrowLabelAddend1Value;
            int end = arrowPromptGamePlay.ArrowLabelSumValue;
            int totalDistance = arrowPromptGamePlay.ArrowLabelDistanceValue;

            bool hasDistance1 = int.TryParse(_txtAddend2?.Text, out int distance1);
            bool hasMiddle = int.TryParse(_txtSum?.Text, out int middle);
            bool hasDistance2 = int.TryParse(_txtComplexAddend3?.Text, out int distance2);
            bool hasTotalDistance = int.TryParse(_txtComplexTotalDistance?.Text, out int submittedTotalDistance);
            bool hasEnd = int.TryParse(_txtComplexSum2?.Text, out int submittedEnd);

            bool IsValidDistance1()
            {
                if (!hasDistance1)
                    return false;
                if (hasMiddle && start + distance1 != middle)
                    return false;
                if (hasDistance2 && distance1 + distance2 != totalDistance)
                    return false;
                return true;
            }

            bool IsValidMiddle()
            {
                if (!hasMiddle)
                    return false;
                if (hasDistance1 && start + distance1 != middle)
                    return false;
                if (hasDistance2 && middle + distance2 != end)
                    return false;
                return true;
            }

            bool IsValidDistance2()
            {
                if (!hasDistance2)
                    return false;
                if (hasMiddle && middle + distance2 != end)
                    return false;
                if (hasDistance1 && distance1 + distance2 != totalDistance)
                    return false;
                return true;
            }

            bool IsValidTotalDistance()
            {
                return hasTotalDistance && submittedTotalDistance == totalDistance;
            }

            bool IsValidEnd()
            {
                return hasEnd && submittedEnd == end;
            }

            void MarkIfFilled(Entry? candidate, bool isValid)
            {
                if (candidate == null || string.IsNullOrWhiteSpace(candidate.Text))
                    return;

                _complexPromptEntryValidationStates[candidate] = isValid;
                if (!isValid)
                {
                    candidate.BackgroundColor = Colors.IndianRed;
                    candidate.TextColor = Colors.Black;
                }
            }

            if (ReferenceEquals(entry, _txtAddend2))
                MarkIfFilled(_txtAddend2, IsValidDistance1());
            if (ReferenceEquals(entry, _txtSum) || hasDistance1)
                MarkIfFilled(_txtAddend2, IsValidDistance1());
            if (ReferenceEquals(entry, _txtSum))
                MarkIfFilled(_txtSum, IsValidMiddle());
            if (ReferenceEquals(entry, _txtComplexAddend3) || hasMiddle)
                MarkIfFilled(_txtSum, IsValidMiddle());
            if (ReferenceEquals(entry, _txtComplexAddend3))
                MarkIfFilled(_txtComplexAddend3, IsValidDistance2());
            if (ReferenceEquals(entry, _txtComplexTotalDistance))
                MarkIfFilled(_txtComplexTotalDistance, IsValidTotalDistance());
            if (ReferenceEquals(entry, _txtComplexSum2))
                MarkIfFilled(_txtComplexSum2, IsValidEnd());

            bool isCorrect = entry switch
            {
                _ when ReferenceEquals(entry, _txtAddend2) => IsValidDistance1(),
                _ when ReferenceEquals(entry, _txtSum) => IsValidMiddle(),
                _ when ReferenceEquals(entry, _txtComplexAddend3) => IsValidDistance2(),
                _ when ReferenceEquals(entry, _txtComplexTotalDistance) => IsValidTotalDistance(),
                _ when ReferenceEquals(entry, _txtComplexSum2) => IsValidEnd(),
                _ => true
            };

            _complexPromptEntryValidationStates[entry] = isCorrect;
            _ = PersistComplexArrowAttemptAsync(entry, isCorrect);
            SelectNumericEntry(entry);
            RefreshNumericEntryAppearance();
            if (!isCorrect)
            {
                entry.BackgroundColor = Colors.IndianRed;
                entry.TextColor = Colors.Black;
            }

            return isCorrect;
        }

        private async Task PersistComplexArrowAttemptAsync(Entry entry, bool isCorrect)
        {
            if (_questionAnswerPartRepository == null ||
                _gamePlay is not BitArrayGamePlay ||
                !TryGetComplexArrowAttemptInfo(entry, out string entryName, out int columnIndex))
            {
                return;
            }

            string valueText = entry.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(valueText))
                return;

            try
            {
                await _questionAnswerPartRepository.AddComplexArrowAttemptAsync(
                    _gamePlay.GameId.ToString(),
                    _gamePlay._questionNumber,
                    Math.Max(1, _gamePlay.GuessNumber + 1),
                    entryName,
                    columnIndex,
                    valueText,
                    isCorrect);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ArrowLabelAttempt] Failed to save {entryName}: {ex}");
            }
        }

        private bool TryGetComplexArrowAttemptInfo(Entry entry, out string entryName, out int columnIndex)
        {
            if (ReferenceEquals(entry, _txtAddend2))
            {
                entryName = "distance1";
                columnIndex = 0;
                return true;
            }

            if (ReferenceEquals(entry, _txtSum))
            {
                entryName = "middle";
                columnIndex = 1;
                return true;
            }

            if (ReferenceEquals(entry, _txtComplexAddend3))
            {
                entryName = "distance2";
                columnIndex = 2;
                return true;
            }

            if (ReferenceEquals(entry, _txtComplexTotalDistance))
            {
                entryName = "totalDistance";
                columnIndex = 3;
                return true;
            }

            if (ReferenceEquals(entry, _txtComplexSum2))
            {
                entryName = "sum";
                columnIndex = 4;
                return true;
            }

            entryName = string.Empty;
            columnIndex = 0;
            return false;
        }

        private bool UsesComplexThroughTenBreakdownInput()
        {
            return UsesArrowLabelPromptStage() &&
                   _config.KeyboardConfig?.ArrowLabelRetryMode == ArrowLabelRetryMode.RevealComplexThroughTen &&
                   _isComplexThroughTenBreakdownVisible;
        }

        private bool UsesLearnerChosenComplexMiddle()
        {
            return UsesComplexThroughTenBreakdownInput() &&
                   _config.KeyboardConfig?.AllowLearnerChosenComplexMiddle == true;
        }

        private bool CanUseSecondHelpForLearnerMiddle()
        {
            if (!UsesComplexThroughTenBreakdownInput() ||
                _txtSum == null ||
                !string.IsNullOrWhiteSpace(_txtSum.Text) ||
                _gamePlay.Status == Statement.True)
            {
                return false;
            }

            if (UsesLearnerChosenComplexMiddle())
                return true;

            return _gamePlay is BitArrayGamePlay arrowGamePlay &&
                   arrowGamePlay.ArrowLabelAddend2Value.HasValue;
        }

        private bool TryGetRoundMiddleBetweenStartAndEnd(out int roundMiddle)
        {
            roundMiddle = 0;
            if (_gamePlay is not BitArrayGamePlay arrowGamePlay)
                return false;

            int start = arrowGamePlay.ArrowLabelAddend1Value;
            int end = arrowGamePlay.ArrowLabelSumValue;
            int min = Math.Min(start, end);
            int max = Math.Max(start, end);
            int candidate = ((min / 10) + 1) * 10;
            if (candidate >= max)
                return false;

            roundMiddle = candidate;
            return true;
        }

        private bool TryFillLearnerChosenRoundMiddle()
        {
            if (!CanUseSecondHelpForLearnerMiddle() ||
                _txtSum == null ||
                _gamePlay is not BitArrayGamePlay arrowGamePlay ||
                !TryGetSecondHelpMiddleValue(arrowGamePlay, out int roundMiddle))
            {
                return false;
            }

            _txtSum.Text = roundMiddle.ToString();
            _isComplexMiddleFilledByHelp = true;
            EntryEnabled(_txtSum, false);
            _complexPromptEntryValidationStates.Remove(_txtSum);

            if (UsesLearnerChosenComplexMiddle())
                ValidateLearnerChosenComplexMiddleEntry(_txtSum, arrowGamePlay);
            else
                _complexPromptEntryValidationStates[_txtSum] = true;

            Entry? nextEntry = GetNextComplexThroughTenEntry(_txtSum);
            if (IsEntryEditable(nextEntry))
                SelectNumericEntry(nextEntry);
            else
                SelectNumericEntry(_txtSum);

            RefreshNumericEntryAppearance();
            if (_btnArrowLabelRetryHelp != null)
                _btnArrowLabelRetryHelp.IsEnabled = false;
            return true;
        }

        private bool TryGetSecondHelpMiddleValue(BitArrayGamePlay arrowGamePlay, out int middleValue)
        {
            if (UsesLearnerChosenComplexMiddle())
                return TryGetRoundMiddleBetweenStartAndEnd(out middleValue);

            if (arrowGamePlay.ArrowLabelAddend2Value.HasValue)
            {
                middleValue = arrowGamePlay.ArrowLabelAddend2Value.Value;
                return true;
            }

            middleValue = 0;
            return false;
        }

        private bool UsesComplexThroughTenDistanceInput()
        {
            return UsesArrowLabelPromptStage() &&
                   _config.KeyboardConfig?.ArrowLabelRetryMode == ArrowLabelRetryMode.None &&
                   GetDisplayedArrowLabelExerciseMode() == ArrowLabelExerciseMode.ComplexBridgeToNextTen;
        }

        private bool UsesComplexNextTenDistanceInput()
        {
            return UsesArrowLabelPromptStage() &&
                   _config.KeyboardConfig?.ArrowLabelRetryMode == ArrowLabelRetryMode.None &&
                   GetDisplayedArrowLabelExerciseMode() == ArrowLabelExerciseMode.ComplexBridgeToAnyNextTen;
        }

        private bool UsesRtlComplexThroughTenPrompt()
        {
            if (_gamePlay is BitArrayGamePlay arrowGamePlay)
                return arrowGamePlay.UsesRtlComplexPrompt;

            return IsComplexArrowLabelPromptMode(GetDisplayedArrowLabelExerciseMode()) &&
                   (_config.GameName?.Contains("rtl complex", StringComparison.OrdinalIgnoreCase) ?? false);
        }

        private Entry? GetNextComplexNextTenEntry(Entry currentEntry)
        {
            List<Entry> entries = new();

            if (_txtAddend2 != null)
                entries.Add(_txtAddend2);
            if (_txtSum != null)
                entries.Add(_txtSum);
            if (_txtComplexAddend3 != null)
                entries.Add(_txtComplexAddend3);

            Entry? missingEntry = GetArrowLabelMissingEntry();
            if (missingEntry != null)
                entries.Add(missingEntry);

            entries = entries
                .Where(IsEntryEditable)
                .Distinct()
                .ToList();

            int currentIndex = entries.IndexOf(currentEntry);
            if (currentIndex < 0)
                return entries.FirstOrDefault();

            return currentIndex + 1 < entries.Count ? entries[currentIndex + 1] : null;
        }

        private Entry? GetCurrentComplexNextTenEntry()
        {
            if (IsEntryEditable(_txtAddend2) && string.IsNullOrWhiteSpace(_txtAddend2.Text))
                return _txtAddend2;

            if (IsEntryEditable(_txtSum) && string.IsNullOrWhiteSpace(_txtSum.Text))
                return _txtSum;

            if (IsEntryEditable(_txtComplexAddend3) && string.IsNullOrWhiteSpace(_txtComplexAddend3.Text))
                return _txtComplexAddend3;

            Entry? missingEntry = GetArrowLabelMissingEntry();
            return IsEntryEditable(missingEntry) ? missingEntry : null;
        }

        private Entry? GetCurrentComplexThroughTenEntry()
        {
            if (UsesLearnerChosenComplexMiddle())
            {
                if (IsEntryEditable(_txtAddend2) && string.IsNullOrWhiteSpace(_txtAddend2.Text))
                    return _txtAddend2;

                if (IsEntryEditable(_txtSum) && string.IsNullOrWhiteSpace(_txtSum.Text))
                    return _txtSum;

                if (IsEntryEditable(_txtComplexAddend3) && string.IsNullOrWhiteSpace(_txtComplexAddend3.Text))
                    return _txtComplexAddend3;

                Entry? learnerMissingEntry = GetArrowLabelMissingEntry();
                return IsEntryEditable(learnerMissingEntry) ? learnerMissingEntry : null;
            }

            if (UsesRtlComplexThroughTenPrompt())
            {
                if (IsEntryEditable(_txtComplexAddend3) && string.IsNullOrWhiteSpace(_txtComplexAddend3.Text))
                    return _txtComplexAddend3;

                if (IsEntryEditable(_txtAddend2) && string.IsNullOrWhiteSpace(_txtAddend2.Text))
                    return _txtAddend2;

                Entry? rtlMissingEntry = GetArrowLabelMissingEntry();
                return IsEntryEditable(rtlMissingEntry) ? rtlMissingEntry : null;
            }

            if (IsEntryEditable(_txtAddend2) && string.IsNullOrWhiteSpace(_txtAddend2.Text))
                return _txtAddend2;

            if (IsEntryEditable(_txtComplexAddend3) && string.IsNullOrWhiteSpace(_txtComplexAddend3.Text))
                return _txtComplexAddend3;

            Entry? missingEntry = GetArrowLabelMissingEntry();
            return IsEntryEditable(missingEntry) ? missingEntry : null;
        }

        private void SelectFirstComplexThroughTenBreakdownEntry()
        {
            Entry? firstEntry = UsesRtlComplexThroughTenPrompt() ? _txtComplexAddend3 : _txtAddend2;
            if (!UsesManagedNumericInput || !UsesComplexThroughTenBreakdownInput() || !IsEntryEditable(firstEntry))
                return;

            _txtAddend2.Text = string.Empty;
            if (UsesLearnerChosenComplexMiddle() && _txtSum != null)
                _txtSum.Text = string.Empty;
            if (_txtComplexAddend3 != null)
                _txtComplexAddend3.Text = string.Empty;

            _complexPromptEntryValidationStates.Remove(_txtAddend2);
            if (_txtSum != null)
                _complexPromptEntryValidationStates.Remove(_txtSum);
            if (_txtComplexAddend3 != null)
                _complexPromptEntryValidationStates.Remove(_txtComplexAddend3);

            SelectNumericEntry(firstEntry);
        }

        private Entry? GetNextComplexThroughTenEntry(Entry currentEntry)
        {
            List<Entry> entries = new();
            Entry? missingEntry = GetArrowLabelMissingEntry();

            if (UsesLearnerChosenComplexMiddle())
            {
                if (_txtAddend2 != null)
                    entries.Add(_txtAddend2);
                if (_txtSum != null)
                    entries.Add(_txtSum);
                if (_txtComplexAddend3 != null)
                    entries.Add(_txtComplexAddend3);
            }
            else if (UsesRtlComplexThroughTenPrompt())
            {
                if (_txtComplexAddend3 != null)
                    entries.Add(_txtComplexAddend3);
                if (_txtAddend2 != null)
                    entries.Add(_txtAddend2);
            }
            else
            {
                if (_txtAddend2 != null)
                    entries.Add(_txtAddend2);
                if (_txtComplexAddend3 != null)
                    entries.Add(_txtComplexAddend3);
            }
            if (missingEntry != null)
                entries.Add(missingEntry);

            entries = entries
                .Where(IsEntryEditable)
                .Distinct()
                .ToList();

            int currentIndex = entries.IndexOf(currentEntry);
            if (currentIndex < 0)
                return entries.FirstOrDefault();

            return currentIndex + 1 < entries.Count ? entries[currentIndex + 1] : null;
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

            int[] targetBits = _config.KeyboardConfig?.IsPrecisionPinchExercise == true
                ? new[] { 1, 1, 0, 0, 0 }
                : new int[5];
            if (_config.KeyboardConfig?.IsPrecisionPinchExercise != true)
            {
                for (int i = 0; i < windowSize; i++)
                {
                    int sourceIndex = windowStart + i;
                    bool isActive = sourceIndex < questionBits.Length && questionBits[sourceIndex];
                    int handIndex = isLeftHand ? (windowSize - 1 - i) : i;
                    targetBits[handIndex] = isActive ? 1 : 0;
                }
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

            if (await RunXorQuestionKeyboardTutorialAsync(gp))
                return;

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

            if (gp.UsesPrecisionShiftTutorial())
            {
                await koh.FadeStaticOverlayAlphaAsync(0.18f, ScaleTutorialMs(220u), "PrecisionShiftDimIn");
                try
                {
                    if (_config.KeyboardConfig.IsPrecisionSignLearningExercise)
                    {
                        await koh.AnimatePrecisionSignLearningAsync(
                            gp.GetTutorialQuestionBits(),
                            gp.GetPrecisionShiftTutorialTargets(),
                            gp.GetPrecisionShiftSideDelta(leftSide: true),
                            gp.IsPrecisionShiftSideGenericShift(leftSide: true),
                            gp.GetPrecisionShiftSideBaseAtTop(leftSide: true),
                            gp.GetPrecisionShiftSideDelta(leftSide: false),
                            gp.IsPrecisionShiftSideGenericShift(leftSide: false),
                            gp.GetPrecisionShiftSideBaseAtTop(leftSide: false),
                            ScaleTutorialMs(900u));
                    }
                    else if (gp.UsesShiftAsMinusFlipTutorial())
                    {
                        await koh.AnimateFlipAcrossAxisAsync(
                            gp.GetShiftAsMinusTutorialBits(),
                            gp.GetShiftAsMinusFlipTutorialTargets(),
                            gp.GetShiftAsMinusFlipAxisSourceIndex(),
                            ScaleTutorialMs(2200u));
                    }
                    else
                    {
                        await koh.AnimateToTargetsAsync(
                            gp.GetTutorialQuestionBits(),
                            gp.GetPrecisionShiftTutorialTargets(),
                            ScaleTutorialMs(2200u));
                    }
                }
                finally
                {
                    await koh.FadeStaticOverlayAlphaAsync(
                        KeyboardOverlayHost.DefaultStaticOverlayAlpha,
                        ScaleTutorialMs(220u),
                        "PrecisionShiftDimOut");
                }
                return;
            }

            if (gp.UsesArrowDirectionTutorial())
            {
                IReadOnlyList<int> tutorialIndices = gp.GetArrowMovementTutorialStepIndices();
                if (tutorialIndices.Count > 0 && _pianoKeyboard != null)
                {
                    int keyCount = _pianoKeyboard.KeyCount;
                    bool isOrdinalArrow = gp.IsOrdinalArrowTutorial();
                    ArrowMovementMode movementMode = gp.CurrentArrowMovementMode;
                    IReadOnlyList<int> arcIndices = isOrdinalArrow
                        ? gp.GetArrowTutorialArcIndices()
                        : Array.Empty<int>();

                    await koh.FadeStaticOverlayAlphaAsync(0.18f, ScaleTutorialMs(220u), "TutStaticDimIn");
                    try
                    {
                        if (movementMode == ArrowMovementMode.AllTogether)
                        {
                            bool[] allBits = new bool[keyCount];
                            foreach (int idx in tutorialIndices)
                            {
                                if (idx >= 0 && idx < allBits.Length)
                                    allBits[idx] = true;
                            }

                            koh.ShowHighlightedBits(allBits, Colors.Yellow, 0.58f);
                            await Task.Delay(ScaleArrowTutorialMs(760));
                            await koh.FadeOutHighlightedBitsAsync(ScaleArrowTutorialMs(220u), "ArrowTutAllTogether");
                        }
                        else if (movementMode is ArrowMovementMode.Splited or ArrowMovementMode.MiddleSplited)
                        {
                            int firstCount = movementMode == ArrowMovementMode.MiddleSplited
                                ? (int)Math.Ceiling(tutorialIndices.Count / 2.0)
                                : Math.Max(1, Math.Min(tutorialIndices.Count - 1, GetVisibleArrowSplitFirstCount(gp, tutorialIndices.Count)));

                            bool[] firstBits = new bool[keyCount];
                            bool[] secondBits = new bool[keyCount];

                            for (int i = 0; i < tutorialIndices.Count; i++)
                            {
                                int idx = tutorialIndices[i];
                                if (idx < 0 || idx >= keyCount)
                                    continue;

                                if (i < firstCount)
                                    firstBits[idx] = true;
                                else
                                    secondBits[idx] = true;
                            }

                            koh.ShowHighlightedBits(firstBits, Colors.Yellow, 0.58f);
                            await Task.Delay(ScaleArrowTutorialMs(520));
                            await koh.FadeOutHighlightedBitsAsync(ScaleArrowTutorialMs(150u), "ArrowTutSplitFirst");
                            koh.ShowHighlightedBits(secondBits, Colors.Yellow, 0.58f);
                            await Task.Delay(ScaleArrowTutorialMs(520));
                            bool[] allBits = firstBits.Zip(secondBits, (first, second) => first || second).ToArray();
                            koh.ShowHighlightedBits(allBits, Colors.Yellow, 0.58f);
                            await Task.Delay(ScaleArrowTutorialMs(420));
                            await koh.FadeOutHighlightedBitsAsync(ScaleArrowTutorialMs(220u), "ArrowTutSplitEnd");
                        }
                        else
                        {
                            bool arpeggioFinalGroup = movementMode == ArrowMovementMode.Arpeggio;
                            for (int step = 0; step < tutorialIndices.Count; step++)
                            {
                                bool[] stepBits = new bool[keyCount];
                                if (isOrdinalArrow || arpeggioFinalGroup)
                                {
                                    int idx = tutorialIndices[step];
                                    if (idx >= 0 && idx < stepBits.Length)
                                    {
                                        stepBits[idx] = true;
                                        _pianoKeyboard.SetTutorialStepLabels(new Dictionary<int, int> { [idx] = step + 1 });
                                    }

                                    Task arcTask = AnimateOrdinalArcPrefixAsync(koh, arcIndices, tutorialIndices, step, ScaleArrowTutorialMs(420u));
                                    koh.ShowHighlightedBits(stepBits, Colors.Yellow, 0.58f);
                                    await Task.WhenAll(Task.Delay(ScaleArrowTutorialMs(420)), arcTask);
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

                            if (arpeggioFinalGroup)
                            {
                                bool[] allBits = new bool[keyCount];
                                Dictionary<int, int> stepNumbers = new();
                                for (int i = 0; i < tutorialIndices.Count; i++)
                                {
                                    int idx = tutorialIndices[i];
                                    if (idx >= 0 && idx < allBits.Length)
                                    {
                                        allBits[idx] = true;
                                        stepNumbers[idx] = i + 1;
                                    }
                                }

                                _pianoKeyboard.SetTutorialStepLabels(stepNumbers);
                                koh.ShowHighlightedBits(allBits, Colors.Yellow, 0.58f);
                                await Task.Delay(ScaleArrowTutorialMs(500));
                            }

                            if (!isOrdinalArrow)
                            {
                                _pianoKeyboard.ClearTutorialStepLabels();
                                await koh.FadeOutHighlightedBitsAsync(ScaleArrowTutorialMs(220u), "ArrowTutCardinalEnd");
                            }
                        }
                    }
                    finally
                    {
                        _pianoKeyboard.ClearTutorialStepLabels();
                        koh.ClearTutorialArcs();
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

        private async Task<bool> RunXorQuestionKeyboardTutorialAsync(BitArrayGamePlay gp)
        {
            if (_config.UIQuestionType != UIQuestionType.LogicalKeyboards ||
                gp.CurrentOperation != Operation.ExclusiveOr)
            {
                return false;
            }

            bool[] question = gp.GetTutorialQuestionBits();
            bool[] question2 = gp.GetTutorialQuestionBits2();
            int bitCount = Math.Min(question.Length, question2.Length);
            if (bitCount == 0)
                return false;

            bool[] differences = new bool[bitCount];
            for (int i = 0; i < bitCount; i++)
                differences[i] = question[i] ^ question2[i];

            if (!differences.Any(bit => bit))
                return true;

            if (_config.UsesCombinedLogicalKeyboard)
            {
                if (_task1Host == null || _keyboardTask1 == null)
                    return false;

                _task1Host.SyncOverlay();
                await _task1Host.EnsureOverlaySyncedAsync();

                int combinedLength = Math.Max(_keyboardTask1.KeyCount, bitCount * 2);
                bool[] combinedDifferences = new bool[combinedLength];
                for (int i = 0; i < bitCount; i++)
                {
                    if (!differences[i])
                        continue;

                    if (i < combinedDifferences.Length)
                        combinedDifferences[i] = true;
                    if (i + bitCount < combinedDifferences.Length)
                        combinedDifferences[i + bitCount] = true;
                }

                await _task1Host.FadeInHighlightedBitsThenClearAsync(
                    combinedDifferences,
                    Colors.Yellow,
                    targetAlpha: 0.58f,
                    fadeInMs: ScaleTutorialMs(1000u),
                    highlightBand: KeyboardOverlayHost.HighlightBand.TowardMiddleBetweenRows,
                    animName: "XorQuestionCombinedFadeIn");

                return true;
            }

            if (_task1Host == null || _task2Host == null)
                return false;

            _task1Host.SyncOverlay();
            _task2Host.SyncOverlay();
            await Task.WhenAll(
                _task1Host.EnsureOverlaySyncedAsync(),
                _task2Host.EnsureOverlaySyncedAsync());

            await Task.WhenAll(
                _task2Host.FadeInHighlightedBitsThenClearAsync(
                    differences,
                    Colors.Yellow,
                    targetAlpha: 0.58f,
                    fadeInMs: ScaleTutorialMs(1000u),
                    highlightBand: KeyboardOverlayHost.HighlightBand.UpperRowBottomThird,
                    animName: "XorQuestionTopFadeIn"),
                _task1Host.FadeInHighlightedBitsThenClearAsync(
                    differences,
                    Colors.Yellow,
                    targetAlpha: 0.58f,
                    fadeInMs: ScaleTutorialMs(1000u),
                    highlightBand: KeyboardOverlayHost.HighlightBand.LowerRowTopThird,
                    animName: "XorQuestionBottomFadeIn"));

            return true;
        }

        private static int GetVisibleArrowSplitFirstCount(BitArrayGamePlay gp, int tutorialStepCount)
        {
            if (tutorialStepCount <= 1)
                return tutorialStepCount;

            int keyCount = gp.BitArrayQuestion?.Length ?? 0;
            if (keyCount <= 0)
                return Math.Max(1, tutorialStepCount / 2);

            int firstCount = gp.dir == Direction.Right
                ? keyCount - gp.aboveNumber + 1
                : gp.aboveNumber;

            if (firstCount <= 0 || firstCount >= tutorialStepCount)
                firstCount = (int)Math.Ceiling(tutorialStepCount / 2.0);

            return Math.Max(1, Math.Min(tutorialStepCount - 1, firstCount));
        }

        private static Task AnimateOrdinalArcPrefixAsync(
            KeyboardOverlayHost koh,
            IReadOnlyList<int> arcIndices,
            IReadOnlyList<int> tutorialIndices,
            int step,
            uint ms)
        {
            if (arcIndices.Count < 2 || step < 0 || step >= tutorialIndices.Count)
                return Task.CompletedTask;

            int targetIndex = tutorialIndices[step];
            int arcPosition = -1;
            int minimumPosition = Math.Min(step + 1, arcIndices.Count - 1);
            for (int i = minimumPosition; i < arcIndices.Count; i++)
            {
                if (arcIndices[i] == targetIndex)
                {
                    arcPosition = i;
                    break;
                }
            }

            if (arcPosition < 0)
                arcPosition = Math.Min(step + 1, arcIndices.Count - 1);

            return koh.AnimateTutorialArcPrefixAsync(arcIndices, arcPosition + 1, ms);
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
                _lblAction.Text = _config.KeyboardConfig?.IsTwoHandCombinationMemorize == true
                    ? gp.GetTwoHandCombinationActionText()
                    : gp.CurrentOperation.ToDString();
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
            _isPageVisible = true;
            _syncToolbarStatusController.Attach();
            if (timer != null && !timer.IsRunning)
                timer.Start();
            if (_pianoKeyboard is PianoKeyboardSync syncKeyboard)
            {
                syncKeyboard.CheckCompletedAsync = checkResult => HandleCheckResultAsync(checkResult, isKeyboardSubmission: true);
                syncKeyboard.SetLifecycleActive(true);
            }

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
            _isPageVisible = false;
            timer?.Stop();
            if (_pianoKeyboard is PianoKeyboardSync syncKeyboard)
            {
                syncKeyboard.SetLifecycleActive(false);
                syncKeyboard.CheckCompletedAsync = null;
            }
            _syncToolbarStatusController.Detach();
            base.OnDisappearing();
        }
        private void InitializeGamePlay()
        {
            _gamePlay = CreateGamePlay();
            _cmdCheck = new Command(CheckGamePlay);
            _cmdNext = new Command(async () => await HandleNextButtonAsync());
        }

        private async Task HandleNextButtonAsync()
        {
            if (!ShouldUseArrowLabelRetryButtons())
            {
                await GenerateNextExerciseAsync();
                return;
            }

            if (_gamePlay.Status == Statement.True)
            {
                if (!_isCorrectExpressionLabelVisibleForCurrentExercise)
                {
                    await ShowCorrectExpressionFeedbackAsync();
                    SetPageInteractionEnabled(true);
                    return;
                }

                await GenerateNextExerciseAsync();
                return;
            }

            CheckGamePlay();
        }

        private async Task HandleArrowLabelRetryHelpAsync()
        {
            if (!ShouldUseArrowLabelRetryButtons())
                return;

            if (_gamePlay.Status == Statement.True)
                return;

            if (_isArrowLabelRetryHelpUsed && CanUseSecondHelpForLearnerMiddle())
            {
                if (TryFillLearnerChosenRoundMiddle())
                {
                    ResetStatusLineToNeutral();
                    RestoreReadyForInputState();
                    await UpdateView(applyUiState: false, allowInputFocus: true);
                }
                return;
            }

            if (TryApplyArrowLabelRetryHelp())
            {
                _isArrowLabelRetryHelpUsed = true;
                if (_btnArrowLabelRetryHelp != null && !CanUseSecondHelpForLearnerMiddle())
                    _btnArrowLabelRetryHelp.IsEnabled = false;

                ResetStatusLineToNeutral();
                RestoreReadyForInputState();
                await UpdateView(applyUiState: false, allowInputFocus: true);
            }
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

            if (_isKeyboard && (!_config.KeyboardConfig.KeyboardOnlyForHelp || IsActiveArrowKeyboardQuestion))
            {
                SetInlineKeyboardCheckVisible(false);
                ExerciseCheckResult checkResult = await _gamePlay.EvaluateAsync(_pianoKeyboard);
                await HandleCheckResultAsync(checkResult, isKeyboardSubmission: true);

            }
            else
            {
                try
                {
                    PPWObject submittedAnswer = BuildSubmittedPpwAnswer();

                    ExerciseCheckResult checkResult = await _gamePlay.EvaluateAsync(submittedAnswer.Addend1, submittedAnswer.Addend2, submittedAnswer.Sum);
                    await HandleCheckResultAsync(checkResult, isKeyboardSubmission: false, onCorrect: () => CapturePreviousAnswer(submittedAnswer));
                }
                catch
                {
                    _lblStatement.Text = Statement.WrongInput;
                    SetPageInteractionEnabled(true);
                }
            }
        }

        private PPWObject BuildSubmittedPpwAnswer()
        {
            if (UsesArrowLabelPromptStage() &&
                IsArrowLabelEquationIntroVisible() &&
                _gamePlay is BitArrayGamePlay equationIntroGamePlay)
            {
                MissingValueTargetFlags missingTarget = equationIntroGamePlay.CurrentArrowLabelMissingTarget;
                int submittedValue = Convert.ToInt32(_arrowEquationAnswerEntry.Text);
                return new PPWObject(
                    missingTarget == MissingValueTargetFlags.Addend1
                        ? submittedValue
                        : equationIntroGamePlay.ArrowLabelAddend1Value,
                    missingTarget is (MissingValueTargetFlags.Addend2 or MissingValueTargetFlags.TotalDistance)
                        ? submittedValue
                        : equationIntroGamePlay.ArrowLabelDistanceValue,
                    missingTarget == MissingValueTargetFlags.Sum
                        ? submittedValue
                        : equationIntroGamePlay.ArrowLabelSumValue);
            }

            if (UsesArrowLabelPromptStage() &&
                IsComplexArrowLabelPromptMode(GetDisplayedArrowLabelExerciseMode()) &&
                _gamePlay is BitArrayGamePlay arrowPromptGamePlay)
            {
                MissingValueTargetFlags missingTarget = arrowPromptGamePlay.CurrentArrowLabelMissingTarget;
                int start = arrowPromptGamePlay.ArrowLabelAddend1Value;
                int end = arrowPromptGamePlay.ArrowLabelSumValue;
                int middle = arrowPromptGamePlay.ArrowLabelAddend2Value ?? 10;
                bool requireComplexBreakdownEntries =
                    _config.KeyboardConfig?.ArrowLabelRetryMode == ArrowLabelRetryMode.RevealComplexThroughTen &&
                    _isComplexThroughTenBreakdownVisible;
                bool requireComplexThroughTenFillOrder = UsesComplexThroughTenDistanceInput();
                bool requireComplexNextTenFillOrder = UsesComplexNextTenDistanceInput();

                if (requireComplexBreakdownEntries || requireComplexThroughTenFillOrder)
                {
                    int firstDistance = Convert.ToInt32(_txtAddend2.Text);
                    int secondDistance = Convert.ToInt32(_txtComplexAddend3.Text);
                    int submittedMiddle = _config.KeyboardConfig?.AllowLearnerChosenComplexMiddle == true
                        ? Convert.ToInt32(_txtSum.Text)
                        : middle;
                    bool requireComplexDistanceEntries =
                        _config.KeyboardConfig?.ArrowLabelRetryMode == ArrowLabelRetryMode.None ||
                        (_config.KeyboardConfig?.ArrowLabelRetryMode == ArrowLabelRetryMode.RevealComplexThroughTen &&
                         _isComplexThroughTenBreakdownVisible);
                    if (requireComplexDistanceEntries &&
                        (firstDistance != submittedMiddle - start ||
                         secondDistance != end - submittedMiddle ||
                         firstDistance + secondDistance != arrowPromptGamePlay.ArrowLabelDistanceValue))
                    {
                        return new PPWObject(PPWGamePlay.NAN, PPWGamePlay.NAN, PPWGamePlay.NAN);
                    }
                }

                if (requireComplexNextTenFillOrder)
                {
                    int firstDistance = Convert.ToInt32(_txtAddend2.Text);
                    int submittedMiddle = Convert.ToInt32(_txtSum.Text);
                    int secondDistance = Convert.ToInt32(_txtComplexAddend3.Text);
                    if (firstDistance != middle - start ||
                        submittedMiddle != middle ||
                        secondDistance != end - middle)
                    {
                        return new PPWObject(PPWGamePlay.NAN, PPWGamePlay.NAN, PPWGamePlay.NAN);
                    }
                }

                return new PPWObject(
                    missingTarget == MissingValueTargetFlags.Addend1
                        ? Convert.ToInt32(_txtAddend1.Text)
                        : arrowPromptGamePlay.ArrowLabelAddend1Value,
                    missingTarget is (MissingValueTargetFlags.Addend2 or MissingValueTargetFlags.TotalDistance)
                        ? Convert.ToInt32(IsArrowLabelEquationIntroVisible() ? _arrowEquationAnswerEntry.Text : _txtComplexTotalDistance.Text)
                        : arrowPromptGamePlay.ArrowLabelDistanceValue,
                    missingTarget == MissingValueTargetFlags.Sum
                        ? Convert.ToInt32(IsArrowLabelEquationIntroVisible() ? _arrowEquationAnswerEntry.Text : _txtComplexSum2.Text)
                        : arrowPromptGamePlay.ArrowLabelSumValue);
            }

            return new PPWObject(
                Convert.ToInt32(_txtAddend1.Text),
                Convert.ToInt32(_txtAddend2.Text),
                Convert.ToInt32(_txtSum.Text));
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
            bool growSequenceFeedback = checkResult.IsCorrect && _sequenceFirstFeedbackProgress > 0;
            if (_config.KeyboardConfig.IsPrecisionPinchSequenceMemorize)
            {
                _sequenceFirstFeedbackProgress = 0;
                _sequenceFeedbackChangeVersion++;
            }

            bool waitForRetryNextButton =
                ShouldUseArrowLabelRetryButtons() &&
                !isKeyboardSubmission &&
                checkResult.IsCorrect &&
                !checkResult.RefreshCurrentQuestion;
            bool willAdvanceToNextExercise = checkResult.Completion == null &&
                                             checkResult.IsCorrect &&
                                             !checkResult.RefreshCurrentQuestion &&
                                             !_gamePlay.GameOver &&
                                             !waitForRetryNextButton;
            await UpdateView(applyUiState: false, allowInputFocus: !willAdvanceToNextExercise);
            bool shouldAutoShowTutorial = UpdateAutoTutorialState(checkResult);

            if (checkResult.IsCorrect)
            {
                onCorrect?.Invoke();
            }

            if (!isKeyboardSubmission && !checkResult.IsCorrect && TryApplyArrowLabelRetryHelp())
            {
                _isArrowLabelRetryHelpUsed = true;
                if (_btnArrowLabelRetryHelp != null && !CanUseSecondHelpForLearnerMiddle())
                    _btnArrowLabelRetryHelp.IsEnabled = false;

                ResetStatusLineToNeutral();
                RestoreReadyForInputState();
                await UpdateView(applyUiState: false, allowInputFocus: true);
                return;
            }

            if (!checkResult.IsWrongInput)
                ApplyFeedbackUiState(checkResult.IsCorrect);
            else
                RestoreReadyForInputState();

            if (growSequenceFeedback && _centerFeedbackBadge != null)
            {
                _centerFeedbackBadge.Scale = 0.75;
                _centerFeedbackBadge.Opacity = 1;
                await _centerFeedbackBadge.ScaleTo(1, 220, Easing.CubicOut);
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
            else if (checkResult.RefreshCurrentQuestion)
            {
                LogNumericInputDebug("HandleCheckResult:RefreshCurrentQuestion:BeforeApplyPending");
                _gamePlay.ResetStatusToNeutral();
                _isArrowLabelRetryHelpVisible = false;

                if (_gamePlay is BitArrayGamePlay bitArrayGamePlay)
                {
                    bitArrayGamePlay.ApplyPendingArrowLabelPromptMode();
                    bitArrayGamePlay.HideCurrentArrowLabelMissingValue();
                }

                RefreshKeyboardControlBar();
                await UpdateView(applyUiState: false, allowInputFocus: true);
                LogNumericInputDebug("HandleCheckResult:RefreshCurrentQuestion:AfterUpdateView");
            }
            else if (isKeyboardSubmission)
            {
                await ApplyAutoAnswerTimeTuningAsync("AutoTuneRetry");
                _pianoKeyboard.PianoInit();
                SetPageInteractionEnabled(true);
                ResetStatusLineToNeutral();
                RestoreReadyForInputState();
            }
            else if (waitForRetryNextButton)
            {
                SetPageInteractionEnabled(true);
                RefreshKeyboardArrowPromptView();
                RefreshNumericEntryAppearance();
                RefreshStatusActionSlot();
                if (_btnNext != null)
                    _btnNext.IsEnabled = ShouldEnableArrowLabelRetryNextButton();
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

        private bool TryApplyArrowLabelRetryHelp()
        {
            if (!UsesArrowLabelRetryStage || _config.KeyboardConfig == null)
                return false;

            if (IsArrowLabelEquationIntroVisible())
                _isArrowLabelEquationIntroVisible = false;

            switch (_config.KeyboardConfig.ArrowLabelRetryMode)
            {
                case ArrowLabelRetryMode.ShowKeyboardHelp:
                    if (_isArrowLabelRetryHelpVisible)
                    {
                        if (_config.KeyboardConfig.UseKeyboardQuestionAfterArrowLabelHelp &&
                            _gamePlay is BitArrayGamePlay arrowGamePlay &&
                            arrowGamePlay.QueueArrowLabelRetryKeyboardQuestion())
                        {
                            arrowGamePlay.ApplyPendingArrowLabelPromptMode();
                            RefreshKeyboardArrowPromptView();
                            RefreshKeyboardControlBar();
                            return true;
                        }

                        return false;
                    }

                    _isArrowLabelRetryHelpVisible = true;
                    RefreshKeyboardControlBar();
                    return true;

                case ArrowLabelRetryMode.RevealComplexThroughTen:
                    if (_isComplexThroughTenBreakdownVisible)
                        return false;

                    _isComplexThroughTenBreakdownVisible = true;
                    _isComplexMiddleFilledByHelp = false;
                    RefreshKeyboardArrowPromptView();
                    SyncPrimaryEntryEnabledState();
                    SelectFirstComplexThroughTenBreakdownEntry();
                    RefreshNumericEntryAppearance();
                    return true;

                default:
                    return false;
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

            if (!checkResult.ShouldDelayFeedback)
                return;

            bool showCorrectExpressionFeedback = ShouldShowCorrectExpressionFeedback(checkResult);
            // Keep vertical precision practice quick, while leaving enough time to
            // perceive the result. Other stages continue to use their configured delay.
            int feedbackDelayMilliseconds =
                _config.KeyboardConfig?.IsVerticalPrecisionPinchExercise == true
                    ? 1000
                    : _config.SecondsTillNextExercise * 1000;

            if (_config.SecondsTillNextExercise <= 0)
            {
                if (!checkResult.RefreshCurrentQuestion)
                    return;

                SetPageInteractionEnabled(false);
                try
                {
                    await Task.Delay(450);
                }
                finally
                {
                    if (!keepDisabledForNextExercise)
                        SetPageInteractionEnabled(true);
                }
                return;
            }

            if (isKeyboardSubmission)
            {
                SetPageInteractionEnabled(false);
                try
                {
                    if (showCorrectExpressionFeedback)
                        await ShowCorrectExpressionFeedbackAsync();
                    else
                        await Task.Delay(feedbackDelayMilliseconds);
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
                if (showCorrectExpressionFeedback)
                    await ShowCorrectExpressionFeedbackAsync();
                else
                    await Task.Delay(feedbackDelayMilliseconds);
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

            if (ShouldUseArrowLabelRetryButtons() &&
                !isKeyboardSubmission &&
                checkResult.IsCorrect &&
                !checkResult.RefreshCurrentQuestion)
            {
                return true;
            }

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
            if (_txtComplexAddend3 != null) EntryEnabled(_txtComplexAddend3, enabled);
            if (_txtComplexSum2 != null) EntryEnabled(_txtComplexSum2, enabled);
            if (_txtComplexTotalDistance != null) EntryEnabled(_txtComplexTotalDistance, enabled);
            RefreshNumericEntryAppearance();
        }

        private async Task GenerateNextExerciseAsync()
        {
            if (_correctExpressionLabel != null)
                _correctExpressionLabel.IsVisible = false;

            _isCorrectExpressionLabelVisibleForCurrentExercise = false;
            _isArrowLabelRetryHelpUsed = false;
            _isComplexMiddleFilledByHelp = false;
            _isArrowLabelEquationIntroVisible = ShouldStartArrowLabelRetryWithEquation();
            _isArrowLabelRetryHelpVisible = false;
            _isComplexThroughTenBreakdownVisible = false;
            if (_arrowEquationAnswerEntry != null)
                _arrowEquationAnswerEntry.Text = string.Empty;
            _complexPromptEntryValidationStates.Clear();
            await AnimateBenchmarkQuestionAdvanceOutAsync();
            await ApplyAutoAnswerTimeTuningAsync("AutoTuneNextQuestion");
            if (!_isPageVisible)
                return;

            ExerciseGenerationResult generatedExercise = await _gamePlay.GenerateExerciseAsync();
            if (!_isPageVisible)
            {
                if (generatedExercise.PersistenceTask != null)
                    await generatedExercise.PersistenceTask;
                return;
            }
            _pianoKeyboard?.RefreshKeyCaptions();
            await UpdateView(true, generatedExercise: generatedExercise);
            await AnimateBenchmarkQuestionAdvanceInAsync();

            // Once the question is visible, a fresh key-down can safely begin the answer.
            // Persistence and tuning below must not leave an already-rendered keyboard disabled.
            if (_isPageVisible && _isKeyboard && _config.SecondsTillAllowInput <= 0)
            {
                SetPageInteractionEnabled(true);
                SetKeyboardInteractionEnabled(true);
            }

            SetPageInteractionEnabled(true);
            if (_isKeyboard)
            {
                SetKeyboardInteractionEnabled(true);

                if (_config.FromNumToNum)
                {
                    _pianoKeyboard.IsEnabled = true;
                }
            }

            // Analytics persistence must not keep PianoKeyboardSync in its checking state
            // after the next question is already visible and accepting an answer.
            _ = PersistDisplayedExerciseAsync(generatedExercise);
        }

        private async Task PersistDisplayedExerciseAsync(ExerciseGenerationResult generatedExercise)
        {
            try
            {
                // Start each save immediately so it captures the currently displayed
                // question before the learner can advance again.
                List<Task> persistenceTasks = new()
                {
                    EnsureInitialTimerSettingSavedAsync(),
                    PersistVisibleQuestionPartsAsync(),
                    PersistSecondaryPpwAsync(),
                    PersistKeyboardQuestionDisplayMetadataAsync()
                };
                if (generatedExercise.PersistenceTask != null)
                    persistenceTasks.Add(generatedExercise.PersistenceTask);

                await Task.WhenAll(persistenceTasks);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ExercisePersistence] Background save failed: {ex}");
            }
        }

        private async Task RunMemorizeHelpAsync(
            KeyboardOverlayHost host,
            BitArrayGamePlay gamePlay)
        {
            if (_tutorialRunning)
                return;

            _tutorialRunning = true;
            SetPlayUiState(PlayUiState.Tutorial);
            bool useAndroidInputBlock = DeviceInfo.Platform == DevicePlatform.Android;
            if (useAndroidInputBlock)
                _pianoKeyboard.InputTransparent = true;
            else
                host.SetTutorialMode(true);
            int seconds = Math.Max(
                1, _config.KeyboardConfig.PrecisionPinchMemorizeDelaySeconds);

            try
            {
                if (_config.KeyboardConfig.IsPrecisionPinchSequenceMemorize)
                {
                    host.SetStaticBits(gamePlay.GetSequenceMemorizeFirstPreview());
                    await Task.Delay(TimeSpan.FromSeconds(seconds));
                    host.SetStaticBits(gamePlay.GetSequenceMemorizeSecondPreview());
                }
                else
                {
                    host.SetStaticBits(gamePlay.GetTutorialQuestionBits());
                }
                await Task.Delay(TimeSpan.FromSeconds(seconds));
            }
            finally
            {
                host.SetStaticBits(Array.Empty<bool>());
                if (useAndroidInputBlock)
                {
                    if (_pianoKeyboard is PianoKeyboardSync syncKeyboard)
                        syncKeyboard.NotifyQuestionReadyForInput();
                    else
                        _pianoKeyboard.InputTransparent = false;
                }
                else
                {
                    host.SetTutorialMode(false);
                }
                _tutorialRunning = false;
                RestoreReadyForInputState();
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
            if (_hasManualAnswerTimeOverride ||
                _isApplyingAutoTune ||
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

                // A manual change may have happened while the recommendation query was
                // running. Manual timing always wins over automatic tuning.
                if (_hasManualAnswerTimeOverride)
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

        private void PrepareAnswerKeyboardForCurrentExercise()
        {
            if (!_isKeyboard || _pianoKeyboard == null || _config.FromNumToNum)
                return;

            if (!ApplyConfiguredKeyboardSeedState())
                _pianoKeyboard.PianoInit();

            if (_config.KeyboardConfig?.IsArrow == true && _gamePlay is BitArrayGamePlay arrowGamePlay)
            {
                _pianoKeyboard.SetTraceOverlayColors(
                    arrowGamePlay.GetStagedArrowTraceOverlayColors(),
                    arrowGamePlay.GetStagedArrowSecondaryTraceOverlayColors());
            }
            else
            {
                _pianoKeyboard.ClearTraceOverlay();
            }
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

        private bool ShouldStartArrowLabelRetryWithEquation()
        {
            return UsesArrowLabelRetryStage;
        }

        private bool IsArrowLabelEquationIntroVisible()
        {
            return ShouldStartArrowLabelRetryWithEquation() &&
                   _isArrowLabelEquationIntroVisible &&
                   UsesArrowLabelPromptStage() &&
                   !IsActiveArrowKeyboardQuestion;
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

            MissingValueTargetFlags missingTarget = _gamePlay is BitArrayGamePlay arrowGamePlay
                ? arrowGamePlay.CurrentArrowLabelMissingTarget
                : _config.ArrowLabelMissingValueTarget;

            if (IsArrowLabelEquationIntroVisible() && _arrowEquationAnswerEntry != null)
                return _arrowEquationAnswerEntry;

            return missingTarget switch
            {
                MissingValueTargetFlags.Addend1 => _txtAddend1,
                MissingValueTargetFlags.TotalDistance => IsComplexArrowLabelPromptMode(GetDisplayedArrowLabelExerciseMode())
                    ? _txtComplexTotalDistance
                    : _txtAddend2,
                MissingValueTargetFlags.Addend2 => IsComplexArrowLabelPromptMode(GetDisplayedArrowLabelExerciseMode())
                    ? _txtComplexTotalDistance
                    : _txtAddend2,
                MissingValueTargetFlags.Sum => IsComplexArrowLabelPromptMode(GetDisplayedArrowLabelExerciseMode())
                    ? _txtComplexSum2
                    : _txtSum,
                _ => null
            };
        }

        private static bool IsComplexArrowLabelPromptMode(ArrowLabelExerciseMode mode)
        {
            return mode is ArrowLabelExerciseMode.ComplexBridgeToNextTen or
                ArrowLabelExerciseMode.ComplexBridgeToAnyNextTen or
                ArrowLabelExerciseMode.ComplexLongDistance;
        }

        private bool ShouldShowComplexArrowBreakdown()
        {
            return _config.KeyboardConfig?.ArrowLabelRetryMode != ArrowLabelRetryMode.RevealComplexThroughTen ||
                   _isComplexThroughTenBreakdownVisible;
        }

        private void ResetArrowLabelPromptEntryColors()
        {
            if (!UsesArrowLabelPromptStage())
                return;

            if (_txtAddend1 != null) _txtAddend1.BackgroundColor = Colors.White;
            if (_txtAddend2 != null) _txtAddend2.BackgroundColor = Colors.White;
            if (_txtSum != null) _txtSum.BackgroundColor = Colors.White;
            if (_txtComplexAddend3 != null) _txtComplexAddend3.BackgroundColor = Colors.White;
            if (_txtComplexSum2 != null) _txtComplexSum2.BackgroundColor = Colors.White;
            if (_txtComplexTotalDistance != null) _txtComplexTotalDistance.BackgroundColor = Colors.White;
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
            if (IsComplexArrowLabelPromptMode(GetDisplayedArrowLabelExerciseMode()))
                return BuildComplexKeyboardArrowPromptView();

            _txtAddend1 ??= CreateKeyboardArrowPromptEntry();
            _txtAddend2 ??= CreateKeyboardArrowPromptEntry();
            _txtSum ??= CreateKeyboardArrowPromptEntry();

            string pathData = GetArrowLabelPromptPathData();
            var layout = GetArrowLabelPromptLayout();

            _keyboardArrowPromptPath = new Microsoft.Maui.Controls.Shapes.Path
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

            AbsoluteLayout.SetLayoutBounds(_keyboardArrowPromptPath, layout.PathBounds);
            promptSurface.Children.Add(_keyboardArrowPromptPath);

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

        private View BuildArrowEquationPromptView()
        {
            _arrowEquationLeftLabel ??= CreateArrowEquationLabel();
            _arrowEquationMiddleLabel ??= CreateArrowEquationLabel();
            _arrowEquationRightLabel ??= CreateArrowEquationLabel();
            _arrowEquationAnswerEntry ??= CreateKeyboardArrowPromptEntry();
            _arrowEquationAnswerEntry.WidthRequest = 72;
            ConfigureNumericEntry(_arrowEquationAnswerEntry);

            HorizontalStackLayout equationRow = new()
            {
                IsVisible = false,
                Spacing = 6,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 16, 0, 14),
                Children =
                {
                    _arrowEquationLeftLabel,
                    _arrowEquationAnswerEntry,
                    _arrowEquationMiddleLabel,
                    _arrowEquationRightLabel
                }
            };

            return equationRow;
        }

        private static Label CreateArrowEquationLabel()
        {
            return new Label
            {
                FontSize = 24,
                TextColor = Colors.Black,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                VerticalOptions = LayoutOptions.Center
            };
        }

        private View BuildComplexKeyboardArrowPromptView()
        {
            _txtAddend1 ??= CreateKeyboardArrowPromptEntry();
            _txtAddend2 ??= CreateKeyboardArrowPromptEntry();
            _txtSum ??= CreateKeyboardArrowPromptEntry();
            _txtComplexAddend3 ??= CreateKeyboardArrowPromptEntry();
            _txtComplexSum2 ??= CreateKeyboardArrowPromptEntry();
            _txtComplexTotalDistance ??= CreateKeyboardArrowPromptEntry();
            _txtSum.WidthRequest = 58;
            _txtComplexTotalDistance.WidthRequest = 64;

            _complexFirstArrowPath = new Microsoft.Maui.Controls.Shapes.Path
            {
                Data = (Geometry)new PathGeometryConverter().ConvertFromInvariantString("M 67,132 L 67,76 L 184,76 M 168,64 L 184,76 L 168,88"),
                Fill = Colors.Transparent,
                Stroke = Colors.Black,
                StrokeThickness = 3,
                InputTransparent = true
            };

            _complexSecondArrowPath = new Microsoft.Maui.Controls.Shapes.Path
            {
                Data = (Geometry)new PathGeometryConverter().ConvertFromInvariantString("M 182,132 L 182,76 L 300,76 M 284,64 L 300,76 L 284,88"),
                Fill = Colors.Transparent,
                Stroke = Colors.Black,
                StrokeThickness = 3,
                InputTransparent = true
            };

            _complexTotalArrowPath = new Microsoft.Maui.Controls.Shapes.Path
            {
                Data = (Geometry)new PathGeometryConverter().ConvertFromInvariantString("M 67,132 L 67,18 L 300,18 L 284,6 M 300,18 L 284,30"),
                Fill = Colors.Transparent,
                Stroke = Colors.Black,
                StrokeThickness = 3,
                InputTransparent = true
            };

            var promptSurface = new AbsoluteLayout
            {
                WidthRequest = 320,
                HeightRequest = 170,
                Padding = new Thickness(0, 10, 0, 0),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start,
                InputTransparent = true
            };

            AbsoluteLayout.SetLayoutBounds(_complexFirstArrowPath, new Rect(0, 0, 320, 170));
            promptSurface.Children.Add(_complexFirstArrowPath);
            AbsoluteLayout.SetLayoutBounds(_complexSecondArrowPath, new Rect(0, 0, 320, 170));
            promptSurface.Children.Add(_complexSecondArrowPath);
            AbsoluteLayout.SetLayoutBounds(_complexTotalArrowPath, new Rect(0, 0, 320, 170));
            promptSurface.Children.Add(_complexTotalArrowPath);

            AbsoluteLayout.SetLayoutBounds(_txtAddend1, new Rect(19, 119, 50, 25));
            promptSurface.Children.Add(_txtAddend1);
            AbsoluteLayout.SetLayoutBounds(_txtAddend2, new Rect(86, 61, 50, 25));
            promptSurface.Children.Add(_txtAddend2);
            AbsoluteLayout.SetLayoutBounds(_txtSum, new Rect(134, 119, 50, 25));
            promptSurface.Children.Add(_txtSum);
            AbsoluteLayout.SetLayoutBounds(_txtComplexAddend3, new Rect(201, 61, 50, 25));
            promptSurface.Children.Add(_txtComplexAddend3);
            AbsoluteLayout.SetLayoutBounds(_txtComplexSum2, new Rect(250, 119, 50, 25));
            promptSurface.Children.Add(_txtComplexSum2);
            AbsoluteLayout.SetLayoutBounds(_txtComplexTotalDistance, new Rect(134, 0, 50, 25));
            promptSurface.Children.Add(_txtComplexTotalDistance);

            return new VerticalStackLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start,
                Spacing = 0,
                Margin = new Thickness(0, 2, 0, 8),
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

            bool showEquationIntro = IsArrowLabelEquationIntroVisible();
            if (_arrowEquationPromptView != null)
                _arrowEquationPromptView.IsVisible = showEquationIntro;

            _keyboardArrowPromptView.IsVisible = !showEquationIntro &&
                (arrowPromptGamePlay.HasArrowLabelPrompt || IsActiveArrowKeyboardQuestion);
            if (!arrowPromptGamePlay.HasArrowLabelPrompt)
                return;

            RefreshArrowEquationPromptView(arrowPromptGamePlay);

            ResetArrowLabelPromptEntryColors();

            ArrowLabelExerciseMode mode = arrowPromptGamePlay.CurrentArrowLabelExerciseMode;
            MissingValueTargetFlags missingTarget = arrowPromptGamePlay.CurrentArrowLabelMissingTarget;
            bool revealCorrectResponse = _gamePlay.Status == Statement.True && UsesArrowCorrectResponseFeedback();
            RefreshSimpleKeyboardArrowPromptPath(mode);

            if (IsComplexArrowLabelPromptMode(mode))
            {
                RefreshComplexKeyboardArrowPromptView(arrowPromptGamePlay, mode, missingTarget, revealCorrectResponse);
                return;
            }

            _txtAddend1.Text = missingTarget == MissingValueTargetFlags.Addend1 && !revealCorrectResponse
                ? string.Empty
                : arrowPromptGamePlay.ArrowLabelAddend1Value.ToString();

            _txtAddend2.Text = missingTarget == MissingValueTargetFlags.Addend2 && !revealCorrectResponse
                ? string.Empty
                : arrowPromptGamePlay.ArrowLabelAddend2Value?.ToString() ?? string.Empty;

            _txtSum.Text = missingTarget == MissingValueTargetFlags.Sum && !revealCorrectResponse
                ? string.Empty
                : arrowPromptGamePlay.ArrowLabelSumValue.ToString();

            RefreshComplexKeyboardArrowPromptView(arrowPromptGamePlay, mode, missingTarget, revealCorrectResponse);
        }

        private void RefreshArrowEquationPromptView(BitArrayGamePlay arrowPromptGamePlay)
        {
            if (_arrowEquationPromptView == null ||
                _arrowEquationLeftLabel == null ||
                _arrowEquationMiddleLabel == null ||
                _arrowEquationRightLabel == null ||
                _arrowEquationAnswerEntry == null)
            {
                return;
            }

            int start = arrowPromptGamePlay.ArrowLabelAddend1Value;
            int distance = arrowPromptGamePlay.ArrowLabelDistanceValue;
            int end = arrowPromptGamePlay.ArrowLabelSumValue;
            MissingValueTargetFlags missingTarget = arrowPromptGamePlay.CurrentArrowLabelMissingTarget;
            bool revealCorrectResponse = _gamePlay.Status == Statement.True && UsesArrowCorrectResponseFeedback();
            bool useSubtractionEquation =
                UsesRtlComplexThroughTenPrompt() ||
                arrowPromptGamePlay.CurrentArrowLabelExerciseMode == ArrowLabelExerciseMode.EndAndLengthWithMissingStart;

            if (useSubtractionEquation)
            {
                if (missingTarget is MissingValueTargetFlags.Addend2 or MissingValueTargetFlags.TotalDistance)
                {
                    _arrowEquationLeftLabel.Text = $"{end} -";
                    _arrowEquationMiddleLabel.Text = "=";
                    _arrowEquationRightLabel.Text = start.ToString();
                    if (string.IsNullOrWhiteSpace(_arrowEquationAnswerEntry.Text) || revealCorrectResponse)
                        _arrowEquationAnswerEntry.Text = revealCorrectResponse ? distance.ToString() : string.Empty;
                }
                else
                {
                    _arrowEquationLeftLabel.Text = $"{end} - {distance} =";
                    _arrowEquationMiddleLabel.Text = string.Empty;
                    _arrowEquationRightLabel.Text = string.Empty;
                    if (string.IsNullOrWhiteSpace(_arrowEquationAnswerEntry.Text) || revealCorrectResponse)
                        _arrowEquationAnswerEntry.Text = revealCorrectResponse ? start.ToString() : string.Empty;
                }

                return;
            }

            if (missingTarget is MissingValueTargetFlags.Addend2 or MissingValueTargetFlags.TotalDistance)
            {
                _arrowEquationLeftLabel.Text = $"{start} +";
                _arrowEquationMiddleLabel.Text = "=";
                _arrowEquationRightLabel.Text = end.ToString();
                if (string.IsNullOrWhiteSpace(_arrowEquationAnswerEntry.Text) || revealCorrectResponse)
                    _arrowEquationAnswerEntry.Text = revealCorrectResponse ? distance.ToString() : string.Empty;
            }
            else
            {
                _arrowEquationLeftLabel.Text = $"{start} + {distance} =";
                _arrowEquationMiddleLabel.Text = string.Empty;
                _arrowEquationRightLabel.Text = string.Empty;
                if (string.IsNullOrWhiteSpace(_arrowEquationAnswerEntry.Text) || revealCorrectResponse)
                    _arrowEquationAnswerEntry.Text = revealCorrectResponse ? end.ToString() : string.Empty;
            }
        }

        private void RefreshSimpleKeyboardArrowPromptPath(ArrowLabelExerciseMode mode)
        {
            if (_keyboardArrowPromptPath == null || IsComplexArrowLabelPromptMode(mode))
                return;

            string pathData = GetArrowLabelPromptPathData();
            var layout = GetArrowLabelPromptLayout();
            _keyboardArrowPromptPath.Data = (Geometry)new PathGeometryConverter().ConvertFromInvariantString(pathData);
            _keyboardArrowPromptPath.StrokeLineCap = layout.UseRoundedStroke ? PenLineCap.Round : PenLineCap.Flat;
            _keyboardArrowPromptPath.StrokeLineJoin = layout.UseRoundedStroke ? PenLineJoin.Round : PenLineJoin.Miter;
            AbsoluteLayout.SetLayoutBounds(_keyboardArrowPromptPath, layout.PathBounds);

            if (_txtAddend1 != null)
                AbsoluteLayout.SetLayoutBounds(_txtAddend1, layout.Addend1Bounds);
            if (_txtAddend2 != null)
                AbsoluteLayout.SetLayoutBounds(_txtAddend2, layout.Addend2Bounds);
            if (_txtSum != null)
                AbsoluteLayout.SetLayoutBounds(_txtSum, layout.SumBounds);
        }

        private void RefreshComplexKeyboardArrowPromptView(
            BitArrayGamePlay arrowPromptGamePlay,
            ArrowLabelExerciseMode mode,
            MissingValueTargetFlags missingTarget,
            bool revealCorrectResponse)
        {
            if (!IsComplexArrowLabelPromptMode(mode) ||
                _txtComplexAddend3 == null ||
                _txtComplexSum2 == null ||
                _txtComplexTotalDistance == null)
            {
                return;
            }

            int start = arrowPromptGamePlay.ArrowLabelAddend1Value;
            int end = arrowPromptGamePlay.ArrowLabelSumValue;
            int? middle = arrowPromptGamePlay.ArrowLabelAddend2Value;
            bool isRtl = UsesRtlComplexThroughTenPrompt();
            bool useFixedComplexMiddle = _config.KeyboardConfig?.UseFixedComplexMiddle == true;
            bool showBreakdown = ShouldShowComplexArrowBreakdown();
            int learnerDistance1 = 0;
            int learnerMiddle = 0;
            int learnerDistance2 = 0;
            bool useLearnerSplit = revealCorrectResponse &&
                TryGetValidLearnerChosenSplit(arrowPromptGamePlay, out learnerDistance1, out learnerMiddle, out learnerDistance2);
            RefreshComplexKeyboardArrowPromptLayout();

            if (_complexFirstArrowPath != null) _complexFirstArrowPath.IsVisible = showBreakdown;
            if (_complexSecondArrowPath != null) _complexSecondArrowPath.IsVisible = showBreakdown;
            if (_txtAddend2 != null) _txtAddend2.IsVisible = showBreakdown;
            if (_txtSum != null) _txtSum.IsVisible = showBreakdown;
            if (_txtComplexAddend3 != null) _txtComplexAddend3.IsVisible = showBreakdown;

            _txtAddend1.Text = missingTarget == MissingValueTargetFlags.Addend1 && !revealCorrectResponse
                ? string.Empty
                : start.ToString();

            bool isTotalDistanceMissing = missingTarget is MissingValueTargetFlags.TotalDistance or MissingValueTargetFlags.Addend2;
            _txtComplexTotalDistance.Text = isTotalDistanceMissing && !revealCorrectResponse
                ? string.Empty
                : arrowPromptGamePlay.ArrowLabelDistanceValue.ToString();

            bool requireFilledBreakdown =
                _config.KeyboardConfig?.ArrowLabelRetryMode == ArrowLabelRetryMode.RevealComplexThroughTen &&
                _isComplexThroughTenBreakdownVisible &&
                !revealCorrectResponse;
            bool shouldPrefillSmallDistances = revealCorrectResponse ||
                (_config.KeyboardConfig?.ArrowLabelRetryMode != ArrowLabelRetryMode.None &&
                 !_isComplexThroughTenBreakdownVisible);
            bool requireNextTenFillOrder = mode == ArrowLabelExerciseMode.ComplexBridgeToAnyNextTen && !revealCorrectResponse;
            if (showBreakdown && middle.HasValue)
            {
                _txtAddend2.Text = useLearnerSplit
                    ? learnerDistance1.ToString()
                    : requireFilledBreakdown || !shouldPrefillSmallDistances
                    ? string.Empty
                    : (middle.Value - start).ToString();
                _txtSum.Text = useLearnerSplit
                    ? learnerMiddle.ToString()
                    : useFixedComplexMiddle || _isComplexMiddleFilledByHelp
                    ? middle.Value.ToString()
                    : (requireNextTenFillOrder ? string.Empty : middle.Value.ToString());
                _txtComplexAddend3.Text = useLearnerSplit
                    ? learnerDistance2.ToString()
                    : requireFilledBreakdown || !shouldPrefillSmallDistances
                    ? string.Empty
                    : (end - middle.Value).ToString();
            }
            else
            {
                _txtComplexAddend3.Text = string.Empty;
            }

            _txtComplexSum2.Text = isRtl || revealCorrectResponse || missingTarget != MissingValueTargetFlags.Sum
                ? end.ToString()
                : string.Empty;

            if (isRtl)
            {
                _txtAddend1.Text = missingTarget == MissingValueTargetFlags.Addend1 && !revealCorrectResponse
                    ? string.Empty
                    : start.ToString();
                if (useFixedComplexMiddle)
                    _txtSum.Text = middle?.ToString() ?? "10";
            }
            else if (missingTarget == MissingValueTargetFlags.Sum && !revealCorrectResponse)
            {
                _txtComplexSum2.Text = string.Empty;
            }

            /*_txtComplexSum2.Text = missingTarget == MissingValueTargetFlags.Sum && !revealCorrectResponse
                ? string.Empty
                : end.ToString();*/

            RefreshNumericEntryAppearance();
        }

        private void RefreshComplexKeyboardArrowPromptLayout()
        {
            if (_complexFirstArrowPath == null ||
                _complexSecondArrowPath == null ||
                _complexTotalArrowPath == null ||
                _txtAddend1 == null ||
                _txtAddend2 == null ||
                _txtSum == null ||
                _txtComplexAddend3 == null ||
                _txtComplexSum2 == null ||
                _txtComplexTotalDistance == null)
            {
                return;
            }

            bool isRtl = UsesRtlComplexThroughTenPrompt();
            string firstArrowPath = isRtl
                ? "M 300,132 L 300,76 L 184,76 M 200,64 L 184,76 L 200,88"
                : "M 67,132 L 67,76 L 184,76 M 168,64 L 184,76 L 168,88";
            string secondArrowPath = isRtl
                ? "M 182,132 L 182,76 L 67,76 M 83,64 L 67,76 L 83,88"
                : "M 182,132 L 182,76 L 300,76 M 284,64 L 300,76 L 284,88";
            string totalArrowPath = isRtl
                ? "M 300,132 L 300,18 L 67,18 M 83,6 L 67,18 L 83,30"
                : "M 67,132 L 67,18 L 300,18 L 284,6 M 300,18 L 284,30";

            _complexFirstArrowPath.Data = (Geometry)new PathGeometryConverter().ConvertFromInvariantString(firstArrowPath);
            _complexSecondArrowPath.Data = (Geometry)new PathGeometryConverter().ConvertFromInvariantString(secondArrowPath);
            _complexTotalArrowPath.Data = (Geometry)new PathGeometryConverter().ConvertFromInvariantString(totalArrowPath);

            AbsoluteLayout.SetLayoutBounds(_txtAddend1, new Rect(19, 119, 50, 25));
            AbsoluteLayout.SetLayoutBounds(_txtAddend2, new Rect(86, 61, 50, 25));
            AbsoluteLayout.SetLayoutBounds(_txtSum, new Rect(134, 119, 50, 25));
            AbsoluteLayout.SetLayoutBounds(_txtComplexAddend3, new Rect(201, 61, 50, 25));
            AbsoluteLayout.SetLayoutBounds(_txtComplexSum2, new Rect(250, 119, 50, 25));
            AbsoluteLayout.SetLayoutBounds(_txtComplexTotalDistance, new Rect(134, 0, 50, 25));
        }

        private string GetArrowLabelPromptPathData()
        {
            ArrowLabelExerciseMode mode = GetDisplayedArrowLabelExerciseMode();

            return mode switch
            {
                ArrowLabelExerciseMode.ComplexBridgeToNextTen =>
                    "M 70,72 L 70,36 M 56,50 L 70,36 L 84,50 " +
                    "M 86,36 L 136,36 M 122,22 L 136,36 L 122,50 " +
                    "M 150,36 L 186,36 L 186,72 M 172,58 L 186,72 L 200,58",
                ArrowLabelExerciseMode.ComplexBridgeToAnyNextTen =>
                    "M 70,72 L 70,36 M 56,50 L 70,36 L 84,50 " +
                    "M 86,36 L 136,36 M 122,22 L 136,36 L 122,50 " +
                    "M 150,36 L 186,36 L 186,72 M 172,58 L 186,72 L 200,58",
                ArrowLabelExerciseMode.ComplexLongDistance =>
                    "M 70,72 L 70,36 M 56,50 L 70,36 L 84,50 " +
                    "M 86,36 L 136,36 M 122,22 L 136,36 L 122,50 " +
                    "M 150,36 L 186,36 L 186,72 M 172,58 L 186,72 L 200,58",
                ArrowLabelExerciseMode.EndAndLengthWithMissingStart =>
                    "M 218,72 L 218,28 L 107,28 M 123,14 L 107,28 L 123,42",
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
                ArrowLabelExerciseMode.ComplexBridgeToNextTen => (
                    new Rect(14, 10, 230, 88),
                    new Rect(35, 64, 50, 25),
                    new Rect(102, 0, 58, 25),
                    new Rect(160, 64, 50, 25),
                    false),
                ArrowLabelExerciseMode.ComplexBridgeToAnyNextTen => (
                    new Rect(14, 10, 230, 88),
                    new Rect(35, 64, 50, 25),
                    new Rect(102, 0, 58, 25),
                    new Rect(160, 64, 50, 25),
                    false),
                ArrowLabelExerciseMode.ComplexLongDistance => (
                    new Rect(14, 10, 230, 88),
                    new Rect(35, 64, 50, 25),
                    new Rect(102, 0, 58, 25),
                    new Rect(160, 64, 50, 25),
                    false),
                ArrowLabelExerciseMode.EndAndLengthWithMissingStart => (
                    new Rect(-16, 10, 220, 88),
                    new Rect(41, 64, 50, 25),
                    new Rect(110, 0, 50, 25),
                    new Rect(154, 64, 50, 25),
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

            if (!source.StartsWith("AutoTune", StringComparison.Ordinal))
                _hasManualAnswerTimeOverride = true;

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
                _hasManualAnswerTimeOverride = true;
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

            // Do not place a transparent hit-test surface over the keyboard. On iOS in
            // particular, the native view can keep winning the next touch immediately
            // after it is hidden. Keyboard input itself dismisses the tuner below.
            if (_answerTimeDismissShield != null)
            {
                _answerTimeDismissShield.IsVisible = false;
                _answerTimeDismissShield.InputTransparent = true;
            }

            if (isVisible)
            {
                PositionAnswerTimeTunerCard();
                AttachAnswerTimeOutsideTap();
            }
            else
            {
                DetachAnswerTimeOutsideTap();
            }

            RefreshAnswerTimePanelIcon();
        }

        private void AttachAnswerTimeOutsideTap()
        {
            if (_rootGrid == null)
                return;

            if (_answerTimeOutsideTap == null)
            {
                _answerTimeOutsideTap = new TapGestureRecognizer();
                _answerTimeOutsideTap.Tapped += (_, args) =>
                {
                    if (!_isAnswerTimeTunerVisible || _rootGrid == null)
                        return;

                    Point? tap = args.GetPosition(_rootGrid);
                    if (tap == null)
                        return;

                    if (IsPointInsideAnswerTimeElement(tap.Value, _answerTimeTunerCard as VisualElement) ||
                        IsPointInsideAnswerTimeElement(tap.Value, _btnAnswerTimePanel))
                    {
                        return;
                    }

                    HideAnswerTimeTuner();
                };
            }

            if (!_rootGrid.GestureRecognizers.Contains(_answerTimeOutsideTap))
                _rootGrid.GestureRecognizers.Add(_answerTimeOutsideTap);
        }

        private void DetachAnswerTimeOutsideTap()
        {
            if (_rootGrid != null && _answerTimeOutsideTap != null)
                _rootGrid.GestureRecognizers.Remove(_answerTimeOutsideTap);
        }

        private bool IsPointInsideAnswerTimeElement(Point tap, VisualElement element)
        {
            if (element == null || !element.IsVisible)
                return false;

            PointF origin = GetPointRelativeToRoot(element);
            double width = element.Width > 0 ? element.Width : element.WidthRequest;
            double height = element.Height > 0 ? element.Height : element.HeightRequest;
            return tap.X >= origin.X && tap.X <= origin.X + width &&
                   tap.Y >= origin.Y && tap.Y <= origin.Y + height;
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
            bool isVerticalPrecisionLayout = _config.KeyboardConfig?.IsVerticalPrecisionPinchExercise == true;
            bool isPianoHigh = UsesSyncKeyboardSubmissionMode() &&
                               (_config.UIQuestionType == UIQuestionType.OnlyKeyboard || !_config.KeyboardConfig.KeyboardOnlyForHelp);
            int pianoHeight = _isKeyboard ? (isPianoHigh ? 120 : 80) : 1;
            if (_isKeyboard && _config.KeyboardConfig.IsArrow) pianoHeight = 220;
            Grid grid = new()
            {
                BackgroundColor = Colors.AntiqueWhite,
                RowDefinitions =
            {
                new RowDefinition { Height = isVerticalPrecisionLayout ? GridLength.Auto : new GridLength(40, GridUnitType.Star) },
                new RowDefinition { Height = _isKeyboard && !_config.KeyboardConfig.KeyboardOnlyForHelp ? GridLength.Auto : new GridLength(0) },
                new RowDefinition { Height = isVerticalPrecisionLayout ? GridLength.Star : new GridLength(pianoHeight, GridUnitType.Star) }
            },
                ColumnDefinitions =
            {
                new ColumnDefinition()
            }
            };
            _rootGrid = grid;
            EnsureCorrectExpressionLabel();

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
                if (ShouldStartArrowLabelRetryWithEquation())
                    _arrowEquationPromptView = BuildArrowEquationPromptView();
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

            if (_arrowEquationPromptView != null)
                vsl.Add(_arrowEquationPromptView);
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
                        double actionColumnWidth = _lblAction.IsVisible ? _lblAction.WidthRequest : 0;
                        Grid centeredOperationRow = new()
                        {
                            HorizontalOptions = LayoutOptions.Center,
                            ColumnSpacing = 0,
                            ColumnDefinitions =
                            {
                                new ColumnDefinition { Width = new GridLength(_txtAddend1.WidthRequest) },
                                new ColumnDefinition { Width = new GridLength(actionColumnWidth) },
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
                if (UsesArrowPromptAnswerWithoutMainKeyboard && _correctExpressionLabel != null)
                    vsl.Add(_correctExpressionLabel);
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
            if (UsesArrowPromptAnswerWithoutMainKeyboard)
                Grid.SetRowSpan(vsl, grid.RowDefinitions.Count);

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
                    syncKeyboard.SequenceFirstProgressChanged += ShowSequenceFirstProgressFeedback;
                }

                // Always host the keyboard inside an overlay host so we can run tutorials on-demand
                if (ShouldHostMainKeyboard)
                {
                    _taskMainHost = new KeyboardOverlayHost(_pianoKeyboard);
                    _taskMainHost.IsVisible = !_config.KeyboardConfig.HideMainKeyboard || _isArrowLabelRetryHelpVisible;
                    if (_config.KeyboardConfig.IsPrecisionPinchExercise)
                    {
                        if (!isVerticalPrecisionLayout)
                        {
                            grid.RowDefinitions[0].Height = new GridLength(0);
                            grid.RowDefinitions[1].Height = new GridLength(0);
                        }
                        View precisionKeyboardStage = _config.KeyboardConfig.IsVerticalPrecisionPinchExercise
                            ? BuildVerticalPrecisionMainKeyboard()
                            : BuildHorizontalPrecisionMainKeyboard();
                        grid.Add(precisionKeyboardStage);
                        Grid.SetRow(precisionKeyboardStage, 2);

                        if (isVerticalPrecisionLayout &&
                            (!_config.KeyboardConfig.IsPrecisionShiftExercise ||
                             _config.KeyboardConfig.PrecisionShiftSynchronizeHands ||
                             _config.KeyboardConfig.IsPrecisionGrammarExercise))
                        {
                            _lblAction.IsVisible = true;
                            _lblAction.HorizontalOptions = LayoutOptions.Center;
                            _lblAction.VerticalOptions = LayoutOptions.Center;
                            grid.Add(_lblAction);
                            Grid.SetRow(_lblAction, 0);
                        }
                    }
                    else
                    {
                        grid.Add(_taskMainHost);
                        Grid.SetRow(_taskMainHost, 2);
                    }
                    View keyboardControlBar = BuildKeyboardControlBar();
                    keyboardControlBar.IsVisible = !_config.KeyboardConfig.HideMainKeyboard || _isArrowLabelRetryHelpVisible;
                    grid.Add(keyboardControlBar);
                    Grid.SetRow(keyboardControlBar, 1);

                    if (_config.KeyboardConfig.IsTwoHandCombinationMemorize)
                    {
                        // This is a page-level overlay, not part of the status row or
                        // keyboard layout. It therefore consumes no measure space and
                        // cannot change the keyboard's width or height.
                        Button settingsButton = new()
                        {
                            Text = "⚙",
                            FontSize = 21,
                            WidthRequest = 44,
                            HeightRequest = 44,
                            Padding = 0,
                            CornerRadius = 22,
                            BackgroundColor = Color.FromArgb("#FFF2DF"),
                            TextColor = Color.FromArgb("#A94F16"),
                            BorderColor = Color.FromArgb("#F2C48D"),
                            BorderWidth = 1,
                            HorizontalOptions = LayoutOptions.End,
                            VerticalOptions = LayoutOptions.Start,
                            Margin = new Thickness(0, 12, 12, 0),
                            ZIndex = 1001,
                            AutomationId = "Stage51SettingsButton"
                        };
                        settingsButton.Clicked += async (_, _) =>
                            await Navigation.PushAsync(new GestureSample.Views.TwoHandCombinationSetupPage(
                                _config.KeyboardConfig, this));
                        grid.Add(settingsButton);
                        Grid.SetRow(settingsButton, 0);
                        Grid.SetRowSpan(settingsButton, grid.RowDefinitions.Count);
                    }
                }

                if (CanShowAnswerTimeTuner())
                {
                    _answerTimeDismissShield = new BoxView
                    {
                        BackgroundColor = Colors.Transparent,
                        IsVisible = false,
                        InputTransparent = true,
                        ZIndex = 997
                    };

                    View answerTimeTuner = BuildAnswerTimeTuner();

                    // The first key press after editing the timer must both dismiss the
                    // editor and remain a real keyboard press; no dismiss overlay sits
                    // between the finger and the keys.
                    _pianoKeyboard.KeyPressStarted += HideAnswerTimeTuner;
                    foreach (MR.Gestures.Button keyButton in _pianoKeyboard.KeyButtons)
                        keyButton.Down += (_, _) => HideAnswerTimeTuner();

                    grid.Add(_answerTimeDismissShield);
                    Grid.SetRow(_answerTimeDismissShield, 1);
                    Grid.SetRowSpan(_answerTimeDismissShield, 2);

                    grid.Add(answerTimeTuner);
                    Grid.SetRow(answerTimeTuner, 1);
                    Grid.SetRowSpan(answerTimeTuner, 2);
                }

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
                    if (_taskMainHost == null) return;
                    
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
                    ZIndex = 999,
                    // Only visible header controls consume taps; the wide empty
                    // centre passes through to the keyboard.
                    InputTransparent = true,
                    CascadeInputTransparent = false
                };

                HorizontalStackLayout leftOverlayButtons = new()
                {
                    Spacing = 8,
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Center
                };

                // init button comes from the keyboard now
                if (!_config.KeyboardConfig.HideMainKeyboard &&
                    _pianoKeyboard is PianoKeyboard pk && pk.BtnInit != null)
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

                if (_config.KeyboardConfig.IsPrecisionPinchExercise &&
                    !_config.KeyboardConfig.IsPrecisionSignLearningExercise)
                {
                    Button btnGuideLine = new()
                    {
                        Text = _config.KeyboardConfig.IsVerticalPrecisionPinchExercise ? "┃" : "━",
                        FontSize = 16,
                        WidthRequest = 34,
                        HeightRequest = 34,
                        Padding = 0,
                        CornerRadius = 17,
                        BackgroundColor = Colors.Black.WithAlpha(0.25f),
                        TextColor = _config.KeyboardConfig.ShowPrecisionPinchGuideLine ? Colors.Red : Colors.Gray,
                        HorizontalOptions = LayoutOptions.Start,
                        VerticalOptions = LayoutOptions.Center,
                        Margin = Thickness.Zero
                    };
                    btnGuideLine.Clicked += (_, _) =>
                    {
                        _config.KeyboardConfig.ShowPrecisionPinchGuideLine =
                            !_config.KeyboardConfig.ShowPrecisionPinchGuideLine;
                        btnGuideLine.TextColor = _config.KeyboardConfig.ShowPrecisionPinchGuideLine
                            ? Colors.Red
                            : Colors.Gray;
                        _taskMainHost?.SetPrecisionPinchGuideVisible(
                            _config.KeyboardConfig.ShowPrecisionPinchGuideLine);
                    };
                    leftOverlayButtons.Add(btnGuideLine);
                    _taskMainHost?.SetPrecisionPinchGuideVisible(
                        _config.KeyboardConfig.ShowPrecisionPinchGuideLine);

                    if (_config.KeyboardConfig.IsVerticalPrecisionPinchExercise &&
                        DeviceInfo.Current.Idiom == DeviceIdiom.Tablet)
                    {
                        Microsoft.Maui.Controls.Shapes.Path handGapIcon = new()
                        {
                            Data = (Geometry)new PathGeometryConverter().ConvertFromInvariantString(
                                "M 1,7 L 17,7 M 5,3 L 1,7 L 5,11 M 13,3 L 17,7 L 13,11"),
                            Stroke = Colors.White,
                            StrokeThickness = 1.7,
                            StrokeLineCap = PenLineCap.Round,
                            StrokeLineJoin = PenLineJoin.Round,
                            WidthRequest = 18,
                            HeightRequest = 14,
                            Aspect = Stretch.Uniform,
                            HorizontalOptions = LayoutOptions.Center,
                            VerticalOptions = LayoutOptions.Center,
                            InputTransparent = true
                        };
                        Border btnHandGap = new()
                        {
                            WidthRequest = 34,
                            HeightRequest = 34,
                            Padding = 0,
                            Margin = Thickness.Zero,
                            StrokeThickness = 0,
                            StrokeShape = new RoundRectangle { CornerRadius = 17 },
                            BackgroundColor = Colors.Black.WithAlpha(0.25f),
                            Content = handGapIcon,
                            HorizontalOptions = LayoutOptions.Start,
                            VerticalOptions = LayoutOptions.Center
                        };
                        TapGestureRecognizer handGapTap = new();
                        handGapTap.Tapped += (_, _) => _togglePrecisionHandGapSlider?.Invoke();
                        btnHandGap.GestureRecognizers.Add(handGapTap);
                        _precisionHandGapButton = btnHandGap;
                        leftOverlayButtons.Add(btnHandGap);
                    }
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

                if (isVerticalPrecisionLayout)
                {
                    overlayButtons.Margin = new Thickness(5, 9, 5, 0);
                    overlayButtons.VerticalOptions = LayoutOptions.Start;
                    _taskMainHost?.Children.Add(overlayButtons);
                }
                else if (_taskMainHost != null)
                {
                    _taskMainHost.Children.Add(overlayButtons);
                }
                else if (!_config.KeyboardConfig.HideMainKeyboard)
                {
                    grid.Add(overlayButtons);
                    Grid.SetRow(overlayButtons, 2);
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


        private View BuildHorizontalPrecisionMainKeyboard()
        {
            Slider widthSlider = new()
            {
                Minimum = 280,
                Maximum = 800,
                Value = 600,
                HorizontalOptions = LayoutOptions.Fill
            };
            Label savedLabel = new()
            {
                Text = "Choose your preferred key width",
                FontSize = 12,
                TextColor = Colors.Black,
                HorizontalTextAlignment = TextAlignment.Center
            };
            Border sliderPanel = new()
            {
                IsVisible = false,
                Padding = new Thickness(8, 2),
                BackgroundColor = Colors.White.WithAlpha(0.94f),
                Stroke = Colors.Gray,
                StrokeThickness = 1,
                Content = new VerticalStackLayout
                {
                    Spacing = 0,
                    Children = { widthSlider, savedLabel }
                }
            };

            Button sliderButton = new()
            {
                Text = "↔",
                FontSize = 18,
                WidthRequest = 42,
                HeightRequest = 36,
                Padding = 0,
                HorizontalOptions = LayoutOptions.End,
                BackgroundColor = Colors.White.WithAlpha(0.94f)
            };
            sliderButton.Clicked += (_, _) => sliderPanel.IsVisible = !sliderPanel.IsVisible;
            _pianoKeyboard.KeyPressStarted += () => sliderPanel.IsVisible = false;
            foreach (MR.Gestures.Button keyButton in _pianoKeyboard.KeyButtons)
                keyButton.Down += (_, _) => sliderPanel.IsVisible = false;

            _lblAction.FontSize = 28;
            _lblAction.HorizontalTextAlignment = TextAlignment.Center;
            Grid controls = new()
            {
                Padding = new Thickness(12, 4),
                RowSpacing = 2,
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Auto)
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                }
            };
            controls.Add(_lblAction, 0, 0);
            Grid.SetColumnSpan(_lblAction, 2);
            controls.Add(sliderPanel, 0, 1);
            controls.Add(sliderButton, 1, 1);

            Grid stage = new()
            {
                BackgroundColor = Colors.AntiqueWhite,
                RowSpacing = 4,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Star)
                }
            };
            stage.Add(controls, 0, 0);
            stage.Add(_taskMainHost, 0, 1);
            _taskMainHost.HorizontalOptions = LayoutOptions.Center;
            _taskMainHost.VerticalOptions = LayoutOptions.Fill;
            _pianoKeyboard.VerticalOptions = LayoutOptions.Fill;

            Guid? activeUserId = ServiceHelper.GetService<CurrentUserSession>().ActiveUser?.Id;
            string preferenceKey = $"precision-pinch-keyboard-width-{activeUserId?.ToString() ?? "anonymous"}";
            double savedWidth = Preferences.Default.Get(preferenceKey, -1d);
            bool initialized = false;

            void ApplyWidth(double requestedWidth)
            {
                if (stage.Width <= 0)
                    return;

                double maximum = stage.Width;
                double minimum = Math.Min(260, maximum);
                requestedWidth = Math.Clamp(requestedWidth, minimum, maximum);
                int keyColumns = _config.KeyboardConfig.KeysInRow;
                int visualColumns = keyColumns + 1;
                double chromeWidth = 5 + ((visualColumns - 1) * 5);
                double keyWidth = Math.Max(12, (requestedWidth - chromeWidth) / keyColumns);
                double exactWidth = _pianoKeyboard.SetExactKeyWidth(keyWidth);
                _taskMainHost.WidthRequest = Math.Min(exactWidth, maximum);
                _taskMainHost.MaximumWidthRequest = Math.Min(exactWidth, maximum);
                _taskMainHost.SyncOverlay();
            }

            stage.SizeChanged += (_, _) =>
            {
                if (stage.Width <= 0)
                    return;

                widthSlider.Maximum = stage.Width;
                widthSlider.Minimum = Math.Min(260, stage.Width);
                if (!initialized)
                {
                    initialized = true;
                    widthSlider.Value = savedWidth > 0
                        ? Math.Clamp(savedWidth, widthSlider.Minimum, widthSlider.Maximum)
                        : widthSlider.Maximum;
                }
                ApplyWidth(widthSlider.Value);
            };
            widthSlider.ValueChanged += (_, args) =>
            {
                ApplyWidth(args.NewValue);
                if (!initialized)
                    return;

                Preferences.Default.Set(preferenceKey, args.NewValue);
                savedLabel.Text = $"Preferred width saved ({args.NewValue:0})";
            };

            return stage;
        }

        private static void ApplyArrowDrawableTuning(
            PrecisionShiftInstructionDrawable drawable,
            PrecisionArrowDesignSettings settings)
        {
            drawable.TowardArrowTipFromBase = (float)settings.ArrowTipFromBase;
            drawable.TowardNumberFromBase = (float)settings.NumberFromBase;
            drawable.TowardShaftStopFromBase = (float)settings.ShaftStopFromBase;
        }

        private View BuildPrecisionArrowDesignLabPanel(Action<PrecisionArrowDesignSettings> changed)
        {
            PrecisionArrowDesignSettings settings = PrecisionArrowDesignSettings.Load();
            Grid controls = new()
            {
                ColumnSpacing = 8,
                RowSpacing = 2,
                ColumnDefinitions =
                {
                    new ColumnDefinition(new GridLength(126)),
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(new GridLength(58))
                }
            };

            bool syncing = false;
            int row = 0;
            controls.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            Picker presetPicker = new()
            {
                Title = "Arrow preset",
                ItemsSource = PrecisionArrowDesignSettings.Presets.Select(preset => preset.Name).ToList(),
                HorizontalOptions = LayoutOptions.Fill,
                HeightRequest = 34
            };
            controls.Add(new Label
            {
                Text = "Preset",
                VerticalTextAlignment = TextAlignment.Center,
                FontAttributes = FontAttributes.Bold
            }, 0, row);
            controls.Add(presetPicker, 1, row);
            Grid.SetColumnSpan(presetPicker, 2);
            row++;

            Slider AddTuningSlider(
                string label,
                double minimum,
                double maximum,
                double value,
                Action<double> setter,
                string format)
            {
                controls.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                Label valueLabel = new()
                {
                    Text = value.ToString(format),
                    HorizontalTextAlignment = TextAlignment.End,
                    VerticalTextAlignment = TextAlignment.Center
                };
                Slider slider = new()
                {
                    Minimum = minimum,
                    Maximum = maximum,
                    Value = value,
                    HeightRequest = 30
                };
                slider.ValueChanged += (_, args) =>
                {
                    valueLabel.Text = args.NewValue.ToString(format);
                    setter(args.NewValue);
                    if (syncing)
                        return;
                    settings.Save();
                    changed(settings);
                };
                controls.Add(new Label
                {
                    Text = label,
                    VerticalTextAlignment = TextAlignment.Center,
                    FontSize = 12
                }, 0, row);
                controls.Add(slider, 1, row);
                controls.Add(valueLabel, 2, row);
                row++;
                return slider;
            }

            Slider tipSlider = AddTuningSlider("Arrow position", 0.08, 0.75,
                settings.ArrowTipFromBase, value => settings.ArrowTipFromBase = value, "0.00");
            Slider numberSlider = AddTuningSlider("Number position", 0.12, 0.92,
                settings.NumberFromBase, value => settings.NumberFromBase = value, "0.00");
            Slider stopSlider = AddTuningSlider("Line stop", 0, 0.25,
                settings.ShaftStopFromBase, value => settings.ShaftStopFromBase = value, "0.00");
            Slider gapSlider = AddTuningSlider("Keyboard gap", 0, 60,
                settings.SideGap, value => settings.SideGap = value, "0");
            Slider verticalSlider = AddTuningSlider("Up / down", -180, 180,
                settings.VerticalOffset, value => settings.VerticalOffset = value, "0");

            controls.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            Label keyCountValue = new()
            {
                Text = settings.AdvancedStageKeyCount.ToString(),
                HorizontalTextAlignment = TextAlignment.End,
                VerticalTextAlignment = TextAlignment.Center
            };
            Stepper keyCountStepper = new()
            {
                Minimum = 6,
                Maximum = 12,
                Increment = 1,
                Value = settings.AdvancedStageKeyCount,
                HorizontalOptions = LayoutOptions.Fill
            };
            keyCountStepper.ValueChanged += (_, args) =>
            {
                settings.AdvancedStageKeyCount = (int)Math.Round(args.NewValue);
                keyCountValue.Text = settings.AdvancedStageKeyCount.ToString();
                settings.Save();
            };
            controls.Add(new Label
            {
                Text = "Keys / hand (reopen)",
                VerticalTextAlignment = TextAlignment.Center,
                FontSize = 12
            }, 0, row);
            controls.Add(keyCountStepper, 1, row);
            controls.Add(keyCountValue, 2, row);
            row++;

            presetPicker.SelectedIndexChanged += (_, _) =>
            {
                int index = presetPicker.SelectedIndex;
                if (index < 0 || index >= PrecisionArrowDesignSettings.Presets.Count)
                    return;

                syncing = true;
                settings.Apply(PrecisionArrowDesignSettings.Presets[index]);
                tipSlider.Value = settings.ArrowTipFromBase;
                numberSlider.Value = settings.NumberFromBase;
                stopSlider.Value = settings.ShaftStopFromBase;
                gapSlider.Value = settings.SideGap;
                verticalSlider.Value = settings.VerticalOffset;
                syncing = false;
                settings.Save();
                changed(settings);
            };

            return new Border
            {
                Padding = new Thickness(10, 6),
                Margin = new Thickness(8),
                Stroke = Colors.Gray,
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 10 },
                BackgroundColor = Colors.White.WithAlpha(0.96f),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End,
                MaximumWidthRequest = 620,
                ZIndex = 50,
                Content = controls
            };
        }

        private View BuildVerticalPrecisionMainKeyboard()
        {
            double screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
            double screenHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
            bool phoneLayout = DeviceInfo.Current.Idiom == DeviceIdiom.Phone || screenWidth < 600;
            bool landscapeLayout = screenWidth > screenHeight;
            bool separateHandZones = DeviceInfo.Current.Idiom == DeviceIdiom.Tablet &&
                                     _config.KeyboardConfig.SeparatePrecisionPinchColumnsOnTablet;
            bool combinationMemorizeStage = _config.KeyboardConfig.IsTwoHandCombinationMemorize;
            Guid? activeUserId = ServiceHelper.GetService<CurrentUserSession>().ActiveUser?.Id;
            string handGapPreferenceKey =
                $"precision-pinch-hand-gap-{(combinationMemorizeStage ? "combination-" : string.Empty)}" +
                $"{activeUserId?.ToString() ?? "anonymous"}";
            double configuredColumnGap = combinationMemorizeStage
                ? 2
                : separateHandZones
                    ? Math.Max(2, _config.KeyboardConfig.PrecisionPinchTabletColumnGap)
                    : 2;
            double precisionColumnGap = Math.Clamp(
                Preferences.Default.Get(handGapPreferenceKey, configuredColumnGap), 2, 240);
            _precisionHandGap = precisionColumnGap;
            PrecisionArrowDesignSettings arrowDesign = PrecisionArrowDesignSettings.Load();
            // Keep the two-column keyboard broad, but compact around the screen centre.
            // The controls are overlays and must never participate in centring the keyboard.
            double maximumVerticalKeyWidth = landscapeLayout ? 180 : phoneLayout ? 150 : 180;
            bool showSideInstructions = _config.KeyboardConfig.IsPrecisionShiftExercise;
            bool showResizeSlider = _config.KeyboardConfig.AllowKeyWidthAdjustment;
            bool sliderInKeyboardHeader = showResizeSlider &&
                                          (phoneLayout || DeviceInfo.Current.Idiom == DeviceIdiom.Tablet);
            double instructionWidth = phoneLayout ? 42 : 50;
            double controlsWidth = phoneLayout ? 42 : 48;
            double controlGap = phoneLayout ? 3 : 6;
            double sliderPanelWidth = phoneLayout ? 46 : 52;
            double buttonGap = sliderPanelWidth + controlGap + 8;
            double instructionGap = Math.Max(0, arrowDesign.SideGap);
            double sideClearance = (showResizeSlider ? controlsWidth + controlGap : 0) +
                                   (showSideInstructions ? instructionWidth + instructionGap : 0);
            double initialAvailableKeyWidth = Math.Max(
                36,
                (screenWidth - (sideClearance * 2) - precisionColumnGap - 8) / 2);
            double keyWidth = Math.Min(maximumVerticalKeyWidth, initialAvailableKeyWidth);
            double keyboardWidth = _pianoKeyboard.SetExactKeyWidth(
                keyWidth,
                separatorWidth: precisionColumnGap,
                columnSpacing: 2);
            const double keyboardHeaderHeight = 52;
            _taskMainHost.WidthRequest = keyboardWidth;
            _taskMainHost.HorizontalOptions = LayoutOptions.Center;
            _taskMainHost.VerticalOptions = LayoutOptions.Center;

            Slider heightSlider = new()
            {
                Minimum = 260,
                Maximum = 520,
                Value = 420,
                Rotation = -90,
                WidthRequest = 280,
                HeightRequest = 42,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            Grid sliderTrack = new()
            {
                WidthRequest = sliderPanelWidth,
                HeightRequest = 300,
                Children = { heightSlider }
            };
            TapGestureRecognizer sliderTrackTap = new();
            sliderTrackTap.Tapped += (_, args) =>
            {
                Point? position = args.GetPosition(sliderTrack);
                if (position is null || sliderTrack.Height <= 0)
                    return;

                // The slider is rotated counter-clockwise: its maximum is at the top.
                // Ignore the small end-cap area so a tap maps exactly to the usable track.
                const double endInset = 10;
                double usableHeight = Math.Max(1, sliderTrack.Height - (endInset * 2));
                double y = Math.Clamp(position.Value.Y - endInset, 0, usableHeight);
                double fractionFromBottom = 1 - (y / usableHeight);
                heightSlider.Value = heightSlider.Minimum +
                                     (fractionFromBottom * (heightSlider.Maximum - heightSlider.Minimum));
            };
            sliderTrack.GestureRecognizers.Add(sliderTrackTap);
            Border sliderPanel = new()
            {
                IsVisible = false,
                InputTransparent = true,
                Padding = 2,
                BackgroundColor = Colors.White.WithAlpha(0.94f),
                Stroke = Colors.Gray,
                StrokeThickness = 1,
                Content = sliderTrack,
                HorizontalOptions = LayoutOptions.Center
            };
            TapGestureRecognizer? outsideSliderTap = null;
            void SetSliderPanelVisibility(bool isVisible)
            {
                sliderPanel.IsVisible = isVisible;
                sliderPanel.InputTransparent = !isVisible;

                if (outsideSliderTap == null)
                    return;

                bool isAttached = _rootGrid.GestureRecognizers.Contains(outsideSliderTap);
                if (isVisible && !isAttached)
                    _rootGrid.GestureRecognizers.Add(outsideSliderTap);
                else if (!isVisible && isAttached)
                    _rootGrid.GestureRecognizers.Remove(outsideSliderTap);
            }
            Color sliderIconColor = sliderInKeyboardHeader ? Colors.White : Colors.Black;
            Microsoft.Maui.Controls.Shapes.Path sliderIcon = new()
            {
                Data = (Geometry)new PathGeometryConverter().ConvertFromInvariantString(
                    "M 7,1 L 7,17 M 3,5 L 7,1 L 11,5 M 3,13 L 7,17 L 11,13"),
                Stroke = sliderIconColor,
                StrokeThickness = 1.7,
                StrokeLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
                WidthRequest = 14,
                HeightRequest = 18,
                Aspect = Stretch.Uniform,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                InputTransparent = true
            };
            Border sliderButton = new()
            {
                WidthRequest = sliderInKeyboardHeader ? 34 : 42,
                HeightRequest = sliderInKeyboardHeader ? 34 : 36,
                Padding = 0,
                Margin = Thickness.Zero,
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = sliderInKeyboardHeader ? 17 : 18
                },
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start,
                BackgroundColor = sliderInKeyboardHeader
                    ? Colors.Black.WithAlpha(0.25f)
                    : Colors.White.WithAlpha(0.94f),
                Content = sliderIcon
            };
            TapGestureRecognizer sliderButtonTap = new();
            sliderButton.GestureRecognizers.Add(sliderButtonTap);
            _pianoKeyboard.KeyPressStarted += () => SetSliderPanelVisibility(false);
            foreach (MR.Gestures.Button keyButton in _pianoKeyboard.KeyButtons)
                keyButton.Down += (_, _) => SetSliderPanelVisibility(false);

            Slider handGapSlider = new()
            {
                Minimum = 2,
                Maximum = 240,
                Value = precisionColumnGap,
                WidthRequest = 280,
                HeightRequest = 42,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            Grid handGapSliderTrack = new()
            {
                WidthRequest = 300,
                HeightRequest = sliderPanelWidth,
                Children = { handGapSlider }
            };
            Border handGapSliderPanel = new()
            {
                IsVisible = false,
                InputTransparent = true,
                Padding = 2,
                BackgroundColor = Colors.White.WithAlpha(0.94f),
                Stroke = Colors.Gray,
                StrokeThickness = 1,
                Content = handGapSliderTrack,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start,
                TranslationY = keyboardHeaderHeight + 4,
                ZIndex = 20
            };
            TapGestureRecognizer? handGapOutsideTap = null;
            void SetHandGapSliderVisibility(bool isVisible)
            {
                handGapSliderPanel.IsVisible = isVisible;
                handGapSliderPanel.InputTransparent = !isVisible;
                if (isVisible)
                {
                    SetSliderPanelVisibility(false);
                    handGapSlider.Value = _precisionHandGap;
                }

                if (handGapOutsideTap == null)
                    return;

                bool isAttached = _rootGrid.GestureRecognizers.Contains(handGapOutsideTap);
                if (isVisible && !isAttached)
                    _rootGrid.GestureRecognizers.Add(handGapOutsideTap);
                else if (!isVisible && isAttached)
                    _rootGrid.GestureRecognizers.Remove(handGapOutsideTap);
            }
            _togglePrecisionHandGapSlider = () =>
                SetHandGapSliderVisibility(!handGapSliderPanel.IsVisible);
            sliderButtonTap.Tapped += (_, _) =>
            {
                bool show = !sliderPanel.IsVisible;
                if (show)
                    SetHandGapSliderVisibility(false);
                SetSliderPanelVisibility(show);
            };
            handGapSlider.ValueChanged += (_, args) =>
            {
                double gap = Math.Clamp(args.NewValue, 2, 240);
                _applyPrecisionHandGap?.Invoke(gap);
            };
            handGapOutsideTap = new TapGestureRecognizer();
            handGapOutsideTap.Tapped += (_, args) =>
            {
                if (!handGapSliderPanel.IsVisible)
                    return;

                Point? panelPosition = args.GetPosition(handGapSliderPanel);
                bool insidePanel = panelPosition is Point panelPoint &&
                                   panelPoint.X >= 0 && panelPoint.X <= handGapSliderPanel.Width &&
                                   panelPoint.Y >= 0 && panelPoint.Y <= handGapSliderPanel.Height;
                Point? buttonPosition = _precisionHandGapButton == null
                    ? null
                    : args.GetPosition(_precisionHandGapButton);
                bool insideButton = buttonPosition is Point buttonPoint &&
                                    buttonPoint.X >= 0 && buttonPoint.X <= _precisionHandGapButton!.Width &&
                                    buttonPoint.Y >= 0 && buttonPoint.Y <= _precisionHandGapButton.Height;
                if (!insidePanel && !insideButton)
                    SetHandGapSliderVisibility(false);
            };
            _pianoKeyboard.KeyPressStarted += () => SetHandGapSliderVisibility(false);
            foreach (MR.Gestures.Button keyButton in _pianoKeyboard.KeyButtons)
                keyButton.Down += (_, _) => SetHandGapSliderVisibility(false);

            _lblAction.FontSize = _config.KeyboardConfig.PrecisionShiftBothHands ? 14 : 18;
            _lblAction.FontFamily = "Consolas";
            _lblAction.HorizontalTextAlignment = TextAlignment.Center;
            _lblAction.VerticalTextAlignment = TextAlignment.Center;
            _lblAction.HeightRequest = 26;

            _verticalLeftShiftDrawable = new PrecisionShiftInstructionDrawable { IsVertical = true };
            _verticalRightShiftDrawable = new PrecisionShiftInstructionDrawable { IsVertical = true };
            ApplyArrowDrawableTuning(_verticalLeftShiftDrawable, arrowDesign);
            ApplyArrowDrawableTuning(_verticalRightShiftDrawable, arrowDesign);
            _verticalLeftShiftInstruction = new GraphicsView
            {
                Drawable = _verticalLeftShiftDrawable,
                WidthRequest = instructionWidth,
                HeightRequest = 150,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                InputTransparent = true
            };
            _verticalRightShiftInstruction = new GraphicsView
            {
                Drawable = _verticalRightShiftDrawable,
                WidthRequest = instructionWidth,
                HeightRequest = 150,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                InputTransparent = true
            };

            Grid side = new()
            {
                WidthRequest = sliderPanelWidth + controlGap + controlsWidth,
                VerticalOptions = LayoutOptions.Center,
                // Let the translated container's empty area pass through to the
                // right-hand keys; its visible child controls remain interactive.
                InputTransparent = true,
                CascadeInputTransparent = false,
                ColumnSpacing = 0,
                ColumnDefinitions =
                {
                    new ColumnDefinition(new GridLength(sliderPanelWidth)),
                    new ColumnDefinition(new GridLength(controlGap)),
                    new ColumnDefinition(new GridLength(controlsWidth))
                }
            };
            side.Add(sliderPanel, 0, 0);
            side.Add(sliderButton, 2, 0);
            sliderPanel.ZIndex = 4;
            sliderButton.ZIndex = 5;
            Grid stage = new()
            {
                BackgroundColor = Colors.AntiqueWhite,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                Padding = 0
            };
            outsideSliderTap = new TapGestureRecognizer();
            outsideSliderTap.Tapped += (_, args) =>
            {
                if (!sliderPanel.IsVisible)
                    return;

                Point? position = args.GetPosition(side);
                bool tappedInsideSliderControls =
                    position is Point point &&
                    point.X >= 0 && point.X <= side.Width &&
                    point.Y >= 0 && point.Y <= side.Height;
                if (!tappedInsideSliderControls)
                    SetSliderPanelVisibility(false);
            };
            Grid keyboardCluster = new()
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Start,
                Padding = 0
            };
            keyboardCluster.Children.Add(_taskMainHost);
            keyboardCluster.Children.Add(handGapSliderPanel);
            if (showSideInstructions)
            {
                _lblAction.IsVisible = false;
                keyboardCluster.Children.Add(_verticalLeftShiftInstruction);
                keyboardCluster.Children.Add(_verticalRightShiftInstruction);
            }
            side.HorizontalOptions = LayoutOptions.Center;
            side.VerticalOptions = sliderInKeyboardHeader
                ? LayoutOptions.Start
                : LayoutOptions.Center;
            side.TranslationY = sliderInKeyboardHeader
                ? (keyboardHeaderHeight - sliderButton.HeightRequest) / 2
                : 0;
            side.ZIndex = 3;
            if (showResizeSlider)
                keyboardCluster.Children.Add(side);
            stage.Children.Add(keyboardCluster);

            if (_config.KeyboardConfig.IsPrecisionArrowDesignLab)
            {
                View designPanel = BuildPrecisionArrowDesignLabPanel(settings =>
                {
                    arrowDesign = settings;
                    instructionGap = Math.Max(0, settings.SideGap);
                    ApplyArrowDrawableTuning(_verticalLeftShiftDrawable, settings);
                    ApplyArrowDrawableTuning(_verticalRightShiftDrawable, settings);
                    _verticalLeftShiftInstruction.TranslationY = settings.VerticalOffset;
                    _verticalRightShiftInstruction.TranslationY = settings.VerticalOffset;
                    if (stage.Width > 0)
                        ApplyResponsiveKeyboardWidth(stage.Width);
                    _verticalLeftShiftInstruction.Invalidate();
                    _verticalRightShiftInstruction.Invalidate();
                });
                stage.Children.Add(designPanel);
            }

            string preferenceKey = $"precision-pinch-keyboard-height-{activeUserId?.ToString() ?? "anonymous"}";
            double savedHeight = Preferences.Default.Get(preferenceKey, -1d);
            bool initialized = false;

            void ApplyResponsiveKeyboardWidth(double availableWidth)
            {
                if (availableWidth <= (sideClearance * 2) + 8)
                    return;
                double availablePerKey =
                    (availableWidth - (sideClearance * 2) - precisionColumnGap - 8) / 2;
                double responsiveKeyWidth = Math.Max(36, Math.Min(maximumVerticalKeyWidth, availablePerKey));
                double exactWidth = _pianoKeyboard.SetExactKeyWidth(
                    responsiveKeyWidth,
                    separatorWidth: precisionColumnGap,
                    columnSpacing: 2);
                _taskMainHost.WidthRequest = exactWidth;
                _taskMainHost.MaximumWidthRequest = exactWidth;
                _taskMainHost.SyncOverlay();

                double keyboardEdge = exactWidth / 2;
                const double keyboardCenter = 0;
                _taskMainHost.TranslationX = keyboardCenter;
                if (showSideInstructions)
                {
                    double instructionOffset = keyboardEdge + instructionGap + (instructionWidth / 2);
                    _verticalLeftShiftInstruction.TranslationX = keyboardCenter - instructionOffset;
                    _verticalRightShiftInstruction.TranslationX = keyboardCenter + instructionOffset;
                    _verticalLeftShiftInstruction.TranslationY = arrowDesign.VerticalOffset;
                    _verticalRightShiftInstruction.TranslationY = arrowDesign.VerticalOffset;
                    if (showResizeSlider)
                    {
                        double buttonCenter = sliderInKeyboardHeader
                            ? keyboardCenter + keyboardEdge - 62
                            : keyboardCenter + keyboardEdge + instructionGap +
                              instructionWidth + buttonGap + (controlsWidth / 2);
                        side.TranslationX = buttonCenter -
                                            ((sliderPanelWidth + controlGap + controlsWidth) / 2) +
                                            (controlsWidth / 2);
                    }
                }
                else
                {
                    double buttonCenter = sliderInKeyboardHeader
                        // On phones and iPads, keep the resize button inside the black
                        // keyboard header, just left of Help.
                        ? keyboardCenter + keyboardEdge - 62
                        : keyboardCenter + keyboardEdge + buttonGap + (controlsWidth / 2);
                    side.TranslationX = buttonCenter -
                                        ((sliderPanelWidth + controlGap + controlsWidth) / 2) +
                                        (controlsWidth / 2);
                }
            }

            _applyPrecisionHandGap = requestedGap =>
            {
                precisionColumnGap = Math.Clamp(requestedGap, 2, 240);
                _precisionHandGap = precisionColumnGap;
                Preferences.Default.Set(handGapPreferenceKey, precisionColumnGap);
                double availableWidth = stage.Width > 0 ? stage.Width : screenWidth;
                ApplyResponsiveKeyboardWidth(availableWidth);
            };

            void ApplyHeight(double requestedHeight)
            {
                if (stage.Height <= 0)
                    return;

                double actionHeight = 0;
                double maximum = Math.Max(1, stage.Height - actionHeight);
                double minimum = Math.Min(220, maximum);
                requestedHeight = Math.Clamp(requestedHeight, minimum, maximum);
                double keyHeight = Math.Max(36,
                    (requestedHeight - keyboardHeaderHeight - 5) / _config.KeyboardConfig.Rows);
                double exactHeight = _pianoKeyboard.SetExactKeyHeight(keyHeight);
                _taskMainHost.HeightRequest = Math.Min(exactHeight, maximum);
                keyboardCluster.HeightRequest = _taskMainHost.HeightRequest;
                _taskMainHost.SyncOverlay();
            }

            stage.SizeChanged += (_, _) =>
            {
                if (stage.Height <= 0)
                    return;

                ApplyResponsiveKeyboardWidth(stage.Width);

                double actionHeight = 0;
                double keyboardAreaHeight = Math.Max(1, stage.Height - actionHeight);
                heightSlider.Maximum = keyboardAreaHeight;
                heightSlider.Minimum = Math.Min(220, keyboardAreaHeight);
                double sliderLength = Math.Clamp(keyboardAreaHeight - 40, 260, 420);
                heightSlider.WidthRequest = sliderLength;
                sliderTrack.HeightRequest = sliderLength + 20;
                if (!initialized)
                {
                    initialized = true;
                    heightSlider.Value = savedHeight > 0
                        ? Math.Clamp(savedHeight, heightSlider.Minimum, heightSlider.Maximum)
                        : phoneLayout
                            ? Math.Clamp(keyboardAreaHeight * 0.68, heightSlider.Minimum, heightSlider.Maximum)
                            : heightSlider.Maximum;
                }
                ApplyHeight(heightSlider.Value);
            };
            heightSlider.ValueChanged += (_, args) =>
            {
                ApplyHeight(args.NewValue);
                if (initialized)
                    Preferences.Default.Set(preferenceKey, args.NewValue);
            };

            return stage;
        }

        private VerticalStackLayout InitLogicalKeyboardsUI()
        {
            VerticalStackLayout vsl = new();
            _keyboardTask2 = new PianoKeyboardReadOnly(_config.KeyboardConfig)
            {
                HorizontalOptions = LayoutOptions.Fill,
                HeightRequest = _config.KeyboardConfig.IsVerticalPrecisionPinchExercise ? 300 : PIANO_HEIGHT2
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
            _legacyShiftInstructionDrawable = new PrecisionShiftInstructionDrawable
            {
                IsVertical = false,
                IsShift = true
            };
            _legacyShiftInstructionView = new GraphicsView
            {
                Drawable = _legacyShiftInstructionDrawable,
                HeightRequest = 62,
                WidthRequest = 230,
                HorizontalOptions = LayoutOptions.Center,
                IsVisible = false,
                InputTransparent = true
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
                Rows = _config.KeyboardConfig.IsVerticalPrecisionPinchExercise
                    ? _config.KeyboardConfig.Rows
                    : 1,
                KeysInRow = _config.KeyboardConfig.KeysInRow
            };
            if (_config.UsesCombinedLogicalKeyboard) config1.Rows = 2;
            _keyboardTask1 = new PianoKeyboardReadOnly(config1)
            {
                HorizontalOptions = LayoutOptions.Fill,
                HeightRequest = _config.KeyboardConfig.IsVerticalPrecisionPinchExercise
                    ? 300
                    : (_config.UsesCombinedLogicalKeyboard) ? (PIANO_HEIGHT2 * 2) + 5 : PIANO_HEIGHT2
            };

            _task2Host = new KeyboardOverlayHost(_keyboardTask2);
            _task1Host = new KeyboardOverlayHost(_keyboardTask1);

            // This stage draws its prompt on the interactive keyboard below.
            if (_config.KeyboardConfig.IsPrecisionPinchExercise)
                return vsl;

            View task2View = _task2Host;
            View task1View = _task1Host;
            if (_config.KeyboardConfig.AllowKeyWidthAdjustment)
            {
                bool verticalCalibration = _config.KeyboardConfig.IsVerticalPrecisionPinchExercise;
                Slider widthSlider = new()
                {
                    Minimum = 240,
                    Maximum = 600,
                    Value = 600,
                    Rotation = verticalCalibration ? -90 : 0,
                    WidthRequest = verticalCalibration ? 230 : -1,
                    HeightRequest = verticalCalibration ? 40 : -1,
                    HorizontalOptions = verticalCalibration ? LayoutOptions.End : LayoutOptions.Fill,
                    VerticalOptions = verticalCalibration ? LayoutOptions.Fill : LayoutOptions.Center
                };
                Label widthStatus = new()
                {
                    Text = "Move the slider to choose your preferred key width",
                    FontSize = 12,
                    HorizontalTextAlignment = TextAlignment.Center,
                    TextColor = Colors.Black
                };
                View sliderPanelContent;
                if (verticalCalibration)
                {
                    sliderPanelContent = new Grid
                    {
                        WidthRequest = 48,
                        HeightRequest = 250,
                        Children = { widthSlider }
                    };
                }
                else
                {
                    sliderPanelContent = new VerticalStackLayout
                    {
                        Spacing = 0,
                        Children = { widthSlider, widthStatus }
                    };
                }
                Border widthPanel = new()
                {
                    IsVisible = false,
                    Padding = new Thickness(8, 2),
                    Margin = verticalCalibration
                        ? new Thickness(0, 42, 4, 42)
                        : new Thickness(44, 4, 44, 0),
                    BackgroundColor = Colors.White.WithAlpha(0.92f),
                    Stroke = Colors.Gray,
                    StrokeThickness = 1,
                    Content = sliderPanelContent,
                    VerticalOptions = verticalCalibration ? LayoutOptions.Center : LayoutOptions.Start,
                    HorizontalOptions = verticalCalibration ? LayoutOptions.End : LayoutOptions.Fill,
                    ZIndex = 101
                };

                Button widthButton = new()
                {
                    Text = verticalCalibration ? "↕" : "↔",
                    FontSize = 18,
                    WidthRequest = 40,
                    HeightRequest = 34,
                    Padding = 0,
                    Margin = new Thickness(4),
                    BackgroundColor = Colors.White.WithAlpha(0.92f),
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Start,
                    ZIndex = 102
                };
                widthButton.Clicked += (_, _) =>
                {
                    widthPanel.IsVisible = !widthPanel.IsVisible;
                };

                Grid adjustableTask1;
                if (verticalCalibration)
                {
                    VerticalStackLayout sideControls = new()
                    {
                        WidthRequest = 58,
                        VerticalOptions = LayoutOptions.Fill,
                        Children = { widthButton, widthPanel }
                    };
                    adjustableTask1 = new Grid
                    {
                        HorizontalOptions = LayoutOptions.Center,
                        ColumnSpacing = 4,
                        ColumnDefinitions =
                        {
                            new ColumnDefinition(GridLength.Auto),
                            new ColumnDefinition(GridLength.Auto)
                        }
                    };
                    adjustableTask1.Add(_task1Host, 0, 0);
                    adjustableTask1.Add(sideControls, 1, 0);
                }
                else
                {
                    adjustableTask1 = new Grid
                    {
                        HorizontalOptions = LayoutOptions.Fill,
                        Children = { _task1Host, widthPanel, widthButton }
                    };
                }
                task1View = adjustableTask1;

                if (verticalCalibration)
                {
                    double screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
                    double verticalKeyWidth = Math.Clamp((screenWidth - 148) / 4, 42, 64);
                    double verticalKeyboardWidth1 = _keyboardTask1.SetExactKeyWidth(verticalKeyWidth);
                    double verticalKeyboardWidth2 = _keyboardTask2.SetExactKeyWidth(verticalKeyWidth);
                    _task1Host.WidthRequest = verticalKeyboardWidth1;
                    _task2Host.WidthRequest = verticalKeyboardWidth2;
                    _task1Host.HorizontalOptions = LayoutOptions.Center;
                    _task2Host.HorizontalOptions = LayoutOptions.Center;
                }

                Guid? activeUserId = ServiceHelper.GetService<CurrentUserSession>().ActiveUser?.Id;
                string dimensionName = verticalCalibration ? "height" : "width";
                string preferenceKey = $"precision-pinch-keyboard-{dimensionName}-{activeUserId?.ToString() ?? "anonymous"}";
                double savedWidth = Preferences.Default.Get(preferenceKey, -1d);
                double maximumAvailableWidth = 0;
                bool sliderInitialized = false;
                void ApplyKeyboardDimension(double requestedDimension)
                {
                    if (maximumAvailableWidth <= 0)
                        return;

                    requestedDimension = Math.Clamp(requestedDimension, widthSlider.Minimum, maximumAvailableWidth);
                    if (verticalCalibration)
                    {
                        double nonKeyHeight = 5;
                        double keyHeight = Math.Max(24,
                            (requestedDimension - nonKeyHeight) / _config.KeyboardConfig.Rows);
                        double task1Height = _keyboardTask1.SetExactKeyHeight(keyHeight);
                        double task2Height = _keyboardTask2.SetExactKeyHeight(keyHeight);
                        _task1Host.HeightRequest = task1Height;
                        _task2Host.HeightRequest = task2Height;
                    }
                    else
                    {
                        int keyColumns = _config.KeyboardConfig.KeysInRow;
                        int visualColumns = keyColumns + (keyColumns <= 10 ? 1 : 0);
                        double chromeWidth = (keyColumns <= 10 ? 5 : 0) +
                                             (Math.Max(0, visualColumns - 1) * 5);
                        double keyWidth = Math.Max(12, (requestedDimension - chromeWidth) / keyColumns);
                        double task1Width = _keyboardTask1.SetExactKeyWidth(keyWidth);
                        double task2Width = _keyboardTask2.SetExactKeyWidth(keyWidth);
                        _task1Host.HorizontalOptions = LayoutOptions.Center;
                        _task2Host.HorizontalOptions = LayoutOptions.Center;
                        _task1Host.WidthRequest = task1Width;
                        _task2Host.WidthRequest = task2Width;
                    }
                    _task1Host.SyncOverlay();
                    _task2Host.SyncOverlay();
                }

                adjustableTask1.SizeChanged += (_, _) =>
                {
                    if (adjustableTask1.Width <= 0)
                        return;

                    maximumAvailableWidth = verticalCalibration
                        ? Math.Min(420, DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density * 0.32)
                        : Math.Max(maximumAvailableWidth, adjustableTask1.Width);
                    widthSlider.Maximum = maximumAvailableWidth;
                    widthSlider.Minimum = Math.Min(verticalCalibration ? 180 : 240, maximumAvailableWidth);
                    if (!sliderInitialized)
                    {
                        sliderInitialized = true;
                        widthSlider.Value = savedWidth > 0
                            ? Math.Clamp(savedWidth, widthSlider.Minimum, maximumAvailableWidth)
                            : maximumAvailableWidth;
                    }
                    ApplyKeyboardDimension(widthSlider.Value);
                };
                widthSlider.ValueChanged += (_, args) =>
                {
                    ApplyKeyboardDimension(args.NewValue);
                    if (!sliderInitialized)
                        return;

                    Preferences.Default.Set(preferenceKey, args.NewValue);
                    widthStatus.Text = $"Preferred {dimensionName} saved ({args.NewValue:0})";
                };
            }

            if (_config.KeyboardConfig.IsVerticalPrecisionPinchExercise)
            {
                Label directionArrow = new()
                {
                    Text = "→",
                    FontSize = 30,
                    TextColor = Colors.Black,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center
                };
                _lblAction.FontSize = 15;
                _lblAction.HorizontalTextAlignment = TextAlignment.Center;
                Grid verticalExercise = new()
                {
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Fill,
                    ColumnSpacing = 8,
                    ColumnDefinitions =
                    {
                        new ColumnDefinition(GridLength.Auto),
                        new ColumnDefinition(new GridLength(48)),
                        new ColumnDefinition(GridLength.Auto)
                    }
                };
                verticalExercise.Add(task2View, 0, 0);
                verticalExercise.Add(new VerticalStackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    Spacing = 0,
                    Children = { directionArrow, _lblAction, _logicalColorActionLayout }
                }, 1, 0);
                verticalExercise.Add(task1View, 2, 0);
                vsl.Add(verticalExercise);
            }
            else
            {
                vsl.Add(task2View);
                vsl.Add(new VerticalStackLayout
                {
                    HorizontalOptions = LayoutOptions.Fill,
                    Spacing = 2,
                    Children = { _lblAction, _legacyShiftInstructionView, _logicalColorActionLayout }
                });
                vsl.Add(task1View);
            }
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
                Text = ShouldUseArrowLabelRetryButtons() ? "→" : "Next",
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

            if (ShouldUseArrowLabelRetryButtons())
            {
                _btnArrowLabelRetryHelp = new Button
                {
                    Text = "?",
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = Thickness.Zero
                };
                _btnArrowLabelRetryHelp.Clicked += async (_, _) => await HandleArrowLabelRetryHelpAsync();
                hslBtns.Add(_btnArrowLabelRetryHelp);
            }

            bool showCheckButton = ShouldShowPpwCheckButton();

            bool useArrowLabelRetryButtons = ShouldUseArrowLabelRetryButtons();

            if (useArrowLabelRetryButtons)
            {
                hslBtns.Add(_btnNext);
            }
            else if (_config.NumberOfMistakesToLose < 0 && !_config.IsHistory)
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
