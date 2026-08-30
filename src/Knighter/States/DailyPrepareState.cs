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
        Start,
        Share,
        Back
    }

    private TouchMenu<Button> touchMenu;

    private RectangleF menuRect;

    private Sprite block;

    private Sprite chain;

    public DailyPrepareState()
    {
        base.TransDuration = 30;
        ShowCoins = false;
        IsOverlay = true;
        menuRect = new RectangleF((float)(base.core.Renderer.ScreenWidth - 148) * 0.5f, (float)(base.core.Renderer.ScreenHeight - 233) * 0.5f, 148f, 233f);
        touchMenu = new TouchMenu<Button>(null, OnButtonRelease, "fg", 10000);
        touchMenu.SetupButton(Button.Start, new RectangleF(menuRect.Left + 8f, menuRect.Top + 176f, menuRect.Width - 16f, 26f), null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Orange, "START RUN", null, icon: false, iconIsPicture: false);
        touchMenu.SetupButton(Button.Share, new RectangleF(menuRect.Left + 8f, menuRect.Top + 204f, menuRect.Width - 16f, 20f), null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Orange, "SHARE LAST RUN", null, icon: false, iconIsPicture: false);        touchMenu.SetupButton(Button.Back, new RectangleF(menuRect.Center.X - 35f, menuRect.Bottom + 10f, 70f, 30f), _(SpriteName.button_back), _(SpriteName.button_back_down));
        block = _(SpriteName.options_block);
        chain = _(SpriteName.gui_chain);
        SendMessage(new PlaySoundMessage(SoundName.trans_2));
    }

    private List<string> ActiveMods()
    {
        List<string> list = new List<string>();
        if (base.core.OptionsData.HardcoreWebs)
        {
            list.Add("hardcore webs");
        }
        if (base.core.OptionsData.DirectionalThrust)
        {
            list.Add("directional thrust");
        }
        if (base.core.OptionsData.BraggAmmo)
        {
            list.Add("bragg ammo");
        }
        if (base.core.OptionsData.VampirePredator)
        {
            list.Add("predator dives");
        }
        if (base.core.OptionsData.UnfriendBats)
        {
            list.Add("unfriend bats");
        }
        if (base.core.OptionsData.FastWings)
        {
            list.Add("fast wings");
        }
        return list;
    }

    public override void Update()
    {
        touchMenu.Update();
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
        touchMenu[Button.Start].Rectangle.Shift(0f, y);
        touchMenu[Button.Share].Rectangle.Shift(0f, y);
        touchMenu[Button.Back].Rectangle.Shift(0f, y);
        base.UpdateTransition();
    }

    public override void Draw()
    {
        float num = 1f - (float)base.Trans / (float)base.TransDuration;
        base.core.Renderer["fg", 9000, false].FillScreen(Color.Black * (1f - num * num * num));
        float num2 = (float)Tween.BackEaseOut(base.Trans, -base.core.Renderer.ScreenHeight, base.core.Renderer.ScreenHeight, base.TransDuration);
        for (int i = chain.Height; menuRect.Top + 21f + num2 - (float)i > (float)(-chain.Height); i += chain.Height)
        {
            base.core.Renderer["fg", 9000, false].DrawSpriteS(chain, new Vector2(menuRect.Left + 20f, menuRect.Top + 21f + num2 - (float)i));
            base.core.Renderer["fg", 9000, false].DrawSpriteS(chain, new Vector2(menuRect.Right - 19f - (float)chain.Width, menuRect.Top + 21f + num2 - (float)i));
        }
        base.core.Renderer["fg", 9000, false].DrawSpriteS(block, menuRect.TopLeft.Shift(0f, num2));
        base.core.Renderer["fg", 9000, false].DrawTextS("DAILY RUN", menuRect.TopLeft.Shift(5f, 57f + num2), new TextProfile
        {
            Width = (int)menuRect.Width - 10,
            Height = 44,
            BoxAlignment = Alignment2D.Left,
            TextAlignment = Alignment2D.Middle,
            Color = default(Color).FromRgb(9462096),
            Decoration = TextDecoration.None,
            Font = Font.Bold,
            Scale = 0.9f
        });
        TextProfile textProfile = new TextProfile
        {
            Width = 110,
            Height = 30,
            BoxAlignment = Alignment2D.Left,
            TextAlignment = Alignment2D.LeftMiddle,
            Decoration = TextDecoration.None,
            Font = Font.Thin,
            Scale = 0.75f
        };
        float y = 84f;
        base.core.Renderer["fg", 9000, false].DrawSpriteS(_(base.core.CurrentCharDesc.Icon), menuRect.TopLeft.Shift(20f, y + 8f + num2), null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
        base.core.Renderer["fg", 9000, false].DrawTextS(__(base.core.CurrentCharDesc.Name), menuRect.TopLeft.Shift(36f, y + num2), textProfile.Alter(TextProfile.OrangeMiddle));
        base.core.Renderer["fg", 9000, false].DrawTextS(DailyRun.TodayKey(), menuRect.TopLeft.Shift(36f, y + 13f + num2), textProfile.Alter(default(Color).FromRgb(9462096)));
        base.core.Renderer["fg", 9000, false].DrawTextS("code: " + DailyRun.SessionSeed(base.core.ProfileData.Character, base.core.OptionsData).ToString("X8"), menuRect.TopLeft.Shift(36f, y + 26f + num2), textProfile.Alter(default(Color).FromRgb(6910328)));
        y += 40f;
        List<string> list = new List<string>();
        Character character = base.core.ProfileData.Character;
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
        base.core.Renderer["fg", 9000, false].DrawTextS("mods:", menuRect.TopLeft.Shift(12f, y + num2), textProfile.Alter(default(Color).FromRgb(9462096)));
        y += 13f;
        if (list.Count == 0)
        {
            base.core.Renderer["fg", 9000, false].DrawTextS("none (vanilla)", menuRect.TopLeft.Shift(24f, y + num2), textProfile.Alter(default(Color).FromRgb(6910328)));
            y += 13f;
        }
        for (int j = 0; j < list.Count; j++)
        {
            base.core.Renderer["fg", 9000, false].DrawTextS("- " + list[j], menuRect.TopLeft.Shift(24f, y + 13 * j + num2), textProfile.Alter(TextProfile.OrangeMiddle));
        }
        touchMenu.Draw();
        base.Draw();
    }

    private void OnButtonRelease(Button button)
    {
        if (button == Button.Start)
        {
            base.core.OptionsData.DailyRunEnabled = true;
            base.core.SaveOptions();
            SendMessage(new CoreEventMessage(CoreEvent.ResetAndStartGame));
        }
        else if (button == Button.Share)
        {
            if (base.core.ProfileData.DailyLastDistance > 0)
            {
                base.core.Sharing.ShareDaily(base.core.ProfileData.DailyLastDistance, base.core.ProfileData.DailyLastCoins, base.core.ProfileData.DailyLastSeed, base.core.ProfileData.DailyLastCharacter, DailyRun.ModsString(base.core.OptionsData, base.core.ProfileData.Character));
            }
            else
            {
                SendMessage(new PlaySoundMessage(SoundName.web_1));
            }
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