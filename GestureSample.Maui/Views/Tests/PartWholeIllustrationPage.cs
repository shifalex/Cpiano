using System.Diagnostics;
using GestureSample.Maui.Models;
using Microsoft.Maui.Graphics;

namespace GestureSample.Views.Tests;

/// <summary>Each relationship has three single-column and three two-column segments.</summary>
public sealed class PartWholeIllustrationPage : ContentPage
{
    private sealed record Lesson(string Name, string[] Captions, string Equation);
    private static readonly Lesson[] Lessons =
    {
        new("חבר · אחד מעל השני", new[] {
            "יד אחת אוחזת באורך 3, והשנייה בחלק באורך 2.",
            "היד השנייה מניחה את החלק מעל הקטע, בלי רווח ובלי חפיפה.",
            "שני החלקים יחד יוצרים אורך 5.",
            "עכשיו בשתי עמודות: אורך 3 משמאל וחלק באורך 2 מימין.",
            "מעלים את החלק הימני עד שתחתיתו בגובה ראש הקטע השמאלי.",
            "מודדים בין העמודות: מהתחתית משמאל עד הראש מימין — 5." }, "3 + 2 = 5"),
        new("חסר · מציבים ורואים מה נשאר", new[] {
            "יד אחת מסמנת שלם באורך 5, והשנייה חלק באורך 2.",
            "מציבים את החלק בתוך השלם, כך שהקצוות העליונים נפגשים.",
            "מתחת לחלק נשאר אורך 3. שתי האחיזות שומרות על אורכן.",
            "בשתי עמודות: השלם באורך 5 משמאל והחלק באורך 2 מימין.",
            "מעלים את החלק מימין עד ששני הקצוות העליונים באותו גובה.",
            "מהתחתית של השלם עד תחתית החלק נשאר אורך 3." }, "5 − 2 = 3"),
        new("חלק וחלק משלים", new[] {
            "יד אחת מסמנת שלם באורך 5, והשנייה חלק באורך 2.",
            "היד השנייה מניחה את החלק בתוך השלם, מהתחתית.",
            "החלק המשלים ממלא את מה שנותר: 3.",
            "בשתי עמודות: השלם באורך 5 משמאל והחלק הידוע, 2, מימין.",
            "משאירים את שתי התחתיות באותו גובה ומשווים את הקצוות העליונים.",
            "מראש החלק מימין עד ראש השלם משמאל חסר אורך 3 — החלק המשלים." }, "2 + 3 = 5"),
        new("חלק בפנים · חיסור", new[] {
            "יד אחת מסמנת שלם באורך 5, והשנייה חלק באורך 2.",
            "היד השנייה מכניסה את החלק לתוך השלם, בקצה העליון.",
            "מחפשים את מה שמחוץ לחלק אבל בתוך השלם: 3.",
            "בשתי עמודות: השלם משמאל והחלק מימין.",
            "מציבים את החלק מימין בתוך גובה השלם, כשהקצוות העליונים מיושרים.",
            "האורך שמתחת לחלק ובתוך השלם הוא 3, גם כשהידיים בעמודות שונות." }, "5 − 2 = 3"),
        new("חלק בחוץ · חיבור", new[] {
            "יד אחת מסמנת שלם באורך 5. השנייה מסמנת בתוכו חלק באורך 2.",
            "היד השנייה עולה מעל השלם. היד הראשונה שומרת על אורך 5.",
            "מודדים את השלם ואת החלק שבחוץ יחד: 7.",
            "בשתי עמודות: השלם משמאל, והחלק מימין נמצא עדיין בתוך גובהו.",
            "מעלים את החלק מימין עד שתחתיתו בגובה ראש השלם משמאל.",
            "מהתחתית משמאל עד הראש מימין: השלם והחלק שבחוץ יחד באורך 7." }, "5 + 2 = 7")
    };

