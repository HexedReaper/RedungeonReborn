using System;
using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.States;

public class GameHud : Component
{
	private enum Button
	{
		Pause
	}

	public enum AlertKind
	{
		Stripe,
		Text
	}

	private class Alert
	{
		public string Id;

		public string Text;

		public int TTL;

		public int Age;

		public Color Color;

		public Sprite Icon;

		public AlertKind Kind;
	}

	private readonly PlayState playState;

	private readonly TouchMenu<Button> touchMenu;

	public AbilitiesHud AbilitiesHud;

	private float displayCoins;

	private Queue<Alert> alerts;

	private Sprite swoosh;

	public GameHud(PlayState playState)
	{
		this.playState = playState;
		touchMenu = new TouchMenu<Button>(null, OnButtonRelease);
		touchMenu.SetupButton(Button.Pause, new RectangleF(-5f, -5 + base.topSafeArea, 30f, 30f), _(SpriteName.button_pause), _(SpriteName.button_pause_pressed));
		alerts = new Queue<Alert>();
		swoosh = _(SpriteName.alert_swoosh);
	}

	public override void Load()
	{
		AbilitiesHud = new AbilitiesHud(playState.Player.Abilities.SkillLevel, shopMode: false, 18 + base.topSafeArea);
		base.Load();
	}

	public override void Update()
	{
		int collectedCoins = playState.Session.CollectedCoins;
		if (Math.Abs((float)collectedCoins - displayCoins) > 1f)
		{
			displayCoins += ((float)collectedCoins - displayCoins) * 0.1f;
		}
		else
		{
			displayCoins = collectedCoins;
		}
		AbilitiesHud.Update();
		if (alerts.Count > 0)
		{
			Alert alert = alerts.Peek();
			alert.Age++;
			if (alert.Age > alert.TTL)
			{
				alerts.Dequeue();
			}
		}
		base.Update();
	}

