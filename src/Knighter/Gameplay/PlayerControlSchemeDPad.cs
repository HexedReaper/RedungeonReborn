using System;
using System.Collections.Generic;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Knighter.Gameplay;

public class PlayerControlSchemeDPad : PlayerControlScheme
{
	private enum Button
	{
		North,
		East,
		South,
		West,
		Action
	}

	private readonly TouchMenu<Button> touchMenu;

	private readonly Dictionary<Direction, Vector2> directionVectors;

	private Direction repeatDirection;

	private int holdingTicks = -1;

	private Button pressedButton;

	private bool firstRepeat = true;

	private int swipeTouchId;

	private Vector2 swipeTouchStart;

	private bool swipeInProgress;

	private const int SWIPE_DEAD_ZONE = 64;

	private const int DOUBLE_TAP_MAX_DELAY = 30;

	private const int DOUBLE_TAP_MAX_SPREAD = 30;

	private int ticksSinceLastTap;

	private bool waitingForSecondTap;

	private Vector2 firstTapPosition;

	private int secontTapId;

	public bool Compact { get; private set; }

	public PlayerControlSchemeDPad(PlayState playState, bool compact = false)
		: base(playState)
	{
		Compact = compact;
		touchMenu = new TouchMenu<Button>(OnButtonPress, OnButtonRelease);
		RectangleF rectangle;
		RectangleF rectangle2;
		RectangleF rectangle3;
		RectangleF rectangleF;
		RectangleF rectangle4;
		if (!compact)
		{
			int num = base.core.Renderer.ScreenHeight / 3;
			int num2 = base.core.Renderer.ScreenWidth / 3;
			int num3 = base.core.Renderer.ScreenWidth - num2 * 2;
			int num4 = base.core.Renderer.ScreenHeight - num;
			rectangle = new RectangleF(0f, num4 + num / 4, num2, num / 2);
			rectangle2 = new RectangleF(base.core.Renderer.ScreenWidth - num2, num4 + num / 4, num2, num / 2);
			rectangle3 = new RectangleF(num2, num4 + num / 2, num3, num / 2);
			rectangleF = new RectangleF(num2, num4, num3, num / 2);
			rectangle4 = new RectangleF(base.core.OptionsData.LeftHandedMode ? (5f / Settings.GuiScale) : ((float)base.core.Renderer.ScreenWidth - 45f / Settings.GuiScale), (float)num4 - 30f / Settings.GuiScale, 40f / Settings.GuiScale, 40f / Settings.GuiScale);
		}
		else
		{
			int num5 = 110;
			rectangleF = new RectangleF(base.core.OptionsData.LeftHandedMode ? (base.core.Renderer.ScreenWidth - num5) : 0, base.core.Renderer.ScreenHeight - num5, num5, num5);
			rectangle = rectangleF;
			rectangle3 = rectangleF;
			rectangle2 = rectangleF;
			rectangle4 = new RectangleF(base.core.OptionsData.LeftHandedMode ? (5f / Settings.GuiScale) : ((float)base.core.Renderer.ScreenWidth - 45f / Settings.GuiScale), (float)(base.core.Renderer.ScreenHeight - num5 / 2) - 22f / Settings.GuiScale, 40f / Settings.GuiScale, 40f / Settings.GuiScale);
		}
		touchMenu.SetupButton(Button.West, rectangle, compact ? null : (base.core.OptionsData.SeeThroughMode ? _(SpriteName.dpad_left_o) : _(SpriteName.dpad_left)), compact ? null : (base.core.OptionsData.SeeThroughMode ? _(SpriteName.dpad_left_pressed_o) : _(SpriteName.dpad_left_pressed)), null, stretch: false, SpriteFlip.None, ButtonColor.Orange, "", null, icon: true, iconIsPicture: false, blink: false, null, null, -3f, 0f, 1f, "", 0.095f, drawShadow: false, SoundName.none, SoundName.none, seeThrough: false, 1f / Settings.GuiScale, compact ? RectSector.West : RectSector.Whole);
		touchMenu.SetupButton(Button.East, rectangle2, compact ? null : (base.core.OptionsData.SeeThroughMode ? _(SpriteName.dpad_right_o) : _(SpriteName.dpad_right)), compact ? null : (base.core.OptionsData.SeeThroughMode ? _(SpriteName.dpad_right_pressed_o) : _(SpriteName.dpad_right_pressed)), null, stretch: false, SpriteFlip.None, ButtonColor.Orange, "", null, icon: true, iconIsPicture: false, blink: false, null, null, -3f, 0f, 1f, "", 0.095f, drawShadow: false, SoundName.none, SoundName.none, seeThrough: false, 1f / Settings.GuiScale, compact ? RectSector.East : RectSector.Whole);
		touchMenu.SetupButton(Button.North, rectangleF, compact ? null : (base.core.OptionsData.SeeThroughMode ? _(SpriteName.dpad_up_o) : _(SpriteName.dpad_up)), compact ? null : (base.core.OptionsData.SeeThroughMode ? _(SpriteName.dpad_up_pressed_o) : _(SpriteName.dpad_up_pressed)), null, stretch: false, SpriteFlip.None, ButtonColor.Orange, "", null, icon: true, iconIsPicture: false, blink: false, null, null, -3f, 0f, 1f, "", 0.095f, drawShadow: false, SoundName.none, SoundName.none, seeThrough: false, 1f / Settings.GuiScale, compact ? RectSector.North : RectSector.Whole);
		touchMenu.SetupButton(Button.South, rectangle3, compact ? null : (base.core.OptionsData.SeeThroughMode ? _(SpriteName.dpad_down_o) : _(SpriteName.dpad_down)), compact ? null : (base.core.OptionsData.SeeThroughMode ? _(SpriteName.dpad_down_pressed_o) : _(SpriteName.dpad_down_pressed)), null, stretch: false, SpriteFlip.None, ButtonColor.Orange, "", null, icon: true, iconIsPicture: false, blink: false, null, null, -3f, 0f, 1f, "", 0.095f, drawShadow: false, SoundName.none, SoundName.none, seeThrough: false, 1f / Settings.GuiScale, compact ? RectSector.South : RectSector.Whole);
		touchMenu.SetupButton(Button.Action, rectangle4, null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Orange, "", null, icon: true, iconIsPicture: false, blink: false, null, null, -3f, 0f, 1f, "", 0.095f, drawShadow: false, SoundName.none, SoundName.none);
		directionVectors = new Dictionary<Direction, Vector2>();
		directionVectors[Direction.North] = new Vector2(0f, -1f);
		directionVectors[Direction.East] = new Vector2(1f, 0f);
		directionVectors[Direction.South] = new Vector2(0f, 1f);
		directionVectors[Direction.West] = new Vector2(-1f, 0f);
	}

