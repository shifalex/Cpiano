using GestureSample.Maui.Handlers;
using GestureSample.Maui.Models;
using GestureSample.Maui.Views;
using Microsoft.Maui.Controls.Shapes;

namespace GestureSample.Views.Tests;

public partial class SimpleViewCellsPage
{
    internal string NavigationSettingsStageName => _config.GameName;
    private sealed record ExerciseReviewSnapshot(Action ApplyVisuals);
    private readonly ExerciseReviewHistory<ExerciseReviewSnapshot> _exerciseReviewHistory = new();
    private bool _manualExerciseAdvance;
    private bool _currentExerciseCompleted;
    private bool _exerciseFeedbackInProgress;
    private bool _exerciseGenerationInProgress;
    private bool _exerciseNavigationBusy;
    private bool _reviewInteractionWasEnabled;
    private readonly Dictionary<Entry, bool> _reviewEntryStates = new();
    private ExerciseReviewSnapshot? _liveExerciseVisuals;
    private Func<Task>? _pendingManualAdvance;
    private View? _exerciseQuestionAnchor;
    private Grid? _exerciseNavigationBar;
    private ExerciseNavigationButton? _reviewBackButton, _reviewForwardButton, _reviewLiveButton;
    private bool IsExerciseReviewInputBlocked => _exerciseReviewHistory.IsReviewing || _pendingManualAdvance != null;

    private void InitializeExerciseNavigation()
    {
        if (_exerciseQuestionAnchor is not View question || question.Parent is not Grid grid) return;
        _manualExerciseAdvance = StageNavigationPreferences.IsManual(_config.GameName);
        ExerciseNavigationButton Arrow(bool forward, string id, string description, bool doubleArrow = false)
        {
            var button = new ExerciseNavigationButton(forward, doubleArrow)
            {
                AutomationId = id,
                TranslationY = 16
            };
            SemanticProperties.SetDescription(button, AppLanguage.Text(description));
            return button;
        }
        _reviewBackButton = Arrow(false, "ExerciseReviewBack", "Previous solution");
        _reviewForwardButton = Arrow(true, "ExerciseReviewForward", "Next exercise");
        _reviewLiveButton = Arrow(true, "ExerciseReviewLive", "Return to current exercise", doubleArrow: true);
        _reviewBackButton.HorizontalOptions = LayoutOptions.Start;
        _reviewBackButton.VerticalOptions = LayoutOptions.Center;
        _reviewBackButton.Margin = new Thickness(12, 0, 0, 0);
        _reviewForwardButton.HorizontalOptions = LayoutOptions.End;
        _reviewForwardButton.VerticalOptions = LayoutOptions.Center;
        _reviewForwardButton.Margin = new Thickness(0, 0, 12, 0);
        _reviewLiveButton.HorizontalOptions = LayoutOptions.End;
        _reviewLiveButton.VerticalOptions = LayoutOptions.End;
        _reviewLiveButton.Margin = new Thickness(0, 0, 12, 4);
        _reviewBackButton.Clicked += (_, _) => NavigateExerciseBack();
        _reviewForwardButton.Clicked += async (_, _) => await NavigateExerciseForwardAsync();
        _reviewLiveButton.Clicked += (_, _) => ReturnToLiveExercise();
        // Overlay the extreme edges of the existing question row. Neither the
        // centered prompt nor the keyboard loses space to navigation controls.
        _exerciseNavigationBar = new Grid
        {
            FlowDirection = FlowDirection.LeftToRight,
            HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill,
            InputTransparent = true, CascadeInputTransparent = false, ZIndex = 10000,
            Children = { _reviewBackButton, _reviewForwardButton, _reviewLiveButton }
        };
        grid.Add(_exerciseNavigationBar, 0, Grid.GetRow(question));
        Grid.SetColumnSpan(_exerciseNavigationBar, Math.Max(1, grid.ColumnDefinitions.Count));
        // Existing stage gears already include navigation settings. Keep a compact
        // fallback only for activities without a settings screen of their own.
        bool hasStageSettings = _config.KeyboardConfig?.IsArrow == true ||
            (_config.KeyboardConfig?.ArrowLabelExerciseMode ?? GestureSample.Maui.ArrowLabelExerciseMode.None) != GestureSample.Maui.ArrowLabelExerciseMode.None ||
            _config.KeyboardConfig?.SupportsGripChildAssistance == true ||
            _config.KeyboardConfig?.IsTwoHandCombinationMemorize == true ||
            _config.GameName.StartsWith("Shift by", StringComparison.Ordinal) ||
            _config.GameName is "Group to side" or "Mixed one operation" or "Mixed two keyboards" or "Or-And-XOR" or "Or-And-Neutralise";
        if (!hasStageSettings)
        {
            var settings = new ToolbarItem { Text = "⚙", AutomationId = "ExerciseNavigationSettings" };
            bool opening = false;
            settings.Clicked += async (_, _) =>
            {
                if (opening) return;
                opening = true;
                try
                {
                    await Navigation.PushAsync(new ContentPage
                    {
                        Title = AppLanguage.Text("Stage settings"),
                        FlowDirection = LanguagePreferences.Get() == InterfaceLanguage.Hebrew ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
                        Content = new ScrollView { Content = new VerticalStackLayout { Padding = 20, Children = { new StageNavigationSettingsView(_config.GameName) } } }
                    });
                }
                finally { opening = false; }
            };
            ToolbarItems.Add(settings);
        }
        RefreshExerciseNavigation();
    }

