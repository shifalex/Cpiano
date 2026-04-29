using GestureSample.Maui.Data.SQLite;
using GestureSample.Maui;
using GestureSample.Maui.Models;

namespace GestureSample.Views
{
    public sealed class KeyboardReplayPage : ContentPage
    {
        private sealed class ReplayFrame
        {
            public DateTime Timestamp { get; init; }
            public bool[] State { get; init; } = Array.Empty<bool>();
            public List<KeyEvent> MarkerEvents { get; init; } = new();
            public string EventLabel { get; init; } = string.Empty;
            public string TimerRegimeText { get; init; } = "Unknown";
        }

        private readonly IReadOnlyList<KeyEvent> _events;
        private readonly IReadOnlyList<TimerChangeEvent> _timerEvents;
        private readonly PianoKeyboardReadOnly _replayKeyboard;
        private readonly AbsoluteLayout _overlayLayer;
        private readonly Label _eventLabel;
        private readonly Label _timerRegimeLabel;
        private readonly Button _replayButton;
        private readonly Button _speedButton;
        private readonly Slider _timelineSlider;
        private readonly Label _timelineLabel;
        private readonly bool[] _initialState;
        private readonly KeyboardQuestion? _question;
        private readonly bool[]? _finalReplayState;
        private readonly string _initialTimerRegimeText;
        private readonly Dictionary<int, BoxView> _activeMarkers = new();
        private readonly Dictionary<int, Stack<int>> _activeMarkerTokensByKey = new();
        private readonly Dictionary<int, KeyEvent> _activeMarkerEventsByKey = new();
        private readonly Color[] _markerPalette =
        {
            Colors.OrangeRed,
            Colors.DeepSkyBlue,
            Colors.MediumSeaGreen,
            Colors.Goldenrod,
            Colors.MediumPurple
        };
        private int _nextMarkerToken;
        private bool _isPlaying;
        private bool _hasAutoPlayed;
        private bool _slowReplayEnabled;
        private bool _isApplyingTimelineValue;
        private int _playbackVersion;
        private int _currentFrameIndex;
        private List<ReplayFrame> _replayFrames = new();
        private List<KeyEvent>? _submitMarkerSnapshot;
        private bool[]? _submitStateSnapshot;
        private KeyEvent? _submitFinalKeyPressEvent;

        public KeyboardReplayPage(
            string title,
            IReadOnlyList<KeyEvent> events,
            KeyboardQuestion? question = null,
            KeyboardConfig? keyboardConfig = null,
            bool[]? finalReplayState = null,
            string? timerRegimeText = null,
            IReadOnlyList<TimerChangeEvent>? timerEvents = null)
        {
            Title = title;
            _events = events?.OrderBy(item => item.EventTime).ThenBy(item => item.id).ToList() ?? new List<KeyEvent>();
            _timerEvents = timerEvents?.OrderBy(item => item.EventTime).ThenBy(item => item.Id).ToList() ?? new List<TimerChangeEvent>();
            _question = question;
            _finalReplayState = finalReplayState?.ToArray();
            _initialTimerRegimeText = string.IsNullOrWhiteSpace(timerRegimeText) ? "Unknown" : timerRegimeText;

            KeyboardConfig replayConfig = question?.CreateKeyboardConfig() ?? keyboardConfig ?? new KeyboardConfig();

            _replayKeyboard = new PianoKeyboardReadOnly(replayConfig)
            {
                HeightRequest = 110,
                HorizontalOptions = LayoutOptions.Fill
            };

            _overlayLayer = new AbsoluteLayout
            {
                InputTransparent = true
            };

            _eventLabel = new Label
            {
                Text = _events.Count == 0 ? "No saved strokes for this question." : "Ready to replay.",
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = Colors.Black
            };

            _timerRegimeLabel = new Label
            {
                Text = $"Answer Time: {_initialTimerRegimeText}",
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = Colors.DimGray,
                FontSize = 13
            };

            _replayButton = new Button
            {
                Text = "Replay",
                HorizontalOptions = LayoutOptions.Center,
                IsEnabled = _events.Count > 0
            };
            _replayButton.Clicked += async (_, _) => await ReplayAsync();
            _speedButton = new Button
            {
                Text = GetSpeedButtonText(),
                HorizontalOptions = LayoutOptions.Center
            };
            _speedButton.Clicked += (_, _) =>
            {
                _slowReplayEnabled = !_slowReplayEnabled;
                _speedButton.Text = GetSpeedButtonText();
            };
            _timelineSlider = new Slider
            {
                Minimum = 0,
                Maximum = 0,
                Value = 0
            };
            _timelineSlider.ValueChanged += OnTimelineSliderValueChanged;
            _timelineLabel = new Label
            {
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = Colors.DimGray,
                FontSize = 12
            };

            Grid replayGrid = new();
            replayGrid.Children.Add(_replayKeyboard);
            replayGrid.Children.Add(_overlayLayer);

            VerticalStackLayout promptLayout = BuildPromptLayout();

            Content = new ScrollView
            {
                Content = new VerticalStackLayout
                {
                    Padding = 16,
                    Spacing = 12,
                    Children =
                    {
                        promptLayout,
                        new Label
                        {
                            Text = "Replay",
                            FontAttributes = FontAttributes.Bold
                        },
                        _timerRegimeLabel,
                        replayGrid,
                        _eventLabel,
                        _timelineSlider,
                        _timelineLabel,
                        new HorizontalStackLayout
                        {
                            HorizontalOptions = LayoutOptions.Center,
                            Spacing = 12,
                            Children =
                            {
                                _replayButton,
                                _speedButton
                            }
                        }
                    }
                }
            };

            _initialState = new bool[_replayKeyboard.KeyCount];
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_hasAutoPlayed || _events.Count == 0)
                return;

