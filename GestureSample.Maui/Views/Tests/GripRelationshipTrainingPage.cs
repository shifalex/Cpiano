using GestureSample.Maui.Models;
using Microsoft.Maui.Graphics;

namespace GestureSample.Views.Tests;

/// <summary>Practice one attribute at a time before composing and subtracting lengths.</summary>
public sealed class GripRelationshipTrainingPage : ContentPage
{
    private readonly TutorialConceptHeading _heading = new();
    private readonly TrainingScene _scene = new();
    private readonly GraphicsView _drawing;
    private readonly Label _progress = new() { TextColor = Colors.LightGray, HorizontalTextAlignment = TextAlignment.Center };
    private readonly Label _question = new() { FontSize = 23, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center };
    private readonly Label _feedback = new() { FontSize = 18, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center };
    private readonly HorizontalStackLayout _answers = new() { Spacing = 10, HorizontalOptions = LayoutOptions.Center };
    private readonly Button _next = new() { Text = "הבא", IsEnabled = false };
    private readonly Button _retry = new() { Text = "אימון מחדש" };
    private int _trial, _attribute;
    private bool _answered, _navigating;
    private GripRelationshipTrial Current => GripRelationshipTrial.Examples[_trial];
    private int QuestionCount => Current.HasSmaller ? 4 : 3;

    public GripRelationshipTrainingPage()
    {
        Title = "אימון · שתי אחיזות";
        BackgroundColor = Color.FromArgb("#101D2C");
        FlowDirection = FlowDirection.RightToLeft;
        _drawing = new GraphicsView { Drawable = _scene, MinimumHeightRequest = 220, FlowDirection = FlowDirection.LeftToRight };
        _retry.Clicked += (_, _) => { _trial = _attribute = 0; ShowQuestion(); };
        _next.Clicked += async (_, _) =>
        {
            if (!_answered || _navigating) return;
            if (_attribute + 1 < QuestionCount) { _attribute++; ShowQuestion(); }
            else if (_trial + 1 < GripRelationshipTrial.Examples.Count) { _trial++; _attribute = 0; ShowQuestion(); }
            else
            {
                _navigating = true;
                try { await Navigation.PushAsync(new PartWholeIllustrationPage()); }
                finally { _navigating = false; }
            }
        };
        var controls = new HorizontalStackLayout { Spacing = 12, HorizontalOptions = LayoutOptions.Center };
        controls.Add(_retry); controls.Add(_next);
        var layout = new Grid { Padding = 16, RowSpacing = 10, RowDefinitions = new RowDefinitionCollection {
            new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto),
            new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Auto) } };
        layout.Add(_heading, 0, 0); layout.Add(_progress, 0, 1); layout.Add(_drawing, 0, 2);
        layout.Add(_question, 0, 3); layout.Add(_answers, 0, 4); layout.Add(_feedback, 0, 5); layout.Add(controls, 0, 6);
        Content = layout;
        ShowQuestion();
    }

    private void ShowQuestion()
    {
        _answered = false;
        _next.IsEnabled = false;
        _next.Text = "הבא";
        _progress.Text = $"זוג {_trial + 1} / {GripRelationshipTrial.Examples.Count} · שאלה {_attribute + 1} / {QuestionCount}";
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
            : _attribute == 2 ? "מיישרים תחתיות להשוואה. הפרש של 1–2 קרוב; הפרש של 4 רחוק. אורכים שווים קרובים גם הם."
            : "משווים את הקצה העליון ואת הקצה התחתון של הקטנה לאלה של הגדולה.";
        _answers.Clear();
        string[] choices = _attribute < 2 ? new[] { "ארוכה יותר", "קצרה יותר", "שוות באורך" }
            : _attribute == 2 ? new[] { "קרובים", "רחוקים" } : new[] { "למעלה", "למטה", "ללא הצמדה" };
        foreach (string choice in choices)
        {
            var button = new Button { Text = choice, MinimumWidthRequest = 88 };
            button.Clicked += (_, _) => Answer(choice);
            _answers.Add(button);
        }
    }

    private void Answer(string choice)
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
        _next.IsEnabled = true;
        foreach (var button in _answers.Children.OfType<Button>()) button.IsEnabled = false;
        _feedback.Text = _attribute switch
        {
            0 => $"נכון. האחיזה משמאל באורך {Current.Left.Length} — {expected}.",
            1 => $"נכון. האחיזה מימין באורך {Current.Right.Length} — {expected}.",
            2 when Current.Left.Length >= 6 && Current.Right.Length >= 6 && Current.LengthsAreClose == true =>
                $"נכון! שני הקטעים ארוכים — {Current.Left.Length} ו־{Current.Right.Length} — אבל האורכים קרובים: ההפרש רק {Current.LengthDifference}.",
            2 => $"נכון. ההפרש בין {Current.Left.Length} ל־{Current.Right.Length} הוא {Current.LengthDifference} — האורכים {expected}.",
            _ => $"נכון. הקטנה: {expected}, ביחס לגדולה."
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _heading.SetConcept("Compare two grips");
        await _heading.SpeakAsync();
    }

    protected override void OnDisappearing()
    {
        _heading.CancelSpeech();
        base.OnDisappearing();
    }

    private sealed class TrainingScene : IDrawable
    {
        public GripRelationshipTrial Trial { get; set; } = GripRelationshipTrial.Examples[0];
        public int Attribute { get; set; }
        public bool Reveal { get; set; }
        private readonly SideGripDrawable _hand = new();
        private const float Unit = 27, Bottom = 356, Left = 150, Right = 280, Width = 78;
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
                _hand.Draw(canvas, tip, Y(grip.Top - .5f), Y(grip.Bottom + .5f), Unit, 82);
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
                    Bracket(canvas, 267, low, high, Reveal ? Trial.LengthDifference.ToString() : "");
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