    private void RefreshExerciseNavigationSettings()
    {
        _manualExerciseAdvance = StageNavigationPreferences.IsManual(_config.GameName);
        if (_btnNext != null) _btnNext.IsVisible = ShouldShowNextButton();
        RefreshExerciseNavigation();
        if (IsExerciseReviewInputBlocked) SetPageInteractionEnabled(false);
    }

    private void RefreshExerciseNavigation()
    {
        if (_exerciseNavigationBar == null) return;
        bool ready = !_exerciseFeedbackInProgress && !_exerciseGenerationInProgress && !_exerciseNavigationBusy;
        _exerciseNavigationBar.IsVisible = _manualExerciseAdvance || _exerciseReviewHistory.Count > 0 || _pendingManualAdvance != null;
        _reviewBackButton!.IsVisible = _exerciseNavigationBar.IsVisible;
        _reviewBackButton!.IsEnabled = ready && _exerciseReviewHistory.CanMoveBack(_currentExerciseCompleted);
        _reviewForwardButton!.IsEnabled = ready && (_exerciseReviewHistory.IsReviewing || _pendingManualAdvance != null);
        _reviewLiveButton!.IsVisible = _exerciseReviewHistory.DistanceFromLive(_currentExerciseCompleted) > 1;
        _reviewLiveButton.IsEnabled = ready;
        foreach (var button in new[] { _reviewBackButton, _reviewForwardButton, _reviewLiveButton })
            button.Shadow = _exerciseReviewHistory.IsReviewing
                ? new Shadow { Brush = Colors.Black, Opacity = 0.5f, Offset = new Point(0, 5), Radius = 14 }
                : null;
        if (_exerciseReviewHistory.IsReviewing && _btnHelp != null) _btnHelp.IsEnabled = false;
    }

    private void WaitForManualExerciseAdvance(ExerciseCheckResult result)
    {
        _pendingManualAdvance = async () =>
        {
            if (PracticeAttemptCompletedAsync != null && await PracticeAttemptCompletedAsync(true)) return;
            if (result.Completion != null) await HandleGameCompletionAsync(result.Completion);
            else if (!_gamePlay.GameOver) await GenerateNextExerciseAsync();
        };
        SetPageInteractionEnabled(false);
        SetTextInputsEnabled(false);
        foreach (var entry in ReviewEntries()) entry.Unfocus();
        RefreshExerciseNavigation();
    }

    private IEnumerable<Entry> ReviewEntries() => new[]
    {
        _txtAddend1, _txtAddend2, _txtSum, _txtComplexAddend3,
        _txtComplexSum2, _txtComplexTotalDistance, _arrowEquationAnswerEntry
    }.OfType<Entry>();