            EnsureReplayFramesBuilt();
            _hasAutoPlayed = true;
            await WaitForReplayKeyboardReadyAsync();
            await Task.Delay(80);
            await ReplayAsync();
        }

        private async Task ReplayAsync()
        {
            EnsureReplayFramesBuilt();

            if (_isPlaying)
            {
                _playbackVersion++;
                _isPlaying = false;
                _replayButton.Text = "Replay";
                return;
            }

            if (_replayFrames.Count <= 1)
                return;

            _isPlaying = true;
            _replayButton.Text = "Stop";
            int playbackVersion = ++_playbackVersion;

            try
            {
                await WaitForReplayKeyboardReadyAsync();
                if (_currentFrameIndex >= _replayFrames.Count - 1)
                {
                    ApplyReplayFrame(0);
                }

                for (int frameIndex = Math.Max(1, _currentFrameIndex + 1); frameIndex < _replayFrames.Count; frameIndex++)
                {
                    if (playbackVersion != _playbackVersion)
                        return;

                    ReplayFrame previousFrame = _replayFrames[Math.Max(0, frameIndex - 1)];
                    ReplayFrame nextFrame = _replayFrames[frameIndex];
                    int delay = (int)Math.Clamp((nextFrame.Timestamp - previousFrame.Timestamp).TotalMilliseconds, 30, 900);
                    await Task.Delay(GetReplayDelay(delay));

                    if (playbackVersion != _playbackVersion)
                        return;

                    ApplyReplayFrame(frameIndex);
                }

                await Task.Delay(GetReplayDelay(500));
            }
            finally
            {
                _isPlaying = false;
                _replayButton.Text = "Replay";
            }
        }

        private void EnsureReplayFramesBuilt()
        {
            if (_replayFrames.Count > 0)
                return;

            _replayFrames = BuildReplayFrames();
            _timelineSlider.Maximum = Math.Max(0, _replayFrames.Count - 1);
            ApplyReplayFrame(0);
        }

