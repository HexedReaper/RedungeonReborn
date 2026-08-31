using System;
using System.Collections.Generic;
using Knighter.Entities;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.States;

public class DailyPrepareState : State
{
    private enum Button
    {
        IconTap,
        Start,
        Share,
        Exit,
        Back
    }

    private TouchMenu<Button> touchMenu;

    private RectangleF menuRect;

    private Sprite block;

    private Sprite chain;

    private Animation charAnim;

    public DailyPrepareState()
    {
        base.TransDuration = 30;
        ShowCoins = false;
        IsOverlay = true;
        menuRect = new RectangleF((float)(base.core.Renderer.ScreenWidth - 148) * 0.5f, (float)(base.core.Renderer.ScreenHeight - 233) * 0.5f, 148f, 233f);
        touchMenu = new TouchMenu<Button>(null, OnButtonRelease, "fg", 10000);
        CharDescription desc = CharDescription.Get[DailyRun.DailyCharacter()];
        string[] seq = desc.AnimSequence.Split('|');
        charAnim = new Animation(desc.AnimSpeed);
        charAnim.Add("live", seq[0], seq[1]);
        touchMenu.SetupButton(Button.IconTap, new RectangleF(menuRect.Center.X - 26f, menuRect.Top + 40f, 52f, 34f), null, null);
        touchMenu.SetupButton(Button.Start, new RectangleF(menuRect.Left + 8f, menuRect.Top + 176f, menuRect.Width - 16f, 26f), _(SpriteName.button), _(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, "START RUN", null, icon: false, iconIsPicture: false);
        touchMenu.SetupButton(Button.Share, new RectangleF(menuRect.Left + 8f, menuRect.Top + 204f, menuRect.Width - 16f, 20f), _(SpriteName.button_green), _(SpriteName.button_green_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Green, "SHARE LAST RUN", null, icon: false, iconIsPicture: false);
        touchMenu.SetupButton(Button.Back, new RectangleF(menuRect.Center.X - 35f, menuRect.Bottom + 10f, 70f, 30f), _(SpriteName.button_back), _(SpriteName.button_back_down));
        touchMenu.SetupButton(Button.Exit, new RectangleF(menuRect.Left + 8f, menuRect.Bottom + 44f, menuRect.Width - 16f, 24f), null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Orange, "EXIT DAILY MODE", null, icon: false, iconIsPicture: false);
        touchMenu[Button.Exit].Hidden = !base.core.OptionsData.DailyRunEnabled;
        block = _(SpriteName.options_block);
        chain = _(SpriteName.gui_chain);
        SendMessage(new PlaySoundMessage(SoundName.trans_2));
    }

    public override void Update()
    {
        touchMenu.Update();
        if (base.core.OptionsData.DailyIconAnimated)
        {
            charAnim.Update();
        }
        IsOpaque = Transition == TransType.None;
        base.core.AudioManager.MusicVolumeBox.Set("daily-prepare", 0.3f, inWorld: false);
        base.Update();
    }

    public override void HandleInput()
    {
        if (Transition == TransType.None)
        {
            touchMenu.HandleInput();
            base.HandleInput();
        }
    }

    public override void UpdateTransition()
    {
        float y = (float)Tween.BackEaseOut(base.Trans, -base.core.Renderer.ScreenHeight, base.core.Renderer.ScreenHeight, base.TransDuration);
        touchMenu[Button.IconTap].Rectangle.Shift(0f, y);
        touchMenu[Button.Start].Rectangle.Shift(0f, y);
        touchMenu[Button.Share].Rectangle.Shift(0f, y);
        touchMenu[Button.Back].Rectangle.Shift(0f, y);
        touchMenu[Button.Exit].Rectangle.Shift(0f, y);
        base.UpdateTransition();
    }

    private TextProfile CenteredProfile(float scale)
    {
        return new TextProfile
        {
            Width = 148,
            Height = 12,
            BoxAlignment = Alignment2D.Middle,
            TextAlignment = Alignment2D.Middle,
            Decoration = TextDecoration.None,
            Font = Font.Thin,
            Scale = scale
        };
    }

    public override void Draw()
    {
        float num = 1f - (float)base.Trans / (float)base.TransDuration;
        base.core.Renderer["fg", 9000, false].FillScreen(Color.Black * (1f - num * num * num));
        float num2 = (float)Tween.BackEaseOut(base.Trans, -base.core.Renderer.ScreenHeight, base.core.Renderer.ScreenHeight, base.TransDuration);
        float stack = (float)Tween.BackEaseOut(base.Trans, -40.0, 40.0, base.TransDuration - 6);
        for (int i = chain.Height; menuRect.Top + 21f + num2 - (float)i > (float)(-chain.Height); i += chain.Height)
        {
            base.core.Renderer["fg", 9000, false].DrawSpriteS(chain, new Vector2(menuRect.Left + 20f, menuRect.Top + 21f + num2 - (float)i));
            base.core.Renderer["fg", 9000, false].DrawSpriteS(chain, new Vector2(menuRect.Right - 19f - (float)chain.Width, menuRect.Top + 21f + num2 - (float)i));
        }
        base.core.Renderer["fg", 9000, false].DrawSpriteS(block, menuRect.TopLeft.Shift(0f, num2));
        base.core.Renderer["fg", 9000, false].DrawTextS("DAILY RUN", menuRect.CenterTop.Shift(0f, 30f + num2), new TextProfile
        {
            Width = 148,
            Height = 20,
            BoxAlignment = Alignment2D.Middle,
            TextAlignment = Alignment2D.Middle,
            Color = default(Color).FromRgb(9462096),
            Decoration = TextDecoration.None,
            Font = Font.Bold,
            Scale = 0.9f
        });
        CharDescription desc = CharDescription.Get[DailyRun.DailyCharacter()];
        Sprite iconSprite = (base.core.OptionsData.DailyIconAnimated ? charAnim.GetCurrentFrame() : _(desc.Icon));
        base.core.Renderer["fg", 9000, false].DrawSpriteS(iconSprite, new Vector2(menuRect.Center.X, menuRect.Top + 62f + num2 + stack), null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
        base.core.Renderer["fg", 9000, false].DrawTextS(__(desc.Name), menuRect.CenterTop.Shift(0f, 78f + num2 + stack), CenteredProfile(0.75f).Alter(TextProfile.OrangeMiddle));
        base.core.Renderer["fg", 9000, false].DrawTextS(DailyRun.TodayKey(), menuRect.CenterTop.Shift(0f, 92f + num2 + stack), CenteredProfile(0.75f).Alter(default(Color).FromRgb(9462096)));
        base.core.Renderer["fg", 9000, false].DrawTextS("code: " + DailyRun.SessionSeed(base.core.OptionsData).ToString("X8"), menuRect.CenterTop.Shift(0f, 106f + num2 + stack), CenteredProfile(0.75f).Alter(default(Color).FromRgb(6910328)));
        float y = 134f + num2 + stack;
        Character character = DailyRun.DailyCharacter();
        List<string> list = new List<string>();
        if (base.core.OptionsData.HardcoreWebs)
        {
            list.Add("hardcore webs");
        }
        if (character == Character.Knight && base.core.OptionsData.DirectionalThrust)
        {
            list.Add("directional thrust");
        }
        if (character == Character.Bragg && base.core.OptionsData.BraggAmmo)
        {
            list.Add("bragg ammo");
        }
        if (character == Character.Vampire && base.core.OptionsData.VampirePredator)
        {
            list.Add("predator dives");
        }
        if (character == Character.Vampire && base.core.OptionsData.UnfriendBats)
        {
            list.Add("unfriend bats");
        }
        if (character == Character.Vampire && base.core.OptionsData.FastWings)
        {
            list.Add("fast wings");
        }
        base.core.Renderer["fg", 9000, false].DrawTextS("MODS", menuRect.CenterTop.Shift(0f, y), CenteredProfile(0.6f).Alter(default(Color).FromRgb(9462096)));
        y += 14f;
        if (list.Count == 0)
        {
            base.core.Renderer["fg", 9000, false].DrawTextS("none (vanilla)", menuRect.CenterTop.Shift(0f, y), CenteredProfile(0.7f).Alter(default(Color).FromRgb(6910328)));
        }
        for (int j = 0; j < list.Count; j++)
        {
            base.core.Renderer["fg", 9000, false].DrawTextS("- " + list[j], menuRect.CenterTop.Shift(0f, y + 13 * j), CenteredProfile(0.7f).Alter(TextProfile.OrangeMiddle));
        }
        touchMenu.Draw();
        base.Draw();
    }

    private void OnButtonRelease(Button button)
    {
        if (button == Button.IconTap)
        {
            base.core.OptionsData.DailyIconAnimated = !base.core.OptionsData.DailyIconAnimated;
            base.core.SaveOptions();
            SendMessage(new PlaySoundMessage(SoundName.swoosh_2));
        }
        else if (button == Button.Start)
        {
            base.core.OptionsData.DailyRunEnabled = true;
            base.core.SaveOptions();
            SendMessage(new CoreEventMessage(CoreEvent.ResetAndStartGame));
        }
        else if (button == Button.Share)
        {
            if (base.core.ProfileData.DailyLastDistance > 0)
            {
                base.core.Sharing.ShareDaily(base.core.ProfileData.DailyLastDistance, base.core.ProfileData.DailyLastCoins, base.core.ProfileData.DailyLastSeed, base.core.ProfileData.DailyLastCharacter, DailyRun.ModsString(base.core.OptionsData, base.core.ProfileData.Character), base.core.ProfileData.DailyLastResultCode);
            }
            else
            {
                SendMessage(new PlaySoundMessage(SoundName.web_1));
            }
        }
        else if (button == Button.Exit)
        {
            base.core.OptionsData.DailyRunEnabled = false;
            base.core.SaveOptions();
            DailyRun.End();
            SendMessage(new PlaySoundMessage(SoundName.piston_retract));
        }
        else if (button == Button.Back)
        {
            SendMessage(new PlaySoundMessage(SoundName.trans_1), 7);
            TransitionOut(CoreEvent.PopState);
            base.OnBackButtonPressed();
        }
    }

    public override void OnBackButtonPressed()
    {
        TransitionOut(CoreEvent.PopState);
        base.OnBackButtonPressed();
    }
}