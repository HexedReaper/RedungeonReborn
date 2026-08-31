using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Knighter.Entities;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Knighter.States;

public class DailyPrepareState : State
{
    private enum Button
    {
        IconTap,
        CodeTap,
        Start,
        Share,
        Back,
        EdYUp,
        EdYDn,
        EdY5Up,
        EdY5Dn,
        EdXUp,
        EdXDn,
        EdSelUp,
        EdSelDn,
        EdDump,
        EdExit
    }

    // ---- layout: paste DUMP output here ----
    // panel-space Y of each row's top; icon/heart Y are centers; HeartX is relative to panel center; GhostX relative to panel left; PanelDY shifts the whole assembly including buttons.
    private const bool EditorEnabled = true;
    private const float PanelDY = 0f;
    private const float CountdownY = 54f;
    private const float TitleY = 68f;
    private const float IconY = 100f;
    private const float NameY = 120f;
    private const float CodeY = 136f;
    private const float ModsLabelY = 150f;
    private const float Mods0Y = 162f;
    private const float Mods1Y = 174f;
    private const float Mods2Y = 186f;
    private const float Mods3Y = 198f;
    private const float CounterY = 210f;
    private const float TodayY = 222f;
    private const float HeartX = -42f;
    private const float HeartY = 227f;
    private const float GhostX = -22f;
    private const float GhostY = 120f;
    private const float GhostLabelY = 136f;
    private const float GhostDistY = 146f;

    private class Row
    {
        public string Name;
        public float X;
        public float Y;
        public bool FreeX;
        public bool Stack;
    }

    private readonly List<Row> rows = new List<Row>();

    private Row rPanel;
    private Row rCountdown;
    private Row rTitle;
    private Row rIcon;
    private Row rName;
    private Row rCode;
    private Row rModsLabel;
    private readonly Row[] rMods = new Row[4];
    private Row rCounter;
    private Row rToday;
    private Row rHeart;
    private Row rGhost;
    private Row rGhostLabel;
    private Row rGhostDist;

    private TouchMenu<Button> touchMenu;

    private RectangleF menuRect;

    private Sprite block;

    private Sprite chain;

    private Animation charAnim;

    private Animation torchFire;

    private bool showResult;

    private int swingT = -1;

    private readonly Character dailyChar;

    private readonly CharDescription desc;

    private readonly int sessionSeed;

    private readonly List<string> mods;

    private bool edit;

    private int sel = -1;

    private float panelDY;

    private int editTouch = -1;

    private Vector2 editLast;

    private int cornerTouch = -1;

    private int cornerT;

