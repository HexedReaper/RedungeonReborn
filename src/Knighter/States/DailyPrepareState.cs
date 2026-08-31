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
        CodeTap,
        ModsTap,
        Start,
        Share,
        Back
    }

    private TouchMenu<Button> touchMenu;

    private RectangleF menuRect;

    private Sprite block;

    private Sprite chain;

    private Animation charAnim;

    private Animation torchFire;

    private bool showResult;

    private bool showMods;

    private int swingT = -1;

    public DailyPrepareState()
    {
        base.TransDuration = 30;
        ShowCoins = false;
        IsOverlay = true;
        menuRect = new RectangleF((float)(base.core.Renderer.ScreenWidth - 148) * 0.5f, (float)(base.core.Renderer.ScreenHeight - 233) * 0.35f, 148f, 233f);
        touchMenu = new TouchMenu<Button>(null, OnButtonRelease, "fg", 10000);
        CharDescription desc = CharDescription.Get[DailyRun.DailyCharacter()];
        string[] seq = desc.AnimSequence.Split('|');
        charAnim = new Animation(desc.AnimSpeed);
        charAnim.Add("live", seq[0], seq[1]);
        charAnim.Play("live");
        torchFire = new Animation(0.1f);
        torchFire.Add("burn", "torch_fire_", "123456");
        torchFire.Play("burn");
        touchMenu.SetupButton(Button.IconTap, new RectangleF(menuRect.Center.X - 26f, menuRect.Top + 92f, 52f, 28f), null, null);
        touchMenu.SetupButton(Button.CodeTap, new RectangleF(menuRect.Center.X - 50f, menuRect.Top + 137f, 100f, 15f), null, null);
        touchMenu.SetupButton(Button.ModsTap, new RectangleF(menuRect.Center.X - 50f, menuRect.Top + 162f, 100f, 15f), null, null);
        touchMenu.SetupButton(Button.Start, new RectangleF(menuRect.Center.X - 52f, menuRect.Bottom + 2f, 104f, 26f), _(SpriteName.button), _(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, "START RUN", null, icon: false, iconIsPicture: false);
        touchMenu.SetupButton(Button.Share, new RectangleF(menuRect.Center.X - 46f, menuRect.Bottom + 44f, 92f, 18f), _(SpriteName.button_green), _(SpriteName.button_green_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Green, "SHARE LAST", null, icon: false, iconIsPicture: false, blink: false, default(Color).FromRgb(11216961), null, -3f, 0f, 0.75f);
        touchMenu.SetupButton(Button.Back, new RectangleF(menuRect.Center.X - 35f, menuRect.Bottom + 76f, 70f, 30f), _(SpriteName.button_back), _(SpriteName.button_back_down));
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
        torchFire.Update();
        if (swingT >= 0)
        {
            swingT++;
            if (swingT == 50)
            {
                SendMessage(new CoreEventMessage(CoreEvent.ResetAndStartGame));
            }
        }
        IsOpaque = Transition == TransType.None;
        base.core.AudioManager.MusicVolumeBox.Set("daily-prepare", 0.3f, inWorld: false);
        base.Update();
    }

    public override void HandleInput()
    {
        if (Transition == TransType.None && swingT < 0)
        {
            touchMenu.HandleInput();
            base.HandleInput();
        }
    }

    public override void UpdateTransition()
    {
        float y = (float)Tween.BackEaseOut(base.Trans, -base.core.Renderer.ScreenHeight, base.core.Renderer.ScreenHeight, base.TransDuration);
        touchMenu[Button.IconTap].Rectangle.Shift(0f, y);
        touchMenu[Button.CodeTap].Rectangle.Shift(0f, y);
        touchMenu[Button.ModsTap].Rectangle.Shift(0f, y);
        touchMenu[Button.Start].Rectangle.Shift(0f, y);
        touchMenu[Button.Share].Rectangle.Shift(0f, y);
        touchMenu[Button.Back].Rectangle.Shift(0f, y);
        base.UpdateTransition();
    }

    private TextProfile CenteredProfile(float scale, bool bold = false)
    {
        return new TextProfile
        {
            Width = 148,
            Height = 13,
            BoxAlignment = Alignment2D.Middle,
            TextAlignment = Alignment2D.Middle,
            Decoration = TextDecoration.None,
            Font = bold ? Font.Bold : Font.Thin,
            Scale = scale
        };
    }

    private void DrawTorch(Vector2 hangPos, float dayFrac)
    {
        base.core.Renderer["fg", 9001, false].DrawSpriteS(_(SpriteName.dungeon_torch), hangPos, null, null, 0f, SpriteFlip.None, SpriteOrigin.TopCenter);
        base.core.Renderer["fg", 9002, false].DrawSpriteS(torchFire.GetCurrentFrame(), hangPos.Shift(0f, 8f), null, null, 0f, SpriteFlip.None, SpriteOrigin.TopCenter);
    }

    public override void Draw()
    {
        float num = 1f - (float)base.Trans / (float)base.TransDuration;
        base.core.Renderer["fg", 9000, false].FillScreen(Color.Black * (1f - num * num * num));
        float num2 = (float)Tween.BackEaseOut(base.Trans, -base.core.Renderer.ScreenHeight, base.core.Renderer.ScreenHeight, base.TransDuration);
        float stack = (float)Tween.BackEaseOut(base.Trans, -40.0, 40.0, base.TransDuration - 6);
        float swing = ((swingT >= 0) ? (1.2f * (float)(swingT * swingT) / 2500f) : 0f);
        float swingSin = (float)Math.Sin(swing);
        float swingCos = (float)Math.Cos(swing);
        DateTime utcNow = DateTime.UtcNow;
        double secondsLeft = 86400.0 - (utcNow.Hour * 3600 + utcNow.Minute * 60 + utcNow.Second);
        float dayFrac = (float)(secondsLeft / 86400.0);
        // float flicker = 0.9f + 0.1f * Component._sin((float)base.ticks * 0.3f);
        for (int i = chain.Height; menuRect.Top + 21f + num2 - (float)i > (float)(-chain.Height); i += chain.Height)
        {
            base.core.Renderer["fg", 9000, false].DrawSpriteS(chain, new Vector2(menuRect.Left + 20f, menuRect.Top + 21f + num2 - (float)i));
            base.core.Renderer["fg", 9000, false].DrawSpriteS(chain, new Vector2(menuRect.Right - 19f - (float)chain.Width, menuRect.Top + 21f + num2 - (float)i));
        }
        Vector2 panelTop = new Vector2(menuRect.Center.X - (menuRect.Top + num2) * swingSin, (menuRect.Top + num2) * swingCos);
        base.core.Renderer["fg", 9000, false].DrawSpriteS(block, panelTop, null, null, swing, SpriteFlip.None, SpriteOrigin.TopCenter);
        base.core.Renderer["fg", 9000, false].DrawTextS("DAILY RUN", menuRect.CenterTop.Shift(0f, 60f + num2), CenteredProfile(1f, bold: true).Alter(default(Color).FromRgb(9462096)));
        DrawTorch(new Vector2(menuRect.Left + 16f, menuRect.Top + 14f + num2), dayFrac);
        DrawTorch(new Vector2(menuRect.Right - 16f, menuRect.Top + 14f + num2), dayFrac);
        CharDescription desc = CharDescription.Get[DailyRun.DailyCharacter()];
        Sprite iconSprite = (base.core.OptionsData.DailyIconAnimated ? charAnim.GetCurrentFrame() : _(desc.Icon));
        base.core.Renderer["fg", 9000, false].DrawSpriteS(iconSprite, new Vector2(menuRect.Center.X, menuRect.Top + 96f + num2 + stack), null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
        base.core.Renderer["fg", 9000, false].DrawTextS(__(desc.Name), menuRect.CenterTop.Shift(0f, 116f + num2 + stack), CenteredProfile(0.8f).Alter(TextProfile.OrangeMiddle));
        base.core.Renderer["fg", 9000, false].DrawTextS(DailyRun.TodayKey(), menuRect.CenterTop.Shift(0f, 131f + num2 + stack), CenteredProfile(0.8f).Alter(default(Color).FromRgb(9462096)));
        bool haveResult = base.core.ProfileData.DailyLastDistance > 0;
        int sealCode = ((showResult && haveResult) ? base.core.ProfileData.DailyLastResultCode : DailyRun.SessionSeed(base.core.OptionsData));
        string sealLabel = ((showResult && haveResult) ? "result: " : "code: ") + sealCode.ToString("X8");
        base.core.Renderer["fg", 9000, false].DrawTextS(sealLabel, menuRect.CenterTop.Shift(0f, 146f + num2 + stack), CenteredProfile(0.8f).Alter((showResult && haveResult) ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328)));
        float y = 170f + num2 + stack;
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
        string modsLabel = "mods " + (showMods ? "-" : "+") + ((list.Count == 0) ? " (none)" : (" (" + list.Count + ")"));
        base.core.Renderer["fg", 9000, false].DrawTextS(modsLabel, menuRect.CenterTop.Shift(0f, y), CenteredProfile(0.7f).Alter(default(Color).FromRgb(9462096)));
        y += 15f;
        if (showMods)
        {
            if (list.Count == 0)
            {
                base.core.Renderer["fg", 9000, false].DrawTextS("none (vanilla)", menuRect.CenterTop.Shift(0f, y), CenteredProfile(0.75f).Alter(default(Color).FromRgb(6910328)));
            }
            for (int j = 0; j < list.Count; j++)
            {
                base.core.Renderer["fg", 9000, false].DrawTextS("- " + list[j], menuRect.CenterTop.Shift(0f, y + 14 * j), CenteredProfile(0.75f).Alter(TextProfile.OrangeMiddle));
            }
            y += 14f * list.Count;
        }
        bool heartLit = (base.core.ProfileData.DailyBestDate == DailyRun.TodayKey() && base.core.ProfileData.DailyBestDistance >= 20);
        float heartPulse = (heartLit ? (1f + 0.12f * Component._sin((float)base.ticks * 0.1f)) : 1f);
        float counterY = y + 8f;
        base.core.Renderer["fg", 9000, false].DrawTextS("dailies played: " + base.core.ProfileData.DailyTotalPlayed, menuRect.CenterTop.Shift(0f, counterY), CenteredProfile(0.75f).Alter(heartLit ? TextProfile.OrangeMiddle : default(Color).FromRgb(9462096)));
        string todayText = (heartLit ? "played today!" : "not played yet");
        float todayW = base.core.Renderer["fg", 9000, false].DrawTextS(todayText, menuRect.CenterTop.Shift(0f, counterY + 15f), CenteredProfile(0.75f).Alter(heartLit ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328))).Width;
        base.core.Renderer["fg", 9000, false].DrawSpriteS(_(SpriteName.bat_heart), new Vector2(menuRect.Center.X - todayW * 0.5f - 10f, counterY + 20f), (heartLit ? Color.White : default(Color).FromRgb(6910328)) * (heartLit ? 1f : 0.7f), Vector2.One * heartPulse, 0f, SpriteFlip.None, SpriteOrigin.Center);
        if (base.core.ProfileData.DailyBestDistance > 0 && base.core.ProfileData.DailyBestDate != DailyRun.TodayKey())
        {
            float bob = Component._sin((float)base.ticks * 0.05f) * 2f;
            Vector2 ghostPos = new Vector2(menuRect.Left - 22f, menuRect.Top + 120f + num2 + bob);
            base.core.Renderer["fg", 9000, false].DrawSpriteS(_(CharDescription.Get[(Character)base.core.ProfileData.DailyBestCharacter].SkullSprite), ghostPos, Color.White * 0.45f, Vector2.One * 0.7f, 0f, SpriteFlip.None, SpriteOrigin.Center);
            base.core.Renderer["fg", 9000, false].DrawTextS("yesterday", new Vector2(ghostPos.X, ghostPos.Y + 16f), CenteredProfile(0.5f).Alter(default(Color).FromRgb(6910328)));
            base.core.Renderer["fg", 9000, false].DrawTextS(base.core.ProfileData.DailyBestDistance + "m", new Vector2(ghostPos.X, ghostPos.Y + 26f), CenteredProfile(0.5f).Alter(TextProfile.OrangeMiddle));
        }
        touchMenu.Draw();
        if (swingT >= 0)
        {
            base.core.Renderer["fg", 10500, false].FillScreen(Color.Black * Component._M(1f, (float)swingT / 30f));
        }
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
        else if (button == Button.CodeTap)
        {
            showResult = !showResult;
            SendMessage(new PlaySoundMessage(SoundName.paper_touch));
        }
        else if (button == Button.ModsTap)
        {
            showMods = !showMods;
            SendMessage(new PlaySoundMessage(SoundName.piston_retract));
        }
        else if (button == Button.Start)
        {
            base.core.OptionsData.DailyRunEnabled = true;
            base.core.SaveOptions();
            swingT = 0;
            SendMessage(new PlaySoundMessage(SoundName.piston_extend));
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