        private List<ReplayFrame> BuildReplayFrames()
        {
            List<ReplayFrame> frames = new();
            bool[] state = _initialState.ToArray();
            Dictionary<int, Stack<KeyEvent>> activeMarkersByKey = new();
            string currentTimerText = _initialTimerRegimeText;
            string currentEventLabel = _events.Count == 0 ? "No saved strokes for this question." : "Ready to replay.";
            DateTime initialTime = _events.FirstOrDefault()?.EventTime
                ?? _question?.Time
                ?? DateTime.Now;

            frames.Add(CreateReplayFrame(initialTime, state, activeMarkersByKey, currentEventLabel, currentTimerText));

            int timerEventIndex = 0;
            int? submittedAttemptNumber = null;
            int? currentAttemptNumber = null;
            bool clearOnNextAttemptInput = false;

            foreach (KeyEvent keyEvent in _events)
            {
                if (ShouldResetForAttemptBoundary(currentAttemptNumber, keyEvent.AttemptNumber))
                {
                    submittedAttemptNumber = null;
                    clearOnNextAttemptInput = true;
                }

                while (timerEventIndex < _timerEvents.Count &&
                       _timerEvents[timerEventIndex].EventTime <= keyEvent.EventTime)
                {
                    currentTimerText = FormatTimerSetting(_timerEvents[timerEventIndex].NewSetting);
                    frames.Add(CreateReplayFrame(_timerEvents[timerEventIndex].EventTime, state, activeMarkersByKey, currentEventLabel, currentTimerText));
                    timerEventIndex++;
                }

                if (ShouldSkipPostSubmitEvent(keyEvent, submittedAttemptNumber))
                    continue;

                if (clearOnNextAttemptInput && keyEvent.EventType != 2)
                {
                    Array.Fill(state, false);
                    activeMarkersByKey.Clear();
                    clearOnNextAttemptInput = false;
                }

                currentAttemptNumber = keyEvent.AttemptNumber;

                if (keyEvent.EventType == 2)
                {
                    submittedAttemptNumber = keyEvent.AttemptNumber;
                    currentEventLabel = FormatEventLabel(keyEvent);
                    frames.Add(CreateReplayFrame(keyEvent.EventTime, state, activeMarkersByKey, currentEventLabel, currentTimerText));
                    continue;
                }

                ApplyEventToState(state, activeMarkersByKey, keyEvent);
                currentEventLabel = FormatEventLabel(keyEvent);
                frames.Add(CreateReplayFrame(keyEvent.EventTime, state, activeMarkersByKey, currentEventLabel, currentTimerText));
            }

            while (timerEventIndex < _timerEvents.Count)
            {
                currentTimerText = FormatTimerSetting(_timerEvents[timerEventIndex].NewSetting);
                frames.Add(CreateReplayFrame(_timerEvents[timerEventIndex].EventTime, state, activeMarkersByKey, currentEventLabel, currentTimerText));
                timerEventIndex++;
            }

            return frames;
        }

        private static ReplayFrame CreateReplayFrame(
            DateTime timestamp,
            bool[] state,
            Dictionary<int, Stack<KeyEvent>> activeMarkersByKey,
            string eventLabel,
            string timerRegimeText)
        {
            return new ReplayFrame
            {
                Timestamp = timestamp,
                State = state.ToArray(),
                MarkerEvents = activeMarkersByKey
                    .SelectMany(item => item.Value.Reverse())
                    .OrderBy(item => item.EventTime)
                    .ThenBy(item => item.id)
                    .ToList(),
                EventLabel = eventLabel,
                TimerRegimeText = timerRegimeText
            };
        }

        private static void ApplyEventToState(bool[] state, Dictionary<int, Stack<KeyEvent>> activeMarkersByKey, KeyEvent keyEvent)
        {
            int keyIndex = keyEvent.KeyNumber - 1;
            if (keyIndex >= 0 && keyIndex < state.Length)
            {
                if (keyEvent.EventType == 1)
                    state[keyIndex] = true;
                else if (keyEvent.EventType == 0)
                    state[keyIndex] = false;
                else if (keyEvent.EventType == 3)
                    Array.Fill(state, false);
            }

            switch (keyEvent.EventType)
            {
                case 1:
                    if (!activeMarkersByKey.TryGetValue(keyEvent.KeyNumber, out Stack<KeyEvent>? markerStack))
                    {
                        markerStack = new Stack<KeyEvent>();
                        activeMarkersByKey[keyEvent.KeyNumber] = markerStack;
                    }
                    markerStack.Push(keyEvent);
                    break;
                case 0:
                    if (activeMarkersByKey.TryGetValue(keyEvent.KeyNumber, out Stack<KeyEvent>? removeStack) && removeStack.Count > 0)
                    {
                        removeStack.Pop();
                        if (removeStack.Count == 0)
                            activeMarkersByKey.Remove(keyEvent.KeyNumber);
                    }
                    break;
                case 3:
                    activeMarkersByKey.Clear();
                    break;
            }
        }

