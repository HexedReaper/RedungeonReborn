using System;
using Android.Runtime;
using Com.Unity3d.Ads;
using Java.Lang;

namespace Knighter;

public class UnityAdsListener : Java.Lang.Object, IUnityAdsListener, IJavaObject, IDisposable
{
	private readonly UnityAdsProxy proxy;

	public UnityAdsListener(UnityAdsProxy proxy)
	{
		this.proxy = proxy;
	}

	public void OnUnityAdsError(UnityAds.UnityAdsError p0, string p1)
	{
	}

	public void OnUnityAdsFinish(string placementId, UnityAds.FinishState state)
	{
		if (proxy.OnVideoCompleted != null)
		{
			proxy.OnVideoCompleted(placementId, state != UnityAds.FinishState.Completed);
		}
		if (proxy.OnHide != null)
		{
			proxy.OnHide();
		}
	}

	public void OnUnityAdsReady(string p0)
	{
	}

	public void OnUnityAdsStart(string p0)
	{
		if (proxy.OnShow != null)
		{
			proxy.OnShow();
		}
	}
}
