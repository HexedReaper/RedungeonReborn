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
	public bool VampirePredator;

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
		VampirePredator = false;
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
		base.core.Storage.TryGetBool("vampire-predator", ref VampirePredator);
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
		base.core.Storage.SetBool("vampire-predator", VampirePredator);
        base.core.Storage.Save();
	}
}
