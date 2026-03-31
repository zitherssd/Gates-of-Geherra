using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.Save;

[Serializable]
public class TimeLock
{
    public string id;                  // e.g. "QuickFight"
    public long endTimeUnix;        // When cooldown ends

    public TimeLock(string id, DateTime endTimeUtc)
    {
        this.id = id;
        this.endTimeUnix = ToUnix(endTimeUtc);

    }

    public bool IsDone => DateTime.UtcNow >= EndTimeUtc;
    public TimeSpan Remaining => EndTimeUtc - DateTime.UtcNow;

    public DateTime EndTimeUtc => FromUnix(endTimeUnix);

    public static long ToUnix(DateTime time)
    {
        return (long)(time - DateTime.UnixEpoch).TotalSeconds;
    }

    public static DateTime FromUnix(long unix)
    {
        return DateTime.UnixEpoch.AddSeconds(unix);
    }
}