    private ExerciseReviewSnapshot CaptureExerciseVisuals()
    {
        var restore = new List<Action>();
        var visited = new HashSet<IVisualTreeElement>();
        void Properties(BindableObject target, params BindableProperty[] properties)
        {
            var values = properties.Select(property => (Property: property, Value: target.GetValue(property))).ToArray();
            restore.Add(() =>
            {
                foreach (var value in values) target.SetValue(value.Property, value.Value);
            });
        }
        void Capture(IVisualTreeElement? element)
        {
            if (element == null || !visited.Add(element)) return;
            if (element is View view)
                Properties(view, VisualElement.IsVisibleProperty, VisualElement.OpacityProperty,
                    VisualElement.BackgroundColorProperty, VisualElement.WidthRequestProperty, VisualElement.HeightRequestProperty,
                    View.MarginProperty, VisualElement.TranslationXProperty, VisualElement.TranslationYProperty,
                    VisualElement.ScaleProperty, AbsoluteLayout.LayoutBoundsProperty, AbsoluteLayout.LayoutFlagsProperty);
            if (element is Entry entry) Properties(entry, Entry.TextProperty, Entry.TextColorProperty);
            if (element is Label label) Properties(label, Label.TextProperty, Label.TextColorProperty);
            if (element is Microsoft.Maui.Controls.Shapes.Path path)
                Properties(path, Microsoft.Maui.Controls.Shapes.Path.DataProperty,
                    Shape.StrokeProperty, Shape.FillProperty, Shape.StrokeThicknessProperty,
                    Shape.StrokeLineCapProperty, Shape.StrokeLineJoinProperty);
            foreach (var child in element.GetVisualChildren()) Capture(child);
        }
        Capture(_exerciseQuestionAnchor);
        Capture(_keyboardArrowPromptView);
        Capture(_arrowEquationPromptView);
        Capture(_correctExpressionLabel);
        Capture(_lblAction);
        foreach (var keyboard in new PianoKeyboardReadOnly?[] { _pianoKeyboard, _keyboardTask1, _keyboardTask2 }.OfType<PianoKeyboardReadOnly>())
        {
            Capture(keyboard);
            restore.Add(keyboard.CaptureVisualReviewRestore());
        }
        return new ExerciseReviewSnapshot(() =>
        {
            foreach (var apply in restore) apply();
        });
    }

    private void CaptureCompletedExerciseReview()
    {
        if (_currentExerciseCompleted) return;
        _exerciseReviewHistory.AddCompleted(CaptureExerciseVisuals());
        _currentExerciseCompleted = true;
    }

    private void NavigateExerciseBack()
    {
        if (_exerciseFeedbackInProgress || _exerciseGenerationInProgress || _exerciseNavigationBusy) return;
        if (!_exerciseReviewHistory.CanMoveBack(_currentExerciseCompleted)) return;
        bool wasReviewing = _exerciseReviewHistory.IsReviewing;
        if (!wasReviewing)
        {
            _liveExerciseVisuals = CaptureExerciseVisuals();
            _reviewInteractionWasEnabled = _isPageInteractionEnabled;
            _reviewEntryStates.Clear();
            foreach (var entry in ReviewEntries())
            {
                _reviewEntryStates[entry] = entry.IsEnabled;
                entry.Unfocus();
                entry.IsEnabled = false;
            }
        }
        _exerciseReviewHistory.MoveBack(_currentExerciseCompleted);
        SetPageInteractionEnabled(false);
        ShowExerciseReview();
    }

    private async Task NavigateExerciseForwardAsync()
    {
        if (_exerciseNavigationBusy || _exerciseFeedbackInProgress || _exerciseGenerationInProgress) return;
        if (_exerciseReviewHistory.IsReviewing)
        {
            _exerciseReviewHistory.MoveForward(_currentExerciseCompleted);
            if (_exerciseReviewHistory.IsReviewing) ShowExerciseReview();
            else RestoreLiveExerciseInteraction();
            return;
        }
        var advance = _pendingManualAdvance;
        if (advance == null) return;
        _exerciseNavigationBusy = true;
        _pendingManualAdvance = null;
        RefreshExerciseNavigation();
        try { await advance(); }
        finally { _exerciseNavigationBusy = false; RefreshExerciseNavigation(); }
    }

    private void ReturnToLiveExercise()
    {
        if (_exerciseFeedbackInProgress || _exerciseGenerationInProgress || _exerciseNavigationBusy) return;
        _exerciseReviewHistory.ReturnToLive();
        RestoreLiveExerciseInteraction();
    }

    private void RestoreLiveExerciseInteraction()
    {
        // Restore only the displayed properties. Never regenerate or regrade a question.
        _liveExerciseVisuals?.ApplyVisuals();
        _liveExerciseVisuals = null;
        SetPageInteractionEnabled(_pendingManualAdvance == null && _reviewInteractionWasEnabled);
        foreach (var pair in _reviewEntryStates) pair.Key.IsEnabled = pair.Value;
        _reviewEntryStates.Clear();
        RefreshExerciseNavigation();
    }

    private void ShowExerciseReview()
    {
        _exerciseReviewHistory.Current.ApplyVisuals();
        foreach (var entry in ReviewEntries()) entry.IsEnabled = false;
        SetPageInteractionEnabled(false);
        RefreshExerciseNavigation();
    }
}
