using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Knighter;

public class Scores : Component, IScores
{
	public void Authenticate()
	{
		if (base.core.ProfileData.AutoSignIn)
		{
			base.core.Game.GooglePlayHelper.SignIn();
		}
	}

	public void ReportBestScore(bool gold)
	{
		try
		{
			if (base.core.Game.GooglePlayHelper.IsConnected)
			{
				base.core.Game.GooglePlayHelper.SubmitScore(GetValue(gold ? 2131165236 : 2131165235), base.core.ProfileData.LastDistance);
			}
		}
		catch (Exception ex)
		{
			Exception("Scores.ReportBestScoreA: " + ex.Message, isFatal: false);
		}
	}

	public void ReportAchievmentsProgress(List<Achievement> achievementsToReport)
	{
		if (!base.core.Game.GooglePlayHelper.IsConnected)
		{
			return;
		}
		foreach (Achievement item in achievementsToReport)
		{
			int progress = base.core.Achievments.GetProgress(item);
			int num = Achievements.Targets[item];
			if (IsOnlyIterativeOnAndroids(item))
			{
				ReportStepsAchievement(item, progress);
			}
			else if (progress >= num)
			{
				ReportAchievment(item);
			}
		}
	}

	private bool IsOnlyIterativeOnAndroids(Achievement achievement)
	{
		return new List<Achievement>
		{
			Achievement.MedusaDrawFiftySigns,
			Achievement.RikCollectHundredFireballs,
			Achievement.PanicBotDepleteHundredZappers,
			Achievement.BragFireHundredTimes,
			Achievement.SmashDozenPanicBots
		}.Contains(achievement);
	}

	private void ReportStepsAchievement(Achievement achievement, int steps)
	{
		try
		{
			if (base.core.Game.GooglePlayHelper.IsConnected)
			{
				string achievementCode = GetAchievementCode(achievement);
				base.core.Game.GooglePlayHelper.SetStepsAchievment(achievementCode, steps);
			}
		}
		catch (Exception ex)
		{
			Exception("Scores.ReportAchievementStepsA: " + ex.Message, isFatal: false);
		}
	}

	public void ReportAchievment(Achievement achievement)
	{
		try
		{
			if (base.core.Game.GooglePlayHelper.IsConnected)
			{
				string achievementCode = GetAchievementCode(achievement);
				base.core.Game.GooglePlayHelper.UnlockAchievement(achievementCode);
			}
		}
		catch (Exception ex)
		{
			Exception("Scores.ReportAchievmentA: " + ex.Message, isFatal: false);
		}
	}

	private static string GetValue(int resourceId)
	{
		return Game.Activity.Resources.GetString(resourceId);
	}

	private static string GetAchievementCode(Achievement achi)
	{
		return achi switch
		{
			Achievement.CollectThousandCoins => GetValue(2131165218), 
			Achievement.CreepScareHundredCreatures => GetValue(2131165223), 
			Achievement.FirstUnlock => GetValue(2131165213), 
			Achievement.FirstUpgrade => GetValue(2131165214), 
			Achievement.IchitakaCollectTwoThousandCoinsWithMagnet => GetValue(2131165225), 
			Achievement.KillHundredBats => GetValue(2131165219), 
			Achievement.KillHundredSlimes => GetValue(2131165221), 
			Achievement.KnightDeflectFiftyThings => GetValue(2131165222), 
			Achievement.LootHundredChests => GetValue(2131165217), 
			Achievement.MageSpendThreeMinutesInSloMo => GetValue(2131165228), 
			Achievement.NathanBreakFiveHundredObstacles => GetValue(2131165224), 
			Achievement.PlayForOneHour => GetValue(2131165216), 
			Achievement.RibLoseFiftySkulls => GetValue(2131165229), 
			Achievement.UnlockAllOfThem => GetValue(2131165215), 
			Achievement.VampireFlyFiftyMetersAsBat => GetValue(2131165226), 
			Achievement.VesnaDieWhileCastingSunrise => GetValue(2131165227), 
			Achievement.Webmaster => GetValue(2131165220), 
			Achievement.RikCollectHundredFireballs => GetValue(2131165231), 
			Achievement.MedusaDrawFiftySigns => GetValue(2131165230), 
			Achievement.PanicBotDepleteHundredZappers => GetValue(2131165232), 
			Achievement.BragFireHundredTimes => GetValue(2131165233), 
			Achievement.SmashDozenPanicBots => GetValue(2131165234), 
			_ => string.Empty, 
		};
	}

	public void ForceReportAllUnlockedAchievements()
	{
		foreach (Achievement value in Enum.GetValues(typeof(Achievement)))
		{
			if (base.core.ProfileData.IsAchievementUnlocked(value))
			{
				if (IsOnlyIterativeOnAndroids(value))
				{
					int steps = Achievements.Targets[value];
					ReportStepsAchievement(value, steps);
				}
				else
				{
					ReportAchievment(value);
				}
			}
		}
	}
}
