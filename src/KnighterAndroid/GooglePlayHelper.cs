using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Drive;
using Android.Gms.Games;
using Android.Gms.Games.Achievement;
using Android.Gms.Games.LeaderBoard;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Java.Lang;

namespace KnighterAndroid;

public class GooglePlayHelper : Java.Lang.Object, GoogleApiClient.IConnectionCallbacks, IJavaObject, IDisposable, GoogleApiClient.IOnConnectionFailedListener
{
	private GoogleApiClient client;

	private Activity activity;

	private bool signedOut = true;

	private bool signingin;

	private bool resolving;

	private List<IAchievement> achievments = new List<IAchievement>();

	private Dictionary<string, List<ILeaderboardScore>> scores = new Dictionary<string, List<ILeaderboardScore>>();

	private const int REQUEST_LEADERBOARD = 9002;

	private const int REQUEST_ALL_LEADERBOARDS = 9003;

	private const int REQUEST_ACHIEVEMENTS = 9004;

	private const int RC_RESOLVE = 9001;

	public bool IsConnected
	{
		get
		{
			if (client != null)
			{
				return client.IsConnected;
			}
			return false;
		}
	}

	public bool SignedOut
	{
		get
		{
			return signedOut;
		}
		set
		{
			if (signedOut == value)
			{
				return;
			}
			signedOut = value;
			using ISharedPreferences sharedPreferences = activity.GetSharedPreferences("googleplayservicessettings", FileCreationMode.Private);
			using ISharedPreferencesEditor sharedPreferencesEditor = sharedPreferences.Edit();
			sharedPreferencesEditor.PutBoolean("SignedOut", signedOut);
			sharedPreferencesEditor.Commit();
		}
	}

	public GravityFlags GravityForPopups { get; set; }

	public View ViewForPopups { get; set; }

	public List<IAchievement> Achievements => achievments;

	public event EventHandler OnSignedIn;

	public event EventHandler OnSignInFailed;

	public event EventHandler OnSignedOut;

	public GoogleApiClient GetGoogleApiClient()
	{
		return client;
	}

	public GooglePlayHelper(Activity activity)
	{
		this.activity = activity;
		GravityForPopups = GravityFlags.Bottom | GravityFlags.AxisSpecified;
	}

	public void Initialize()
	{
		ISharedPreferences sharedPreferences = activity.GetSharedPreferences("googleplayservicessettings", FileCreationMode.Private);
		signedOut = sharedPreferences.GetBoolean("SignedOut", defValue: true);
		if (!signedOut)
		{
			CreateClient();
		}
	}

	private void CreateClient()
	{
		GoogleApiClient.Builder builder = new GoogleApiClient.Builder(activity, this, this);
		builder.AddApi(GamesClass.API);
		builder.AddScope(GamesClass.ScopeGames);
		builder.AddApi(DriveClass.API);
		builder.AddScope(DriveClass.ScopeAppfolder);
		builder.SetGravityForPopups((int)GravityForPopups);
		if (ViewForPopups != null)
		{
			builder.SetViewForPopups(ViewForPopups);
		}
		client = builder.Build();
	}

	public void Start()
	{
		if ((!SignedOut || signingin) && client != null && !client.IsConnected)
		{
			client.Connect();
		}
	}

	public void Stop()
	{
		if (client != null && client.IsConnected)
		{
			client.Disconnect();
		}
	}

	public void Reconnect()
	{
		if (client != null)
		{
			client.Reconnect();
		}
	}

	public void SignOut()
	{
		SignedOut = true;
		if (client.IsConnected)
		{
			GamesClass.SignOut(client);
			Stop();
			client.Dispose();
			client = null;
			if (OnSignedOut != null)
			{
				OnSignedOut(this, EventArgs.Empty);
			}
		}
	}

	public void SignIn()
	{
		signingin = true;
		if (client == null)
		{
			CreateClient();
		}
		if (!client.IsConnected && !client.IsConnecting && GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(activity) == 0)
		{
			Start();
		}
	}