	public override Vector2 SkillButtonCenter()
	{
		if (touchMenu != null)
		{
			return touchMenu[Button.Action].Rectangle.Center;
		}
		return Vector2.Zero;
	}

	public override void Load()
	{
		InitSkillButton();
		base.Load();
	}

	public override void UpdateTransition()
	{
		touchMenu[Button.West].Rectangle.Shift((float)Tween.BackEaseOut(playState.Trans, -50.0, 50.0, playState.TransDuration));
		touchMenu[Button.East].Rectangle.Shift((float)Tween.BackEaseOut(playState.Trans, 50.0, -50.0, playState.TransDuration));
		touchMenu[Button.North].Rectangle.Shift(0f, (float)Tween.BackEaseOut(playState.Trans, 150.0, -150.0, playState.TransDuration));
		touchMenu[Button.South].Rectangle.Shift(0f, (float)Tween.BackEaseOut(playState.Trans, 150.0, -150.0, playState.TransDuration));
		base.UpdateTransition();
	}

	public override void HandleInput()
	{
		touchMenu.HandleInput();
		foreach (TouchLocation item in base.core.TouchState)
		{
			bool flag = false;
			bool flag2 = !touchMenu[Button.East].Rectangle.Contains(item.Position) && !touchMenu[Button.West].Rectangle.Contains(item.Position) && !touchMenu[Button.North].Rectangle.Contains(item.Position) && !touchMenu[Button.South].Rectangle.Contains(item.Position);
			if ((item.State == TouchLocationState.Pressed && !swipeInProgress) & flag2)
			{
				swipeTouchId = item.Id;
				swipeTouchStart = item.Position;
				swipeInProgress = true;
			}
			if (swipeInProgress && item.Id == swipeTouchId)
			{
				Vector2 vector = item.Position - swipeTouchStart;
				TouchLocationState state = item.State;
				if (state == TouchLocationState.Released)
				{
					if (Math.Abs(vector.X) > 64f)
					{
						playState.Player.OnSwipe();
						flag = true;
					}
					swipeInProgress = false;
				}
			}
			if (!waitingForSecondTap)
			{
				if (((item.Id != secontTapId && item.State == TouchLocationState.Released && !flag) & flag2) && item.Position.Y < touchMenu[Button.North].Rectangle.Top)
				{
					waitingForSecondTap = true;
					ticksSinceLastTap = 0;
					firstTapPosition = item.Position;
				}
			}
			else if (item.State == TouchLocationState.Pressed && (item.Position - firstTapPosition).Length() < 30f)
			{
				secontTapId = item.Id;
				waitingForSecondTap = false;
				playState.Player.OnDoubleTap();
			}
		}
	}

