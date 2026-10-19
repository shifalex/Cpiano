using System.Diagnostics;
using GestureSample.Maui.Models;
using Microsoft.Maui.Graphics;

namespace GestureSample.Views.Tests;

/// <summary>Learner-paced prototype: one visual change and one short explanation per segment.</summary>
public sealed class IntervalAnimationPage : ContentPage
{
    private readonly IntervalScene _scene = new();
    private readonly TutorialConceptHeading _heading = new();
    private readonly GraphicsView _drawing;
    private readonly Label _caption = new() { FontSize = 22, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center };
    private readonly Label _progress = new() { FontSize = 13, TextColor = Color.FromArgb("#A8B9CC"), HorizontalTextAlignment = TextAlignment.Center };
    private readonly Button _next = new() { Text = "מתחילים", BackgroundColor = Color.FromArgb("#63DAC5"), TextColor = Color.FromArgb("#10232B") };
    private readonly Button _replay = new() { Text = "שוב", IsEnabled = false };
    private readonly Button _back = new() { Text = "הקודם", IsEnabled = false };
    private readonly HorizontalStackLayout _answers = new() { Spacing = 12, HorizontalOptions = LayoutOptions.Center, IsVisible = false };
    private CancellationTokenSource? _animation;
    private int _step = -1;
    private bool _paused;
    private static readonly string[] Captions =
    {
        "שתי אצבעות מסמנות את קצות האחיזה.",
        "מודדים מתחתית הקליד התחתון עד ראש הקליד העליון.",
        "סופרים אורכי קליד, כולל שני הקצוות: 1, 2, 3, 4.",
        "האחיזה עולה. האורך נשאר 4.",
        "האחיזה נפתחת בקליד אחד. האורך גדל ל־5.",
        "האחיזה ירדה. מה אורך האחיזה עכשיו?"
    };

