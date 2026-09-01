using System;
using System.Globalization;
using Knighter.Entities;
using Knighter.Helpers;

namespace Knighter;

public static class DailyRun
{
    public static bool Active;

    public static Screenshot LastScreenshot;

    private static int seed;

    public static void Begin(int s, Core core)
    {
        Active = true;
        seed = s;
        Character daily = DailyCharacter();
        core.ProfileData.BeginDailyCharacterOverride(daily);
        core.ProfileData.Character = daily;
        core.ProfileData.Characters[daily].Unlocked = true;
        core.ProfileData.Characters[daily].Level = CharDescription.Get[daily].Levels.Count;
    }

    public static void End()
    {
        Active = false;
        Core.Instance.ProfileData.EndDailyCharacterOverride();
    }

    public static int AttemptsToday => Core.Instance.ProfileData.DailyAttemptsToday();

    public static void CountAttempt()
    {
        Core.Instance.ProfileData.CountDailyAttempt();
    }

    public static string TodayKey()
    {
        return DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    public static int TodaysSeed()
    {
        string key = "redungeon-daily-" + TodayKey();
        int hash = 17;
        foreach (char c in key)
        {
            hash = hash * 31 + c;
        }
        return hash;
    }

    public static Character DailyCharacter()
    {
        Array values = Enum.GetValues(typeof(Character));
        int index = new Random(TodaysSeed() ^ 0x5f5f).Next(values.Length);
        return (Character)values.GetValue(index);
    }

    public static int SessionSeed(OptionsData options)
    {
        int dc = (int)DailyCharacter();
        int h = TodaysSeed();
        h = h * 31 + dc;
        h = h * 31 + (options.HardcoreWebs ? 1 : 0);
        h = h * 31 + ((dc == (int)Character.Knight && options.DirectionalThrust) ? 1 : 0);
        h = h * 31 + ((dc == (int)Character.Bragg && options.BraggAmmo) ? 1 : 0);
        h = h * 31 + ((dc == (int)Character.Vampire && options.VampirePredator) ? 1 : 0);
        h = h * 31 + ((dc == (int)Character.Vampire && options.UnfriendBats) ? 1 : 0);
        h = h * 31 + ((dc == (int)Character.Vampire && options.FastWings) ? 1 : 0);
        return h;
    }

    public static int ResultCode(int sessionSeed, int distance, int coins, int revives)
    {
        int h = sessionSeed;
        h = h * 31 + distance;
        h = h * 31 + coins;
        h = h * 31 + revives;
        if (h == int.MinValue)
        {
            h = 42;
        }
        return h;
    }

    public static string ModsString(OptionsData o, Character character)
    {
        string text = "";
        int n = 0;
        if (o.HardcoreWebs)
        {
            text = ((n > 0) ? (text + " · ") : text) + "hardcore webs";
            n++;
        }
        if (character == Character.Knight && o.DirectionalThrust)
        {
            text = ((n > 0) ? (text + " · ") : text) + "dir thrust";
            n++;
        }
        if (character == Character.Bragg && o.BraggAmmo)
        {
            text = ((n > 0) ? (text + " · ") : text) + "bragg ammo";
            n++;
        }
        if (character == Character.Vampire && o.VampirePredator)
        {
            text = ((n > 0) ? (text + " · ") : text) + "predator";
            n++;
        }
        if (character == Character.Vampire && o.UnfriendBats)
        {
            text = ((n > 0) ? (text + " · ") : text) + "unfriend bats";
            n++;
        }
        if (character == Character.Vampire && o.FastWings)
        {
            text = ((n > 0) ? (text + " · ") : text) + "fast wings";
            n++;
        }
        if (n == 0)
        {
            return "vanilla";
        }
        return text;
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