using System;
using System.Globalization;
using Knighter.Entities;
using Knighter.Helpers;

namespace Knighter;

public static class DailyRun
{
    public static bool Active;

    private static int seed;

    public static void Begin(int s)
    {
        Active = true;
        seed = s;
    }

    public static void End()
    {
        Active = false;
    }

    public static int TodaysSeed()
    {
        string key = "redungeon-daily-" + DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        int hash = 17;
        foreach (char c in key)
        {
            hash = hash * 31 + c;
        }
        return hash;
    }

    public static int SessionSeed(Character character, OptionsData options)
    {
        int h = TodaysSeed();
        h = h * 31 + (int)character;
        h = h * 31 + (options.HardcoreWebs ? 1 : 0);
        h = h * 31 + (options.DirectionalThrust ? 1 : 0);
        h = h * 31 + (options.BraggAmmo ? 1 : 0);
        h = h * 31 + (options.VampirePredator ? 1 : 0);
        h = h * 31 + (options.UnfriendBats ? 1 : 0);
        h = h * 31 + (options.FastWings ? 1 : 0);
        return h;
    }

    public static int Next(int channel, int index, int from, int to)
    {
        if (!Active)
        {
            return SciHelper.GetRandom(from, to);
        }
        int h = seed;
        h = h * 31 + channel;
        h = h * 31 + index;
        return new Random(h).Next(from, to + 1);
    }

    public static bool Chance(int channel, int index, float chance)
    {
        return (float)Next(channel, index, 1, 100) <= chance * 100f;
    }
}