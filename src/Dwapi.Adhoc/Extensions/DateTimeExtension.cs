using System;

namespace Dwapi.Adhoc.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Convert date to Unix timestamp
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static double ToUnixTimestamp(this DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalMilliseconds);
        }
    }
}
