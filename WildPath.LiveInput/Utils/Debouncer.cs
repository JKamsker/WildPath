using System;

namespace WildPath.LiveInput.Utils;

internal class Debouncer
{
    public TimeSpan Interval { get; }
    public DateTime LastCall { get; private set; }

    public bool CanCall => (DateTime.UtcNow - LastCall) > Interval;

    public Debouncer(TimeSpan interval)
    {
        Interval = interval;
        LastCall = DateTime.UtcNow - (interval * 2);
    }

    public void Call()
    {
        LastCall = DateTime.UtcNow;
    }
    
    public bool TryCall()
    {
        if (CanCall)
        {
            Call();
            return true;
        }

        return false;
    }
}
