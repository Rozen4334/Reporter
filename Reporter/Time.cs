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
        private delegate TimeSpan CallBackDelegate(string match);
        private readonly Dictionary<string, CallBackDelegate> SpanCallback = new();

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

        private TimeSpan GetSpanFromString(string input)
        {
            if (!TimeSpan.TryParse(input, out TimeSpan span))
            {
                _ = input.ToLower().Trim();
                MatchCollection matches = Regex.Matches(input);
                if (matches.Any())
                    foreach (Match match in matches)
                        if (SpanCallback.TryGetValue(match.Groups[2].Value, out CallBackDelegate callback))
                            span += callback(match.Groups[1].Value);
            }
            return span;
        }

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