	public override void Update()
	{
		if (base.core.OptionsData.HoldToRun)
		{
			if (!base.core.CurrentPlayState.IsTopState)
			{
				holdingTicks = -1;
			}
			if (holdingTicks >= 0 && touchMenu[pressedButton].IsDown)
			{
				holdingTicks++;
				if (holdingTicks >= 11 + (firstRepeat ? 7 : 0))
				{
					firstRepeat = false;
					playState.Jump(directionVectors[repeatDirection]);
					holdingTicks = 0;
				}
			}
		}
		if (waitingForSecondTap)
		{
			ticksSinceLastTap++;
			if (ticksSinceLastTap > 30)
			{
				waitingForSecondTap = false;
			}
		}
		base.Update();
	}

	public override void Draw()
	{
		if (!base.core.TakingScreenshot)
		{
			touchMenu.Draw();
			if (Compact)
			{
				base.core.Renderer["fg", 0, false].DrawSpriteS(_((touchMenu[Button.North].Pressed ? "dpad_n" : (touchMenu[Button.West].Pressed ? "dpad_w" : (touchMenu[Button.East].Pressed ? "dpad_e" : (touchMenu[Button.South].Pressed ? "dpad_s" : "dpad")))) + (base.core.OptionsData.SeeThroughMode ? "_o" : "")), touchMenu[Button.South].Rectangle.Center.Shift(0f, 3f), base.core.OptionsData.SeeThroughMode ? (TextProfile.OrangeMiddle * 0.6f) : Color.White, Vector2.One * (1f / Settings.GuiScale), 0f, SpriteFlip.None, SpriteOrigin.Center);
			}
			DrawSkillButton(touchMenu[Button.Action].Rectangle.Center, touchMenu[Button.Action].IsDown);
		}
	}

	public override void Reset()
	{
		touchMenu.ReleaseButtons();
	}

	private void OnButtonPress(Button button)
	{
		switch (button)
		{
		case Button.North:
		case Button.East:
		case Button.South:
		case Button.West:
			playState.Jump(directionVectors[(Direction)button]);
			repeatDirection = (Direction)button;
			holdingTicks = 0;
			pressedButton = button;
			break;
		case Button.Action:
			TapSkillButton();
			break;
		}
	}

	private void OnButtonRelease(Button button)
	{
		holdingTicks = -1;
		firstRepeat = true;
	}
}
