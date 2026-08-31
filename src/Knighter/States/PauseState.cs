using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.States;

public class PauseState : State
{
	private enum Button
	{
		BackToGame,
		Exit,
		Options,
		Share,
        ExitDaily
	}

	private readonly TouchMenu<Button> touchMenu;

	private bool stoppedGame;

	private Sprite screenshotSprite;

	private bool hideScreenshot;

	public PauseState()
	{
		base.TransDuration = 25;
		ShowCoins = false;
		touchMenu = new TouchMenu<Button>(null, OnButtonRelease, "fg", 20);
		int num = base.core.Renderer.ScreenHeight - 40;
		int num2 = (base.core.Renderer.ScreenWidth - 20) / 4;
		touchMenu.SetupButton(Button.BackToGame, new RectangleF(10 + num2, num, num2 * 2, 30f), base.core.SpriteManager.GetSprite(SpriteName.button), base.core.SpriteManager.GetSprite(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, "", _(SpriteName.icon_play), icon: true, iconIsPicture: false, blink: true);
		touchMenu.SetupButton(Button.Exit, new RectangleF(10f, num, num2, 30f), null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Orange, "", _(SpriteName.icon_exit));
		float num3 = (float)base.core.Renderer.ScreenWidth * 0.4f;
		Vector2 screenCenter = base.core.Renderer.ScreenCenter;
		RectangleF rectangle = new RectangleF(screenCenter.X - num3 * 0.5f - 3f, screenCenter.Y - num3 * 0.5f - 3f, num3 + 6f, num3 + 20f);
		touchMenu.SetupButton(Button.Share, rectangle, null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Orange, "", null, icon: true, iconIsPicture: false, blink: false, null, null, -3f, 0f, 1f, "", 0.095f, drawShadow: false, SoundName.paper_touch, SoundName.paper);
		touchMenu.SetupButton(Button.Options, new RectangleF(10 + 3 * num2, num, num2, 30f), null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Orange, "", _(SpriteName.icon_options));
		touchMenu.SetupButton(Button.ExitDaily, new RectangleF(10f, 10f + base.topSafeArea, 130f, 22f), null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Orange, "EXIT DAILY MODE", null, icon: false, iconIsPicture: false);
        touchMenu[Button.ExitDaily].Hidden = !base.core.OptionsData.DailyRunEnabled;
		screenshotSprite = base.core.SpriteManager.MakeFullSpriteFromScreenshot(base.core.GameplayScreenshot);
		int num4 = screenshotSprite.Width / 5;
		int num5 = (screenshotSprite.Height - num4 * 3) / 2;
		screenshotSprite = screenshotSprite.Reduce(num4, num5, num4, num5);
	}

	public override void Load()
	{
		Screen("pause");
		SendMessage(new PlaySoundMessage(SoundName.trans_2));
		base.Load();
	}

	public override void UpdateTransition()
	{
		touchMenu[Button.BackToGame].Rectangle.Shift(0f, (float)Tween.BackEaseOut(TransD(2, 4), 150.0, -150.0, base.TransDuration - 4));
		touchMenu[Button.Exit].Rectangle.Shift(0f, (float)Tween.BackEaseOut(base.Trans, 50.0, -50.0, base.TransDuration));
		touchMenu[Button.Options].Rectangle.Shift(0f, (float)Tween.BackEaseOut(base.Trans, 50.0, -50.0, base.TransDuration));
		base.UpdateTransition();
	}

	public override void Update()
	{
		base.core.AudioManager.MusicVolumeBox.Set("pause", 0.3f, inWorld: false);
		base.core.CurrentPlayState.Camera.ZoomBox.Set("pause", 1f - (float)Tween.BackEaseOut(base.Trans, 0.0, 0.30000001192092896, base.TransDuration), inWorld: false);
		base.Update();
	}

	public override void HandleInput()
	{
		if (Transition == TransType.None)
		{
			touchMenu.HandleInput();
		}
		base.HandleInput();
	}

	public override void Draw()
	{
		float num = 0.6f * (float)base.Trans / (float)base.TransDuration;
		if (Transition == TransType.Out)
		{
			num = ((!stoppedGame) ? 0.6f : (0.6f + (1f - num) * 0.4f));
		}
		float num2 = (float)base.Trans / (float)base.TransDuration;
		num2 *= num2;
		if (touchMenu[Button.Share].IsDown)
		{
			num2 *= 0.9f;
		}
		float num3 = (float)base.core.Renderer.ScreenWidth * 0.4f * num2;
		Vector2 vector = base.core.Renderer.ScreenCenter.Shift(0f, (float)base.core.Renderer.ScreenHeight * 0.05f);
		RectangleF rectangleF = new RectangleF(vector.X - num3 * 0.5f, vector.Y - num3 * 0.5f, num3, num3);
		base.core.Renderer["fg", 10, false].FillScreen(Color.Black * num);
		Color value = (touchMenu[Button.Share].IsDown ? Color.Gray : (Color.LightGray * num2));
		if (screenshotSprite != null && !hideScreenshot)
		{
			float rotation = Component._sin((float)base.ticks * 0.05f) * 0.1f;
			base.core.Renderer["fg", 10, false].DrawSpriteS(base.core.SpriteManager.Pixel, rectangleF.Center, value, new Vector2(rectangleF.Width, rectangleF.Height), rotation, SpriteFlip.None, SpriteOrigin.Center);
			base.core.Renderer["fg", 10, false].DrawSpriteS(screenshotSprite, rectangleF.Center, Color.White * num2, new Vector2(rectangleF.Width - 4f, rectangleF.Height - 4f) / new Vector2(screenshotSprite.Width, screenshotSprite.Height), rotation, SpriteFlip.None, SpriteOrigin.Center);
		}
		base.core.Renderer["fg", 10, false].DrawSpriteS(_(Settings.ShareIcon), rectangleF.Center, null, Vector2.One * num2, 0f, SpriteFlip.None, SpriteOrigin.Center);
		float num4 = (float)Tween.BackEaseOut(base.Trans, -80.0, 80.0, base.TransDuration);
		Vector2 vector2 = new Vector2(base.core.Renderer.ScreenCenter.X, 15f + num4 + (float)base.topSafeArea);
		Sprite sprite = _(SpriteName.gui_chain);
		for (float num5 = vector2.Y - (float)sprite.Height; num5 > (float)(-sprite.Height); num5 -= (float)sprite.Height)
		{
			base.core.Renderer["fg", 15, false].DrawSpriteS(sprite, new Vector2(vector2.X - 32f - (float)(sprite.Width / 2), num5));
			base.core.Renderer["fg", 15, false].DrawSpriteS(sprite, new Vector2(vector2.X + 32f - (float)(sprite.Width / 2), num5));
		}
		Sprite sprite2 = _(SpriteName.pause_block);
		base.core.Renderer["fg", 10, false].DrawSpriteS(sprite2, vector2, null, null, 0f, SpriteFlip.None, SpriteOrigin.TopCenter);
		base.core.Renderer["fg", 10, false].DrawTextS(__(SId.PAUSE_paused), vector2.Shift(0f, 21f), new TextProfile
		{
			Width = base.core.Renderer.ScreenWidth - 20,
			BoxAlignment = Alignment2D.Middle,
			TextAlignment = Alignment2D.Middle,
			Color = default(Color).FromRgb(9212825),
			SecondColor = default(Color).FromRgb(1645605),
			Decoration = TextDecoration.Extrude2,
			Font = Font.Bold,
			Scale = 1.2f
		});
		touchMenu.Draw();
		base.Draw();
	}

	private void OnButtonRelease(Button button)
	{
		switch (button)
		{
		case Button.BackToGame:
			OnBackButtonPressed();
			break;
		case Button.Exit:
			SendMessage(new PlaySoundMessage(SoundName.trans_1));
			TransitionOut(CoreEvent.ResetGame);
			stoppedGame = true;
			break;
		case Button.Options:
			SendMessage(new CoreEventMessage(CoreEvent.ShowOptions));
			break;
		case Button.ExitDaily:
            base.core.OptionsData.DailyRunEnabled = false;
            base.core.SaveOptions();
            DailyRun.End();
            SendMessage(new PlaySoundMessage(SoundName.trans_1));
            TransitionOut(CoreEvent.ResetGame);
            stoppedGame = true;
            break;
		case Button.Share:
		{
			string shareMessage = string.Format(__(SId.SHARE_from_pause), "Redungeon", "Eneminds", "Nitrome", "Google Play: goo.gl/FUb9zH");
			SendMessage(new PushStateMessage(new ScreenshotState(touchMenu[Button.Share].Rectangle.Center.Shift(0f, -16f), (float)base.core.Renderer.ScreenWidth * 0.4f, shareMessage)));
			hideScreenshot = true;
			break;
		}
		}
	}

	public override void OnReturn()
	{
		hideScreenshot = false;
		base.OnReturn();
	}

	public override void OnBackButtonPressed()
	{
		SendMessage(new PlaySoundMessage(SoundName.trans_1));
		TransitionOut(CoreEvent.HidePause);
		base.OnBackButtonPressed();
	}
}