	public void ShowAlert(string id, string message, Color color, int ttl = 120, SpriteName? icon = null, AlertKind kind = AlertKind.Stripe)
	{
		Alert alert = new Alert
		{
			Id = id,
			Text = message,
			Color = color,
			TTL = ttl,
			Age = 0,
			Icon = ((!icon.HasValue) ? null : _(icon.Value)),
			Kind = kind
		};
		bool flag = false;
		foreach (Alert alert2 in alerts)
		{
			if (alert2.Id == id)
			{
				alert2.Text = alert.Text;
				alert2.Color = alert.Color;
				alert2.TTL = ttl + alert2.Age;
				alert2.Icon = alert.Icon;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			alerts.Enqueue(alert);
		}
	}

	public bool HandleInput()
	{
		if (playState.Transition != State.TransType.None)
		{
			return false;
		}
		return touchMenu.HandleInput();
	}

	private void OnButtonRelease(Button button)
	{
		TryToPause();
	}

	public void TryToPause()
	{
		if (!base.core.CurrentPlayState.Player.Dead)
		{
			SendMessage(new CoreEventMessage(CoreEvent.MakeScreenshotWhileDrawing));
			base.core.CurrentPlayState.Pause(enteringBackground: false);
			playState.TransitionOut(CoreEvent.ShowPause);
		}
	}

	public override void Draw()
	{
		if (base.core.TakingScreenshot)
		{
			return;
		}
		if (playState == base.core.GetCurrentState())
		{
			float num = 0f;
			if (playState.Transition != State.TransType.None)
			{
				num = (float)Tween.BackEaseOut(playState.Trans, -50.0, 50.0, playState.TransDuration);
			}
			touchMenu[Button.Pause].Rectangle.Shift(num);
			base.core.Renderer["fg", -1, false].DrawRectangleS(new RectangleF(-1f, num - 10f, base.core.Renderer.ScreenWidth + 2, AbilitiesHud.panelMiddle.Height - 3 + 10 + base.topSafeArea), Color.Black * 0.5f);
			base.core.Renderer["fg", -2, false].DrawRectangleS(new RectangleF(-1f, num - 10f, base.core.Renderer.ScreenWidth + 2, AbilitiesHud.panelMiddle.Height - 3 + 18 + base.topSafeArea), Color.Black * 0.25f);
			base.core.Renderer["fg", -1, false].DrawSpriteS(AbilitiesHud.panelBottom, new Vector2(0f, num + (float)base.topSafeArea + 21f), null, new Vector2(base.core.Renderer.ScreenWidth, 1f));
		}
		if (base.core.GetCurrentState() != playState || !playState.Started)
		{
			return;
		}
		touchMenu.Draw();
		bool newBest = base.core.CurrentPlayState.NewBest;
		string text = playState.Session.Distance + __(SId.MISC_meters);
		string text2 = "";
		if (base.core.ProfileData.BestDistance != 0)
		{
			text2 = (newBest ? __(SId.HUD_new_best) : string.Format(__(SId.HUD_best), base.core.ProfileData.BestDistance + __(SId.MISC_meters)));
		}
		float num2 = (float)Tween.BackEaseOut(playState.Trans, -15.0, 15.0, playState.TransDuration) + (float)base.topSafeArea;
		base.core.Renderer["fg"].DrawTextS(text, new Vector2(base.core.Renderer.ScreenWidth / 2, 8.5f + num2), new TextProfile
		{
			Font = Font.Bold,
			Color = Color.White,
			SecondColor = Color.Black,
			BoxAlignment = Alignment2D.Center,
			TextAlignment = Alignment2D.Middle,
			Decoration = TextDecoration.Contour,
			Width = base.core.Renderer.ScreenWidth,
			Scale = 1f
		});
		base.core.Renderer["fg"].DrawTextS(text2, new Vector2(base.core.Renderer.ScreenWidth / 4, 9f + num2), new TextProfile
		{
			Font = Font.Bold,
			Color = TextProfile.OrangeLight,
			SecondColor = Color.Black,
			BoxAlignment = Alignment2D.Center,
			TextAlignment = Alignment2D.Middle,
			Decoration = TextDecoration.Contour,
			Width = base.core.Renderer.ScreenWidth / 6,
			Scale = (newBest ? (0.55f + (float)Math.Sin((float)base.ticks / 20f) * 0.05f) : 0.5f)
		});
		string text3 = "+^" + (int)Math.Ceiling(displayCoins);
		base.core.Renderer["fg", 10000, false].DrawTextS(text3, new Vector2(base.core.Renderer.ScreenWidth - 3, 4f + num2), TextProfile.OrangeBoldText.Alter(null, boxAlignment: Alignment2D.Right, textAlignment: Alignment2D.Right, width: base.core.Renderer.ScreenWidth, decoration: TextDecoration.Contour, secondColor: Color.Black));
		if (base.core.GetCurrentState() == playState)
		{
			AbilitiesHud.Draw();
			if (alerts.Count > 0)
			{
				Alert alert = alerts.Peek();
				int num3 = ((alert.Kind == AlertKind.Text) ? 25 : 20);
				float num4 = 1f;
				if (alert.Age < num3)
				{
					num4 = (float)Tween.BackEaseOut(alert.Age, 0.0, 1.0, num3);
				}
				if (alert.Age > alert.TTL - num3)
				{
					num4 = 0f - (float)Tween.BackEaseOut(alert.TTL - alert.Age, 0.0, 1.0, num3);
				}
				float num5 = Math.Abs(num4);
				switch (alert.Kind)
				{
				case AlertKind.Stripe:
				{
					base.core.Renderer["fg", 1001, false].DrawRectangleS(new RectangleF(-1f, 54f - 8f * num5 + (float)base.topSafeArea, base.core.Renderer.ScreenWidth + 2, 16f * num5), alert.Color * num5 * 0.7f);
					for (int i = 1; i <= 3; i++)
					{
						base.core.Renderer["fg", 1001, false].DrawSpriteS(swoosh, new Vector2(base.core.Renderer.ScreenWidth + 50 - base.ticks * i * 6 % (base.core.Renderer.ScreenWidth + 100), 54 + base.topSafeArea), alert.Color, new Vector2(2f, num5), 0f, (i % 2 != 0) ? SpriteFlip.Vertical : SpriteFlip.None, SpriteOrigin.Center);
					}
					float x = (float)(base.core.Renderer.ScreenWidth / 2) + (float)base.core.Renderer.ScreenWidth * ((num4 >= 0f) ? (1f - num4) : (num5 - 1f));
					float num6 = ((alert.Icon != null) ? (alert.Icon.Width + 4) : 0);
					float width = base.core.Renderer["fg", 1001, false].DrawTextS(alert.Text, new Vector2(num6 / 2f + x, 52 + base.topSafeArea), new TextProfile
					{
						Font = Font.Bold,
						Color = Color.White,
						BoxAlignment = Alignment2D.Middle,
						TextAlignment = Alignment2D.Middle,
						Decoration = TextDecoration.None,
						Width = base.core.Renderer.ScreenWidth - (int)num6,
						Height = 20,
						Scale = 1f + Component._sin((float)base.ticks * 0.07f) * 0.01f
					}).Width;
					if (alert.Icon != null)
					{
						base.core.Renderer["fg", 1001, false].DrawSpriteS(alert.Icon, new Vector2(x - width / 2f, 54 + base.topSafeArea), Color.White * num5, Vector2.One * num5, 0f, SpriteFlip.None, SpriteOrigin.Center);
					}
					break;
				}
				case AlertKind.Text:
				{
					float x = base.core.Renderer.ScreenWidth / 2;
					base.core.Renderer["fg", 1001, false].DrawTextS(alert.Text, new Vector2(x, 50f * num5 + (float)base.topSafeArea), new TextProfile
					{
						Font = Font.Bold,
						Color = alert.Color,
						SecondColor = Color.Black,
						BoxAlignment = Alignment2D.Middle,
						TextAlignment = Alignment2D.Middle,
						Decoration = TextDecoration.Contour,
						Width = (int)((float)base.core.Renderer.ScreenWidth * 1.5f),
						Height = 20,
						Scale = (2f + Component._sin((float)alert.Age * 0.07f) * 0.2f) * num5
					});
					break;
				}
				}
			}
		}
		base.Draw();
	}
}
