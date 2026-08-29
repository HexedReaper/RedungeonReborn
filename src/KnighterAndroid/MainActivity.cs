using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Knighter;
using Microsoft.Xna.Framework;
using Plugin.CurrentActivity;
using Plugin.InAppBilling;

namespace KnighterAndroid;

public class MainActivity : AndroidGameActivity
{
	private const string PublicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEApkL4oi0mANvYlhnBdPnOtBmuGwPnvlsK+QCn3pYQF9bNqoptwvE0BVbkZpWe/Q0nXcEw+rmps40iiulUa1akRTTHmQ89m7M/jzwH2LMoJqvPuWDSyYOhbOUxoNm3jQGs8WsLB9HGSyjt7IGWMRFAJXmFAtWJXbOlwEK2nDWEWircbBpYZvp95R0oH1vkZlqGb+42X/4YcpYhSb8HfeMvVBGIqpAZSKaxxeukEGQ4uFYV6CTLWD35j13wrcR2y86hLg6wPGNzuZ9pzzGmV2DXZ7Gs6/SJ6bQrjk2EGeiM/+1hwFLZU3Fa2ucNGbkJl5tmsOJWH/0R/FA/ggD31fTk3wIDAQAB";

	private GooglePlayHelper googlePlayHelper;

	private Game1 game;

	protected override void OnCreate(Bundle bundle)
	{
		base.OnCreate(bundle);
		game = new Game1();
		SetContentView((View)game.Services.GetService(typeof(View)));
		game.Run();
		googlePlayHelper = new GooglePlayHelper(this);
		googlePlayHelper.GravityForPopups = GravityFlags.Top;
		googlePlayHelper.Initialize();
		game.GooglePlayHelper = googlePlayHelper;
		HideSystemUi();
		Window.DecorView.SystemUiVisibilityChange += delegate
		{
			HideSystemUi();
		};
		CrossCurrentActivity.Current.Activity = this;
	}

	public override void OnWindowFocusChanged(bool hasFocus)
	{
		base.OnWindowFocusChanged(hasFocus);
		HideSystemUi();
	}

	protected override void OnStart()
	{
		base.OnStart();
		googlePlayHelper.Start();
	}

	protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
	{
		base.OnActivityResult(requestCode, resultCode, data);
		googlePlayHelper.OnActivityResult(requestCode, resultCode, data);
		InAppBillingImplementation.HandleActivityResult(requestCode, resultCode, data);
	}

	protected override void OnPause()
	{
		base.OnPause();
		game.OnEnteringBackground();
	}

	protected override void OnResume()
	{
		base.OnResume();
		HideSystemUi();
	}

	protected override void OnStop()
	{
		base.OnStop();
		googlePlayHelper.Stop();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void HideSystemUi()
	{
		if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
		{
			int num = 1798;
			num |= 0x1000;
			Window.DecorView.SystemUiVisibility = (StatusBarVisibility)num;
		}
	}
}
