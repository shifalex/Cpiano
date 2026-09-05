using System.Collections;

namespace GestureSample.Maui.Models
{
    internal class PianoKeyboardSync : PianoKeyboard
    {
        protected IDispatcherTimer timer;
        protected int _seconds_pressed = 0;
        private int _secondsPressingToAnswerSetting;
        private int _secondsToAnswer;
        protected virtual bool IS_WHOLE_TIMER => _secondsPressingToAnswerSetting < 0;
        protected virtual int SECONDS_TO_ANSWER => _secondsToAnswer;
        public Func<ExerciseCheckResult, Task>? CheckCompletedAsync { get; set; }
        public event Action<double>? SequenceFirstProgressChanged;
        public virtual bool SupportsAnswerTimeTuner => true;
        public int AnswerTimeSetting => _secondsPressingToAnswerSetting;



        int[] pressCounter;
        protected readonly ProgressBar _pressProgress;
        protected DateTime? _pressStartUtc;
        private bool _isChecking = false;
        private bool _isLifecycleActive = true;
        private bool _isTickRunning;
        private DateTime? _lastCorrectSequenceFirstUtc;
        private string? _lastSequenceCueSignature;
        private DateTime? _holdGestureStartedUtc;
        private DateTime? _lastHoldHintUtc;
        private bool[] _holdGestureBits = Array.Empty<bool>();
        private bool _holdGestureWasCorrect;
        private int _incorrectQuickTapCount;
        public event Action<bool[], int>? HoldHintRequested;
        public event Action? HoldHintCancelled;

        public PianoKeyboardSync(PPWGamePlay gamePlay, Label lblTimer, ProgressBar pressProgress, KeyboardConfig pianoConfig)
            : base(gamePlay, lblTimer, pianoConfig)
        {
            _pressProgress = pressProgress;
            _pressProgress.Progress = 0;
            //_pressProgress.Opacity = 0;
            //TODO:REALLY NOT SURE WHERE IS THE CORRECT PLACE FOR THIS - it should be in the gui - piano should give only text
            //_lblTimer.FontSize = 55;//(_seconds_pressed >= SECONDS_TO_ANSWER) ? 55 : 30;
            _secondsPressingToAnswerSetting = pianoConfig.SecondsPressingToAnswer;
            _secondsToAnswer = Math.Abs(_secondsPressingToAnswerSetting);
            UpdateProgressColor();
            pressCounter  = new int[NUMBER_OF_KEYS + 1];
            TimerInit();
            timer.Start();
        }

        public virtual string SecondsToEnd
        {
            get
            {
                return string.Format("00:0{0}", SECONDS_TO_ANSWER - _seconds_pressed);
            }
        }
        protected bool AnyPressed()
        {
            return !(_addend1 == 0 && _addend2 == 0);
        }

        protected virtual bool HasActiveTimedAnswer()
        {
            return AnyPressed() && !IsGripPracticeStartingCopy();
        }

        private bool UsesStartingGripCue =>
            _pianoConfig.IsGripTransformationPracticeExercise ||
            _pianoConfig.IsTwoHandCombinationMemorize;

        private bool IsGripPracticeStartingCopy() =>
            UsesStartingGripCue &&
            _gamePlay is BitArrayGamePlay gripGame &&
            ToBitArray().SequenceEqual(_pianoConfig.IsTwoHandCombinationMemorize
                ? gripGame.GetSequenceMemorizeFirstPreview()
                : gripGame.BitArrayQuestion);

        protected void ResetProgressVisual()
        {
            _pressStartUtc = null;
            //_seconds_pressed = 0;
            _pressProgress.Progress = 0;
            _pressProgress.Opacity = 0;
            _pressProgress.IsVisible = false;
            _lblTimer.IsVisible = true;

        }

        private void ShowProgressVisual()
        {

            //Console.WriteLine("Before progress bar");
            _pressProgress.IsVisible = true;
            _pressProgress.Opacity = 1;
            _lblTimer.IsVisible = false;
        }

        private void UpdateProgressColor()
        {
            _pressProgress.ProgressColor = IS_WHOLE_TIMER
                ? Color.FromArgb("#FF7A00")
                : Colors.DodgerBlue;
        }

