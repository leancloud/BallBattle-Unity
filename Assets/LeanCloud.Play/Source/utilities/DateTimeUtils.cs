using System;

namespace LeanCloud.Play {
    internal static class DateTimeUtils {
        internal static long Now {
            get { 
                return (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            }
        }
    }
}
