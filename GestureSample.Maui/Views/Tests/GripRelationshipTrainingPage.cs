using GestureSample.Maui.Models;
using GestureSample.Maui.Handlers;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Graphics;

namespace GestureSample.Views.Tests;

/// <summary>Practice one attribute at a time before composing and subtracting lengths.</summary>
public sealed class GripRelationshipTrainingPage : ContentPage
{
    private readonly TutorialConceptHeading _heading = new();
    private readonly TrainingScene _scene = new();
    private readonly GraphicsView _drawing;
    private readonly Label _progress = new() { TextColor = Colors.LightGray, HorizontalTextAlignment = TextAlignment.Center };
    private Label _question = new() { FontSize = 23, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center };
    private readonly Label _feedback = new() { FontSize = 18, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center };
    private VerticalStackLayout _answers = new() { Spacing = 10, HorizontalOptions = LayoutOptions.Fill };
    private readonly Button _next = new() { Text = "הבא", IsEnabled = false };
    private readonly Button _retry = new() { Text = "אימון מחדש" };
    private int _trial, _attribute;
    private bool _answered, _navigating;
    private CancellationTokenSource? _presentation;
    private CancellationTokenSource? _autoAdvance;
    private CancellationTokenSource? _questionSpeech;
    private bool _pageVisible;
    private bool _narrationEnabled = Preferences.Default.Get("grip_relationship_question_narration", false);
    private readonly Button _narration = new()
    {
        AutomationId = "GripRelationshipQuestionNarration",
        FontSize = 24, WidthRequest = 48, HeightRequest = 48, Padding = 0,
        CornerRadius = 24, TextColor = Colors.White
    };
    private readonly Grid _practice = new() { ColumnSpacing = 12,
        FlowDirection = FlowDirection.LeftToRight,
        ColumnDefinitions = { new(new GridLength(1, GridUnitType.Star)),
            new(new GridLength(2, GridUnitType.Star)), new(new GridLength(1, GridUnitType.Star)) } };
    private readonly VerticalStackLayout _questionPanel = new() { Spacing = 18,
        VerticalOptions = LayoutOptions.Start, FlowDirection = FlowDirection.RightToLeft };
    private readonly VerticalStackLayout _leftQuestionPanel = new() { Spacing = 18,
        VerticalOptions = LayoutOptions.Start, FlowDirection = FlowDirection.RightToLeft };
    private GripRelationshipTrial Current => GripRelationshipTrial.Examples[_trial];
    private int[] Attributes => new[] { 0, 1 }
        .Concat(Current.HasSmaller ? new[] { 3 } : Array.Empty<int>())
        .Concat(Current.LengthsAreClose.HasValue ? new[] { 2 } : Array.Empty<int>()).ToArray();
    private int QuestionCount => Attributes.Length;
    private int QuestionIndex => Array.IndexOf(Attributes, _attribute);

