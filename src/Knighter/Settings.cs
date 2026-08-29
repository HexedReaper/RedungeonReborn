using Knighter.Graphics;

namespace Knighter;

public static class Settings
{
	public enum OS
	{
		Unknown,
		iOS,
		Android
	}

	public enum TerminatorDebugMode
	{
		None,
		Disabled,
		StopBehindPlayer,
		ComeAndGo
	}

	public static OS Os = OS.Android;

	public static float PixelScale = 5f;

	public static float GuiScale = 0.85f;

	public const int TileSize = 16;

	public static bool HighlightOccupiedTiles = false;

	public static bool DrawDebugShapes = false;

	public static bool DrawDebugMessages = false;

	public static bool DrawDebugWatches = false;

	public static bool ShowDebugButtons = true;

	public static bool SkipShopAnimations = false;

	public static bool SkipAds = false;

	public static bool ShowModuleGroups = false;

	public static bool UseCustomShaders = SystemCalls.IsRunningOnDevice();

	public static bool HideScreenshotOverlays = false;

	public static TerminatorDebugMode TerminatorMode = TerminatorDebugMode.Disabled;

	public const string NameOfGame = "Redungeon";

	public const string NameOfCompany = "Eneminds";

	public const string NameOfPublisher = "Nitrome";

	public const int OfferToRateAfter = 3;

	public const int OfferToRatePeriod = 3;

	public const int OfferToFeedbackPeriod = 5;

	public const int OfferToLikeOrFollowAfter = 3;

	public const int OfferToLikeOrFollowPeriod = 4;

	public const int FirstRevivePrice = 100;

	public const int SecondRevivePrice = 500;

	public const int LikeReward = 200;

	public const int FollowReward = 200;

	public const int WatchAdReward1 = 50;

	public const int WatchAdReward2 = 100;

	public const int WatchAdReward3 = 150;

	public const int CoinsForOffer1 = 3000;

	public const int CoinsForOffer2 = 12000;

	public const int CoinsForOffer3 = 50000;

	public const string FacebookUrl = "https://m.facebook.com/nitrome";

	public const string NitromeTwitterUrl = "https://mobile.twitter.com/nitrome";

	public const string WebUrl = "https://www.eneminds.com";

	public const string FeedbackEmail = "feedback@eneminds.com";

	public const string EnemindsTwitterUrl = "https://mobile.twitter.com/eneminds";

	public const string EnemindsFacebookUrl = "https://m.facebook.com/eneminds";

	public const string NitromeUrl = "https://play.google.com/store/apps/developer?id=Nitrome";

	public const string BundleId = "com.nitrome.redungeon";

	public const string UnityAdsId = "116215";

	public const string AdMobId = "ca-app-pub-0896659817499072/5912200267";

	public const string ShortDownloadLinks = "Google Play: goo.gl/FUb9zH";

	public const string GameInStoreUrl = "https://play.google.com/store/apps/details?id=com.nitrome.redungeon";

	public const string PlatformPanicInStoreUrl = "https://play.google.com/store/apps/details?id=com.nitrome.platformpanic";

	public const string AnalyticsId = "UA-3919088-26";

	public static bool AnalyticsEnabled = true;

	public static bool AnalyticsTrackScreens = false;

	public static bool AnalyticsTrackEvents = false;

	public static bool AnalyticsTrackExceptions = false;

	public static SpriteName ShareIcon
	{
		get
		{
			if (Os != OS.iOS)
			{
				return SpriteName.icon_share_android;
			}
			return SpriteName.icon_share_ios;
		}
	}
}
