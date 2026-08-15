namespace GestureSample.Maui.Models
{
    public sealed record PrecisionArrowDesignPreset(
        string Name,
        double ArrowTipFromBase,
        double NumberFromBase,
        double ShaftStopFromBase,
        double SideGap,
        double VerticalOffset);

    public sealed class PrecisionArrowDesignSettings
    {
        private const string Prefix = "precision-arrow-design-";

        // Add or edit presets here; the debug stage picks this list up automatically.
        public static IReadOnlyList<PrecisionArrowDesignPreset> Presets { get; } = new[]
        {
            new PrecisionArrowDesignPreset("Quarter", 0.25, 0.68, 0.00, 6, 0),
            new PrecisionArrowDesignPreset("High + close", 0.22, 0.64, 0.02, 2, -70),
            new PrecisionArrowDesignPreset("Very close", 0.18, 0.58, 0.00, 0, -35),
            new PrecisionArrowDesignPreset("Open", 0.34, 0.76, 0.08, 24, 20)
        };

        public double ArrowTipFromBase { get; set; } = 0.25;
        public double NumberFromBase { get; set; } = 0.68;
        public double ShaftStopFromBase { get; set; }
        public double SideGap { get; set; } = 6;
        public double VerticalOffset { get; set; }

        public static PrecisionArrowDesignSettings Load() => new()
        {
            ArrowTipFromBase = Preferences.Default.Get(Prefix + "tip", 0.25d),
            NumberFromBase = Preferences.Default.Get(Prefix + "number", 0.68d),
            ShaftStopFromBase = Preferences.Default.Get(Prefix + "stop", 0d),
            SideGap = Preferences.Default.Get(Prefix + "gap", 6d),
            VerticalOffset = Preferences.Default.Get(Prefix + "vertical", 0d)
        };

        public void Apply(PrecisionArrowDesignPreset preset)
        {
            ArrowTipFromBase = preset.ArrowTipFromBase;
            NumberFromBase = preset.NumberFromBase;
            ShaftStopFromBase = preset.ShaftStopFromBase;
            SideGap = preset.SideGap;
            VerticalOffset = preset.VerticalOffset;
        }

        public void Save()
        {
            Preferences.Default.Set(Prefix + "tip", ArrowTipFromBase);
            Preferences.Default.Set(Prefix + "number", NumberFromBase);
            Preferences.Default.Set(Prefix + "stop", ShaftStopFromBase);
            Preferences.Default.Set(Prefix + "gap", SideGap);
            Preferences.Default.Set(Prefix + "vertical", VerticalOffset);
        }
    }
}