    public IntervalAnimationPage()
    {
        Title = "אורך האחיזה · טסט אנימציה";
        var illustrations = new ToolbarItem { Text = "חיבור, חיסור וחלקים" };
        illustrations.Clicked += async (_, _) => await Navigation.PushAsync(new PartWholeIllustrationPage());
        ToolbarItems.Add(illustrations);
        var relationshipTraining = new ToolbarItem { Text = "אימון · שתי אחיזות" };
        relationshipTraining.Clicked += async (_, _) => await Navigation.PushAsync(new GripRelationshipTrainingPage());
        ToolbarItems.Add(relationshipTraining);
        BackgroundColor = Color.FromArgb("#101D2C");
        FlowDirection = FlowDirection.RightToLeft;
        _drawing = new GraphicsView { Drawable = _scene, MinimumHeightRequest = 240, FlowDirection = FlowDirection.LeftToRight };
        var pause = new Button { Text = "השהיה" };
        pause.Clicked += (_, _) => { _paused = !_paused; pause.Text = _paused ? "המשך תנועה" : "השהיה"; };
        _next.Clicked += async (_, _) =>
        {
            if (_step == 5)
                await Navigation.PushAsync(new GripRelationshipTrainingPage());
            else
                await PlayAsync(_step + 1);
        };
        _replay.Clicked += async (_, _) => await PlayAsync(_step);
        _back.Clicked += async (_, _) => await PlayAsync(_step - 1);
        foreach (int value in new[] { 4, 5, 6 })
        {
            var answer = new Button { Text = value.ToString(), WidthRequest = 64 };
            answer.Clicked += (_, _) =>
            {
                _caption.Text = value == 5 ? "נכון. המיקום השתנה; האורך נשאר 5." : "סופרים גם את קלידי הקצה: האורך הוא 5 קלידים.";
                _scene.Measure = _scene.IntervalLength;
                _drawing.Invalidate();
                _next.IsEnabled = true;
            };
            _answers.Add(answer);
        }
        var controls = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star), new(GridLength.Star), new(GridLength.Star) }, ColumnSpacing = 8 };
        controls.Add(_back, 0); controls.Add(_replay, 1); controls.Add(pause, 2); controls.Add(_next, 3);
        var layout = new Grid { Padding = 16, RowSpacing = 12,
            RowDefinitions = new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Auto) } };
        layout.Add(_heading, 0, 0); layout.Add(_progress, 0, 1); layout.Add(_drawing, 0, 2); layout.Add(_caption, 0, 3);
        layout.Add(_answers, 0, 4); layout.Add(controls, 0, 5);
        Content = layout;
        _progress.Text = "מסתכלים · מודדים · מזיזים";
        _caption.Text = "איך רואים את אורך האחיזה?";
    }

    private async Task PlayAsync(int step)
    {
        _animation?.Cancel();
        var source = new CancellationTokenSource();
        _animation = source;
        _step = Math.Clamp(step, 0, 5);
        _heading.SetConcept("Segment length");
        _caption.Text = Captions[_step];
        _progress.Text = $"{_step + 1} / {Captions.Length}";
        _answers.IsVisible = false;
        _next.IsEnabled = _back.IsEnabled = _replay.IsEnabled = false;
        _scene.SetFrame(_step, 0);
        try
        {
            await _heading.SpeakAsync(source.Token);
            source.Token.ThrowIfCancellationRequested();
            var clock = Stopwatch.StartNew();
            double elapsed = 0;
            while (elapsed < 2600)
            {
                await Task.Delay(16, source.Token);
                double delta = clock.Elapsed.TotalMilliseconds;
                clock.Restart();
                if (!_paused) elapsed += delta;
                _scene.SetFrame(_step, (float)Math.Min(1, elapsed / 2600));
                _drawing.Invalidate();
            }
            _answers.IsVisible = _step == 5;
            _next.Text = "הבא";
            _next.IsEnabled = _step != 5;
            _back.IsEnabled = _step > 0;
            _replay.IsEnabled = true;
        }
        catch (OperationCanceledException) { }
        finally { if (ReferenceEquals(_animation, source)) _animation = null; source.Dispose(); }
    }

    protected override void OnDisappearing()
    {
        _animation?.Cancel();
        _heading.CancelSpeech();
        base.OnDisappearing();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_step >= 0) { _replay.IsEnabled = true; _back.IsEnabled = _step > 0; _next.IsEnabled = true; }
        _heading.SetConcept("Segment length");
        await _heading.SpeakAsync();
    }

    private sealed class IntervalScene : IDrawable
    {
        private readonly SideGripDrawable _hand = new();
        private float _lower = 1, _upper = 4, _handAlpha, _measureAlpha;
        public float Measure { get; set; }
        // Inclusive extent: bottom of the lower key to top of the upper key.
        public float IntervalLength => _upper - _lower + 1;

        public void SetFrame(int step, float progress)
        {
            float t = progress * progress * (3 - 2 * progress);
            _lower = step < 3 ? 1 : step == 3 ? 1 + t : step == 5 ? 2 - t : 2;
            _upper = step < 3 ? 4 : step == 3 ? 4 + t : step == 4 ? 5 + t : 6 - t;
            _handAlpha = step == 0 ? t : 1;
            _measureAlpha = step == 0 ? 0 : step == 1 ? t : 1;
            Measure = step < 2 || step == 5 ? 0 : IntervalLength * (step == 2 ? progress : 1);
        }

        public void Draw(ICanvas canvas, RectF bounds)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0) return;
            canvas.SaveState();
            float scale = Math.Min(bounds.Width / 380, bounds.Height / 430);
            canvas.Translate((bounds.Width - 380 * scale) / 2, (bounds.Height - 430 * scale) / 2);
            canvas.Scale(scale, scale);
            const float keyHeight = 48, keyX = 95, keyWidth = 95;
            float Y(float row) => 382 - row * keyHeight;
            for (int row = 0; row < 8; row++)
            {
                canvas.FillColor = row == (int)MathF.Round(_upper) || row == (int)MathF.Round(_lower)
                    ? Colors.Yellow : Color.FromArgb("#EDF1F5");
                canvas.FillRoundedRectangle(keyX, Y(row) - keyHeight / 2 + 1, keyWidth, keyHeight - 2, 5);
            }
            float upperY = Y(_upper), lowerY = Y(_lower);
            float intervalTop = upperY - keyHeight / 2;
            float intervalBottom = lowerY + keyHeight / 2;
            canvas.StrokeColor = Colors.Red.WithAlpha(_measureAlpha);
            canvas.StrokeSize = 3;
            canvas.DrawLine(74, intervalTop, 74, intervalBottom);
            canvas.DrawLine(67, intervalTop, 88, intervalTop);
            canvas.DrawLine(67, intervalBottom, 88, intervalBottom);
            for (int i = 1; i <= (int)Measure; i++)
            {
                float y = intervalBottom - i * keyHeight;
                canvas.DrawLine(68, y, 80, y);
                canvas.FontColor = Colors.White;
                canvas.FontSize = 19;
                canvas.DrawString(i.ToString(), 28, y, 32, keyHeight, HorizontalAlignment.Center, VerticalAlignment.Center);
            }
            // Contact faces coincide with key centers; pads occupy the outward halves.
            _hand.Draw(canvas, keyX + keyWidth * .13f, upperY, lowerY, keyHeight, 140, _handAlpha,
                upperFingerExtension: keyWidth * .13f - keyHeight / 8);
            canvas.RestoreState();
        }
    }
}