    private readonly PartsScene _scene = new();
    private readonly TutorialConceptHeading _heading = new();
    private static readonly string[] ConceptTitles =
        { "Add", "Subtract", "Part and complementary part", "Part inside", "Part outside" };
    private readonly GraphicsView _drawing;
    private readonly Label _caption = new() { FontSize = 21, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center };
    private readonly Label _equation = new() { FontSize = 28, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center, FlowDirection = FlowDirection.LeftToRight };
    private readonly Label _progress = new() { TextColor = Colors.LightGray, HorizontalTextAlignment = TextAlignment.Center };
    private readonly Button _next = new() { Text = "הבא" };
    private readonly Button _replay = new() { Text = "שוב" };
    private readonly Button _back = new() { Text = "הקודם" };
    private readonly Button _pause = new() { Text = "השהיה" };
    private CancellationTokenSource? _animation;
    private int _lesson, _step;
    private bool _paused;

    public PartWholeIllustrationPage()
    {
        Title = "אורך · חלק · שלם";
        BackgroundColor = Color.FromArgb("#101D2C");
        FlowDirection = FlowDirection.RightToLeft;
        _drawing = new GraphicsView { Drawable = _scene, MinimumHeightRequest = 220, FlowDirection = FlowDirection.LeftToRight };
        var picker = new Picker { Title = "בחרו המחשה", TextColor = Colors.White,
            ItemsSource = new[] { "אורך קטע", "אימון · שתי אחיזות" }.Concat(Lessons.Select(x => x.Name)).ToList(), SelectedIndex = 2 };
        picker.SelectedIndexChanged += async (_, _) =>
        {
            if (picker.SelectedIndex < 0) return;
            if (picker.SelectedIndex <= 1)
            {
                _animation?.Cancel();
                _heading.CancelSpeech();
                var target = picker.SelectedIndex == 0
                    ? Navigation.NavigationStack.FirstOrDefault(page => page is IntervalAnimationPage)
                    : Navigation.NavigationStack.FirstOrDefault(page => page is GripRelationshipTrainingPage);
                if (target != null)
                {
                    while (Navigation.NavigationStack.Last() != target)
                        await Navigation.PopAsync(false);
                }
                else
                    await Navigation.PushAsync(picker.SelectedIndex == 0
                        ? new IntervalAnimationPage() : new GripRelationshipTrainingPage());
                return;
            }
            _lesson = picker.SelectedIndex - 2;
            await PlayAsync(0);
        };
        _next.Clicked += async (_, _) =>
        {
            if (_step < Lessons[_lesson].Captions.Length - 1)
                await PlayAsync(_step + 1);
            else
                // The last concept returns to the first concept, segment length.
                picker.SelectedIndex = _lesson + 1 < Lessons.Length ? _lesson + 3 : 0;
        };
        _replay.Clicked += async (_, _) => await PlayAsync(_step);
        _back.Clicked += async (_, _) => await PlayAsync(Math.Max(0, _step - 1));
        _pause.Clicked += (_, _) => { _paused = !_paused; _pause.Text = _paused ? "המשך תנועה" : "השהיה"; };
        var controls = new Grid { ColumnSpacing = 8, ColumnDefinitions = new ColumnDefinitionCollection {
            new(GridLength.Star), new(GridLength.Star), new(GridLength.Star), new(GridLength.Star) } };
        controls.Add(_back, 0); controls.Add(_replay, 1); controls.Add(_pause, 2); controls.Add(_next, 3);
        var layout = new Grid { Padding = 16, RowSpacing = 10, RowDefinitions = new RowDefinitionCollection {
            new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Auto) } };
        layout.Add(picker, 0, 0); layout.Add(_heading, 0, 1); layout.Add(_progress, 0, 2); layout.Add(_drawing, 0, 3);
        layout.Add(_caption, 0, 4); layout.Add(_equation, 0, 5); layout.Add(controls, 0, 6);
        Content = layout;
    }

    private async Task PlayAsync(int step)
    {
        _animation?.Cancel();
        var source = new CancellationTokenSource();
        _animation = source;
        _step = step;
        _heading.SetConcept(ConceptTitles[_lesson]);
        _paused = false;
        _pause.Text = "השהיה";
        _caption.Text = Lessons[_lesson].Captions[step];
        _equation.Text = " "; // The symbols appear only after the visual relationship is established.
        _progress.Text = $"{step + 1} / {Lessons[_lesson].Captions.Length} · {(step < 3 ? "עמודה אחת" : "שתי עמודות")}";
        _next.IsEnabled = _back.IsEnabled = _replay.IsEnabled = false;
        _scene.SetFrame(_lesson, step, 0);
        _drawing.Invalidate();
        try
        {
            await _heading.SpeakAsync(source.Token);
            source.Token.ThrowIfCancellationRequested();
            var clock = Stopwatch.StartNew();
            double elapsed = 0;
            while (elapsed < 2200)
            {
                await Task.Delay(16, source.Token);
                double delta = clock.Elapsed.TotalMilliseconds;
                clock.Restart();
                if (!_paused) elapsed += delta;
                _scene.SetFrame(_lesson, step, (float)Math.Min(1, elapsed / 2200));
                _drawing.Invalidate();
            }
            if (step % 3 == 2) _equation.Text = Lessons[_lesson].Equation;
            _next.Text = "הבא";
            _next.IsEnabled = _replay.IsEnabled = true;
            _back.IsEnabled = step > 0;
        }
        catch (OperationCanceledException) { }
        finally { if (ReferenceEquals(source, _animation)) _animation = null; source.Dispose(); }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await PlayAsync(_step);
    }

    protected override void OnDisappearing()
    {
        _animation?.Cancel();
        _heading.CancelSpeech();
        base.OnDisappearing();
    }

    private sealed class PartsScene : IDrawable
    {
        private readonly SideGripDrawable _wholeHand = new();
        private readonly SideGripDrawable _partHand = new();
        private int _lesson, _step;
        private float _motion, _reveal;
        private const float Unit = 44, Bottom = 382, KeyX = 245, KeyWidth = 85;
        // Keep only the space needed for a bracket, its number and small side margins.
        private const float ColumnGap = 52;
        private const float SecondColumnX = KeyX + KeyWidth + ColumnGap, SceneWidth = 550;
        private static float Y(float height) => Bottom - height * Unit;

        public void SetFrame(int lesson, int step, float progress)
        {
            _lesson = lesson; _step = step;
            float t = progress * progress * (3 - 2 * progress);
            int phase = step % 3;
            _motion = phase == 0 ? 0 : phase == 1 ? t : 1;
            _reveal = phase == 2 ? t : 0;
        }

        public void Draw(ICanvas canvas, RectF bounds)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0) return;
            canvas.SaveState();
            float scale = Math.Min(bounds.Width / SceneWidth, bounds.Height / 420);
            canvas.Translate((bounds.Width - SceneWidth * scale) / 2, (bounds.Height - 420 * scale) / 2);
            canvas.Scale(scale, scale);
            float whole = _lesson == 0 ? 3 : 5;
            // Addition/subtraction place fixed lengths. The result is a separate extent,
            // never an enlargement or contraction of either hand's grip.
            float grip = whole;
            bool twoColumns = _step >= 3;
            float partX = twoColumns ? SecondColumnX : KeyX;
            float partBottom = _lesson switch { 0 or 1 or 3 => 3 * _motion, 4 => 3 + 2 * _motion, _ => 0 };

            // Yellow always refers to a whole pressed key; red always denotes length.
            for (int column = 0; column < (twoColumns ? 2 : 1); column++)
            {
                float x = column == 0 ? KeyX : SecondColumnX;
                for (int row = 0; row < 8; row++)
                {
                    bool wholeContact = column == 0 && (row == 0 || row == (int)grip - 1);
                    bool partContact = (column == 1 || !twoColumns) &&
                        (row == (int)MathF.Round(partBottom) || row == (int)MathF.Round(partBottom + 1));
                    canvas.FillColor = wholeContact || partContact ? Colors.Yellow : Color.FromArgb("#EDF1F5");
                    canvas.FillRoundedRectangle(x, Y(row + 1) + 1, KeyWidth, Unit - 2, 4);
                }
            }
            // Keep the starting whole visible as a reference when a part moves.
            Bracket(canvas, 55, 0, whole, whole.ToString("0"), .45f);
            canvas.FillColor = Colors.Red.WithAlpha(.09f);
            canvas.FillRectangle(KeyX, Y(whole), KeyWidth, whole * Unit);

            // The red outline follows the second hand; pressed keys stay on their grid.
            canvas.StrokeColor = Colors.Red;
            canvas.StrokeSize = 2;
            canvas.DrawRectangle(partX + 6, Y(partBottom + 2) + 3, KeyWidth - 12, 2 * Unit - 6);
            canvas.FontColor = Color.FromArgb("#101D2C");
            canvas.FontSize = 22;
            canvas.DrawString("2", partX + 6, Y(partBottom + 2), KeyWidth - 12, 2 * Unit,
                HorizontalAlignment.Center, VerticalAlignment.Center);
            // Dashed join is located exactly at the boundary shared by the two parts.
            if (_motion > .98f)
            {
                float join = _lesson == 2 ? 2 : _lesson == 4 ? 5 : 3;
                canvas.StrokeColor = Colors.Red;
                canvas.StrokeDashPattern = new float[] { 4, 4 };
                canvas.DrawLine(KeyX - 8, Y(join), partX + KeyWidth + 8, Y(join));
                if (twoColumns)
                {
                    // Project corresponding heights across the gap without changing either grip.
                    foreach (float height in new[] { 0f, whole, partBottom, partBottom + 2 }.Distinct())
                        canvas.DrawLine(KeyX + KeyWidth, Y(height), SecondColumnX, Y(height));
                }
                canvas.StrokeDashPattern = null;
            }
            if (_reveal > 0)
            {
                float resultBottom = _lesson == 2 ? 2 : 0;
                float resultLength = _lesson switch { 0 => 5, 4 => 7, _ => 3 };
                Bracket(canvas, twoColumns ? SecondColumnX - 14 : 112, resultBottom, resultLength,
                    resultLength.ToString("0"), _reveal);
            }
            // Opposing palms keep both hands legible when their grips share the keyboard.
            float wholeTip = KeyX + KeyWidth * .60f;
            canvas.SaveState();
            canvas.Translate(2 * wholeTip, 0);
            canvas.Scale(-1, 1);
            _wholeHand.Draw(canvas, wholeTip, Y(grip - .5f), Y(.5f), Unit, 95, .9f);
            canvas.RestoreState();
            _partHand.Draw(canvas, partX + KeyWidth * .40f,
                Y(partBottom + 1.5f), Y(partBottom + .5f), Unit, 95, .9f);
            canvas.RestoreState();
        }

        private static void Bracket(ICanvas canvas, float x, float bottom, float length, string label, float alpha)
        {
            canvas.StrokeColor = Colors.Red.WithAlpha(alpha);
            canvas.StrokeSize = 3;
            canvas.DrawLine(x, Y(bottom), x, Y(bottom + length));
            canvas.DrawLine(x - 6, Y(bottom), x + 10, Y(bottom));
            canvas.DrawLine(x - 6, Y(bottom + length), x + 10, Y(bottom + length));
            canvas.FontColor = Colors.White.WithAlpha(alpha);
            canvas.FontSize = 23;
            canvas.DrawString(label, x - 34, Y(bottom + length), 28, length * Unit,
                HorizontalAlignment.Center, VerticalAlignment.Center);
        }
    }
}
