using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LineupStatus
{
    Available,
    Locked,
    Forced
}

static class LineupStatusMethods
{
    public static LineupStatus Next(this LineupStatus s)
    {
        switch (s)
        {
            case LineupStatus.Available:
                return LineupStatus.Locked;
            case LineupStatus.Locked:
                return LineupStatus.Forced;
            case LineupStatus.Forced:
                return LineupStatus.Available;
        }
        return s;
    }
}
