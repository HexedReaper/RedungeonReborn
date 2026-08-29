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
		base.core.Storage.Save();
	}
}
