using Com.Unity3d.Ads;
using Microsoft.Xna.Framework;

namespace Knighter;

public class UnityAdsProxy : Component, IAdProxy
{
	private readonly UnityAdsListener unityAdsListener;

	public AdsManager.OnShowDelegate OnShow { get; set; }

	public AdsManager.OnHideDelegate OnHide { get; set; }

	public AdsManager.OnVideoCompletedDelegate OnVideoCompleted { get; set; }

	public UnityAdsProxy()
	{
		unityAdsListener = new UnityAdsListener(this);
	}

	public void Initialize()
	{
		UnityAds.Initialize(Game.Activity, "116215", unityAdsListener);
	}

	public bool CanShow()
	{
		return UnityAds.IsReady;
	}

	public void Show()
	{
		UnityAds.Show(Game.Activity);
	}
}
