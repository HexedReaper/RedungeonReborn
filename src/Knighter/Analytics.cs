using Android.Gms.Analytics;
using Microsoft.Xna.Framework;

namespace Knighter;

public class Analytics : Component, IAnalytics
{
	private static GoogleAnalytics instance;

	private static Tracker tracker;

	private static readonly int DispatchPeriodInSeconds = -1;

	public void Initialize()
	{
		if (Settings.AnalyticsEnabled)
		{
			instance = GoogleAnalytics.GetInstance(Game.Activity.ApplicationContext);
			instance.SetLocalDispatchPeriod(DispatchPeriodInSeconds);
			tracker = instance.NewTracker("UA-3919088-26");
			tracker.EnableExceptionReporting(enable: true);
			tracker.EnableAdvertisingIdCollection(enabled: true);
			tracker.EnableAutoActivityTracking(enabled: true);
			tracker.EnableExceptionReporting(enable: true);
		}
	}

	public void TrackScreen(string screenName)
	{
		if (Settings.AnalyticsEnabled && Settings.AnalyticsTrackScreens)
		{
			tracker.SetScreenName(screenName);
			tracker.Send(new HitBuilders.ScreenViewBuilder().Build());
		}
	}

	public void TrackEvent(AnalyticsCategory category, string action, string label, int value)
	{
		if (Settings.AnalyticsEnabled && Settings.AnalyticsTrackEvents && category != AnalyticsCategory.Run)
		{
			HitBuilders.EventBuilder eventBuilder = new HitBuilders.EventBuilder();
			eventBuilder.SetCategory(category.ToString());
			eventBuilder.SetAction(action);
			eventBuilder.SetLabel(label);
			eventBuilder.SetValue(value);
			tracker.Send(eventBuilder.Build());
		}
	}

	public void TrackEvent(AnalyticsCategory category, string action, string label)
	{
		if (Settings.AnalyticsEnabled && Settings.AnalyticsTrackEvents && category != AnalyticsCategory.Run)
		{
			HitBuilders.EventBuilder eventBuilder = new HitBuilders.EventBuilder();
			eventBuilder.SetCategory(category.ToString());
			eventBuilder.SetAction(action);
			eventBuilder.SetLabel(label);
			tracker.Send(eventBuilder.Build());
		}
	}

	public void TrackException(string message, bool isFatal)
	{
		if (Settings.AnalyticsEnabled && Settings.AnalyticsTrackExceptions)
		{
			HitBuilders.ExceptionBuilder exceptionBuilder = new HitBuilders.ExceptionBuilder();
			exceptionBuilder.SetDescription(message);
			exceptionBuilder.SetFatal(isFatal);
			tracker.Send(exceptionBuilder.Build());
		}
	}

	public void Dispatch()
	{
		if (Settings.AnalyticsEnabled)
		{
			instance.DispatchLocalHits();
		}
	}
}
