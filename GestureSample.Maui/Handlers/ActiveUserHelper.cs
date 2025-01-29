using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureSample.Maui.Handlers
{
    public static class ActiveUserHelper
    {
        private const string CurrentUserKey = "CurrentUserId";

        public static Guid? CurrentUserId
        {
            get
            {
                var storedId = Preferences.Get(CurrentUserKey, string.Empty);
                if (Guid.TryParse(storedId, out var parsedId))
                    return parsedId;

                return null;
            }
            set
            {
                if (value.HasValue)
                    Preferences.Set(CurrentUserKey, value.Value.ToString());
                else
                    Preferences.Remove(CurrentUserKey);
            }
        }

        public static string CurrentUserName
        {
            get
            {
                var storedId = Preferences.Get("userName", string.Empty);
                return storedId;
                            }
            set
            {
                if (value!=null)
                    Preferences.Set("userName", value);
                else
                    Preferences.Remove("userName");
            }
        }
    }
}
