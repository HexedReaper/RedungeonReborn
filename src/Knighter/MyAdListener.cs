using Android.Gms.Ads;

namespace Knighter;

public class MyAdListener : AdListener
{
	private readonly AdMobProxy proxy;

	public MyAdListener(AdMobProxy proxy)
	{
		this.proxy = proxy;
	}

	public override void OnAdClosed()
	{
		if (proxy.OnVideoCompleted != null)
		{
			proxy.OnVideoCompleted("", skipped: false);
		}
		if (proxy.OnHide != null)
		{
			proxy.OnHide();
		}
		proxy.LoadNextAd();
		base.OnAdClosed();
	}

	public override void OnAdOpened()
	{
		if (proxy.OnShow != null)
		{
			proxy.OnShow();
		}
		base.OnAdOpened();
	}
}
