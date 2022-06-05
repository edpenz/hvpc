using System.Diagnostics;

public class Vt
{
    public const String CURSOR_LEFT = "\x001b[0G"; // Move cursor to start of line
    public const String CLEAR_RIGHT = "\x001b[0K"; // Clear line from right of cursor
}

public class Throbber
{
    private static long PERIOD_MILLISECONDS = 125;
    private static String[] TEXTS = {
        "   ",
        ".  ",
        ".. ",
        "...",
        " ..",
        "  .",
    };

    private Stopwatch timer;

    public Throbber()
    {
        timer = new Stopwatch();
        timer.Start();
    }

    public String Text
    {
        get
        {
            var elapsedPeriods = timer.ElapsedMilliseconds / PERIOD_MILLISECONDS;
            return TEXTS[elapsedPeriods % TEXTS.Length];
        }
    }

    public long RemainingMilliseconds
    {
        get
        {
            var periodDone = timer.ElapsedMilliseconds % PERIOD_MILLISECONDS;
            return PERIOD_MILLISECONDS - periodDone;
        }
    }

    public void Sleep()
    {
        Thread.Sleep((int)RemainingMilliseconds);
    }
}