        public virtual void UpdateAnswerTimeSetting(int secondsPressingToAnswer)
        {
            _secondsPressingToAnswerSetting = secondsPressingToAnswer;
            _secondsToAnswer = Math.Abs(secondsPressingToAnswer);
            _pianoConfig.SecondsPressingToAnswer = secondsPressingToAnswer;
            UpdateProgressColor();
            ResetSyncState();
        }
        protected virtual void TimerInit()
        {
            timer = Application.Current.Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromMilliseconds(16);

            timer.Tick += async (s, e) =>
            {
                if (!_isLifecycleActive || _isChecking || _isTickRunning)
                    return;

                _isTickRunning = true;
                try
                {
                    if (!_isLifecycleActive || _isChecking)
                        return;

                    //Debugging.DevLog.Line("What's going on?");

                    if (SECONDS_TO_ANSWER == 0)
                    {
                        ResetProgressVisual();
                        return;
                    }

                    if (!HasActiveTimedAnswer())
                    {
                        ResetProgressVisual();
                        return;
                    }

                    _pressStartUtc ??= DateTime.UtcNow;

                    TimeSpan elapsed = DateTime.UtcNow - _pressStartUtc.Value;
                    double progressDelaySeconds = GetSequenceProgressDelaySeconds();
                    double progressElapsedSeconds = elapsed.TotalSeconds - progressDelaySeconds;
                    if (progressElapsedSeconds <= 0)
                    {
                        HideProgressVisualWithoutResettingTimer();
                        return;
                    }

                    double progress = progressElapsedSeconds / SECONDS_TO_ANSWER;

                    if (progress < 0) progress = 0;
                    if (progress > 1) progress = 1;

                    ShowProgressVisual();
                    _pressProgress.Progress = progress;

                    if (progress >= 1.0 && CanSubmitCurrentSequenceState())
                    {
                        if (TryShowIncorrectSequenceLastWithoutSubmission())
                        {
                            ResetProgressVisual();
                            _pressStartUtc = null;
                            return;
                        }

                        await PianoInitWithTimer();
                    }
                }
                finally
                {
                    _isTickRunning = false;
                }
            };
        }

