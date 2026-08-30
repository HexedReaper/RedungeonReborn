using System;
using Knighter.Entities;
using Knighter.Localization;

namespace Knighter;

public class Sharing : Component
{
    public void RateUs()
    {
        base.core.SystemCalls.OpenUrl("https://play.google.com/store/apps/details?id=com.nitrome.redungeon");
    }

    public void SendFeedback()
    {
        string versionString = base.core.SystemCalls.GetVersionString(withBuildNumber: false);
        string text = string.Format("{0}%20feedback%20(version%20{1})", "Redungeon", versionString);
        if (Settings.Os == Settings.OS.Android)
        {
            text = text.Replace("(", "[").Replace(")", "]");
        }
        string url = string.Format("mailto:{0}?subject={1}", "feedback@eneminds.com", text);
        base.core.SystemCalls.OpenUrl(url);
    }

    public void GoToWebPage()
    {
        base.core.SystemCalls.OpenUrl("https://www.eneminds.com");
    }

    public void ShareDaily(int distance, int coins, int seed, int character, string mods)
    {
        string charName = __(CharDescription.Get[(Character)character].Name);
        string text = string.Format("Redungeon Daily {0}\n{1} · {2}\n{3}m · {4} coins\nverify: {5}", DailyRun.TodayKey(), charName, mods, distance, coins, seed.ToString("X8"));
        Screenshot gameplayScreenshot = base.core.GameplayScreenshot;
        if (gameplayScreenshot != null)
        {
            base.core.SystemCalls.ShowSharingMenu(text, gameplayScreenshot);
        }
        else
        {
            string text2 = Uri.EscapeDataString(text);
            base.core.SystemCalls.OpenUrl(string.Format("https://twitter.com/intent/tweet?text={0}", text2));
        }
    }

    public void GoToNitromePage()
    {
        base.core.SystemCalls.OpenUrl("https://play.google.com/store/apps/developer?id=Nitrome");
    }
}