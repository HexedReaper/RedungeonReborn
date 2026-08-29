using System;
using System.IO;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.OS;
using Android.Support.V4.Content;
using Java.IO;
using Microsoft.Xna.Framework;

namespace Knighter;

public class SystemCalls : Component, ISystemCalls
{
	public event EventHandler InternetStatusChanged
	{
		add
		{
		}
		remove
		{
		}
	}

	public void OpenUrl(string url)
	{
		try
		{
			Android.Net.Uri uri = Android.Net.Uri.Parse(url);
			Intent intent = new Intent("android.intent.action.VIEW", uri);
			Game.Activity.StartActivity(intent);
		}
		catch
		{
			Exception("SystemCalls.OpenUrl: " + url, isFatal: false);
		}
	}

	public bool IsInternetAvailable()
	{
		if (!(Game.Activity.GetSystemService("connectivity") is ConnectivityManager { ActiveNetworkInfo: var activeNetworkInfo }))
		{
			return false;
		}
		return activeNetworkInfo?.IsConnected ?? false;
	}

	public void ShowLeaderboards()
	{
		if (base.core.Game.GooglePlayHelper.IsConnected && !base.core.Game.GooglePlayHelper.SignedOut)
		{
			string leaderboardCode = Game.Activity.Resources.GetString(2131165235);
			base.core.Game.GooglePlayHelper.ShowLeaderBoardIntentForLeaderboard(leaderboardCode);
		}
		else
		{
			base.core.Game.GooglePlayHelper.SignIn();
		}
	}

	public void ShowAchievments()
	{
		if (base.core.Game.GooglePlayHelper.IsConnected && !base.core.Game.GooglePlayHelper.SignedOut)
		{
			base.core.Game.GooglePlayHelper.ShowAchievements();
		}
		else
		{
			base.core.Game.GooglePlayHelper.SignIn();
		}
	}

	public void ShowSharingMenu(string text, Screenshot screenshot)
	{
		Game.Activity.StartActivity(CreateShareIntent(text, screenshot));
	}

	public string GetVersionString(bool withBuildNumber = true)
	{
		Context applicationContext = Game.Activity.Application.ApplicationContext;
		PackageInfo packageInfo = applicationContext.PackageManager.GetPackageInfo(applicationContext.PackageName, (PackageInfoFlags)0);
		string text = "- - -";
		text = packageInfo.VersionName;
		if (withBuildNumber)
		{
			text = text + " (build " + packageInfo.VersionCode + ")";
		}
		return text;
	}

	private Intent CreateShareIntent(string text, Screenshot screenshot)
	{
		Context applicationContext = Game.Activity.Application.ApplicationContext;
		Java.IO.File file = new Java.IO.File(applicationContext.FilesDir.Path);
		file.Mkdir();
		Java.IO.File file2 = new Java.IO.File(file.Path, "screenshot.png");
		using (FileStream stream = new FileStream(file2.Path, FileMode.OpenOrCreate, FileAccess.Write))
		{
			screenshot.Texture.SaveAsPng(stream, screenshot.Texture.Width, screenshot.Texture.Height);
		}
		Intent intent = new Intent("android.intent.action.SEND");
		intent.SetType("plain/text");
		intent.PutExtra("android.intent.extra.TEXT", text);
		if (file2 != null && file2.Exists())
		{
			Android.Net.Uri uriForFile = FileProvider.GetUriForFile(applicationContext, "com.nitrome.redungeon.provider", file2);
			intent.SetType("image/png");
			intent.PutExtra("android.intent.extra.STREAM", uriForFile);
		}
		intent.AddFlags(ActivityFlags.GrantReadUriPermission);
		return intent;
	}

	public static bool IsRunningOnDevice()
	{
		return true;
	}

	public void MinimizeGame()
	{
		Intent intent = new Intent("android.intent.action.MAIN");
		intent.AddCategory("android.intent.category.HOME");
		intent.SetFlags(ActivityFlags.NewTask);
		Game.Activity.StartActivity(intent);
	}

	public string GetDeviceName()
	{
		return Build.Device;
	}

	public string GetDeviceUniqueId()
	{
		return Build.Serial;
	}
}