        private void ApplyReplayFrame(int frameIndex)
        {
            if (frameIndex < 0 || frameIndex >= _replayFrames.Count)
                return;

            ReplayFrame frame = _replayFrames[frameIndex];
            _currentFrameIndex = frameIndex;

            RenderReplayState(frame.State, highlightPressedKeys: true);
            ClearActiveMarkers();
            foreach (KeyEvent markerEvent in frame.MarkerEvents)
                AddActiveMarker(markerEvent);

            _eventLabel.Text = frame.EventLabel;
            _timerRegimeLabel.Text = $"Answer Time: {frame.TimerRegimeText}";

            _isApplyingTimelineValue = true;
            _timelineSlider.Value = frameIndex;
            _timelineLabel.Text = $"Frame {frameIndex + 1}/{_replayFrames.Count}";
            _isApplyingTimelineValue = false;
        }

        private void OnTimelineSliderValueChanged(object? sender, ValueChangedEventArgs e)
        {
            if (_isApplyingTimelineValue || _replayFrames.Count == 0)
                return;

            _playbackVersion++;
            _isPlaying = false;
            _replayButton.Text = "Replay";

            int frameIndex = (int)Math.Round(e.NewValue);
            ApplyReplayFrame(frameIndex);
        }

        private static bool ShouldResetForAttemptBoundary(int? currentAttemptNumber, int nextAttemptNumber)
        {
            if (!currentAttemptNumber.HasValue)
                return false;

            if (currentAttemptNumber.Value == nextAttemptNumber)
                return false;

            return currentAttemptNumber.Value > 0 || nextAttemptNumber > 0;
        }

        private static bool ShouldSkipPostSubmitEvent(KeyEvent nextEvent, int? submittedAttemptNumber)
        {
            if (!submittedAttemptNumber.HasValue)
                return false;

            if (submittedAttemptNumber.Value != nextEvent.AttemptNumber)
                return false;

            return nextEvent.EventType != 2;
        }

        private int GetReplayDelay(int baseDelay)
        {
            return _slowReplayEnabled ? baseDelay * 3 : baseDelay;
        }

        private string GetSpeedButtonText()
        {
            return _slowReplayEnabled ? "Speed: Slow x3" : "Speed: Normal";
        }

        private async Task WaitForReplayKeyboardReadyAsync()
        {
            if (IsReplayKeyboardReady())
                return;

            TaskCompletionSource layoutReady = new();
            EventHandler? sizeChangedHandler = null;
            sizeChangedHandler = (_, _) =>
            {
                if (!IsReplayKeyboardReady())
                    return;

                _replayKeyboard.SizeChanged -= sizeChangedHandler;
                layoutReady.TrySetResult();
            };

            _replayKeyboard.SizeChanged += sizeChangedHandler;

            await Task.WhenAny(layoutReady.Task, Task.Delay(1200));
            _replayKeyboard.SizeChanged -= sizeChangedHandler;

            if (!IsReplayKeyboardReady())
                await Task.Delay(120);
        }

        private bool IsReplayKeyboardReady()
        {
            return _replayKeyboard.KeyButtons.Count > 0 &&
                   _replayKeyboard.KeyButtons.All(button => button.Width > 0 && button.Height > 0);
        }

        private static string FormatEventLabel(KeyEvent keyEvent)
        {
            return $"{keyEvent.EventTimeText}  {keyEvent.EventTypeText}  key {keyEvent.KeyNumber}  x {keyEvent.RelativeXText}  y {keyEvent.RelativeYText}";
        }

        private static string FormatTimerSetting(int setting)
        {
            if (setting == 0)
                return "Off";

            int seconds = Math.Abs(setting);
            string mode = setting < 0 ? "Whole Answer" : "After Last Key";
            return $"{seconds}s | {mode}";
        }

