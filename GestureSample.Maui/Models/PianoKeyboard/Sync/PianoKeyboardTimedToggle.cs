namespace GestureSample.Maui.Models
{
    internal class PianoKeyboardTimedToggle : PianoKeyboardSync
    {
        public PianoKeyboardTimedToggle(PPWGamePlay gamePlay, Label lblTimer, ProgressBar pressProgress, KeyboardConfig pianoConfig)
            : base(gamePlay, lblTimer, pressProgress, pianoConfig)
        {
        }

        private bool HasChangedFromInitial()
        {
            bool[] current = ToBitArray();
            if (initColors == null || current.Length != initColors.Length)
                return current.Any(bit => bit);

            for (int i = 0; i < current.Length; i++)
            {
                if (current[i] != initColors[i])
                    return true;
            }

            return false;
        }

        protected override bool HasActiveTimedAnswer()
        {
            return HasChangedFromInitial();
        }

        protected override bool InnerKeyDown(MR.Gestures.Button sender)
        {
            if (!IS_WHOLE_TIMER)
            {
                _seconds_pressed = 0;
                _pressStartUtc = null;
                _pressProgress.Progress = 0;
            }

            if (sender.BackgroundColor == COLOR_PRESSED)
            {
                sender.BackgroundColor = COLOR_FREE;
                if (Convert.ToInt32(sender.CommandParameter) > 5)
                    _addend2--;
                else
                    _addend1--;
            }
            else
            {
                sender.BackgroundColor = COLOR_PRESSED;
                if (Convert.ToInt32(sender.CommandParameter) > 5)
                    _addend2++;
                else
                    _addend1++;
            }

            if (_addend1 < 0) _addend1 = 0;
            if (_addend2 < 0) _addend2 = 0;

            if (!HasChangedFromInitial())
                ResetProgressVisual();
            else
                _pressStartUtc ??= DateTime.UtcNow;

            return true;
        }

        protected override bool InnerKeyUp(MR.Gestures.Button sender)
        {
            return true;
        }
    }
}