    public DailyPrepareState()
    {
        base.TransDuration = 30;
        ShowCoins = false;
        IsOverlay = true;
        menuRect = new RectangleF((float)(base.core.Renderer.ScreenWidth - 148) * 0.5f, (float)(base.core.Renderer.ScreenHeight - 233) * 0.35f, 148f, 233f);
        panelDY = PanelDY;
        menuRect.Shift(0f, panelDY);
        touchMenu = new TouchMenu<Button>(null, OnButtonRelease, "fg", 10000);
        dailyChar = DailyRun.DailyCharacter();
        desc = CharDescription.Get[dailyChar];
        sessionSeed = DailyRun.SessionSeed(base.core.OptionsData);
        mods = CollectMods(base.core.OptionsData, dailyChar);
        rPanel = AddRow("panel", 0f, freeX: false, stack: false);
        rCountdown = AddRow("countdown", CountdownY, freeX: false, stack: false);
        rTitle = AddRow("title", TitleY, freeX: false, stack: false);
        rIcon = AddRow("icon", IconY, freeX: false, stack: true);
        rName = AddRow("name", NameY, freeX: false, stack: true);
        rCode = AddRow("code", CodeY, freeX: false, stack: true);
        rModsLabel = AddRow("mods label", ModsLabelY, freeX: false, stack: true);
        rMods[0] = AddRow("mods 0", Mods0Y, freeX: false, stack: true);
        rMods[1] = AddRow("mods 1", Mods1Y, freeX: false, stack: true);
        rMods[2] = AddRow("mods 2", Mods2Y, freeX: false, stack: true);
        rMods[3] = AddRow("mods 3", Mods3Y, freeX: false, stack: true);
        rCounter = AddRow("counter", CounterY, freeX: false, stack: true);
        rToday = AddRow("today", TodayY, freeX: false, stack: true);
        rHeart = AddRow("heart", HeartY, freeX: true, stack: true);
        rHeart.X = HeartX;
        rGhost = AddRow("ghost", GhostY, freeX: true, stack: true);
        rGhost.X = GhostX;
        rGhostLabel = AddRow("ghost lbl", GhostLabelY, freeX: false, stack: true);
        rGhostDist = AddRow("ghost dist", GhostDistY, freeX: false, stack: true);
        string[] seq = desc.AnimSequence.Split('|');
        charAnim = new Animation(desc.AnimSpeed);
        charAnim.Add("live", seq[0], seq[1]);
        charAnim.Play("live");
        torchFire = new Animation(0.1f);
        torchFire.Add("burn", "torch_fire_", "123456");
        torchFire.Play("burn");
        touchMenu.SetupButton(Button.IconTap, new RectangleF(menuRect.Center.X - 30f, menuRect.Top + 80f, 60f, 40f), null, null);
        touchMenu.SetupButton(Button.CodeTap, new RectangleF(menuRect.Center.X - 60f, menuRect.Top + CodeY - 9f, 120f, 18f), null, null);
        touchMenu.SetupButton(Button.Start, new RectangleF(menuRect.Center.X - 52f, menuRect.Bottom + 2f, 104f, 26f), _(SpriteName.button), _(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, "START RUN", null, icon: false, iconIsPicture: false);
        touchMenu.SetupButton(Button.Share, new RectangleF(menuRect.Center.X - 46f, menuRect.Bottom + 44f, 92f, 18f), _(SpriteName.button_green), _(SpriteName.button_green_pressed), _(SpriteName.button_green), stretch: true, SpriteFlip.None, ButtonColor.Green, "SHARE LAST", null, icon: false, iconIsPicture: false, blink: false, default(Color).FromRgb(11216961), null, -3f, 0f, 0.75f);
        touchMenu[Button.Share].Disabled = base.core.ProfileData.DailyLastDistance <= 0;
        touchMenu.SetupButton(Button.Back, new RectangleF(menuRect.Center.X - 35f, menuRect.Bottom + 76f, 70f, 30f), _(SpriteName.button_back), _(SpriteName.button_back_down));
        SetupEditorButton(Button.EdYUp, "Y+1", 0);
        SetupEditorButton(Button.EdYDn, "Y-1", 1);
        SetupEditorButton(Button.EdY5Up, "Y+5", 2);
        SetupEditorButton(Button.EdY5Dn, "Y-5", 3);
        SetupEditorButton(Button.EdXUp, "X+1", 4);
        SetupEditorButton(Button.EdXDn, "X-1", 5);
        SetupEditorButton(Button.EdSelUp, "sel+", 6);
        SetupEditorButton(Button.EdSelDn, "sel-", 7);
        SetupEditorButton(Button.EdDump, "DUMP", 8);
        SetupEditorButton(Button.EdExit, "EXIT", 9);
        block = _(SpriteName.options_block);
        chain = _(SpriteName.gui_chain);
        SendMessage(new PlaySoundMessage(SoundName.trans_2));
    }

    private Row AddRow(string name, float y, bool freeX, bool stack)
    {
        Row row = new Row();
        row.Name = name;
        row.X = 0f;
        row.Y = y;
        row.FreeX = freeX;
        row.Stack = stack;
        rows.Add(row);
        return row;
    }

    private void SetupEditorButton(Button button, string label, int slot)
    {
        RectangleF r = new RectangleF(base.core.Renderer.ScreenWidth - 29f, 6f + (float)slot * 17f, 27f, 15f);
        touchMenu.SetupButton(button, r, _(SpriteName.button), _(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Stone, label, null, icon: false, iconIsPicture: false);
        touchMenu[button].Hidden = true;
    }

    private void SetEdit(bool on)
    {
        edit = on;
        touchMenu[Button.IconTap].Visible = !on;
        touchMenu[Button.CodeTap].Visible = !on;
        touchMenu[Button.Start].Visible = !on;
        touchMenu[Button.Share].Visible = !on;
        touchMenu[Button.Back].Visible = !on;
        touchMenu[Button.EdYUp].Visible = on;
        touchMenu[Button.EdYDn].Visible = on;
        touchMenu[Button.EdY5Up].Visible = on;
        touchMenu[Button.EdY5Dn].Visible = on;
        touchMenu[Button.EdXUp].Visible = on;
        touchMenu[Button.EdXDn].Visible = on;
        touchMenu[Button.EdSelUp].Visible = on;
        touchMenu[Button.EdSelDn].Visible = on;
        touchMenu[Button.EdDump].Visible = on;
        touchMenu[Button.EdExit].Visible = on;
        touchMenu.ReleaseButtons();
        SendMessage(new PlaySoundMessage(on ? SoundName.piston_extend : SoundName.piston_retract));
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
            if (EditorEnabled)
            {
                CornerToggle();
            }
            touchMenu.HandleInput();
            if (edit)
            {
                EditorDrag();
            }
        }
        base.HandleInput();
    }

    private void CornerToggle()
    {
        foreach (TouchLocation tl in base.core.TouchState)
        {
            if (tl.State == TouchLocationState.Pressed && tl.Position.X < 56f && tl.Position.Y < 44f)
            {
                cornerTouch = tl.Id;
                cornerT = 0;
            }
            else if (tl.Id == cornerTouch)
            {
                if (tl.State == TouchLocationState.Released)
                {
                    cornerTouch = -1;
                }
                else
                {
                    cornerT++;
                    if (cornerT >= 40)
                    {
                        cornerTouch = -1;
                        SetEdit(!edit);
                    }
                }
            }
        }
    }

    private void EditorDrag()
    {
        foreach (TouchLocation tl in base.core.TouchState)
        {
            if (tl.State == TouchLocationState.Pressed)
            {
                editTouch = tl.Id;
                editLast = tl.Position;
                SelectAt(tl.Position);
            }
            else if (tl.Id == editTouch)
            {
                if (tl.State == TouchLocationState.Moved)
                {
                    DragSel(tl.Position - editLast);
                    editLast = tl.Position;
                }
                else if (tl.State == TouchLocationState.Released)
                {
                    editTouch = -1;
                }
            }
        }
    }

    private void SelectAt(Vector2 p)
    {
        for (int i = 0; i < rows.Count; i++)
        {
            Row r = rows[i];
            float ly = ((r == rPanel) ? (menuRect.Top + 9f) : (menuRect.Top + r.Y + 5f));
            if (p.X >= menuRect.Left - 58f && p.X <= menuRect.Left - 4f && p.Y >= ly - 9f && p.Y <= ly + 9f)
            {
                if (sel != i)
                {
                    sel = i;
                    SendMessage(new PlaySoundMessage(SoundName.paper_touch));
                }
                return;
            }
        }
    }

    private void DragSel(Vector2 d)
    {
        if (sel < 0)
        {
            return;
        }
        Row r = rows[sel];
        if (r == rPanel)
        {
            float snapped = (float)Math.Round(panelDY + d.Y);
            if (snapped != panelDY)
            {
                menuRect.Shift(0f, snapped - panelDY);
            }
            panelDY = snapped;
            return;
        }
        r.Y = (float)Math.Round(r.Y + d.Y);
        if (r.FreeX)
        {
            r.X = (float)Math.Round(r.X + d.X);
        }
    }

    private void Nudge(float dx, float dy)
    {
        if (sel < 0)
        {
            return;
        }
        Row r = rows[sel];
        if (r == rPanel)
        {
            if (dy != 0f)
            {
                panelDY += dy;
                menuRect.Shift(0f, dy);
            }
            return;
        }
        r.Y += dy;
        if (r.FreeX)
        {
            r.X += dx;
        }
    }

    private void CycleSel(int dir)
    {
        if (rows.Count == 0)
        {
            return;
        }
        sel = (sel + dir + rows.Count) % rows.Count;
        SendMessage(new PlaySoundMessage(SoundName.paper_touch));
    }

    private void DumpLayout()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("// ---- DailyPrepareState layout dump ----\n");
        sb.Append("private const float PanelDY = ").Append(Fmt(panelDY)).Append('\n');
        sb.Append("private const float CountdownY = ").Append(Fmt(rCountdown.Y)).Append('\n');
        sb.Append("private const float TitleY = ").Append(Fmt(rTitle.Y)).Append('\n');
        sb.Append("private const float IconY = ").Append(Fmt(rIcon.Y)).Append('\n');
        sb.Append("private const float NameY = ").Append(Fmt(rName.Y)).Append('\n');
        sb.Append("private const float CodeY = ").Append(Fmt(rCode.Y)).Append('\n');
        sb.Append("private const float ModsLabelY = ").Append(Fmt(rModsLabel.Y)).Append('\n');
        sb.Append("private const float Mods0Y = ").Append(Fmt(rMods[0].Y)).Append('\n');
        sb.Append("private const float Mods1Y = ").Append(Fmt(rMods[1].Y)).Append('\n');
        sb.Append("private const float Mods2Y = ").Append(Fmt(rMods[2].Y)).Append('\n');
        sb.Append("private const float Mods3Y = ").Append(Fmt(rMods[3].Y)).Append('\n');
        sb.Append("private const float CounterY = ").Append(Fmt(rCounter.Y)).Append('\n');
        sb.Append("private const float TodayY = ").Append(Fmt(rToday.Y)).Append('\n');
        sb.Append("private const float HeartX = ").Append(Fmt(rHeart.X)).Append('\n');
        sb.Append("private const float HeartY = ").Append(Fmt(rHeart.Y)).Append('\n');
        sb.Append("private const float GhostX = ").Append(Fmt(rGhost.X)).Append('\n');
        sb.Append("private const float GhostY = ").Append(Fmt(rGhost.Y)).Append('\n');
        sb.Append("private const float GhostLabelY = ").Append(Fmt(rGhostLabel.Y)).Append('\n');
        sb.Append("private const float GhostDistY = ").Append(Fmt(rGhostDist.Y)).Append('\n');
        Console.WriteLine(sb.ToString());
        SendMessage(new PlaySoundMessage(SoundName.paper_touch));
    }

