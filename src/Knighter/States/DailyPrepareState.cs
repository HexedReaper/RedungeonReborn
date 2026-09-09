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
        Start,
        Share,
        Back
    }

    // ---- layout tuned on device via UiLayoutEditor; paste DUMP over these, then set EditorEnabled = false ----
    private const bool EditorEnabled = true;
    private const float PanelDY = 0f;
    private const float PanelScale = 1.1f;   // whole-assembly scale; 1.19 max fits narrowest screens
    private const float CountdownX = 0f;
    private const float CountdownY = 54f;
    private const float TitleX = 0f;
    private const float TitleY = 68f;
    private const float IconX = 2f;
    private const float IconY = 85f;
    private const float NameX = 0f;
    private const float NameY = 102f;
    private const float CodeX = -1f;
    private const float CodeY = 114f;
    private const float ModsLabelX = -1f;
    private const float ModsLabelY = 128f;
    private const float ModsX = -1f;
    private const float ModsY = 139f;
    private const float ModsPitch = 13f;
    private const float CounterX = -40f;
    private const float CounterY = 199f;
    private const float TodayX = -36f;
    private const float TodayY = 212f;
    private const float HeartX = -1f;      // rel panel center
    private const float HeartY = 214f;
    private const float StatsY = 225f;
    private const float GhostX = 119f;     // rel panel left
    private const float GhostY = 189f;
    private const float StartBtnX = 2f;
    private const float StartBtnY = 5f;    // below panel bottom
    private const float StartBtnW = 104f;
    private const float StartBtnH = 26f;
    private const float ShareBtnX = 4f;
    private const float ShareBtnY = 44f;
    private const float ShareBtnW = 92f;
    private const float ShareBtnH = 18f;
    private const float BackBtnX = 4f;
    private const float BackBtnY = 63f;
    private const float BackBtnW = 70f;
    private const float BackBtnH = 30f;

    private TouchMenu<Button> touchMenu;

    private RectangleF menuRect;

    private Sprite block;

    private Sprite chain;

    private Animation charAnim;

    private bool showResult;

    private int swingT = -1;

    private readonly Character dailyChar;

    private readonly CharDescription desc;

    private readonly int sessionSeed;

    private readonly List<string> mods;

    // live layout (editor writes here; S() scales it)
    private float panelDY = PanelDY;
    private float panelScale = PanelScale;
    private float countdownX = CountdownX;
    private float countdownY = CountdownY;
    private float titleX = TitleX;
    private float titleY = TitleY;
    private float iconX = IconX;
    private float iconY = IconY;
    private float nameX = NameX;
    private float nameY = NameY;
    private float codeX = CodeX;
    private float codeY = CodeY;
    private float modsLabelX = ModsLabelX;
    private float modsLabelY = ModsLabelY;
    private float modsX = ModsX;
    private float modsY = ModsY;
    private float modsPitch = ModsPitch;
    private float counterX = CounterX;
    private float counterY = CounterY;
    private float todayX = TodayX;
    private float todayY = TodayY;
    private float heartX = HeartX;
    private float heartY = HeartY;
    private float statsY = StatsY;
    private float ghostX = GhostX;
    private float ghostY = GhostY;
    private float startBtnX = StartBtnX;
    private float startBtnY = StartBtnY;
    private float startBtnW = StartBtnW;
    private float startBtnH = StartBtnH;
    private float shareBtnX = ShareBtnX;
    private float shareBtnY = ShareBtnY;
    private float shareBtnW = ShareBtnW;
    private float shareBtnH = ShareBtnH;
    private float backBtnX = BackBtnX;
    private float backBtnY = BackBtnY;
    private float backBtnW = BackBtnW;
    private float backBtnH = BackBtnH;

    private UiLayoutEditor edLayout;

    private UiLayoutEditor.Item edPanelScale;

    private UiLayoutEditor.Item edPanelDY;

    private UiLayoutEditor.Item edCountdown;

    private UiLayoutEditor.Item edTitle;

    private UiLayoutEditor.Item edIcon;

    private UiLayoutEditor.Item edName;

    private UiLayoutEditor.Item edCode;

    private UiLayoutEditor.Item edModsLabel;

    private UiLayoutEditor.Item edMods;

    private UiLayoutEditor.Item edModsPitch;

    private UiLayoutEditor.Item edCounter;

    private UiLayoutEditor.Item edToday;

    private UiLayoutEditor.Item edHeart;

    private UiLayoutEditor.Item edStats;

    private UiLayoutEditor.Item edGhost;

    private UiLayoutEditor.Item edStart;

    private UiLayoutEditor.Item edShare;

    private UiLayoutEditor.Item edBack;

    public DailyPrepareState()
    {
        base.TransDuration = 30;
        ShowCoins = false;
        IsOverlay = true;
        dailyChar = DailyRun.DailyCharacter();
        desc = CharDescription.Get[dailyChar];
        sessionSeed = DailyRun.SessionSeed(base.core.OptionsData);
        mods = DailyRun.CollectMods(base.core.OptionsData, dailyChar);
        string[] seq = desc.AnimSequence.Split('|');
        charAnim = new Animation(desc.AnimSpeed);
        charAnim.Add("live", seq[0], seq[1]);
        charAnim.Play("live");
        touchMenu = new TouchMenu<Button>(null, OnButtonRelease, "fg", 10000);
        touchMenu.SetupButton(Button.IconTap, new RectangleF(0f, 0f, 60f, 40f), null, null);
        touchMenu.SetupButton(Button.CodeTap, new RectangleF(0f, 0f, 120f, 18f), null, null);
        touchMenu.SetupButton(Button.Start, new RectangleF(0f, 0f, 104f, 26f), _(SpriteName.button), _(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, "START RUN", null, icon: false, iconIsPicture: false);
        touchMenu.SetupButton(Button.Share, new RectangleF(0f, 0f, 92f, 18f), _(SpriteName.button_green), _(SpriteName.button_green_pressed), _(SpriteName.button_green), stretch: true, SpriteFlip.None, ButtonColor.Green, "SHARE LAST", null, icon: false, iconIsPicture: false, blink: false, default(Color).FromRgb(11216961), null, -3f, 0f, 0.75f * PanelScale);
        touchMenu[Button.Share].Disabled = base.core.ProfileData.DailyLastDistance <= 0;
        touchMenu.SetupButton(Button.Back, new RectangleF(0f, 0f, 70f, 30f), _(SpriteName.button_back), _(SpriteName.button_back_down));
        RepositionAll();
        block = _(SpriteName.options_block);
        chain = _(SpriteName.gui_chain);
        if (EditorEnabled)
        {
            WireEditor();
        }
        SendMessage(new PlaySoundMessage(SoundName.trans_2));
    }

    private float S(float v)
    {
        return v * panelScale;
    }

    private void RepositionAll()
    {
        menuRect = new RectangleF((float)(base.core.Renderer.ScreenWidth - S(148f)) * 0.5f, (float)(base.core.Renderer.ScreenHeight - S(233f)) * 0.35f + panelDY, S(148f), S(233f));
        float cx = menuRect.Center.X;
        touchMenu[Button.IconTap].Rectangle = new RectangleF(cx + S(iconX) - S(30f), menuRect.Top + S(iconY) - S(20f), S(60f), S(40f));
        touchMenu[Button.CodeTap].Rectangle = new RectangleF(cx + S(codeX) - S(60f), menuRect.Top + S(codeY) - S(9f), S(120f), S(18f));
        touchMenu[Button.Start].Rectangle = new RectangleF(cx + S(startBtnX) - S(startBtnW) * 0.5f, menuRect.Bottom + S(startBtnY), S(startBtnW), S(startBtnH));
        touchMenu[Button.Share].Rectangle = new RectangleF(cx + S(shareBtnX) - S(shareBtnW) * 0.5f, menuRect.Bottom + S(shareBtnY), S(shareBtnW), S(shareBtnH));
        touchMenu[Button.Back].Rectangle = new RectangleF(cx + S(backBtnX) - S(backBtnW) * 0.5f, menuRect.Bottom + S(backBtnY), S(backBtnW), S(backBtnH));
    }

    private void WireEditor()
    {
        edLayout = new UiLayoutEditor("DailyPrepareState");
        edPanelScale = edLayout.Add("PanelScale", 0f, panelScale, () => new Vector2(menuRect.Center.X, menuRect.Top - 12f * panelScale), null, false, true, true);
        edPanelDY = edLayout.Add("PanelDY", 0f, panelDY, () => new Vector2(menuRect.Center.X, menuRect.Top + 6f), null, false, true);
        edCountdown = edLayout.Add("Countdown", countdownX, countdownY, () => new Vector2(menuRect.Center.X + S(countdownX), menuRect.Top + S(countdownY)));
        edTitle = edLayout.Add("Title", titleX, titleY, () => new Vector2(menuRect.Center.X + S(titleX), menuRect.Top + S(titleY)));
        edIcon = edLayout.Add("Icon", iconX, iconY, () => new Vector2(menuRect.Center.X + S(iconX), menuRect.Top + S(iconY)));
        edName = edLayout.Add("Name", nameX, nameY, () => new Vector2(menuRect.Center.X + S(nameX), menuRect.Top + S(nameY)));
        edCode = edLayout.Add("Code", codeX, codeY, () => new Vector2(menuRect.Center.X + S(codeX), menuRect.Top + S(codeY)));
        edModsLabel = edLayout.Add("ModsLabel", modsLabelX, modsLabelY, () => new Vector2(menuRect.Center.X + S(modsLabelX), menuRect.Top + S(modsLabelY)));
        edMods = edLayout.Add("Mods", modsX, modsY, () => new Vector2(menuRect.Center.X + S(modsX), menuRect.Top + S(modsY)));
        edModsPitch = edLayout.Add("ModsPitch", 0f, modsPitch, () => new Vector2(menuRect.Center.X, menuRect.Top + S(modsY + modsPitch)), null, false, true);
        edCounter = edLayout.Add("Counter", counterX, counterY, () => new Vector2(menuRect.Center.X + S(counterX), menuRect.Top + S(counterY)));
        edToday = edLayout.Add("Today", todayX, todayY, () => new Vector2(menuRect.Center.X + S(todayX), menuRect.Top + S(todayY)));
        edHeart = edLayout.Add("Heart", heartX, heartY, () => new Vector2(menuRect.Center.X + S(heartX), menuRect.Top + S(heartY)));
        edStats = edLayout.Add("Stats", 0f, statsY, () => new Vector2(menuRect.Left + S(10f), menuRect.Top + S(statsY)), null, false, true);
        edGhost = edLayout.Add("Ghost", ghostX, ghostY, () => new Vector2(menuRect.Left + S(ghostX), menuRect.Top + S(ghostY)));
        edStart = edLayout.Add("StartBtn", startBtnX, startBtnY, () => touchMenu[Button.Start].Rectangle.Center, null, false, false, false, true, startBtnW, startBtnH);
        edShare = edLayout.Add("ShareBtn", shareBtnX, shareBtnY, () => touchMenu[Button.Share].Rectangle.Center, null, false, false, false, true, shareBtnW, shareBtnH);
        edBack = edLayout.Add("BackBtn", backBtnX, backBtnY, () => touchMenu[Button.Back].Rectangle.Center, null, false, false, false, true, backBtnW, backBtnH);
        edLayout.DragScale = 1f / panelScale;
    }

    private void ApplyEditor()
    {
        panelDY = edPanelDY.Y;
        panelScale = edPanelScale.Y;
        edLayout.DragScale = 1f / panelScale;
        countdownX = edCountdown.X;
        countdownY = edCountdown.Y;
        titleX = edTitle.X;
        titleY = edTitle.Y;
        iconX = edIcon.X;
        iconY = edIcon.Y;
        nameX = edName.X;
        nameY = edName.Y;
        codeX = edCode.X;
        codeY = edCode.Y;
        modsLabelX = edModsLabel.X;
        modsLabelY = edModsLabel.Y;
        modsX = edMods.X;
        modsY = edMods.Y;
        modsPitch = edModsPitch.Y;
        counterX = edCounter.X;
        counterY = edCounter.Y;
        todayX = edToday.X;
        todayY = edToday.Y;
        heartX = edHeart.X;
        heartY = edHeart.Y;
        statsY = edStats.Y;
        ghostX = edGhost.X;
        ghostY = edGhost.Y;
        startBtnX = edStart.X;
        startBtnY = edStart.Y;
        startBtnW = edStart.W;
        startBtnH = edStart.H;
        shareBtnX = edShare.X;
        shareBtnY = edShare.Y;
        shareBtnW = edShare.W;
        shareBtnH = edShare.H;
        backBtnX = edBack.X;
        backBtnY = edBack.Y;
        backBtnW = edBack.W;
        backBtnH = edBack.H;
        RepositionAll();
    }

    public override void Update()
    {
        touchMenu.Update();
        if (base.core.OptionsData.DailyIconAnimated)
        {
            charAnim.Update();
        }
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
            if (EditorEnabled && edLayout.HandleInput())
            {
                ApplyEditor();
                return;
            }
            touchMenu.HandleInput();
            base.HandleInput();
        }
    }

    public override void UpdateTransition()
    {
        float y = (float)Tween.BackEaseOut(base.Trans, -base.core.Renderer.ScreenHeight, base.core.Renderer.ScreenHeight, base.TransDuration);
        touchMenu[Button.IconTap].Rectangle.Shift(0f, y);
        touchMenu[Button.CodeTap].Rectangle.Shift(0f, y);
        touchMenu[Button.Start].Rectangle.Shift(0f, y);
        touchMenu[Button.Share].Rectangle.Shift(0f, y);
        touchMenu[Button.Back].Rectangle.Shift(0f, y);
        base.UpdateTransition();
    }

    private TextProfile CenteredProfile(float scale, bool bold = false)
    {
        return new TextProfile
        {
            Width = (int)S(148f),
            Height = 13,
            BoxAlignment = Alignment2D.Middle,
            TextAlignment = Alignment2D.Middle,
            Decoration = TextDecoration.None,
            Font = bold ? Font.Bold : Font.Thin,
            Scale = scale * panelScale
        };
    }

    private TextProfile LeftProfile(float scale)
    {
        return new TextProfile
        {
            Width = (int)S(64f),
            Height = 13,
            BoxAlignment = Alignment2D.Left,
            TextAlignment = Alignment2D.Left,
            Decoration = TextDecoration.None,
            Font = Font.Thin,
            Scale = scale * panelScale
        };
    }

    private TextProfile RightProfile(float scale)
    {
        return new TextProfile
        {
            Width = (int)S(64f),
            Height = 13,
            BoxAlignment = Alignment2D.Right,
            TextAlignment = Alignment2D.Right,
            Decoration = TextDecoration.None,
            Font = Font.Thin,
            Scale = scale * panelScale
        };
    }

    private Vector2 SwingPoint(Vector2 p, float swingSin, float swingCos)
    {
        float dx = p.X - menuRect.Center.X;
        float dy = p.Y;
        return new Vector2(menuRect.Center.X + dx * swingCos - dy * swingSin, dx * swingSin + dy * swingCos);
    }

    private void DrawSwung(Sprite sprite, Vector2 center, float swing, float swingSin, float swingCos)
    {
        base.core.Renderer["fg", 9000, false].DrawSpriteS(sprite, SwingPoint(center, swingSin, swingCos), null, null, swing, SpriteFlip.None, SpriteOrigin.Center);
    }

    public override void Draw()
    {
        float num = 1f - (float)base.Trans / (float)base.TransDuration;
        base.core.Renderer["fg", 9000, false].FillScreen(Color.Black * (1f - num * num * num));
        float num2 = (float)Tween.BackEaseOut(base.Trans, -base.core.Renderer.ScreenHeight, base.core.Renderer.ScreenHeight, base.TransDuration);
        float swing = ((swingT >= 0) ? (1.2f * (float)(swingT * swingT) / 2500f) : 0f);
        float swingSin = (float)Math.Sin(swing);
        float swingCos = (float)Math.Cos(swing);
        DateTime utcNow = DateTime.UtcNow;
        double secondsLeft = 86400.0 - (utcNow.Hour * 3600 + utcNow.Minute * 60 + utcNow.Second);
        for (int i = chain.Height; menuRect.Top + S(21f) + num2 - (float)i > (float)(-chain.Height); i += chain.Height)
        {
            DrawSwung(chain, new Vector2(menuRect.Left + S(20f) + (float)chain.Width * 0.5f, menuRect.Top + S(21f) + num2 - (float)i + (float)chain.Height * 0.5f), swing, swingSin, swingCos);
            DrawSwung(chain, new Vector2(menuRect.Right - S(19f) - (float)chain.Width * 0.5f, menuRect.Top + S(21f) + num2 - (float)i + (float)chain.Height * 0.5f), swing, swingSin, swingCos);
        }
        Vector2 panelTop = new Vector2(menuRect.Center.X - (menuRect.Top + num2) * swingSin, (menuRect.Top + num2) * swingCos);
        base.core.Renderer["fg", 9000, false].DrawSpriteS(block, panelTop, null, Vector2.One * panelScale, swing, SpriteFlip.None, SpriteOrigin.TopCenter);
        float topT = menuRect.Top + num2;
        float cx = menuRect.Center.X;
        int totalMinutes = (int)(secondsLeft / 60.0);
        int hoursLeft = totalMinutes / 60;
        int minsLeft = totalMinutes % 60;
        string countdown = "resets in " + ((hoursLeft > 0) ? (hoursLeft + "h " + minsLeft.ToString("00") + "m") : (minsLeft + "m"));
        base.core.Renderer["fg", 9000, false].DrawTextS(countdown, new Vector2(cx + S(countdownX), topT + S(countdownY)), CenteredProfile(0.7f).Alter(default(Color).FromRgb(9462096)));
        base.core.Renderer["fg", 9000, false].DrawTextS("DAILY RUN", new Vector2(cx + S(titleX), topT + S(titleY)), CenteredProfile(1f, bold: true).Alter(default(Color).FromRgb(9462096)));
        bool iconDown = touchMenu[Button.IconTap].IsDown;
        Sprite iconSprite = (base.core.OptionsData.DailyIconAnimated ? charAnim.GetCurrentFrame() : _(desc.Icon));
        base.core.Renderer["fg", 9000, false].DrawSpriteS(iconSprite, new Vector2(cx + S(iconX), topT + S(iconY)), null, Vector2.One * (iconDown ? 1.08f : 1f) * panelScale, 0f, SpriteFlip.None, SpriteOrigin.Center);
        base.core.Renderer["fg", 9000, false].DrawTextS(__(desc.Name), new Vector2(cx + S(nameX), topT + S(nameY)), CenteredProfile(0.8f).Alter(TextProfile.OrangeMiddle));
        bool haveResult = base.core.ProfileData.DailyLastDistance > 0;
        bool showingResult = showResult && haveResult;
        bool codeDown = touchMenu[Button.CodeTap].IsDown;
        int sealCode = (showingResult ? base.core.ProfileData.DailyLastResultCode : sessionSeed);
        string sealLabel = ((showingResult ? "result: " : "code: ") + sealCode.ToString("X8"));
        base.core.Renderer["fg", 9000, false].DrawTextS(sealLabel, new Vector2(cx + S(codeX), topT + S(codeY)), CenteredProfile(0.8f).Alter((showingResult || codeDown) ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328)));
        base.core.Renderer["fg", 9000, false].DrawTextS("mods (" + mods.Count + ")", new Vector2(cx + S(modsLabelX), topT + S(modsLabelY)), CenteredProfile(0.7f).Alter(default(Color).FromRgb(9462096)));
        if (mods.Count == 0)
        {
            base.core.Renderer["fg", 9000, false].DrawTextS("none (vanilla)", new Vector2(cx + S(modsX), topT + S(modsY)), CenteredProfile(0.7f).Alter(default(Color).FromRgb(6910328)));
        }
        for (int j = 0; j < mods.Count; j++)
        {
            base.core.Renderer["fg", 9000, false].DrawTextS("- " + mods[j], new Vector2(cx + S(modsX), topT + S(modsY + modsPitch * (float)j)), CenteredProfile(0.7f).Alter(TextProfile.OrangeMiddle));
        }
        bool heartLit = (base.core.ProfileData.DailyBestDate == DailyRun.TodayKey() && base.core.ProfileData.DailyBestDistance >= 20);
        float heartPulse = (heartLit ? (1f + 0.12f * Component._sin((float)base.ticks * 0.1f)) : 1f);
        base.core.Renderer["fg", 9000, false].DrawTextS("attempts: " + DailyRun.AttemptsToday, new Vector2(cx + S(counterX), topT + S(counterY)), CenteredProfile(0.75f).Alter(heartLit ? TextProfile.OrangeMiddle : default(Color).FromRgb(9462096)));
        string todayText = (heartLit ? "played today!" : "not played yet");
        base.core.Renderer["fg", 9000, false].DrawTextS(todayText, new Vector2(cx + S(todayX), topT + S(todayY)), CenteredProfile(0.75f).Alter(heartLit ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328)));
        base.core.Renderer["fg", 9000, false].DrawSpriteS(_(SpriteName.bat_heart), new Vector2(cx + S(heartX), topT + S(heartY)), (heartLit ? Color.White : default(Color).FromRgb(6910328)) * (heartLit ? 1f : 0.7f), Vector2.One * heartPulse * panelScale, 0f, SpriteFlip.None, SpriteOrigin.Center);
        string streakDate = base.core.ProfileData.DailyStreakDate;
        int streakShown = ((streakDate == DailyRun.TodayKey() || streakDate == DailyRun.TodayKeyMinus(1)) ? base.core.ProfileData.DailyStreak : 0);
        base.core.Renderer["fg", 9000, false].DrawTextS("streak: " + streakShown, new Vector2(menuRect.Left + S(10f), topT + S(statsY)), LeftProfile(0.7f).Alter((streakShown > 0) ? TextProfile.OrangeMiddle : default(Color).FromRgb(9462096)));
        if (heartLit && base.core.ProfileData.DailyBestDistance > 0)
        {
            base.core.Renderer["fg", 9000, false].DrawTextS("best: " + base.core.ProfileData.DailyBestDistance + "m", new Vector2(menuRect.Right - S(10f), topT + S(statsY)), RightProfile(0.7f).Alter(TextProfile.OrangeMiddle));
        }
        if (base.core.ProfileData.DailyBestDistance > 0 && base.core.ProfileData.DailyBestDate != DailyRun.TodayKey())
        {
            float bob = Component._sin((float)base.ticks * 0.05f) * 2f;
            float gx = menuRect.Left + S(ghostX);
            base.core.Renderer["fg", 9000, false].DrawSpriteS(_(CharDescription.Get[(Character)base.core.ProfileData.DailyBestCharacter].SkullSprite), new Vector2(gx, topT + S(ghostY) + bob), Color.White * 0.45f, Vector2.One * 0.7f * panelScale, 0f, SpriteFlip.None, SpriteOrigin.Center);
            base.core.Renderer["fg", 9000, false].DrawTextS("yesterday", new Vector2(gx, topT + S(ghostY) + S(16f) + bob), CenteredProfile(0.5f).Alter(default(Color).FromRgb(6910328)));
            base.core.Renderer["fg", 9000, false].DrawTextS(base.core.ProfileData.DailyBestDistance + "m", new Vector2(gx, topT + S(ghostY) + S(26f) + bob), CenteredProfile(0.5f).Alter(TextProfile.OrangeMiddle));
        }
        touchMenu.Draw();
        if (EditorEnabled)
        {
            edLayout.Draw();
        }
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
            if (base.core.ProfileData.DailyLastDistance > 0)
            {
                showResult = !showResult;
                SendMessage(new PlaySoundMessage(SoundName.paper_touch));
            }
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
            base.core.Sharing.ShareDaily(base.core.ProfileData.DailyLastDistance, base.core.ProfileData.DailyLastCoins, base.core.ProfileData.DailyLastSeed, base.core.ProfileData.DailyLastCharacter, DailyRun.ModsString(base.core.OptionsData, (Character)base.core.ProfileData.DailyLastCharacter), base.core.ProfileData.DailyLastResultCode);
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
        if (EditorEnabled && edLayout != null && edLayout.Edit)
        {
            edLayout.HandleInput();
            return;
        }
        TransitionOut(CoreEvent.PopState);
        base.OnBackButtonPressed();
    }
}