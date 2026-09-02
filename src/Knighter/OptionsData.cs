namespace Knighter;

public class OptionsData : Component
{
	public bool PlayMusic;

	public bool PlaySounds;

	public bool SwipeControl;

	public bool CompactDPad;

	public bool HoldToRun;

	public bool TapToStep;

	public bool LeftHandedMode;

	public bool SeeThroughMode;
	public bool HardcoreWebs;
	public bool DirectionalThrust;
	public bool BraggAmmo;
	public bool BraggFeathers;
	public bool BraggJam;
	public bool VampirePredator;
	public bool UnfriendBats;
	public bool FastWings;
	public bool AchievementToasts;
	public bool DailyIconAnimated;
	public bool DailyRunEnabled;

	public OptionsData()
	{
		PlayMusic = true;
		PlaySounds = true;
		SwipeControl = true;
		CompactDPad = false;
		HoldToRun = false;
		TapToStep = true;
		LeftHandedMode = false;
		SeeThroughMode = false;
		HardcoreWebs = false;
		DirectionalThrust = false;
		BraggAmmo = false;
		BraggFeathers = false;
		BraggJam = false;
		VampirePredator = false;
		UnfriendBats = false;
		FastWings = false;
		AchievementToasts = true;
		DailyIconAnimated = false;
		DailyRunEnabled = false;
	}

	public void LoadFromStorage()
	{
		base.core.Storage.TryGetBool("play-music", ref PlayMusic);
		base.core.Storage.TryGetBool("play-sounds", ref PlaySounds);
		base.core.Storage.TryGetBool("swipe-control", ref SwipeControl);
		base.core.Storage.TryGetBool("compact-dpad", ref CompactDPad);
		base.core.Storage.TryGetBool("hold-to-run", ref HoldToRun);
		base.core.Storage.TryGetBool("tap-to-step", ref TapToStep);
		base.core.Storage.TryGetBool("left-handed", ref LeftHandedMode);
		base.core.Storage.TryGetBool("see-through", ref SeeThroughMode);
        base.core.Storage.TryGetBool("hardcore-webs", ref HardcoreWebs);
		base.core.Storage.TryGetBool("directional-thrust", ref DirectionalThrust);
		base.core.Storage.TryGetBool("bragg-ammo", ref BraggAmmo);
		base.core.Storage.TryGetBool("bragg-feathers", ref BraggFeathers);
        base.core.Storage.TryGetBool("bragg-gunjam", ref BraggJam);
		base.core.Storage.TryGetBool("vampire-predator", ref VampirePredator);
		base.core.Storage.TryGetBool("unfriend-bats", ref UnfriendBats);
		base.core.Storage.TryGetBool("fast-wings", ref FastWings);
		base.core.Storage.TryGetBool("achievement-toasts", ref AchievementToasts);
		base.core.Storage.TryGetBool("daily-icon-animated", ref DailyIconAnimated);
		base.core.Storage.TryGetBool("daily-run", ref DailyRunEnabled);
	}

	public void SaveIntoStorage()
	{
		base.core.Storage.SetBool("play-music", PlayMusic);
		base.core.Storage.SetBool("play-sounds", PlaySounds);
		base.core.Storage.SetBool("swipe-control", SwipeControl);
		base.core.Storage.SetBool("compact-dpad", CompactDPad);
		base.core.Storage.SetBool("hold-to-run", HoldToRun);
		base.core.Storage.SetBool("tap-to-step", TapToStep);
		base.core.Storage.SetBool("left-handed", LeftHandedMode);
        base.core.Storage.SetBool("see-through", SeeThroughMode);
        base.core.Storage.SetBool("hardcore-webs", HardcoreWebs);
		base.core.Storage.SetBool("directional-thrust", DirectionalThrust);
		base.core.Storage.SetBool("bragg-ammo", BraggAmmo);
		base.core.Storage.SetBool("bragg-feathers", BraggFeathers);
        base.core.Storage.SetBool("bragg-gunjam", BraggJam);
		base.core.Storage.SetBool("vampire-predator", VampirePredator);
		base.core.Storage.SetBool("unfriend-bats", UnfriendBats);
		base.core.Storage.SetBool("fast-wings", FastWings);
		base.core.Storage.SetBool("achievement-toasts", AchievementToasts);
		base.core.Storage.SetBool("daily-icon-animated", DailyIconAnimated);
		base.core.Storage.SetBool("daily-run", DailyRunEnabled);
        base.core.Storage.Save();
	}
}
