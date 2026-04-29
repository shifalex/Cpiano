namespace GestureSample.Maui.Models
{
    public sealed class KeyboardSnapshotView : ContentView
    {
        private bool _rebuildScheduled;
        private string? _lastSnapshotSignature;

        public KeyboardSnapshotView()
        {
            InputTransparent = true;
        }

        public static readonly BindableProperty KeysProperty =
            BindableProperty.Create(nameof(Keys), typeof(bool[]), typeof(KeyboardSnapshotView), default(bool[]), propertyChanged: OnSnapshotChanged);

        public static readonly BindableProperty KeyColorsProperty =
            BindableProperty.Create(nameof(KeyColors), typeof(Color[]), typeof(KeyboardSnapshotView), default(Color[]), propertyChanged: OnSnapshotChanged);

        public static readonly BindableProperty KeysInRowProperty =
            BindableProperty.Create(nameof(KeysInRow), typeof(int), typeof(KeyboardSnapshotView), 10, propertyChanged: OnSnapshotChanged);

        public static readonly BindableProperty RowsProperty =
            BindableProperty.Create(nameof(Rows), typeof(int), typeof(KeyboardSnapshotView), 1, propertyChanged: OnSnapshotChanged);

        public static readonly BindableProperty WeightsProperty =
            BindableProperty.Create(nameof(Weights), typeof(int[]), typeof(KeyboardSnapshotView), default(int[]), propertyChanged: OnSnapshotChanged);

        public static readonly BindableProperty AboveNumberProperty =
            BindableProperty.Create(nameof(AboveNumber), typeof(int?), typeof(KeyboardSnapshotView), default(int?), propertyChanged: OnSnapshotChanged);

        public static readonly BindableProperty ArrowLengthProperty =
            BindableProperty.Create(nameof(ArrowLength), typeof(int?), typeof(KeyboardSnapshotView), default(int?), propertyChanged: OnSnapshotChanged);

        public static readonly BindableProperty DirectionProperty =
            BindableProperty.Create(nameof(Direction), typeof(Direction), typeof(KeyboardSnapshotView), Direction.Right, propertyChanged: OnSnapshotChanged);

        public static readonly BindableProperty CompactProperty =
            BindableProperty.Create(nameof(Compact), typeof(bool), typeof(KeyboardSnapshotView), true, propertyChanged: OnSnapshotChanged);

        public bool[] Keys
        {
            get => (bool[])GetValue(KeysProperty);
            set => SetValue(KeysProperty, value);
        }

        public Color[] KeyColors
        {
            get => (Color[])GetValue(KeyColorsProperty);
            set => SetValue(KeyColorsProperty, value);
        }

        public int KeysInRow
        {
            get => (int)GetValue(KeysInRowProperty);
            set => SetValue(KeysInRowProperty, value);
        }

        public int Rows
        {
            get => (int)GetValue(RowsProperty);
            set => SetValue(RowsProperty, value);
        }

        public int[] Weights
        {
            get => (int[])GetValue(WeightsProperty);
            set => SetValue(WeightsProperty, value);
        }

        public int? AboveNumber
        {
            get => (int?)GetValue(AboveNumberProperty);
            set => SetValue(AboveNumberProperty, value);
        }

        public int? ArrowLength
        {
            get => (int?)GetValue(ArrowLengthProperty);
            set => SetValue(ArrowLengthProperty, value);
        }

        public Direction Direction
        {
            get => (Direction)GetValue(DirectionProperty);
            set => SetValue(DirectionProperty, value);
        }

        public bool Compact
        {
            get => (bool)GetValue(CompactProperty);
            set => SetValue(CompactProperty, value);
        }

        private static void OnSnapshotChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is KeyboardSnapshotView view)
            {
                view.ScheduleRebuild();
            }
        }

        private void ScheduleRebuild()
        {
            if (_rebuildScheduled)
                return;

            _rebuildScheduled = true;
            Dispatcher.Dispatch(() =>
            {
                _rebuildScheduled = false;
                Rebuild();
            });
        }

        private void Rebuild()
        {
            bool[] keys = Keys;
            Color[] keyColors = KeyColors;
            int keyCount = keyColors?.Length ?? keys?.Length ?? 0;
            if (keyCount == 0)
            {
                Content = null;
                return;
            }

            int keysInRow = KeysInRow > 0 ? KeysInRow : keyCount;
            int rows = Rows > 0 ? Rows : 1;

            if (keysInRow * rows < keyCount)
            {
                rows = (int)Math.Ceiling((double)keyCount / Math.Max(1, keysInRow));
            }

            bool hasArrow = AboveNumber.HasValue && ArrowLength.HasValue;
            string snapshotSignature = BuildSnapshotSignature(
                keys,
                keyColors,
                keysInRow,
                rows,
                Weights,
                hasArrow,
                Direction,
                AboveNumber,
                ArrowLength,
                Compact);
            if (_lastSnapshotSignature == snapshotSignature)
                return;

            _lastSnapshotSignature = snapshotSignature;

            KeyboardConfig config = new()
            {
                KeysInRow = Math.Max(1, keysInRow),
                Rows = Math.Max(1, rows),
                IsArrow = hasArrow,
                IsArrowLengthDynamic = hasArrow,
                ShowNumbersOnKeys = Weights != null && Weights.Length > 0,
                WeightsArray = Weights?.ToArray()
            };

            double compactWidth = hasArrow
                ? Math.Max(240, (keysInRow * 22) + 32)
                : Math.Max(170, (keysInRow * 18) + 24);
            double compactHeight = hasArrow
                ? Math.Max(102, 70 + (rows * 20))
                : Math.Max(40, 34 + (rows * 12));
            double regularWidth = hasArrow
                ? Math.Max(280, (keysInRow * 26) + 40)
                : Math.Max(220, (keysInRow * 22) + 28);
            double regularHeight = hasArrow
                ? Math.Max(118, 84 + (rows * 24))
                : Math.Max(70, 50 + (rows * 20));

            PianoKeyboardReadOnly keyboard = new(config)
            {
                HeightRequest = Compact ? compactHeight : regularHeight,
                WidthRequest = -1,
                MinimumWidthRequest = Compact ? compactWidth : regularWidth,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center,
                InputTransparent = true,
                Scale = 1,
                Margin = hasArrow ? new Thickness(0, 4, 0, 0) : Thickness.Zero
            };

            HeightRequest = keyboard.HeightRequest;
            MinimumHeightRequest = keyboard.HeightRequest;

            if (keyColors != null && keyColors.Length > 0)
                keyboard.PianoInit(keyColors);
            else
                keyboard.PianoInit(keys);

            if (AboveNumber.HasValue && ArrowLength.HasValue)
            {
                keyboard.AddArrow(Direction, AboveNumber.Value, ArrowLength.Value);
            }

            Content = keyboard;
        }

        private static string BuildSnapshotSignature(
            bool[]? keys,
            Color[]? keyColors,
            int keysInRow,
            int rows,
            int[]? weights,
            bool hasArrow,
            Direction direction,
            int? aboveNumber,
            int? arrowLength,
            bool compact)
        {
            string keyBits = keys == null ? string.Empty : string.Join(string.Empty, keys.Select(bit => bit ? '1' : '0'));
            string colorBits = keyColors == null
                ? string.Empty
                : string.Join('|', keyColors.Select(color => $"{color.Red:0.###},{color.Green:0.###},{color.Blue:0.###},{color.Alpha:0.###}"));
            string weightBits = weights == null ? string.Empty : string.Join(',', weights);

            return $"{keysInRow}:{rows}:{hasArrow}:{direction}:{aboveNumber}:{arrowLength}:{compact}:{keyBits}:{colorBits}:{weightBits}";
        }
    }
}