	public void UnlockAchievement(string achievementCode)
	{
		GamesClass.Achievements.Unlock(client, achievementCode);
	}

	public void IncrementAchievement(string achievementCode, int progress)
	{
		GamesClass.Achievements.Increment(client, achievementCode, progress);
	}

	public void SetStepsAchievment(string achievementCode, int numSteps)
	{
		GamesClass.Achievements.SetSteps(client, achievementCode, numSteps);
	}

	public void ShowAchievements()
	{
		Intent achievementsIntent = GamesClass.Achievements.GetAchievementsIntent(client);
		activity.StartActivityForResult(achievementsIntent, 9004);
	}

	public void SubmitScore(string leaderboardCode, long value)
	{
		GamesClass.Leaderboards.SubmitScore(client, leaderboardCode, value);
	}

	public void SubmitScore(string leaderboardCode, long value, string metadata)
	{
		GamesClass.Leaderboards.SubmitScore(client, leaderboardCode, value, metadata);
	}

	public void ShowLeaderBoardIntentForLeaderboard(string leaderboardCode)
	{
		Intent leaderboardIntent = GamesClass.Leaderboards.GetLeaderboardIntent(client, leaderboardCode);
		activity.StartActivityForResult(leaderboardIntent, 9002);
	}

	public void ShowAllLeaderBoardsIntent()
	{
		Intent allLeaderboardsIntent = GamesClass.Leaderboards.GetAllLeaderboardsIntent(client);
		activity.StartActivityForResult(allLeaderboardsIntent, 9003);
	}

	public async Task LoadAchievements()
	{
		IAchievementsLoadAchievementsResult achievementsLoadAchievementsResult = await GamesClass.Achievements.LoadAsync(client, forceReload: false);
		if (achievementsLoadAchievementsResult != null)
		{
			achievments.Clear();
			achievments.AddRange(achievementsLoadAchievementsResult.Achievements);
		}
	}

	public async Task LoadTopScores(string leaderboardCode)
	{
		ILeaderboardsLoadScoresResult leaderboardsLoadScoresResult = await GamesClass.Leaderboards.LoadTopScoresAsync(client, leaderboardCode, 2, 0, 25);
		if (leaderboardsLoadScoresResult != null)
		{
			string leaderboardId = leaderboardsLoadScoresResult.Leaderboard.LeaderboardId;
			if (!scores.ContainsKey(leaderboardId))
			{
				scores.Add(leaderboardId, new List<ILeaderboardScore>());
			}
			scores[leaderboardId].Clear();
			scores[leaderboardId].AddRange(leaderboardsLoadScoresResult.Scores);
		}
	}

	public void OnConnected(Bundle connectionHint)
	{
		resolving = false;
		SignedOut = false;
		signingin = false;
		if (OnSignedIn != null)
		{
			OnSignedIn(this, EventArgs.Empty);
		}
	}

	public void OnConnectionSuspended(int resultCode)
	{
		resolving = false;
		SignedOut = false;
		signingin = false;
		client.Disconnect();
		if (OnSignInFailed != null)
		{
			OnSignInFailed(this, EventArgs.Empty);
		}
	}

	public void OnConnectionFailed(ConnectionResult result)
	{
		if (resolving)
		{
			return;
		}
		if (result.HasResolution)
		{
			resolving = true;
			result.StartResolutionForResult(activity, 9001);
			return;
		}
		resolving = false;
		SignedOut = false;
		signingin = false;
		if (OnSignInFailed != null)
		{
			OnSignInFailed(this, EventArgs.Empty);
		}
	}

	public void OnActivityResult(int requestCode, Result resultCode, Intent data)
	{
		if (requestCode != 9001)
		{
			return;
		}
		if (resultCode == Result.Ok)
		{
			Start();
			return;
		}
		resolving = false;
		SignedOut = true;
		signingin = false;
		if (OnSignInFailed != null)
		{
			OnSignInFailed(this, EventArgs.Empty);
		}
	}
}
