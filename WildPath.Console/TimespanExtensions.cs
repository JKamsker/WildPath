namespace WildPath.Console;

public static class TimespanExtensions
{
    public static CancellationToken ToCancellationToken(this TimeSpan timeSpan)
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(timeSpan);
        return cts.Token;
    }
}