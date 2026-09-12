using Microsoft.Maui.Storage;

namespace GestureSample.Maui.Handlers
{
    public class UserPreferenceService
    {
        private const string NumericInputModePrefix = "user_numeric_input_mode_";

        public NumericInputMode GetPreferredNumericInputMode(Guid? userId)
        {
            if (!userId.HasValue || userId.Value == Guid.Empty)
                return NumericInputMode.Auto;

            string key = NumericInputModePrefix + userId.Value.ToString("D");
            int storedValue = Preferences.Default.Get(key, (int)NumericInputMode.Auto);
            return Enum.IsDefined(typeof(NumericInputMode), storedValue)
                ? (NumericInputMode)storedValue
                : NumericInputMode.Auto;
        }

        public void SetPreferredNumericInputMode(Guid? userId, NumericInputMode mode)
        {
            if (!userId.HasValue || userId.Value == Guid.Empty)
                return;

            string key = NumericInputModePrefix + userId.Value.ToString("D");
            Preferences.Default.Set(key, (int)mode);
        }
    }
}
