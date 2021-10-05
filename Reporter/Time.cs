using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Reporter
{
    public class Time
    {
        public Time()
        {
            SpanCallback["second"] = Seconds;
            SpanCallback["seconds"] = Seconds;
            SpanCallback["sec"] = Seconds;
            SpanCallback["s"] = Seconds;

            SpanCallback["minute"] = Minutes;
            SpanCallback["minutes"] = Minutes;
            SpanCallback["min"] = Minutes;
            SpanCallback["m"] = Minutes;

            SpanCallback["hour"] = Hours;
            SpanCallback["hours"] = Hours;
            SpanCallback["h"] = Hours;

            SpanCallback["day"] = Days;
            SpanCallback["days"] = Days;
            SpanCallback["d"] = Days;

            SpanCallback["week"] = Weeks;
            SpanCallback["weeks"] = Weeks;
            SpanCallback["w"] = Weeks;

            SpanCallback["month"] = Months;
            SpanCallback["months"] = Months;
        }

        delegate TimeSpan CallBackDelegate(string match);

        readonly Dictionary<string, CallBackDelegate> SpanCallback = new();

        private TimeSpan Seconds(string match)
            => new(0, 0, int.Parse(match));

        private TimeSpan Minutes(string match)
            => new(0, int.Parse(match), 0);

        private TimeSpan Hours(string match)
            => new(int.Parse(match), 0, 0);

        private TimeSpan Days(string match)
            => new(int.Parse(match), 0, 0, 0);

        private TimeSpan Weeks(string match)
            => new((int.Parse(match) * 7), 0, 0, 0);

        private TimeSpan Months(string match)
            => new((int.Parse(match) * 30), 0, 0, 0);

        private readonly Regex Regex = new(@"(\d*)\s*([a-zA-Z]*)\s*(?:and|,)?\s*");

        /// <summary>
        /// Gets a valid <see cref="TimeSpan"/> from the provided <c>string</c>.
        /// </summary>
        /// <param name="input">The disposable <c>string</c> where the timespan originates from</param>
        /// <returns>A <see cref="TimeSpan"/> struct. that holds all valid entries from the provided <c>string</c>.</returns>
        public TimeSpan GetSpanFromString(string input)
        {
            if (!TimeSpan.TryParse(input, out TimeSpan span))
            {
                _ = input.ToLower().Trim();
                MatchCollection matches = Regex.Matches(input);
                if (matches.Any())
                {
                    foreach (Match match in matches)
                    {
                        if (SpanCallback.TryGetValue(match.Groups[2].Value, out CallBackDelegate callback))
                        {
                            Console.WriteLine(span);
                            span += callback(match.Groups[1].Value);
                        }
                    }
                }
            }
            return span;
        }

        /// <summary>
        /// Gets a valid <see cref="DateTime"/> from the provided <c>string</c>.
        /// </summary>
        /// <param name="input">The disposable <c>string</c> where the timespan originates from</param>
        /// <param name="time">An optional <see cref="DateTime"/> entry to match the originating <see cref="TimeSpan"/> with</param>
        /// <returns>A <see cref="DateTime"/> struct. that holds all valid entries from the provided <c>string</c> summed with the optional <paramref name="time"/></returns>
        public bool GetFromString(string input, out DateTime time)
        {
            var span = GetSpanFromString(input);
            time = DateTime.UtcNow.Subtract(span);
            if (span != TimeSpan.Zero)
                return true;
            return false;
        }
    }
}

