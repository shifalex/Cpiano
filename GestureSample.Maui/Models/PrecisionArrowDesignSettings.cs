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
        private const string TipKey = Prefix + "tip-moving-half-v1";
        private const string NumberKey = Prefix + "number-moving-half-v1";

        // Add or edit presets here; the debug stage picks this list up automatically.
        public static IReadOnlyList<PrecisionArrowDesignPreset> Presets { get; } = new[]
        {
            new PrecisionArrowDesignPreset("Half", 0.50, 0.75, 0.00, 6, 0),
            new PrecisionArrowDesignPreset("High + close", 0.22, 0.64, 0.02, 2, -70),
            new PrecisionArrowDesignPreset("Very close", 0.18, 0.58, 0.00, 0, -35),
            new PrecisionArrowDesignPreset("Open", 0.34, 0.76, 0.08, 24, 20)
        };

        public double ArrowTipFromBase { get; set; } = 0.50;
        public double NumberFromBase { get; set; } = 0.75;
        public double ShaftStopFromBase { get; set; }
        public double SideGap { get; set; } = 6;
        public double VerticalOffset { get; set; }
        public int AdvancedStageKeyCount { get; set; } = 7;

        public static PrecisionArrowDesignSettings Load()
        {
            int defaultAdvancedStageKeyCount = DeviceInfo.Current.Idiom == DeviceIdiom.Tablet ? 9 : 7;
            return new PrecisionArrowDesignSettings
            {
                ArrowTipFromBase = Preferences.Default.Get(TipKey, 0.50d),
                NumberFromBase = Preferences.Default.Get(NumberKey, 0.75d),
                ShaftStopFromBase = Preferences.Default.Get(Prefix + "stop", 0d),
                SideGap = Preferences.Default.Get(Prefix + "gap", 6d),
                VerticalOffset = Preferences.Default.Get(Prefix + "vertical", 0d),
                AdvancedStageKeyCount = Math.Clamp(
                    Preferences.Default.Get(
                        Prefix + "advanced-key-count", defaultAdvancedStageKeyCount), 6, 12)
            };
        }

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
            Preferences.Default.Set(TipKey, ArrowTipFromBase);
            Preferences.Default.Set(NumberKey, NumberFromBase);
            Preferences.Default.Set(Prefix + "stop", ShaftStopFromBase);
            Preferences.Default.Set(Prefix + "gap", SideGap);
            Preferences.Default.Set(Prefix + "vertical", VerticalOffset);
            Preferences.Default.Set(Prefix + "advanced-key-count",
                Math.Clamp(AdvancedStageKeyCount, 6, 12));
        }
    }
}
