using Android.App;
using Android.OS;
using Android.Views;

namespace KnighterAndroid;

[Activity(Theme = "@style/Theme.Splash", MainLauncher = true, NoHistory = true)]
public class SplashActivity : Activity
{
	protected override void OnCreate(Bundle bundle)
	{
		base.OnCreate(bundle);
		HideSystemUi();
		Window.DecorView.SystemUiVisibilityChange += delegate
		{
			HideSystemUi();
		};
		StartActivity(typeof(MainActivity));
	}

	public override void OnWindowFocusChanged(bool hasFocus)
	{
		base.OnWindowFocusChanged(hasFocus);
		HideSystemUi();
	}

	protected override void OnResume()
	{
		base.OnResume();
		HideSystemUi();
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
