using Android.Gms.Ads;
using Java.Lang;
using Microsoft.Xna.Framework;

namespace Knighter;

public class AdMobProxy : Object, IAdProxy
{
	private InterstitialAd interstitial;

	public AdsManager.OnShowDelegate OnShow { get; set; }

	public AdsManager.OnHideDelegate OnHide { get; set; }

	public AdsManager.OnVideoCompletedDelegate OnVideoCompleted { get; set; }

	public void Initialize()
	{
		interstitial = new InterstitialAd(Game.Activity);
		interstitial.AdUnitId = "ca-app-pub-0896659817499072/5912200267";
		interstitial.AdListener = new MyAdListener(this);
		LoadNextAd();
	}

	public bool CanShow()
	{
		if (interstitial != null)
		{
			return interstitial.IsLoaded;
		}
		return false;
	}

	public void Show()
	{
		interstitial.Show();
	}

	public void LoadNextAd()
	{
		interstitial.LoadAd(new AdRequest.Builder().Build());
	}
}
