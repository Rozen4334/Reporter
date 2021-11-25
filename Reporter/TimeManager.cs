using System.Text.RegularExpressions;

namespace Reporter;

public class TimeManager
{
    // Delegate for method handling
    private readonly Dictionary<string, Func<string, TimeSpan>> _callback = new();

    // Regular expression to handle time coming in
    private readonly Regex Regex = new(@"(\d*)\s*([a-zA-Z]*)\s*(?:and|,)?\s*");

    /// <summary>
    /// Ctor
    /// </summary>
    public TimeManager()
    {
        // Configure all matching attributes to delegate
        _callback["second"] = Seconds;
        _callback["seconds"] = Seconds;
        _callback["sec"] = Seconds;
        _callback["s"] = Seconds;

        _callback["minute"] = Minutes;
        _callback["minutes"] = Minutes;
        _callback["min"] = Minutes;
        _callback["m"] = Minutes;

        _callback["hour"] = Hours;
        _callback["hours"] = Hours;
        _callback["h"] = Hours;

        _callback["day"] = Days;
        _callback["days"] = Days;
        _callback["d"] = Days;

        _callback["week"] = Weeks;
        _callback["weeks"] = Weeks;
        _callback["w"] = Weeks;

        _callback["month"] = Months;
        _callback["months"] = Months;
    }

    // Gets the second param into timespan
    private TimeSpan Seconds(string match)
        => new(0, 0, int.Parse(match));

    // Gets the minute param into timespan
    private TimeSpan Minutes(string match)
        => new(0, int.Parse(match), 0);

    // Gets the hour param into timespan
    private TimeSpan Hours(string match)
        => new(int.Parse(match), 0, 0);

    // Gets the day param into timespan
    private TimeSpan Days(string match)
        => new(int.Parse(match), 0, 0, 0);

    // Gets the week param into timespan
    private TimeSpan Weeks(string match)
        => new((int.Parse(match) * 7), 0, 0, 0);

    // Gets the month param into timespan
    private TimeSpan Months(string match)
        => new((int.Parse(match) * 30), 0, 0, 0);

    // Gets the timespan struct from a string
    private TimeSpan GetSpanFromString(string input)
    {
        if (!TimeSpan.TryParse(input, out TimeSpan span))
        {
            _ = input.ToLower().Trim();
            MatchCollection matches = Regex.Matches(input);
            if (matches.Any())
                foreach (Match match in matches)
                    if (_callback.TryGetValue(match.Groups[2].Value, out var callback))
                        span += callback(match.Groups[1].Value);
        }
        return span;
    }

    /// <summary>
    /// Retrieves the time from a string.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public bool TryGetFromString(string input, out DateTime time)
    {
        var span = GetSpanFromString(input);
        time = DateTime.UtcNow.Subtract(span);
        return span != TimeSpan.Zero;
    }
}