    private static string Fmt(float v)
    {
        return Math.Round(v).ToString("0", CultureInfo.InvariantCulture) + "f;";
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
            Width = 148,
            Height = 13,
            BoxAlignment = Alignment2D.Middle,
            TextAlignment = Alignment2D.Middle,
            Decoration = TextDecoration.None,
            Font = bold ? Font.Bold : Font.Thin,
            Scale = scale
        };
    }

    private TextProfile LabelProfile(float scale)
    {
        return new TextProfile
        {
            Width = 56,
            Height = 10,
            BoxAlignment = Alignment2D.Middle,
            TextAlignment = Alignment2D.Middle,
            Decoration = TextDecoration.None,
            Font = Font.Thin,
            Scale = scale
        };
    }

    private static List<string> CollectMods(OptionsData o, Character c)
    {
        List<string> list = new List<string>();
        if (o.HardcoreWebs)
        {
            list.Add("hardcore webs");
        }
        if (c == Character.Knight && o.DirectionalThrust)
        {
            list.Add("directional thrust");
        }
        if (c == Character.Bragg && o.BraggAmmo)
        {
            list.Add("bragg ammo");
        }
        if (c == Character.Vampire && o.VampirePredator)
        {
            list.Add("predator dives");
        }
        if (c == Character.Vampire && o.UnfriendBats)
        {
            list.Add("unfriend bats");
        }
        if (c == Character.Vampire && o.FastWings)
        {
            list.Add("fast wings");
        }
        return list;
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

    private void DrawTorch(Vector2 hangPos, float dayFrac, float swing, float swingSin, float swingCos)
    {
        Vector2 pos = SwingPoint(hangPos, swingSin, swingCos);
        base.core.Renderer["fg", 9001, false].DrawSpriteS(_(SpriteName.dungeon_torch), pos, null, null, swing, SpriteFlip.None, SpriteOrigin.TopCenter);
        float size = (0.6f + 0.4f * dayFrac) * (0.94f + 0.06f * Component._sin((float)base.ticks * 0.09f));
        Vector2 flamePos = pos + new Vector2(-8f * swingSin, 8f * swingCos);
        base.core.Renderer["fg", 9002, false].DrawSpriteS(torchFire.GetCurrentFrame(), flamePos, null, Vector2.One * size, swing, SpriteFlip.None, SpriteOrigin.TopCenter);
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
        for (int i = chain.Height; menuRect.Top + 21f + num2 - (float)i > (float)(-chain.Height); i += chain.Height)
        {
            DrawSwung(chain, new Vector2(menuRect.Left + 20f + (float)chain.Width * 0.5f, menuRect.Top + 21f + num2 - (float)i + (float)chain.Height * 0.5f), swing, swingSin, swingCos);
            DrawSwung(chain, new Vector2(menuRect.Right - 19f - (float)chain.Width * 0.5f, menuRect.Top + 21f + num2 - (float)i + (float)chain.Height * 0.5f), swing, swingSin, swingCos);
        }
        Vector2 panelTop = new Vector2(menuRect.Center.X - (menuRect.Top + num2) * swingSin, (menuRect.Top + num2) * swingCos);
        base.core.Renderer["fg", 9000, false].DrawSpriteS(block, panelTop, null, null, swing, SpriteFlip.None, SpriteOrigin.TopCenter);
        float topT = menuRect.Top + num2;
        float topS = topT + stack;
        int totalMinutes = (int)(secondsLeft / 60.0);
        int hoursLeft = totalMinutes / 60;
        int minsLeft = totalMinutes % 60;
        string countdown = "resets in " + ((hoursLeft > 0) ? (hoursLeft + "h " + minsLeft.ToString("00") + "m") : (minsLeft + "m"));
        base.core.Renderer["fg", 9000, false].DrawTextS(countdown, new Vector2(menuRect.Center.X, topT + rCountdown.Y), CenteredProfile(0.7f).Alter(default(Color).FromRgb(9462096)));
        base.core.Renderer["fg", 9000, false].DrawTextS("DAILY RUN", new Vector2(menuRect.Center.X, topT + rTitle.Y), CenteredProfile(1f, bold: true).Alter(default(Color).FromRgb(9462096)));
        DrawTorch(new Vector2(menuRect.Left + 16f, topT + 14f), dayFrac, swing, swingSin, swingCos);
        DrawTorch(new Vector2(menuRect.Right - 16f, topT + 14f), dayFrac, swing, swingSin, swingCos);
        bool iconDown = touchMenu[Button.IconTap].IsDown;
        Sprite iconSprite = (base.core.OptionsData.DailyIconAnimated ? charAnim.GetCurrentFrame() : _(desc.Icon));
        base.core.Renderer["fg", 9000, false].DrawSpriteS(iconSprite, new Vector2(menuRect.Center.X, topS + rIcon.Y), null, Vector2.One * (iconDown ? 1.08f : 1f), 0f, SpriteFlip.None, SpriteOrigin.Center);
        base.core.Renderer["fg", 9000, false].DrawTextS(__(desc.Name), new Vector2(menuRect.Center.X, topS + rName.Y), CenteredProfile(0.8f).Alter(TextProfile.OrangeMiddle));
        bool haveResult = base.core.ProfileData.DailyLastDistance > 0;
        bool showingResult = showResult && haveResult;
        bool codeDown = touchMenu[Button.CodeTap].IsDown;
        int sealCode = (showingResult ? base.core.ProfileData.DailyLastResultCode : sessionSeed);
        string sealLabel = ((showingResult ? "result: " : "code: ") + sealCode.ToString("X8"));
        base.core.Renderer["fg", 9000, false].DrawTextS(sealLabel, new Vector2(menuRect.Center.X, topS + rCode.Y), CenteredProfile(0.8f).Alter((showingResult || codeDown) ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328)));
        base.core.Renderer["fg", 9000, false].DrawTextS("mods (" + mods.Count + ")", new Vector2(menuRect.Center.X, topS + rModsLabel.Y), CenteredProfile(0.7f).Alter(default(Color).FromRgb(9462096)));
        if (mods.Count == 0)
        {
            base.core.Renderer["fg", 9000, false].DrawTextS("none (vanilla)", new Vector2(menuRect.Center.X, topS + rMods[0].Y), CenteredProfile(0.7f).Alter(default(Color).FromRgb(6910328)));
        }
        for (int j = 0; j < mods.Count; j++)
        {
            base.core.Renderer["fg", 9000, false].DrawTextS("- " + mods[j], new Vector2(menuRect.Center.X, topS + rMods[j].Y), CenteredProfile(0.7f).Alter(TextProfile.OrangeMiddle));
        }
        bool heartLit = (base.core.ProfileData.DailyBestDate == DailyRun.TodayKey() && base.core.ProfileData.DailyBestDistance >= 20);
        float heartPulse = (heartLit ? (1f + 0.12f * Component._sin((float)base.ticks * 0.1f)) : 1f);
        base.core.Renderer["fg", 9000, false].DrawTextS("dailies played: " + base.core.ProfileData.DailyTotalPlayed, new Vector2(menuRect.Center.X, topS + rCounter.Y), CenteredProfile(0.75f).Alter(heartLit ? TextProfile.OrangeMiddle : default(Color).FromRgb(9462096)));
        string todayText = (heartLit ? "played today!" : "not played yet");
        base.core.Renderer["fg", 9000, false].DrawTextS(todayText, new Vector2(menuRect.Center.X, topS + rToday.Y), CenteredProfile(0.75f).Alter(heartLit ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328)));
        base.core.Renderer["fg", 9000, false].DrawSpriteS(_(SpriteName.bat_heart), new Vector2(menuRect.Center.X + rHeart.X, topS + rHeart.Y), (heartLit ? Color.White : default(Color).FromRgb(6910328)) * (heartLit ? 1f : 0.7f), Vector2.One * heartPulse, 0f, SpriteFlip.None, SpriteOrigin.Center);
        if (edit || (base.core.ProfileData.DailyBestDistance > 0 && base.core.ProfileData.DailyBestDate != DailyRun.TodayKey()))
        {
            float bob = (edit ? 0f : Component._sin((float)base.ticks * 0.05f) * 2f);
            float ghostX = menuRect.Left + rGhost.X;
            base.core.Renderer["fg", 9000, false].DrawSpriteS(_(CharDescription.Get[(Character)base.core.ProfileData.DailyBestCharacter].SkullSprite), new Vector2(ghostX, topS + rGhost.Y + bob), Color.White * 0.45f, Vector2.One * 0.7f, 0f, SpriteFlip.None, SpriteOrigin.Center);
            base.core.Renderer["fg", 9000, false].DrawTextS("yesterday", new Vector2(ghostX, topS + rGhostLabel.Y + bob), CenteredProfile(0.5f).Alter(default(Color).FromRgb(6910328)));
            base.core.Renderer["fg", 9000, false].DrawTextS(base.core.ProfileData.DailyBestDistance + "m", new Vector2(ghostX, topS + rGhostDist.Y + bob), CenteredProfile(0.5f).Alter(TextProfile.OrangeMiddle));
        }
        touchMenu.Draw();
        if (edit)
        {
            DrawEditorOverlay(topT, topS);
        }
        if (swingT >= 0)
        {
            base.core.Renderer["fg", 10500, false].FillScreen(Color.Black * Component._M(1f, (float)swingT / 30f));
        }
        base.Draw();
    }

    private void DrawEditorOverlay(float topT, float topS)
    {
        var L = base.core.Renderer["fg", 9600, false];
        L.FillScreen(Color.Black * 0.35f);
        L.DrawTextS("LAYOUT EDIT", new Vector2(menuRect.Center.X, 8f), CenteredProfile(0.8f, bold: true).Alter(TextProfile.OrangeMiddle));
        L.DrawTextS("tap name to select - drag or nudge", new Vector2(menuRect.Center.X, 22f), CenteredProfile(0.5f).Alter(default(Color).FromRgb(9462096)));
        for (int i = 0; i < rows.Count; i++)
        {
            Row r = rows[i];
            float ly = ((r == rPanel) ? (menuRect.Top + 9f) : ((r.Stack ? topS : topT) + r.Y + 5f));
            L.DrawTextS(r.Name, new Vector2(menuRect.Left - 30f, ly), LabelProfile(0.45f).Alter((i == sel) ? TextProfile.OrangeMiddle : default(Color).FromRgb(9462096)));
            if (i == sel && r != rPanel)
            {
                float ax = menuRect.Center.X;
                if (r == rHeart)
                {
                    ax = menuRect.Center.X + r.X;
                }
                if (r == rGhost || r == rGhostLabel || r == rGhostDist)
                {
                    ax = menuRect.Left + rGhost.X;
                }
                L.DrawTextS("+", new Vector2(ax, ly), LabelProfile(0.7f).Alter(TextProfile.OrangeMiddle));
            }
        }
    }

    private void OnButtonRelease(Button button)
    {
        if (edit)
        {
            switch (button)
            {
            case Button.EdExit:
                SetEdit(false);
                return;
            case Button.EdDump:
                DumpLayout();
                return;
            case Button.EdSelUp:
                CycleSel(1);
                return;
            case Button.EdSelDn:
                CycleSel(-1);
                return;
            case Button.EdYUp:
                Nudge(0f, 1f);
                return;
            case Button.EdYDn:
                Nudge(0f, -1f);
                return;
            case Button.EdY5Up:
                Nudge(0f, 5f);
                return;
            case Button.EdY5Dn:
                Nudge(0f, -5f);
                return;
            case Button.EdXUp:
                Nudge(1f, 0f);
                return;
            case Button.EdXDn:
                Nudge(-1f, 0f);
                return;
            }
        }
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
            else
            {
                SendMessage(new PlaySoundMessage(SoundName.web_1));
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
        if (edit)
        {
            SetEdit(false);
            return;
        }
        TransitionOut(CoreEvent.PopState);
        base.OnBackButtonPressed();
    }
}