        protected async Task PianoInitWithTimer()
        {
            if (_isChecking)
                return;

            _isChecking = true;
            CancelHoldHint();
            timer.Stop();
            IsEnabled = true;
            InputTransparent = true;

            try
            {
                // Hide progress immediately so feedback can appear on lblStatus
                ResetProgressVisual();

                ExerciseCheckResult checkResult = await _gamePlay.EvaluateAsync(this);
                if (CheckCompletedAsync != null)
                    await CheckCompletedAsync(checkResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {DateTime.Now:dd/MM/yyyy HH:mm:ss}: Timed keyboard submit failed - {ex}");
                ResetProgressVisual();
                InputTransparent = false;
            }
            finally
            {
                _isChecking = false;
                if (_isLifecycleActive)
                    timer.Start();
            }
        }

        private void HideProgressVisualWithoutResettingTimer()
        {
            _pressProgress.Progress = 0;
            _pressProgress.Opacity = 0;
            _pressProgress.IsVisible = false;
            _lblTimer.IsVisible = true;
        }

        private double GetSequenceProgressDelaySeconds()
        {
            if (!_pianoConfig.IsPrecisionPinchSequenceMemorize)
                return 0;

            // In target-only Stage 5.1 every non-empty grip is an attempt. Let the
            // timer communicate that immediately, even before the target is correct.
            if (_pianoConfig.IsTwoHandCombinationMemorize)
                return 0;

            if (!IsReadySequenceFinalCandidate())
                return double.PositiveInfinity;

            return 0.5;
        }

        protected override async Task OnKeyStateChangedAsync(bool isDown)
        {
            if (_isChecking)
            {
                return;
            }

            UpdateHoldHint(isDown);

            if (isDown)
            {
                // A glide into a new key is timing-equivalent to a fresh key press.
                _pressStartUtc = null;
                _pressProgress.Progress = 0;
            }

            if (isDown && TryAcceptSequenceFirstWithoutSubmission())
                return;

            if (UsesStartingGripCue)
            {
                if (IsGripPracticeStartingCopy())
                {
                    ShowSequenceCue(ToBitArray(), isCorrect: true);
                    ResetProgressVisual();
                    return;
                }
                ClearSequenceCue();
            }

            if (UpdateSequenceFirstProgress() && isDown)
                return;

            if (!isDown)
                return;

            if (!_pianoConfig.AllowImmediateCorrectPrecisionAnswer ||
                !_gamePlay.IsCloseEnough(this, allowedDifferences: 0))
            {
                return;
            }

            if (IsWaitingForSequenceLast())
                return;

            RememberSequenceFirstIfNeeded();
            await PianoInitWithTimer();
        }

        private bool TryAcceptSequenceFirstWithoutSubmission()
        {
            if (!_pianoConfig.IsPrecisionPinchSequenceMemorize ||
                _gamePlay is not BitArrayGamePlay sequenceGame ||
                !sequenceGame.IsSequenceMemorizeFirstResponse())
            {
                return false;
            }

            bool[] pressed = ToBitArray();
            bool[] first = sequenceGame.GetSequenceMemorizeFirstPreview();
            if (!IsCompleteSequenceCombination(pressed, first))
            {
                ClearSequenceCue();
                return false;
            }

            bool isCorrect = _gamePlay.IsCloseEnough(this, allowedDifferences: 0);
            ShowSequenceCue(pressed, isCorrect);
            if (!isCorrect)
                return true;

            _lastCorrectSequenceFirstUtc = DateTime.UtcNow;
            _pressStartUtc = null;
            _pressProgress.Progress = 0;
            if (!sequenceGame.AdvanceSequenceMemorizeToLastResponse())
                return false;

            SequenceFirstProgressChanged?.Invoke(1);
            return true;
        }

        private void UpdateHoldHint(bool isDown)
        {
            // Toggle keyboards retain their answer on release; sequences have their
            // own recognition cues. Neither should teach a sustained physical hold.
            if (SECONDS_TO_ANSWER <= 0 || _pianoConfig.IsMulticolor ||
                _pianoConfig.IsPrecisionPinchSequenceMemorize ||
                this is PianoKeyboardTimedToggle || !_isLifecycleActive)
                return;

            DateTime now = DateTime.UtcNow;
            if (isDown)
            {
                HoldHintCancelled?.Invoke();
                _holdGestureStartedUtc ??= now;
                _holdGestureBits = ToBitArray();
                // Capture correctness before releasing keys clears the answer.
                _holdGestureWasCorrect = _gamePlay.IsCloseEnough(this, allowedDifferences: 0);
                return;
            }

            if (AnyPressed())
                return;

            DateTime? started = _holdGestureStartedUtc;
            _holdGestureStartedUtc = null;
            if (!started.HasValue || (now - started.Value).TotalMilliseconds > 450 ||
                !_holdGestureBits.Any(bit => bit))
                return;

            if (_holdGestureWasCorrect)
                _incorrectQuickTapCount = 0;
            else if (++_incorrectQuickTapCount < 3)
                return;

            if (_lastHoldHintUtc.HasValue && (now - _lastHoldHintUtc.Value).TotalSeconds < 8)
                return;

            // This is guidance only: never evaluate or submit the released answer.
            _lastHoldHintUtc = now;
            _incorrectQuickTapCount = 0;
            HoldHintRequested?.Invoke(_holdGestureBits, SECONDS_TO_ANSWER);
        }

        private void CancelHoldHint()
        {
            _holdGestureStartedUtc = null;
            _holdGestureBits = Array.Empty<bool>();
            _holdGestureWasCorrect = false;
            HoldHintCancelled?.Invoke();
        }

        protected override async Task<bool> OnBeforeKeyUpAsync()
        {
            if (_pianoConfig.IsTwoHandCombinationMemorize)
                return false;

            if (!IsReadySequenceFinalCandidate())
            {
                return false;
            }

            if (TryShowIncorrectSequenceLastWithoutSubmission())
            {
                ResetProgressVisual();
                _pressStartUtc = null;
                return false;
            }

            await PianoInitWithTimer();
            return true;
        }

        private bool TryShowIncorrectSequenceLastWithoutSubmission()
        {
            if (_pianoConfig.IsTwoHandCombinationMemorize)
                return false;

            if (!IsReadySequenceFinalCandidate() ||
                _gamePlay.IsCloseEnough(this, allowedDifferences: 0))
            {
                return false;
            }

            ShowSequenceCue(ToBitArray(), isCorrect: false);
            return true;
        }

        private bool UpdateSequenceFirstProgress()
        {
            if (!IsWaitingForSequenceLast() ||
                _gamePlay is not BitArrayGamePlay sequenceGame)
            {
                return false;
            }

            bool[] pressed = ToBitArray();
            bool[] first = sequenceGame.GetSequenceMemorizeFirstPreview();
            if (!IsCompleteSequenceCombination(pressed, first))
            {
                ClearSequenceCue();
                return false;
            }

            if (!pressed.SequenceEqual(first))
            {
                ClearSequenceCue();
                return false;
            }

            ShowSequenceCue(pressed, isCorrect: true);
            _lastCorrectSequenceFirstUtc = DateTime.UtcNow;
            _pressStartUtc = null;
            _pressProgress.Progress = 0;
            return true;
        }

        private static bool IsCompleteSequenceCombination(bool[] pressed, bool[] first)
        {
            int expectedCount = first.Count(bit => bit);
            return expectedCount > 0 && pressed.Count(bit => bit) == expectedCount;
        }

        private void ShowSequenceCue(bool[] pressed, bool isCorrect)
        {
            string signature = string.Concat(pressed.Select(bit => bit ? '1' : '0'));
            if (signature == _lastSequenceCueSignature)
                return;

            _lastSequenceCueSignature = signature;
            SequenceFirstProgressChanged?.Invoke(isCorrect ? 1 : -1);
        }

        private void ClearSequenceCue()
        {
            if (_lastSequenceCueSignature == null)
                return;

            _lastSequenceCueSignature = null;
            SequenceFirstProgressChanged?.Invoke(0);
        }

        private void RememberSequenceFirstIfNeeded()
        {
            if (_pianoConfig.IsPrecisionPinchSequenceMemorize &&
                _gamePlay is BitArrayGamePlay sequenceGame &&
                sequenceGame.IsSequenceMemorizeFirstResponse())
            {
                _lastCorrectSequenceFirstUtc = DateTime.UtcNow;
            }
        }

        private bool IsWaitingForSequenceLast()
        {
            return _pianoConfig.IsPrecisionPinchSequenceMemorize &&
                   _gamePlay is BitArrayGamePlay sequenceGame &&
                   !sequenceGame.IsSequenceMemorizeFirstResponse();
        }

        private bool HasRecentSequenceFirst()
        {
            int seconds = Math.Max(1, _pianoConfig.PrecisionSequenceRecognitionWindowSeconds);
            return _lastCorrectSequenceFirstUtc.HasValue &&
                   DateTime.UtcNow - _lastCorrectSequenceFirstUtc.Value <= TimeSpan.FromSeconds(seconds);
        }

        private bool CanSubmitCurrentSequenceState()
        {
            return !_pianoConfig.IsPrecisionPinchSequenceMemorize ||
                   _pianoConfig.IsTwoHandCombinationMemorize ||
                   IsReadySequenceFinalCandidate();
        }

        private bool IsReadySequenceFinalCandidate()
        {
            if (!IsWaitingForSequenceLast() ||
                !HasRecentSequenceFirst() ||
                _gamePlay is not BitArrayGamePlay sequenceGame)
            {
                return false;
            }

            bool[] pressed = ToBitArray();
            bool[] first = sequenceGame.GetSequenceMemorizeFirstPreview();
            return IsCompleteSequenceCombination(pressed, first) &&
                   !pressed.SequenceEqual(first);
        }

        public void SetLifecycleActive(bool active)
        {
            CancelHoldHint();
            _isLifecycleActive = active;
            if (!active)
            {
                timer?.Stop();
                _pressStartUtc = null;
                _seconds_pressed = 0;
                _isTickRunning = false;
                ResetProgressVisual();
                InputTransparent = true;
                return;
            }

            InputTransparent = false;
            if (!_isChecking && timer != null && !timer.IsRunning)
                timer.Start();
        }

        public void NotifyQuestionReadyForInput()
        {
            if (!_isLifecycleActive)
                return;

            _isChecking = false;
            if (_pianoConfig.IsTwoHandCombinationMemorize && IsWaitingForSequenceLast())
                _lastCorrectSequenceFirstUtc = DateTime.UtcNow;
            InputTransparent = false;
            if (timer != null && !timer.IsRunning)
                timer.Start();
        }

        public override void PianoInit()
        {
            base.PianoInit();
            ResetSyncState();
        }

        private void ResetSyncState()
        {
            CancelHoldHint();
            _incorrectQuickTapCount = 0;
            _lastHoldHintUtc = null;
            _seconds_pressed = 0;
            _pressStartUtc = null;
            _lastSequenceCueSignature = null;
            _isChecking = false;
            InputTransparent = false;
            for (int i = 0; i < pressCounter.Length; i++)
                pressCounter[i] = 0;

            ResetProgressVisual();
        }

        //ArrayList<MR.Gestures.Button> twicePressedKeys = new();

        protected override bool InnerKeyDown(MR.Gestures.Button sender)
        {
            int keyIndex = Convert.ToInt32(sender.CommandParameter) - 1;

            if (_pianoConfig.IsMulticolor)
            {
                if (!IS_WHOLE_TIMER)
                {
                    _seconds_pressed = 0;
                    _pressStartUtc = null;
                    _pressProgress.Progress = 0;
                }

                if (sender.BackgroundColor == COLOR_FREE)
                    sender.BackgroundColor = COLOR_PRESSED;
                else if (sender.BackgroundColor == COLOR_PRESSED)
                    sender.BackgroundColor = SECOND_COLOR;
                else if (sender.BackgroundColor == SECOND_COLOR)
                    sender.BackgroundColor = COLOR_FREE;
                else
                    sender.BackgroundColor = COLOR_FREE;

                bool hadPressedBeforeColor = AnyPressed();
                if (!hadPressedBeforeColor)
                    _pressStartUtc = DateTime.UtcNow;

                return true;
            }

            if (UsePermutationTraceColors())
            {
                if (sender.BackgroundColor == COLOR_PRESSED)
                {
                    pressCounter[keyIndex]++;
                    return true;
                }

                if (!IS_WHOLE_TIMER)
                {
                    _seconds_pressed = 0;
                    _pressStartUtc = null;
                    _pressProgress.Progress = 0;
                }

                bool hadPressedBeforeTrace = AnyPressed();
                SetPermutationTracePressed(sender);

                if (Convert.ToInt32(sender.CommandParameter) > 5)
                    _addend2++;
                else
                    _addend1++;

                if (!hadPressedBeforeTrace)
                    _pressStartUtc = DateTime.UtcNow;

                return true;
            }

            if (sender.BackgroundColor == COLOR_PRESSED)
            {
                if (pressCounter[keyIndex] == 0)
                {
                    sender.BackgroundColor = COLOR_FREE;

                    if (Convert.ToInt32(sender.CommandParameter) > 5)
                        _addend2--;
                    else
                        _addend1--;

                    if (_addend1 < 0) _addend1 = 0;
                    if (_addend2 < 0) _addend2 = 0;

                    if (!AnyPressed())
                        ResetProgressVisual();

                    return true;
                }

                pressCounter[keyIndex]++;
                return true;
            }
            if(!IS_WHOLE_TIMER)
            {
                _seconds_pressed = 0;
                _pressStartUtc = null;
                _pressProgress.Progress = 0;
            }
            sender.BackgroundColor = COLOR_PRESSED;

            bool hadPressedBefore = AnyPressed();

            sender.BackgroundColor = COLOR_PRESSED;

            if (Convert.ToInt32(sender.CommandParameter) > 5)
                _addend2++;
            else
                _addend1++;

            if (!hadPressedBefore)
                _pressStartUtc = DateTime.UtcNow;

            return true;
        }
        protected override bool InnerKeyUp(MR.Gestures.Button sender)
        {
            if (_pianoConfig.IsMulticolor)
            {
                return true;
            }

            if (UsePermutationTraceColors())
            {
                if (sender.BackgroundColor != COLOR_PRESSED)
                {
                    if (pressCounter[Convert.ToInt32(sender.CommandParameter) - 1] > 0)
                        pressCounter[Convert.ToInt32(sender.CommandParameter) - 1]--;
                    return true;
                }

                if (!IS_WHOLE_TIMER)
                {
                    _seconds_pressed = 0;
                    _pressStartUtc = null;
                    _pressProgress.Progress = 0;
                }

                ReleasePermutationTracePressed(sender);

                if (Convert.ToInt32(sender.CommandParameter) > 5)
                    _addend2--;
                else
                    _addend1--;

                if (_addend1 < 0) _addend1 = 0;
                if (_addend2 < 0) _addend2 = 0;

                if (!AnyPressed())
                {
                    ResetProgressVisual();
                }

                return true;
            }

            if (sender.BackgroundColor == COLOR_FREE)
            {
               if(pressCounter[Convert.ToInt32(sender.CommandParameter)-1] > 0) pressCounter[Convert.ToInt32(sender.CommandParameter)-1]--; return true;
            }
            if (!IS_WHOLE_TIMER)
            {
                _seconds_pressed = 0;
                _pressStartUtc = null;
                _pressProgress.Progress = 0;
            }//OnPropertyChanged(nameof(SecondsToEnd));  
            sender.BackgroundColor = COLOR_FREE;

            if (Convert.ToInt32(sender.CommandParameter) > 5)
                _addend2--;
            else
                _addend1--;

            //TODO:make closingButonHandle private function  
            if (_addend1 < 0) _addend1 = 0;
            if (_addend2 < 0) _addend2 = 0;

            if (!AnyPressed())
            {
                ResetProgressVisual();
            }

            return true;
        }
    }
}