        private void RenderReplayState(bool[] state, bool highlightPressedKeys)
        {
            if (highlightPressedKeys)
            {
                _replayKeyboard.PianoInit(state.ToArray());
                return;
            }

            Color[] colors = new Color[state.Length];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = Colors.White;

            _replayKeyboard.PianoInit(colors);
        }

        private VerticalStackLayout BuildPromptLayout()
        {
            VerticalStackLayout promptLayout = new()
            {
                Spacing = 8
            };

            if (_question == null || _question.keyboard1 == null || _question.keyboard1.Length == 0)
            {
                promptLayout.IsVisible = false;
                return promptLayout;
            }

            promptLayout.Children.Add(new Label
            {
                Text = "Question",
                FontAttributes = FontAttributes.Bold
            });

            promptLayout.Children.Add(new HorizontalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    new BoxView
                    {
                        WidthRequest = 12,
                        HeightRequest = 12,
                        CornerRadius = 6,
                        Color = _question.ResultLightColor,
                        VerticalOptions = LayoutOptions.Center
                    },
                    new Label
                    {
                        Text = $"{_question.ResultStatusText}  |  {_question.QuestionCodeText}",
                        VerticalOptions = LayoutOptions.Center,
                        LineBreakMode = LineBreakMode.WordWrap
                    }
                }
            });

            promptLayout.Children.Add(new VerticalStackLayout
            {
                Spacing = 4,
                Children =
                {
                    new Label
                    {
                        Text = "Q",
                        FontAttributes = FontAttributes.Bold,
                        FontSize = 12
                    },
                    new KeyboardSnapshotView
                    {
                        Keys = _question.keyboard1,
                        KeysInRow = _question.KeyboardKeysInRow,
                        Rows = _question.KeyboardRows,
                        AboveNumber = _question.aboveNumber,
                        ArrowLength = _question.length,
                        Direction = _question.dir,
                        Compact = false,
                        HorizontalOptions = LayoutOptions.Fill
                    }
                }
            });

            if (_question.HasVisibleSecondQuestionKeyboard)
            {
                promptLayout.Children.Add(new VerticalStackLayout
                {
                    Spacing = 4,
                    Children =
                    {
                        new Label
                        {
                            Text = "Q2",
                            FontAttributes = FontAttributes.Bold,
                            FontSize = 12,
                            VerticalOptions = LayoutOptions.Center
                        },
                        new KeyboardSnapshotView
                        {
                            Keys = _question.keyboard2,
                            KeysInRow = _question.KeyboardKeysInRow,
                            Rows = _question.KeyboardRows,
                            Compact = false,
                            HorizontalOptions = LayoutOptions.Fill
                        }
                    }
                });
            }

            return promptLayout;
        }

        private void UpdateMarkers(KeyEvent keyEvent)
        {
            switch (keyEvent.EventType)
            {
                case 1:
                    AddActiveMarker(keyEvent);
                    break;
                case 0:
                    RemoveActiveMarker(keyEvent.KeyNumber);
                    break;
                case 2:
                    CaptureSubmitMarkerSnapshot();
                    break;
                case 3:
                    ClearActiveMarkers();
                    break;
            }
        }

        private void AddActiveMarker(KeyEvent keyEvent)
        {
            int keyIndex = keyEvent.KeyNumber - 1;
            if (keyIndex < 0 || keyIndex >= _replayKeyboard.KeyButtons.Count)
                return;

            BoxView marker = CreateMarker(_nextMarkerToken);
            int token = _nextMarkerToken++;

            _activeMarkers[token] = marker;
            if (!_activeMarkerTokensByKey.TryGetValue(keyEvent.KeyNumber, out Stack<int>? markerStack))
            {
                markerStack = new Stack<int>();
                _activeMarkerTokensByKey[keyEvent.KeyNumber] = markerStack;
            }

            markerStack.Push(token);
            _activeMarkerEventsByKey[keyEvent.KeyNumber] = keyEvent;
            _overlayLayer.Children.Add(marker);
            PositionMarker(marker, keyEvent);
        }

        private void RemoveActiveMarker(int keyNumber)
        {
            if (!_activeMarkerTokensByKey.TryGetValue(keyNumber, out Stack<int>? markerStack) || markerStack.Count == 0)
                return;

            int token = markerStack.Pop();
            if (_activeMarkers.TryGetValue(token, out BoxView? marker))
            {
                _overlayLayer.Children.Remove(marker);
                _activeMarkers.Remove(token);
            }

            if (markerStack.Count == 0)
            {
                _activeMarkerTokensByKey.Remove(keyNumber);
                _activeMarkerEventsByKey.Remove(keyNumber);
            }
        }

        private void ClearActiveMarkers()
        {
            foreach (BoxView marker in _activeMarkers.Values.ToList())
            {
                _overlayLayer.Children.Remove(marker);
            }

            _activeMarkers.Clear();
            _activeMarkerTokensByKey.Clear();
            _activeMarkerEventsByKey.Clear();
            _nextMarkerToken = 0;
        }

        private void CaptureSubmitMarkerSnapshot()
        {
            _submitMarkerSnapshot = _activeMarkerEventsByKey
                .OrderBy(item => item.Value.EventTime)
                .ThenBy(item => item.Value.id)
                .Select(item => item.Value)
                .ToList();
        }

        private KeyEvent? GetLatestSubmitKeyPressEvent()
        {
            return _submitMarkerSnapshot?
                .OrderBy(item => item.EventTime)
                .ThenBy(item => item.id)
                .LastOrDefault();
        }

        private void RestoreFinalMarkers(bool[] finalState, IReadOnlyList<KeyEvent> renderedEvents, int? finalAttemptNumber)
        {
            ClearActiveMarkers();

            if (_submitMarkerSnapshot != null && _submitMarkerSnapshot.Count > 0)
            {
                foreach (KeyEvent keyEvent in _submitMarkerSnapshot)
                    AddActiveMarker(keyEvent);
                return;
            }

            for (int keyIndex = 0; keyIndex < finalState.Length; keyIndex++)
            {
                if (!finalState[keyIndex])
                    continue;

                KeyEvent? finalDownEvent = renderedEvents
                    .Where(item => item.EventType == 1 &&
                                   item.KeyNumber == keyIndex + 1 &&
                                   (!finalAttemptNumber.HasValue || item.AttemptNumber == finalAttemptNumber.Value))
                    .OrderBy(item => item.EventTime)
                    .ThenBy(item => item.id)
                    .LastOrDefault();

                if (finalDownEvent != null)
                    AddActiveMarker(finalDownEvent);
            }
        }

        private static KeyEvent? GetFinalKeyPressEvent(IReadOnlyList<KeyEvent> renderedEvents, bool[] finalState, int? finalAttemptNumber)
        {
            return renderedEvents
                .Where(item => item.EventType == 1 &&
                               item.KeyNumber > 0 &&
                               item.KeyNumber <= finalState.Length &&
                               finalState[item.KeyNumber - 1] &&
                               (!finalAttemptNumber.HasValue || item.AttemptNumber == finalAttemptNumber.Value))
                .OrderBy(item => item.EventTime)
                .ThenBy(item => item.id)
                .LastOrDefault();
        }

        private BoxView CreateMarker(int token)
        {
            return new BoxView
            {
                WidthRequest = 16,
                HeightRequest = 16,
                CornerRadius = 8,
                BackgroundColor = _markerPalette[token % _markerPalette.Length],
                Opacity = 0.9,
                InputTransparent = true
            };
        }

        private void PositionMarker(BoxView marker, KeyEvent keyEvent)
        {
            int keyIndex = keyEvent.KeyNumber - 1;
            if (keyIndex < 0 || keyIndex >= _replayKeyboard.KeyButtons.Count)
                return;

            VisualElement keyButton = _replayKeyboard.KeyButtons[keyIndex];
            if (keyButton.Width <= 0 || keyButton.Height <= 0)
                return;

            double relativeX = keyEvent.RelativeX ?? 0.5;
            double relativeY = keyEvent.RelativeY ?? 0.5;
            double markerX = keyButton.X + (relativeX * keyButton.Width) - (marker.WidthRequest / 2);
            double markerY = keyButton.Y + (relativeY * keyButton.Height) - (marker.HeightRequest / 2);

            AbsoluteLayout.SetLayoutBounds(marker, new Rect(markerX, markerY, marker.WidthRequest, marker.HeightRequest));
        }
    }
}
