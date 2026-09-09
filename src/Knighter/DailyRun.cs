using System;
using System.Globalization;
using System.Collections.Generic;
using Knighter.Entities;
using Knighter.Helpers;

namespace Knighter;

public static class DailyRun
{
    public static bool Active;

    public static Screenshot LastScreenshot;

    private static int seed;

    // pre-daily snapshot so Begin()'s temporary unlock/max never becomes permanent
    private static bool snapValid;

    private static Character snapChar;

    private static bool snapUnlocked;

    private static int snapLevel;

    private static Character snapSelected;

    // attempts on the current daily (session-scope for now; persisted via ProfileData next)
    private static string attemptsDayKey = "";

    private static int attemptsToday;

    public static int AttemptsToday => Core.Instance.ProfileData.DailyAttemptsToday();

    public static void CountAttempt()
    {
        Core.Instance.ProfileData.CountDailyAttempt();
    }

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

    public static string TodayKey()
    {
        return DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    public static string TodayKeyMinus(int days)
    {
        return DateTime.UtcNow.AddDays(-days).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
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
        h = h * 31 + ((dc == (int)Character.Bragg && options.BraggFeathers) ? 1 : 0);
        h = h * 31 + ((dc == (int)Character.Bragg && options.BraggJam) ? 1 : 0);
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

    public static List<string> CollectMods(OptionsData o, Character character)
    {
        List<string> list = new List<string>();
        if (o.HardcoreWebs)
        {
            list.Add("hardcore webs");
        }
        if (character == Character.Knight && o.DirectionalThrust)
        {
            list.Add("dir thrust");
        }
        if (character == Character.Bragg && o.BraggAmmo)
        {
            list.Add("scavenger ammo");
        }
        if (character == Character.Bragg && o.BraggFeathers)
        {
            list.Add("feathers");
        }
        if (character == Character.Bragg && o.BraggJam)
        {
            list.Add("gun jam");
        }
        if (character == Character.Vampire && o.VampirePredator)
        {
            list.Add("predator");
        }
        if (character == Character.Vampire && o.UnfriendBats)
        {
            list.Add("unfriend bats");
        }
        if (character == Character.Vampire && o.FastWings)
        {
            list.Add("fast wings");
        }
        return list;
    }

    public static string ModsString(OptionsData o, Character character)
    {
        List<string> list = CollectMods(o, character);
        if (list.Count == 0)
        {
            return "vanilla";
        }
        return string.Join(" · ", list);
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