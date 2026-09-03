using System;
using System.IO;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.OS;
using Android.Support.V4.Content;
using Java.IO;
using Java.Interop;
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
        string[] shareLines = text.Split('\n');
        string shareTitle = string.Join(" — ", shareLines, 0, Math.Min(3, shareLines.Length));
        intent.PutExtra("android.intent.extra.TITLE", shareTitle);
        if (file2 != null && file2.Exists())
        {
            Android.Net.Uri uriForFile = FileProvider.GetUriForFile(applicationContext, "com.nitrome.redungeon.provider", file2);
            intent.SetType("image/png");
            intent.PutExtra("android.intent.extra.STREAM", uriForFile);
        }
		CopyToClipboard(text);
        intent.AddFlags(ActivityFlags.GrantReadUriPermission);
        return intent;
	}

	private void CopyToClipboard(string text)
    {
        try
        {
            Console.WriteLine("[SHARE] clipboard: start");
            Java.Lang.Object clipboard = Game.Activity.GetSystemService("clipboard");
            Console.WriteLine("[SHARE] svc handle=" + ((clipboard != null) ? clipboard.Handle : IntPtr.Zero));
            if (clipboard == null || clipboard.Handle == IntPtr.Zero)
            {
                Console.WriteLine("[SHARE] clipboard service null - abort");
                return;
            }
            IntPtr clipClass = Android.Runtime.JNIEnv.FindClass("android/content/ClipData");
            Console.WriteLine("[SHARE] clipClass=" + clipClass);
            IntPtr newPlainText = Android.Runtime.JNIEnv.GetStaticMethodID(clipClass, "newPlainText", "(Ljava/lang/CharSequence;Ljava/lang/CharSequence;)Landroid/content/ClipData;");
            IntPtr clip = Android.Runtime.JNIEnv.CallStaticObjectMethod(clipClass, newPlainText, new Android.Runtime.JValue(Android.Runtime.JNIEnv.NewString("Redungeon Daily")), new Android.Runtime.JValue(Android.Runtime.JNIEnv.NewString(text)));
            Console.WriteLine("[SHARE] clip=" + clip);
            IntPtr cbPtr = clipboard.Handle;
            IntPtr cbClass = Android.Runtime.JNIEnv.GetObjectClass(cbPtr);
            IntPtr setPrimary = Android.Runtime.JNIEnv.GetMethodID(cbClass, "setPrimaryClip", "(Landroid/content/ClipData;)V");
            Android.Runtime.JNIEnv.CallVoidMethod(cbPtr, setPrimary, new Android.Runtime.JValue(clip));
            Console.WriteLine("[SHARE] clipboard OK");
            ShowToast("Daily stats copied - paste into post");
        }
        catch (Exception e)
        {
            Console.WriteLine("[SHARE] clipboard FAILED: " + e);
        }
    }

    private void ShowToast(string message)
    {
        try
        {
            IntPtr toastClass = Android.Runtime.JNIEnv.FindClass("android/widget/Toast");
            IntPtr makeText = Android.Runtime.JNIEnv.GetStaticMethodID(toastClass, "makeText", "(Landroid/content/Context;Ljava/lang/CharSequence;I)Landroid/widget/Toast;");
            IntPtr toast = Android.Runtime.JNIEnv.CallStaticObjectMethod(toastClass, makeText, new Android.Runtime.JValue(Game.Activity.Handle), new Android.Runtime.JValue(Android.Runtime.JNIEnv.NewString(message)), new Android.Runtime.JValue(1));
            IntPtr tClass = Android.Runtime.JNIEnv.GetObjectClass(toast);
            IntPtr show = Android.Runtime.JNIEnv.GetMethodID(tClass, "show", "()V");
            Android.Runtime.JNIEnv.CallVoidMethod(toast, show);
        }
        catch (Exception e)
        {
            Console.WriteLine("[SHARE] toast FAILED: " + e);
        }
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