    public GripRelationshipTrainingPage()
    {
        Title = "אימון · שתי אחיזות";
        BackgroundColor = Color.FromArgb("#101D2C");
        FlowDirection = FlowDirection.RightToLeft;
        _drawing = new GraphicsView { Drawable = _scene, MinimumHeightRequest = 220, FlowDirection = FlowDirection.LeftToRight };
        _retry.Clicked += (_, _) => { _trial = _attribute = 0; ShowQuestion(); };
        _next.Clicked += async (_, _) => await AdvanceAsync();
        var controls = new HorizontalStackLayout { Spacing = 12, HorizontalOptions = LayoutOptions.Center };
        controls.Add(_retry); controls.Add(_next);
        var narrationControls = new HorizontalStackLayout { Spacing = 8, HorizontalOptions = LayoutOptions.Center };
        narrationControls.Add(_narration);
        RefreshNarrationIcon();
        _narration.Clicked += async (_, _) =>
        {
            _narrationEnabled = !_narrationEnabled;
            Preferences.Default.Set("grip_relationship_question_narration", _narrationEnabled);
            RefreshNarrationIcon();
            _questionSpeech?.Cancel();
            if (_narrationEnabled && _pageVisible)
            {
                _heading.CancelSpeech();
                await NarrateQuestionAsync();
            }
        };
        _practice.Add(new ScrollView { Content = _leftQuestionPanel }, 0);
        _practice.Add(_drawing, 1);
        _practice.Add(new ScrollView { Content = _questionPanel }, 2);
        var layout = new Grid { Padding = 16, RowSpacing = 10, RowDefinitions = new RowDefinitionCollection {
            new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto),
            new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Auto) } };
        layout.Add(_heading, 0, 0); layout.Add(_progress, 0, 1); layout.Add(_practice, 0, 2);
        layout.Add(narrationControls, 0, 4);
        layout.Add(_feedback, 0, 5); layout.Add(controls, 0, 6);
        Content = layout;
    }

    private async Task AdvanceAsync()
    {
        if (!_answered || _navigating) return;
        _autoAdvance?.Cancel();
        if (QuestionIndex + 1 < QuestionCount) { _attribute = Attributes[QuestionIndex + 1]; ShowQuestion(); }
        else if (_trial + 1 < GripRelationshipTrial.Examples.Count) { _trial++; _attribute = 0; ShowQuestion(); }
        else
        {
            _navigating = true;
            _answered = false;
            try { await Navigation.PushAsync(new PartWholeIllustrationPage()); }
            finally { _navigating = false; }
        }
    }

    private void ShowQuestion()
    {
        _autoAdvance?.Cancel();
        _questionSpeech?.Cancel();
        _answered = false;
        _next.IsEnabled = false;
        _next.Text = "הבא";
        _progress.Text = $"זוג {_trial + 1} / {GripRelationshipTrial.Examples.Count} · שאלה {QuestionIndex + 1} / {QuestionCount}";
        if (QuestionIndex == 0)
        {
            _leftQuestionPanel.Clear();
            _questionPanel.Clear();
        }
        // Each new question is appended; completed questions remain above it.
        _question = new Label { FontSize = 23, TextColor = Colors.White,
            HorizontalTextAlignment = TextAlignment.Center };
        _answers = new VerticalStackLayout { Spacing = 10, HorizontalOptions = LayoutOptions.Fill };
        var questionGroup = new VerticalStackLayout { Spacing = 10 };
        questionGroup.Add(_question);
        questionGroup.Add(_answers);
        bool showOnLeft = _attribute == 0 ||
            (_attribute == 3 && Current.Left.Length < Current.Right.Length);
        (showOnLeft ? _leftQuestionPanel : _questionPanel).Add(questionGroup);
        _scene.Trial = Current;
        _scene.Attribute = _attribute;
        _scene.Reveal = false;
        _drawing.Invalidate();
        _question.Text = _attribute switch
        {
            0 => "מה אורך האחיזה משמאל ביחס לזו שמימין?",
            1 => "מה אורך האחיזה מימין ביחס לזו שמשמאל?",
            2 => "האורכים קרובים או רחוקים זה מזה?",
            _ => "איפה האחיזה הקטנה מוצמדת ביחס לגדולה?"
        };
        _feedback.Text = _attribute < 2 ? "משווים את האורכים: ארוכה יותר או קצרה יותר ביחס לאחיזה השנייה."
            : _attribute == 2 ? "קרובים: הפרש של קליד אחד ושני האורכים גדולים מ־4. רחוקים: האורך הגדול יותר מפי 2 מהקטן וההפרש לפחות 3 קלידים."
            : "משווים את הקצה העליון ואת הקצה התחתון של הקטנה לאלה של הגדולה.";
        _answers.Clear();
        string[] choices = _attribute < 2 ? new[] { "ארוכה יותר", "קצרה יותר", "שוות באורך" }
            : _attribute == 2 ? new[] { "קרובים", "רחוקים" } : new[] { "למעלה", "למטה", "ללא הצמדה" };
        foreach (string choice in choices)
        {
            var button = new Button { Text = choice, MinimumWidthRequest = 88 };
            int attribute = _attribute;
            int trial = _trial;
            button.Clicked += async (_, _) =>
            {
                if (_attribute == attribute && _trial == trial) await AnswerAsync(choice);
            };
            _answers.Add(button);
        }
        if (QuestionIndex == 0)
            _ = PresentHandsAsync();
        else
        {
            _scene.VisibleHands = 2;
            _drawing.Invalidate();
        }
        _ = NarrateQuestionAsync();
    }

    private void RefreshNarrationIcon()
    {
        _narration.Text = _narrationEnabled ? "🔊" : "🔇";
        _narration.BackgroundColor = _narrationEnabled ? Color.FromArgb("#168657") : Color.FromArgb("#334155");
        string description = _narrationEnabled ? "כיבוי הקראת שאלות" : "הפעלת הקראת שאלות";
        SemanticProperties.SetDescription(_narration, description);
        ToolTipProperties.SetText(_narration, description);
    }

    private async Task NarrateQuestionAsync()
    {
        _questionSpeech?.Cancel();
        if (!_pageVisible || !_narrationEnabled || _answered) return;
        using var source = new CancellationTokenSource();
        _questionSpeech = source;
        string question = _attribute switch
        {
            0 => "How long is the left grip compared with the right grip?",
            1 => "How long is the right grip compared with the left grip?",
            2 => "Are the lengths close together or far apart?",
            _ => "Where is the smaller grip aligned with the larger grip?"
        };
        try
        {
            await AppLanguage.SpeakAsync(question, gripping: true, cancellationToken: source.Token);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Grip language question] Narration unavailable: {ex.Message}");
        }
        finally { if (ReferenceEquals(_questionSpeech, source)) _questionSpeech = null; }
    }

    private async Task PresentHandsAsync()
    {
        _presentation?.Cancel();
        using var source = new CancellationTokenSource();
        _presentation = source;
        _answers.IsEnabled = false;
        _scene.VisibleHands = 1;
        _drawing.Invalidate();
        try
        {
            await Task.Delay(650, source.Token);
            _scene.VisibleHands = 2;
            _drawing.Invalidate();
            _answers.IsEnabled = true;
        }
        catch (OperationCanceledException) { }
        finally { if (ReferenceEquals(_presentation, source)) _presentation = null; }
    }

    private async Task AnswerAsync(string choice)
    {
        if (_answered) return;
        string expected = _attribute switch
        {
            0 => Current.CompareLength(left: true),
            1 => Current.CompareLength(left: false),
            2 => Current.LengthsAreClose switch { true => "קרובים", false => "רחוקים", _ => throw new InvalidOperationException("Unclassified length difference") },
            _ => Current.Alignment
        };
        _scene.Reveal = true;
        _drawing.Invalidate();
        if (choice != expected)
        {
            _feedback.Text = _attribute < 2 ? "נסו שוב: הסתכלו על האורך האדום, מתחתית הקליד עד ראש הקליד."
                : _attribute == 2 ? $"נסו שוב: האורכים הם {Current.Left.Length} ו־{Current.Right.Length}, וההפרש הוא {Current.LengthDifference}. משווים אורכים, בלי קשר למיקום."
                : "נסו שוב: קו העזר מצביע על הקצוות. הצמדה פירושה שהם בדיוק באותו גובה.";
            return;
        }
        _answered = true;
        _questionSpeech?.Cancel();
        _next.IsEnabled = true;
        _question.TextColor = Colors.LimeGreen;
        var correctButton = _answers.Children.OfType<Button>().Single(button => button.Text == expected);
        _answers.Clear();
        correctButton.BackgroundColor = Color.FromArgb("#168657");
        correctButton.TextColor = Colors.White;
        correctButton.InputTransparent = true;
        _answers.Add(correctButton);
        _feedback.Text = _attribute switch
        {
            0 => $"נכון. האחיזה משמאל באורך {Current.Left.Length} — {expected}.",
            1 => $"נכון. האחיזה מימין באורך {Current.Right.Length} — {expected}.",
            2 when Current.Left.Length >= 6 && Current.Right.Length >= 6 && Current.LengthsAreClose == true =>
                $"נכון! שני הקטעים ארוכים — {Current.Left.Length} ו־{Current.Right.Length} — אבל האורכים קרובים: ההפרש רק {Current.LengthDifference}.",
            2 => $"נכון. ההפרש בין {Current.Left.Length} ל־{Current.Right.Length} הוא {Current.LengthDifference} — האורכים {expected}.",
            _ => $"נכון. הקטנה: {expected}, ביחס לגדולה."
        };
        using var source = new CancellationTokenSource();
        _autoAdvance = source;
        try
        {
            await Task.Delay(700, source.Token);
            await AdvanceAsync();
        }
        catch (OperationCanceledException) { }
        finally { if (ReferenceEquals(_autoAdvance, source)) _autoAdvance = null; }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _pageVisible = true;
        _heading.SetConcept("Compare two grips");
        ShowQuestion();
        if (!_narrationEnabled) await _heading.SpeakAsync();
    }

    protected override void OnDisappearing()
    {
        _pageVisible = false;
        _questionSpeech?.Cancel();
        _heading.CancelSpeech();
        _autoAdvance?.Cancel();
        _presentation?.Cancel();
        base.OnDisappearing();
    }

    private sealed class TrainingScene : IDrawable
    {
        public GripRelationshipTrial Trial { get; set; } = GripRelationshipTrial.Examples[0];
        public int Attribute { get; set; }
        public bool Reveal { get; set; }
        public int VisibleHands { get; set; }
        private readonly SideGripDrawable _hand = new();
        private const float Unit = 27, Bottom = 356, Left = 150, Right = 246, Width = 78;
        private static float Y(float height) => Bottom - height * Unit;

        public void Draw(ICanvas canvas, RectF bounds)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0) return;
            canvas.SaveState();
            float scale = Math.Min(bounds.Width / 475, bounds.Height / 385);
            canvas.Translate((bounds.Width - 475 * scale) / 2, (bounds.Height - 385 * scale) / 2);
            canvas.Scale(scale, scale);
            for (int column = 0; column < 2; column++)
            {
                var grip = column == 0 ? Trial.Left : Trial.Right;
                // Normalize origins only for the length comparison; retain real positions
                // for the separate alignment question.
                if (Attribute == 2) grip = grip with { Bottom = 1 };
                float x = column == 0 ? Left : Right;
                for (int row = 0; row < 12; row++)
                {
                    canvas.FillColor = row == grip.Bottom || row == grip.Top - 1 ? Colors.Yellow : Color.FromArgb("#EDF1F5");
                    canvas.FillRoundedRectangle(x, Y(row + 1) + 1, Width, Unit - 2, 3);
                }
                if (Attribute == column || (Reveal && Attribute < 2))
                {
                    canvas.StrokeColor = Colors.White;
                    canvas.StrokeSize = 2;
                    canvas.DrawRectangle(x - 3, Y(grip.Top) - 2, Width + 6, grip.Length * Unit + 4);
                }
                float tip = x + Width * (column == 0 ? .60f : .40f);
                canvas.SaveState();
                if (column == 0) { canvas.Translate(2 * tip, 0); canvas.Scale(-1, 1); }
                if (column < VisibleHands)
                    _hand.Draw(canvas, tip, Y(grip.Top - .5f), Y(grip.Bottom + .5f), Unit, 82,
                        upperFingerExtension: Width * .4f - Unit / 8);
                canvas.RestoreState();
                if (Attribute < 2 && Attribute == column)
                    Bracket(canvas, column == 0 ? 30 : 438, grip.Bottom, grip.Top, Reveal ? grip.Length.ToString() : "");
            }
            if (Attribute == 2)
            {
                int low = 1 + Math.Min(Trial.Left.Length, Trial.Right.Length);
                int high = 1 + Math.Max(Trial.Left.Length, Trial.Right.Length);
                canvas.StrokeColor = Colors.Red;
                canvas.StrokeSize = 2;
                canvas.StrokeDashPattern = new float[] { 4, 4 };
                foreach (int height in new[] { 1, low, high }.Distinct())
                    canvas.DrawLine(Left + Width, Y(height), Right, Y(height));
                canvas.StrokeDashPattern = null;
                if (Trial.LengthDifference > 0)
                    Bracket(canvas, 237, low, high, Reveal ? Trial.LengthDifference.ToString() : "");
                else if (Reveal)
                {
                    canvas.FontColor = Colors.White;
                    canvas.FontSize = 19;
                    canvas.DrawString("0", 235, Y(high) - 25, 32, 25,
                        HorizontalAlignment.Center, VerticalAlignment.Center);
                }
            }
            if (Attribute == 3)
            {
                canvas.StrokeColor = Colors.Red;
                canvas.StrokeSize = 2;
                canvas.StrokeDashPattern = new float[] { 4, 4 };
                foreach (int height in new[] { Trial.Larger.Top, Trial.Larger.Bottom })
                    canvas.DrawLine(Left, Y(height), Right + Width, Y(height));
                canvas.StrokeDashPattern = null;
            }
            canvas.RestoreState();
        }

        private static void Bracket(ICanvas canvas, float x, int bottom, int top, string text)
        {
            canvas.StrokeColor = Colors.Red;
            canvas.StrokeSize = 3;
            canvas.DrawLine(x, Y(bottom), x, Y(top));
            canvas.DrawLine(x - 5, Y(bottom), x + 7, Y(bottom));
            canvas.DrawLine(x - 5, Y(top), x + 7, Y(top));
            canvas.FontColor = Colors.White;
            canvas.FontSize = 20;
            canvas.DrawString(text, x - 31, Y(top), 26, (top - bottom) * Unit,
                HorizontalAlignment.Center, VerticalAlignment.Center);
        }
    }
}
