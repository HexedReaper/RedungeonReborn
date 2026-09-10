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

    // ---- layout baked from on-device UiLayoutEditor dump (2026-09-10) ----
    // to re-tune: set EditorEnabled = true, hold top-left corner and dump
    private const bool EditorEnabled = false;
    private const float PanelDY = -25f;
    private const float PanelScale = 1.1f;
    private const float CountdownX = 0f;
    private const float CountdownY = 62f;
    private const float CountdownScale = 0.65f;
    private const float CountdownAlign = 0f;
    private const float CountdownColor = 16732240f;
    private const float TitleX = 0f;
    private const float TitleY = 51f;
    private const float TitleScale = 1f;
    private const float TitleAlign = 0f;
    private const float TitleColor = 16732240f;
    private const float IconX = 1f;
    private const float IconY = 95f;
    private const float IconScaleX = 1f;
    private const float IconScaleY = 1f;
    private const float NameX = 1f;
    private const float NameY = 77f;
    private const float NameScale = 0.8f;
    private const float NameAlign = 0f;
    private const float NameColor = 16732240f;
    private const float CodeX = -1f;
    private const float CodeY = 114f;
    private const float CodeScale = 0.7f;
    private const float CodeAlign = 0f;
    private const float CodeColor = 6910328f;
    private const float ModsLabelX = -1f;
    private const float ModsLabelY = 127f;
    private const float ModsLabelScale = 0.7f;
    private const float ModsLabelAlign = 0f;
    private const float ModsLabelColor = 7536463f;
    private const float ModsX = -2f;
    private const float ModsY = 137f;
    private const float ModsScale = 0.7f;
    private const float ModsAlign = 0f;
    private const float ModsColor = 5215487f;
    private const float ModsPitch = 13f;
    private const float CounterX = -40f;
    private const float CounterY = 193f;
    private const float CounterScale = 0.75f;
    private const float CounterAlign = 0f;
    private const float CounterColor = 6910328f;
    private const float TodayX = -36f;
    private const float TodayY = 204f;
    private const float TodayScale = 0.75f;
    private const float TodayAlign = 0f;
    private const float TodayColor = 6910328f;
    private const float HeartX = -3f;
    private const float HeartY = 206f;
    private const float HeartScaleX = 1f;
    private const float HeartScaleY = 1f;
    private const float StatsX = 13f;
    private const float StatsY = 217f;
    private const float StatsW = 10f;
    private const float StatsH = 20f;
    private const float GhostX = 119f;
    private const float GhostY = 180f;
    private const float GhostScale = 0.9f;
    private const float GhostTextX = 0f;
    private const float GhostTextY = 15f;
    private const float GhostTextScale = 0.55f;
    private const float StartBtnX = 29f;
    private const float StartBtnY = 2f;
    private const float StartBtnW = 72f;
    private const float StartBtnH = 30f;
    private const float ShareBtnX = -37f;
    private const float ShareBtnY = 1f;
    private const float ShareBtnW = 56f;
    private const float ShareBtnH = 34f;
    private const float BackBtnX = -1f;
    private const float BackBtnY = 34f;
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
    private float statsX = StatsX;
    private float statsY = StatsY;
    private float statsW = StatsW;
    private float statsH = StatsH;
    private float ghostX = GhostX;
    private float ghostY = GhostY;
    private float countdownScale = CountdownScale;
    private float titleScale = TitleScale;
    private float nameScale = NameScale;
    private float codeScale = CodeScale;
    private float modsLabelScale = ModsLabelScale;
    private float modsScale = ModsScale;
    private float counterScale = CounterScale;
    private float todayScale = TodayScale;
    private float ghostScale = GhostScale;
    private float ghostTextScale = GhostTextScale;
    private float ghostTextX = GhostTextX;
    private float ghostTextY = GhostTextY;
    private float iconScaleX = IconScaleX;
    private float iconScaleY = IconScaleY;
    private float heartScaleX = HeartScaleX;
    private float heartScaleY = HeartScaleY;
    private float countdownAlign = CountdownAlign;
    private float titleAlign = TitleAlign;
    private float nameAlign = NameAlign;
    private float codeAlign = CodeAlign;
    private float modsLabelAlign = ModsLabelAlign;
    private float modsAlign = ModsAlign;
    private float counterAlign = CounterAlign;
    private float todayAlign = TodayAlign;
    private float countdownColor = CountdownColor;
    private float titleColor = TitleColor;
    private float nameColor = NameColor;
    private float codeColor = CodeColor;
    private float modsLabelColor = ModsLabelColor;
    private float modsColor = ModsColor;
    private float counterColor = CounterColor;
    private float todayColor = TodayColor;
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

    private UiLayoutEditor.Item edGhostText;

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
        edCountdown.HasScale = true;
        edCountdown.TextScale = countdownScale;
        edCountdown.HasAlign = true;
        edCountdown.Align = countdownAlign;
        edCountdown.HasColor = true;
        edCountdown.ColorRgb = countdownColor;
        edTitle = edLayout.Add("Title", titleX, titleY, () => new Vector2(menuRect.Center.X + S(titleX), menuRect.Top + S(titleY)));
        edTitle.HasScale = true;
        edTitle.TextScale = titleScale;
        edTitle.HasAlign = true;
        edTitle.Align = titleAlign;
        edTitle.HasColor = true;
        edTitle.ColorRgb = titleColor;
        edIcon = edLayout.Add("Icon", iconX, iconY, () => new Vector2(menuRect.Center.X + S(iconX), menuRect.Top + S(iconY)));
        edIcon.HasScaleXY = true;
        edIcon.ScaleX = iconScaleX;
        edIcon.ScaleY = iconScaleY;
        edName = edLayout.Add("Name", nameX, nameY, () => new Vector2(menuRect.Center.X + S(nameX), menuRect.Top + S(nameY)));
        edName.HasScale = true;
        edName.TextScale = nameScale;
        edName.HasAlign = true;
        edName.Align = nameAlign;
        edName.HasColor = true;
        edName.ColorRgb = nameColor;
        edCode = edLayout.Add("Code", codeX, codeY, () => new Vector2(menuRect.Center.X + S(codeX), menuRect.Top + S(codeY)));
        edCode.HasScale = true;
        edCode.TextScale = codeScale;
        edCode.HasAlign = true;
        edCode.Align = codeAlign;
        edCode.HasColor = true;
        edCode.ColorRgb = codeColor;
        edModsLabel = edLayout.Add("ModsLabel", modsLabelX, modsLabelY, () => new Vector2(menuRect.Center.X + S(modsLabelX), menuRect.Top + S(modsLabelY)));
        edModsLabel.HasScale = true;
        edModsLabel.TextScale = modsLabelScale;
        edModsLabel.HasAlign = true;
        edModsLabel.Align = modsLabelAlign;
        edModsLabel.HasColor = true;
        edModsLabel.ColorRgb = modsLabelColor;
        edMods = edLayout.Add("Mods", modsX, modsY, () => new Vector2(menuRect.Center.X + S(modsX), menuRect.Top + S(modsY)));
        edMods.HasScale = true;
        edMods.TextScale = modsScale;
        edMods.HasAlign = true;
        edMods.Align = modsAlign;
        edMods.HasColor = true;
        edMods.ColorRgb = modsColor;
        edModsPitch = edLayout.Add("ModsPitch", 0f, modsPitch, () => new Vector2(menuRect.Center.X, menuRect.Top + S(modsY + modsPitch)), null, false, true);
        edCounter = edLayout.Add("Counter", counterX, counterY, () => new Vector2(menuRect.Center.X + S(counterX), menuRect.Top + S(counterY)));
        edCounter.HasScale = true;
        edCounter.TextScale = counterScale;
        edCounter.HasAlign = true;
        edCounter.Align = counterAlign;
        edCounter.HasColor = true;
        edCounter.ColorRgb = counterColor;
        edToday = edLayout.Add("Today", todayX, todayY, () => new Vector2(menuRect.Center.X + S(todayX), menuRect.Top + S(todayY)));
        edToday.HasScale = true;
        edToday.TextScale = todayScale;
        edToday.HasAlign = true;
        edToday.Align = todayAlign;
        edToday.HasColor = true;
        edToday.ColorRgb = todayColor;
        edHeart = edLayout.Add("Heart", heartX, heartY, () => new Vector2(menuRect.Center.X + S(heartX), menuRect.Top + S(heartY)));
        edHeart.HasScaleXY = true;
        edHeart.ScaleX = heartScaleX;
        edHeart.ScaleY = heartScaleY;
        edStats = edLayout.Add("Stats", statsX, statsY, () => new Vector2(menuRect.Left + S(statsX), menuRect.Top + S(statsY)), null, false, false, false, true, statsW, statsH);
        edGhost = edLayout.Add("Ghost", ghostX, ghostY, () => new Vector2(menuRect.Left + S(ghostX), menuRect.Top + S(ghostY)));
        edGhost.HasScale = true;
        edGhost.TextScale = ghostScale;
        edGhostText = edLayout.Add("GhostText", ghostTextX, ghostTextY, () => new Vector2(menuRect.Left + S(ghostX + ghostTextX), menuRect.Top + S(ghostY + ghostTextY)));
        edGhostText.HasScale = true;
        edGhostText.TextScale = ghostTextScale;
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
        statsX = edStats.X;
        statsY = edStats.Y;
        statsW = edStats.W;
        statsH = edStats.H;
        ghostX = edGhost.X;
        ghostY = edGhost.Y;
        countdownScale = edCountdown.TextScale;
        titleScale = edTitle.TextScale;
        nameScale = edName.TextScale;
        codeScale = edCode.TextScale;
        modsLabelScale = edModsLabel.TextScale;
        modsScale = edMods.TextScale;
        counterScale = edCounter.TextScale;
        todayScale = edToday.TextScale;
        ghostScale = edGhost.TextScale;
        ghostTextScale = edGhostText.TextScale;
        ghostTextX = edGhostText.X;
        ghostTextY = edGhostText.Y;
        iconScaleX = edIcon.ScaleX;
        iconScaleY = edIcon.ScaleY;
        heartScaleX = edHeart.ScaleX;
        heartScaleY = edHeart.ScaleY;
        countdownAlign = edCountdown.Align;
        titleAlign = edTitle.Align;
        nameAlign = edName.Align;
        codeAlign = edCode.Align;
        modsLabelAlign = edModsLabel.Align;
        modsAlign = edMods.Align;
        counterAlign = edCounter.Align;
        todayAlign = edToday.Align;
        countdownColor = SanitizeRgb(edCountdown.ColorRgb, "Countdown");
        titleColor = SanitizeRgb(edTitle.ColorRgb, "Title");
        nameColor = SanitizeRgb(edName.ColorRgb, "Name");
        codeColor = SanitizeRgb(edCode.ColorRgb, "Code");
        modsLabelColor = SanitizeRgb(edModsLabel.ColorRgb, "ModsLabel");
        modsColor = SanitizeRgb(edMods.ColorRgb, "Mods");
        counterColor = SanitizeRgb(edCounter.ColorRgb, "Counter");
        todayColor = SanitizeRgb(edToday.ColorRgb, "Today");
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

    private TextProfile MakeProfile(float scale, float align, bool bold = false)
    {
        Alignment2D a = ((align < 0.5f) ? Alignment2D.Middle : ((align < 1.5f) ? Alignment2D.Left : Alignment2D.Right));
        return new TextProfile
        {
            Width = (int)S(148f),
            Height = 13,
            BoxAlignment = a,
            TextAlignment = a,
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

    private static float SanitizeRgb(float rgb, string who)
    {
        int v = (int)rgb;
        if (v < 0 || v > 16777215)
        {
            Console.WriteLine("[LAYOUT] " + who + " color out of range: " + v + " - clamped to white");
            v = 16777215;
        }
        return v;
    }

    private Color Col(float rgb)
    {
        return default(Color).FromRgb((int)rgb);
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
        bool urgent = totalMinutes < 60;
        Color countdownTint = (urgent ? default(Color).FromRgb(16732240) * (0.7f + 0.3f * Component._sin((float)base.ticks * 0.2f)) : Col(countdownColor));
        string countdown = "resets in " + ((hoursLeft > 0) ? (hoursLeft + "h " + minsLeft.ToString("00") + "m") : (minsLeft + "m")) + (urgent ? "!" : "");
        base.core.Renderer["fg", 9000, false].DrawTextS(countdown, new Vector2(cx + S(countdownX), topT + S(countdownY)), MakeProfile(countdownScale, countdownAlign).Alter(countdownTint));
        base.core.Renderer["fg", 9000, false].DrawTextS("DAILY RUN", new Vector2(cx + S(titleX), topT + S(titleY)), MakeProfile(titleScale, titleAlign, bold: true).Alter(Col(titleColor)));
        bool iconDown = touchMenu[Button.IconTap].IsDown;
        Sprite iconSprite = (base.core.OptionsData.DailyIconAnimated ? charAnim.GetCurrentFrame() : _(desc.Icon));
        base.core.Renderer["fg", 9000, false].DrawSpriteS(iconSprite, new Vector2(cx + S(iconX), topT + S(iconY)), null, new Vector2(panelScale * iconScaleX, panelScale * iconScaleY) * (iconDown ? 1.08f : 1f), 0f, SpriteFlip.None, SpriteOrigin.Center);
        base.core.Renderer["fg", 9000, false].DrawTextS(__(desc.Name), new Vector2(cx + S(nameX), topT + S(nameY)), MakeProfile(nameScale, nameAlign).Alter(Col(nameColor)));
        bool haveResult = base.core.ProfileData.DailyLastDistance > 0;
        bool showingResult = showResult && haveResult;
        bool codeDown = touchMenu[Button.CodeTap].IsDown;
        int sealCode = (showingResult ? base.core.ProfileData.DailyLastResultCode : sessionSeed);
        string sealLabel = ((showingResult ? "result: " : "code: ") + sealCode.ToString("X8"));
        base.core.Renderer["fg", 9000, false].DrawTextS(sealLabel, new Vector2(cx + S(codeX), topT + S(codeY)), MakeProfile(codeScale, codeAlign).Alter(Col((showingResult || codeDown) ? 16732240f : codeColor)));
        base.core.Renderer["fg", 9000, false].DrawTextS("mods (" + mods.Count + ")", new Vector2(cx + S(modsLabelX), topT + S(modsLabelY)), MakeProfile(modsLabelScale, modsLabelAlign).Alter(Col(modsLabelColor)));
        if (mods.Count == 0)
        {
            base.core.Renderer["fg", 9000, false].DrawTextS("none (vanilla)", new Vector2(cx + S(modsX), topT + S(modsY)), MakeProfile(modsScale, modsAlign).Alter(Col(modsColor)));
        }
        for (int j = 0; j < mods.Count; j++)
        {
            base.core.Renderer["fg", 9000, false].DrawTextS("- " + mods[j], new Vector2(cx + S(modsX), topT + S(modsY + modsPitch * (float)j)), MakeProfile(modsScale, modsAlign).Alter(Col(modsColor)));
        }
        bool heartLit = (base.core.ProfileData.DailyBestDate == DailyRun.TodayKey() && base.core.ProfileData.DailyBestDistance >= 20);
        float heartPulse = (heartLit ? (1f + 0.12f * Component._sin((float)base.ticks * 0.1f)) : 1f);
        base.core.Renderer["fg", 9000, false].DrawTextS("attempts: " + DailyRun.AttemptsToday, new Vector2(cx + S(counterX), topT + S(counterY)), MakeProfile(counterScale, counterAlign).Alter(Col(counterColor)));
        string todayText = (heartLit ? "played today!" : "not played yet");
        base.core.Renderer["fg", 9000, false].DrawTextS(todayText, new Vector2(cx + S(todayX), topT + S(todayY)), MakeProfile(todayScale, todayAlign).Alter(Col(todayColor)));
        base.core.Renderer["fg", 9000, false].DrawSpriteS(_(SpriteName.bat_heart), new Vector2(cx + S(heartX), topT + S(heartY)), (heartLit ? Color.White : default(Color).FromRgb(6910328)) * (heartLit ? 1f : 0.7f), new Vector2(panelScale * heartScaleX * heartPulse, panelScale * heartScaleY * heartPulse), 0f, SpriteFlip.None, SpriteOrigin.Center);
        string streakDate = base.core.ProfileData.DailyStreakDate;
        int streakShown = ((streakDate == DailyRun.TodayKey() || streakDate == DailyRun.TodayKeyMinus(1)) ? base.core.ProfileData.DailyStreak : 0);
        float statsScale = statsH * 0.035f;
        base.core.Renderer["fg", 9000, false].DrawTextS("streak: " + streakShown, new Vector2(menuRect.Left + S(statsX), topT + S(statsY)), LeftProfile(statsScale).Alter((streakShown > 0) ? TextProfile.OrangeMiddle : default(Color).FromRgb(9462096)));
        if (heartLit && base.core.ProfileData.DailyBestDistance > 0)
        {
            base.core.Renderer["fg", 9000, false].DrawTextS("best: " + base.core.ProfileData.DailyBestDistance + "m", new Vector2(menuRect.Right - S(statsW), topT + S(statsY)), RightProfile(statsScale).Alter(TextProfile.OrangeMiddle));
        }
        bool editMode = (EditorEnabled && edLayout != null && edLayout.Edit);
        if (base.core.ProfileData.DailyBestDistance > 0 && (editMode || base.core.ProfileData.DailyBestDate != DailyRun.TodayKey()))
        {
            float bob = Component._sin((float)base.ticks * 0.05f) * 2f;
            float gx = menuRect.Left + S(ghostX);
            base.core.Renderer["fg", 9000, false].DrawSpriteS(_(CharDescription.Get[(Character)base.core.ProfileData.DailyBestCharacter].SkullSprite), new Vector2(gx, topT + S(ghostY) + bob), Color.White * 0.45f, Vector2.One * 0.7f * panelScale * ghostScale, 0f, SpriteFlip.None, SpriteOrigin.Center);
            base.core.Renderer["fg", 9000, false].DrawTextS("yesterday", new Vector2(gx + S(ghostTextX), topT + S(ghostY + ghostTextY) + bob), MakeProfile(ghostTextScale, 0f).Alter(default(Color).FromRgb(6910328)));
            base.core.Renderer["fg", 9000, false].DrawTextS(base.core.ProfileData.DailyBestDistance + "m", new Vector2(gx + S(ghostTextX), topT + S(ghostY + ghostTextY + 10f) + bob), MakeProfile(ghostTextScale, 0f).Alter(TextProfile.OrangeMiddle));
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