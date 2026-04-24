using GestureSample.Maui.Data.SQLite;
using GestureSample.Maui;
using GestureSample.Maui.Models;

namespace GestureSample.Views
{
    public sealed class KeyboardReplayPage : ContentPage
    {
        private readonly IReadOnlyList<KeyEvent> _events;
        private readonly PianoKeyboardReadOnly _replayKeyboard;
        private readonly AbsoluteLayout _overlayLayer;
        private readonly Label _eventLabel;
        private readonly Button _replayButton;
        private readonly bool[] _initialState;
        private readonly KeyboardQuestion? _question;
        private readonly bool[]? _finalReplayState;
        private readonly Dictionary<int, BoxView> _activeMarkers = new();
        private readonly Dictionary<int, Stack<int>> _activeMarkerTokensByKey = new();
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

        public KeyboardReplayPage(
            string title,
            IReadOnlyList<KeyEvent> events,
            KeyboardQuestion? question = null,
            KeyboardConfig? keyboardConfig = null,
            bool[]? finalReplayState = null)
        {
            Title = title;
            _events = events?.OrderBy(item => item.EventTime).ThenBy(item => item.id).ToList() ?? new List<KeyEvent>();
            _question = question;
            _finalReplayState = finalReplayState?.ToArray();

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

            _replayButton = new Button
            {
                Text = "Replay",
                HorizontalOptions = LayoutOptions.Center,
                IsEnabled = _events.Count > 0
            };
            _replayButton.Clicked += async (_, _) => await ReplayAsync();

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
                        replayGrid,
                        _eventLabel,
                        _replayButton
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

            _hasAutoPlayed = true;
            await WaitForReplayKeyboardReadyAsync();
            await Task.Delay(80);
            await ReplayAsync();
        }

        private async Task ReplayAsync()
        {
            if (_isPlaying || _events.Count == 0)
                return;

            _isPlaying = true;
            _replayButton.IsEnabled = false;

            try
            {
                await WaitForReplayKeyboardReadyAsync();
                bool[] state = _initialState.ToArray();
                RenderReplayState(state, highlightPressedKeys: false);
                ClearActiveMarkers();

                DateTime? previousTime = null;
                int? currentAttemptNumber = null;
                foreach (KeyEvent keyEvent in _events)
                {
                    if (ShouldResetForAttemptBoundary(currentAttemptNumber, keyEvent.AttemptNumber))
                    {
                        Array.Fill(state, false);
                        RenderReplayState(state, highlightPressedKeys: false);
                        ClearActiveMarkers();
                    }

                    if (previousTime.HasValue)
                    {
                        int delay = (int)Math.Clamp((keyEvent.EventTime - previousTime.Value).TotalMilliseconds, 30, 900);
                        await Task.Delay(delay);
                    }

                    ApplyEvent(state, keyEvent, highlightPressedKeys: true);
                    previousTime = keyEvent.EventTime;
                    currentAttemptNumber = keyEvent.AttemptNumber;
                }

                ClearActiveMarkers();
                RenderReplayState(GetFinalReplayState(state), highlightPressedKeys: true);
                await Task.Delay(500);
            }
            finally
            {
                _isPlaying = false;
                _replayButton.IsEnabled = true;
            }
        }

        private static bool ShouldResetForAttemptBoundary(int? currentAttemptNumber, int nextAttemptNumber)
        {
            if (!currentAttemptNumber.HasValue)
                return false;

            if (currentAttemptNumber.Value == nextAttemptNumber)
                return false;

            return currentAttemptNumber.Value > 0 || nextAttemptNumber > 0;
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

        private void ApplyEvent(bool[] state, KeyEvent keyEvent, bool highlightPressedKeys)
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

            RenderReplayState(state, highlightPressedKeys);
            UpdateMarkers(keyEvent);
            _eventLabel.Text = $"{keyEvent.EventTimeText}  {keyEvent.EventTypeText}  key {keyEvent.KeyNumber}  x {keyEvent.RelativeXText}  y {keyEvent.RelativeYText}";
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

        private bool[] GetFinalReplayState(bool[] fallbackState)
        {
            if (_finalReplayState != null && _finalReplayState.Length > 0)
            {
                bool[] finalState = new bool[_replayKeyboard.KeyCount];
                int length = Math.Min(finalState.Length, _finalReplayState.Length);
                Array.Copy(_finalReplayState, finalState, length);
                return finalState;
            }

            if (_question?.HasSubmittedKeyboard == true && _question.SubmittedKeyboard != null)
            {
                bool[] finalState = new bool[_replayKeyboard.KeyCount];
                int length = Math.Min(finalState.Length, _question.SubmittedKeyboard.Length);
                Array.Copy(_question.SubmittedKeyboard, finalState, length);
                return finalState;
            }

            return fallbackState.ToArray();
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
                _activeMarkerTokensByKey.Remove(keyNumber);
        }

        private void ClearActiveMarkers()
        {
            foreach (BoxView marker in _activeMarkers.Values.ToList())
            {
                _overlayLayer.Children.Remove(marker);
            }

            _activeMarkers.Clear();
            _activeMarkerTokensByKey.Clear();
            _nextMarkerToken = 0;
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
