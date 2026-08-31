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

    // ---- layout: paste DUMP output here, then set EditorEnabled = false ----
    private const bool EditorEnabled = true;
    private const float PanelDY = 0f;
    private const float CountdownX = 0f;
    private const float CountdownY = 54f;
    private const float TitleX = 0f;
    private const float TitleY = 68f;
    private const float IconX = 0f;
    private const float IconY = 100f;
    private const float NameX = 0f;
    private const float NameY = 120f;
    private const float CodeX = 0f;
    private const float CodeY = 136f;
    private const float ModsLabelX = 0f;
    private const float ModsLabelY = 150f;
    private const float Mods0X = 0f;
    private const float Mods0Y = 162f;
    private const float Mods1X = 0f;
    private const float Mods1Y = 174f;
    private const float Mods2X = 0f;
    private const float Mods2Y = 186f;
    private const float Mods3X = 0f;
    private const float Mods3Y = 198f;
    private const float CounterX = 0f;
    private const float CounterY = 210f;
    private const float TodayX = 0f;
    private const float TodayY = 222f;
    private const float HeartX = -42f;      // rel panel center
    private const float HeartY = 227f;
    private const float GhostX = 16f;       // rel panel left
    private const float GhostY = 120f;
    private const float StartBtnX = 0f;
    private const float StartBtnY = 2f;     // below panel bottom
    private const float ShareBtnX = 0f;
    private const float ShareBtnY = 44f;
    private const float BackBtnX = 0f;
    private const float BackBtnY = 76f;

    private class Row
    {
        public string Name;
        public float X;
        public float Y;
        public bool IsButton;
        public Button Key;
        public float BW;
        public float BH;
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
    private Row rStart;
    private Row rShare;
    private Row rBack;

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

    private int cornerTouch = -1;

    private int cornerT;

    private int edTouch = -1;

    private int edTool = -1;

    private int edSel = -1;

    private bool edMoved;

    private Vector2 edStart;

    private float edOrigX;

    private float edOrigY;

    public DailyPrepareState()
    {
        base.TransDuration = 30;
        ShowCoins = false;
        IsOverlay = true;
        menuRect = new RectangleF((float)(base.core.Renderer.ScreenWidth - 148) * 0.5f, (float)(base.core.Renderer.ScreenHeight - 233) * 0.35f, 148f, 233f);
        panelDY = PanelDY;
        menuRect.Shift(0f, panelDY);
        dailyChar = DailyRun.DailyCharacter();
        desc = CharDescription.Get[dailyChar];
        sessionSeed = DailyRun.SessionSeed(base.core.OptionsData);
        mods = CollectMods(base.core.OptionsData, dailyChar);
        rPanel = AddRow("panel", 0f);
        rCountdown = AddRow("countdown", CountdownY);
        rCountdown.X = CountdownX;
        rTitle = AddRow("title", TitleY);
        rTitle.X = TitleX;
        rIcon = AddRow("icon", IconY);
        rIcon.X = IconX;
        rName = AddRow("name", NameY);
        rName.X = NameX;
        rCode = AddRow("code", CodeY);
        rCode.X = CodeX;
        rModsLabel = AddRow("mods label", ModsLabelY);
        rModsLabel.X = ModsLabelX;
        for (int i = 0; i < 4; i++)
        {
            rMods[i] = AddRow("mods " + i, ModsY(i));
            rMods[i].X = ModsX(i);
        }
        rCounter = AddRow("counter", CounterY);
        rCounter.X = CounterX;
        rToday = AddRow("today", TodayY);
        rToday.X = TodayX;
        rHeart = AddRow("heart", HeartY);
        rHeart.X = HeartX;
        rGhost = AddRow("ghost", GhostY);
        rGhost.X = GhostX;
        rStart = AddButtonRow("start btn", Button.Start, StartBtnY, StartBtnX, 104f, 26f);
        rShare = AddButtonRow("share btn", Button.Share, ShareBtnY, ShareBtnX, 92f, 18f);
        rBack = AddButtonRow("back btn", Button.Back, BackBtnY, BackBtnX, 70f, 30f);
        touchMenu = new TouchMenu<Button>(null, OnButtonRelease, "fg", 10000);
        string[] seq = desc.AnimSequence.Split('|');
        charAnim = new Animation(desc.AnimSpeed);
        charAnim.Add("live", seq[0], seq[1]);
        charAnim.Play("live");
        torchFire = new Animation(0.1f);
        torchFire.Add("burn", "torch_fire_", "123456");
        torchFire.Play("burn");
        touchMenu.SetupButton(Button.IconTap, new RectangleF(0f, 0f, 60f, 40f), null, null);
        touchMenu.SetupButton(Button.CodeTap, new RectangleF(0f, 0f, 120f, 18f), null, null);
        touchMenu.SetupButton(Button.Start, new RectangleF(0f, 0f, 104f, 26f), _(SpriteName.button), _(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, "START RUN", null, icon: false, iconIsPicture: false);
        touchMenu.SetupButton(Button.Share, new RectangleF(0f, 0f, 92f, 18f), _(SpriteName.button_green), _(SpriteName.button_green_pressed), _(SpriteName.button_green), stretch: true, SpriteFlip.None, ButtonColor.Green, "SHARE LAST", null, icon: false, iconIsPicture: false, blink: false, default(Color).FromRgb(11216961), null, -3f, 0f, 0.75f);
        touchMenu[Button.Share].Disabled = base.core.ProfileData.DailyLastDistance <= 0;
        touchMenu.SetupButton(Button.Back, new RectangleF(0f, 0f, 70f, 30f), _(SpriteName.button_back), _(SpriteName.button_back_down));
        RepositionAll();
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

    private static float ModsX(int i)
    {
        switch (i)
        {
        case 0: return Mods0X;
        case 1: return Mods1X;
        case 2: return Mods2X;
        default: return Mods3X;
        }
    }

    private static float ModsY(int i)
    {
        switch (i)
        {
        case 0: return Mods0Y;
        case 1: return Mods1Y;
        case 2: return Mods2Y;
        default: return Mods3Y;
        }
    }

    private Row AddRow(string name, float y)
    {
        Row row = new Row();
        row.Name = name;
        row.X = 0f;
        row.Y = y;
        rows.Add(row);
        return row;
    }

    private Row AddButtonRow(string name, Button key, float y, float x, float w, float h)
    {
        Row row = AddRow(name, y);
        row.IsButton = true;
        row.Key = key;
        row.X = x;
        row.BW = w;
        row.BH = h;
        return row;
    }

    private void SetupEditorButton(Button button, string label, int slot)
    {
        touchMenu.SetupButton(button, EdRect(slot), _(SpriteName.button), _(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Stone, label, null, icon: false, iconIsPicture: false);
        touchMenu[button].Hidden = true;
    }

    private RectangleF EdRect(int slot)
    {
        return new RectangleF(base.core.Renderer.ScreenWidth - 29f, 6f + (float)slot * 17f, 27f, 15f);
    }

    private void RepositionAll()
    {
        float cx = menuRect.Center.X;
        touchMenu[Button.IconTap].Rectangle = new RectangleF(cx - 30f, menuRect.Top + rIcon.Y - 20f, 60f, 40f);
        touchMenu[Button.CodeTap].Rectangle = new RectangleF(cx - 60f, menuRect.Top + rCode.Y - 9f, 120f, 18f);
        SetBtnRect(rStart);
        SetBtnRect(rShare);
        SetBtnRect(rBack);
    }

    private void SetBtnRect(Row r)
    {
        float cx = menuRect.Center.X + r.X;
        touchMenu[r.Key].Rectangle = new RectangleF(cx - r.BW * 0.5f, menuRect.Bottom + r.Y, r.BW, r.BH);
    }

    private void SetEdit(bool on)
    {
        edit = on;
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
        edTouch = -1;
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
            if (edit)
            {
                EditorInput();
            }
            else
            {
                if (EditorEnabled)
                {
                    CornerToggle();
                }
                touchMenu.HandleInput();
                base.HandleInput();
            }
        }
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

    private void EditorInput()
    {
        foreach (TouchLocation tl in base.core.TouchState)
        {
            if (tl.State == TouchLocationState.Pressed)
            {
                edTouch = tl.Id;
                edStart = tl.Position;
                edMoved = false;
                edTool = ToolbarSlot(tl.Position);
                edSel = -1;
                if (edTool < 0)
                {
                    SelectAt(tl.Position);
                    if (sel >= 0)
                    {
                        edSel = sel;
                        edOrigX = rows[sel].X;
                        edOrigY = ((rows[sel] == rPanel) ? panelDY : rows[sel].Y);
                    }
                }
            }
            else if (tl.Id == edTouch)
            {
                if (tl.State == TouchLocationState.Moved)
                {
                    Vector2 d = tl.Position - edStart;
                    if (Math.Abs(d.X) > 2f || Math.Abs(d.Y) > 2f)
                    {
                        edMoved = true;
                    }
                    ApplyDrag(d);
                }
                else if (tl.State == TouchLocationState.Released)
                {
                    if (edTool >= 0 && !edMoved)
                    {
                        ToolbarAction(edTool);
                    }
                    edTouch = -1;
                }
            }
        }
    }

    private int ToolbarSlot(Vector2 p)
    {
        for (int i = 0; i < 10; i++)
        {
            if (EdRect(i).Contains(p))
            {
                return i;
            }
        }
        return -1;
    }

    private void ToolbarAction(int slot)
    {
        switch (slot)
        {
        case 0:
            NudgeStep(0f, 1f);
            break;
        case 1:
            NudgeStep(0f, -1f);
            break;
        case 2:
            NudgeStep(0f, 5f);
            break;
        case 3:
            NudgeStep(0f, -5f);
            break;
        case 4:
            NudgeStep(1f, 0f);
            break;
        case 5:
            NudgeStep(-1f, 0f);
            break;
        case 6:
            CycleSel(1);
            break;
        case 7:
            CycleSel(-1);
            break;
        case 8:
            DumpLayout();
            break;
        case 9:
            SetEdit(false);
            break;
        }
    }

    private bool RowActive(Row r)
    {
        for (int j = 0; j < 4; j++)
        {
            if (r == rMods[j] && j >= mods.Count)
            {
                return false;
            }
        }
        return true;
    }

    private void SelectAt(Vector2 p)
    {
        int found = -1;
        if (p.Y < menuRect.Top + 14f && p.X > menuRect.Left - 12f && p.X < menuRect.Right + 12f)
        {
            found = rows.IndexOf(rPanel);
        }
        if (found < 0)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i].IsButton && touchMenu[rows[i].Key].Rectangle.Contains(p))
                {
                    found = i;
                    break;
                }
            }
        }
        if (found < 0 && Math.Abs(p.X - (menuRect.Center.X + rHeart.X)) <= 12f && Math.Abs(p.Y - (menuRect.Top + rHeart.Y)) <= 12f)
        {
            found = rows.IndexOf(rHeart);
        }
        if (found < 0 && Math.Abs(p.X - (menuRect.Left + rGhost.X)) <= 16f && Math.Abs(p.Y - (menuRect.Top + rGhost.Y)) <= 20f)
        {
            found = rows.IndexOf(rGhost);
        }
        if (found < 0)
        {
            float best = 9f;
            for (int i = 0; i < rows.Count; i++)
            {
                Row r = rows[i];
                if (r == rPanel || r.IsButton || r == rHeart || r == rGhost || !RowActive(r))
                {
                    continue;
                }
                float dy = Math.Abs(p.Y - (menuRect.Top + r.Y));
                if (dy < best)
                {
                    best = dy;
                    found = i;
                }
            }
        }
        if (found >= 0 && found != sel)
        {
            sel = found;
            SendMessage(new PlaySoundMessage(SoundName.paper_touch));
        }
    }

    private void ApplyDrag(Vector2 d)
    {
        if (edSel < 0)
        {
            return;
        }
        Row r = rows[edSel];
        if (r == rPanel)
        {
            float ny = (float)Math.Round(edOrigY + d.Y);
            float delta = ny - panelDY;
            if (delta != 0f)
            {
                panelDY = ny;
                menuRect.Shift(0f, delta);
                RepositionAll();
            }
            return;
        }
        float nx = (float)Math.Round(edOrigX + d.X);
        float nyy = (float)Math.Round(edOrigY + d.Y);
        bool changed = (nx != r.X) || (nyy != r.Y);
        r.X = nx;
        r.Y = nyy;
        if (changed && r.IsButton)
        {
            RepositionAll();
        }
    }

    private void NudgeStep(float dx, float dy)
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
                RepositionAll();
            }
            return;
        }
        r.X += dx;
        r.Y += dy;
        if (r.IsButton)
        {
            RepositionAll();
        }
    }

    private void CycleSel(int dir)
    {
        if (rows.Count == 0)
        {
            return;
        }
        int i = sel;
        for (int tries = 0; tries < rows.Count; tries++)
        {
            i = (i + dir + rows.Count) % rows.Count;
            if (RowActive(rows[i]))
            {
                break;
            }
        }
        sel = i;
        SendMessage(new PlaySoundMessage(SoundName.paper_touch));
    }

    private void DumpLayout()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("// ---- DailyPrepareState layout (paste over consts) ----\n");
        sb.Append("private const float PanelDY = ").Append(Fmt(panelDY)).Append('\n');
        DumpRow(sb, "Countdown", rCountdown);
        DumpRow(sb, "Title", rTitle);
        DumpRow(sb, "Icon", rIcon);
        DumpRow(sb, "Name", rName);
        DumpRow(sb, "Code", rCode);
        DumpRow(sb, "ModsLabel", rModsLabel);
        DumpRow(sb, "Mods0", rMods[0]);
        DumpRow(sb, "Mods1", rMods[1]);
        DumpRow(sb, "Mods2", rMods[2]);
        DumpRow(sb, "Mods3", rMods[3]);
        DumpRow(sb, "Counter", rCounter);
        DumpRow(sb, "Today", rToday);
        sb.Append("private const float HeartX = ").Append(Fmt(rHeart.X)).Append(" // rel panel center\n");
        sb.Append("private const float HeartY = ").Append(Fmt(rHeart.Y)).Append('\n');
        sb.Append("private const float GhostX = ").Append(Fmt(rGhost.X)).Append(" // rel panel left\n");
        sb.Append("private const float GhostY = ").Append(Fmt(rGhost.Y)).Append('\n');
        DumpBtn(sb, "StartBtn", rStart);
        DumpBtn(sb, "ShareBtn", rShare);
        DumpBtn(sb, "BackBtn", rBack);
        sb.Append("// then set EditorEnabled = false\n");
        Console.WriteLine(sb.ToString());
        SendMessage(new PlaySoundMessage(SoundName.paper_touch));
    }

    private void DumpRow(StringBuilder sb, string name, Row r)
    {
        sb.Append("private const float ").Append(name).Append("X = ").Append(Fmt(r.X)).Append('\n');
        sb.Append("private const float ").Append(name).Append("Y = ").Append(Fmt(r.Y)).Append('\n');
    }

    private void DumpBtn(StringBuilder sb, string name, Row r)
    {
        sb.Append("private const float ").Append(name).Append("X = ").Append(Fmt(r.X)).Append('\n');
        sb.Append("private const float ").Append(name).Append("Y = ").Append(Fmt(r.Y)).Append(" // below panel bottom\n");
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
        base.core.Renderer["fg", 9000, false].DrawTextS(countdown, new Vector2(menuRect.Center.X + rCountdown.X, topT + rCountdown.Y), CenteredProfile(0.7f).Alter(default(Color).FromRgb(9462096)));
        base.core.Renderer["fg", 9000, false].DrawTextS("DAILY RUN", new Vector2(menuRect.Center.X + rTitle.X, topT + rTitle.Y), CenteredProfile(1f, bold: true).Alter(default(Color).FromRgb(9462096)));
        DrawTorch(new Vector2(menuRect.Left + 16f, topT + 14f), dayFrac, swing, swingSin, swingCos);
        DrawTorch(new Vector2(menuRect.Right - 16f, topT + 14f), dayFrac, swing, swingSin, swingCos);
        bool iconDown = touchMenu[Button.IconTap].IsDown;
        Sprite iconSprite = (base.core.OptionsData.DailyIconAnimated ? charAnim.GetCurrentFrame() : _(desc.Icon));
        base.core.Renderer["fg", 9000, false].DrawSpriteS(iconSprite, new Vector2(menuRect.Center.X + rIcon.X, topS + rIcon.Y), null, Vector2.One * (iconDown ? 1.08f : 1f), 0f, SpriteFlip.None, SpriteOrigin.Center);
        base.core.Renderer["fg", 9000, false].DrawTextS(__(desc.Name), new Vector2(menuRect.Center.X + rName.X, topS + rName.Y), CenteredProfile(0.8f).Alter(TextProfile.OrangeMiddle));
        bool haveResult = base.core.ProfileData.DailyLastDistance > 0;
        bool showingResult = showResult && haveResult;
        bool codeDown = touchMenu[Button.CodeTap].IsDown;
        int sealCode = (showingResult ? base.core.ProfileData.DailyLastResultCode : sessionSeed);
        string sealLabel = ((showingResult ? "result: " : "code: ") + sealCode.ToString("X8"));
        base.core.Renderer["fg", 9000, false].DrawTextS(sealLabel, new Vector2(menuRect.Center.X + rCode.X, topS + rCode.Y), CenteredProfile(0.8f).Alter((showingResult || codeDown) ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328)));
        base.core.Renderer["fg", 9000, false].DrawTextS("mods (" + mods.Count + ")", new Vector2(menuRect.Center.X + rModsLabel.X, topS + rModsLabel.Y), CenteredProfile(0.7f).Alter(default(Color).FromRgb(9462096)));
        if (mods.Count == 0)
        {
            base.core.Renderer["fg", 9000, false].DrawTextS("none (vanilla)", new Vector2(menuRect.Center.X + rMods[0].X, topS + rMods[0].Y), CenteredProfile(0.7f).Alter(default(Color).FromRgb(6910328)));
        }
        for (int j = 0; j < mods.Count; j++)
        {
            base.core.Renderer["fg", 9000, false].DrawTextS("- " + mods[j], new Vector2(menuRect.Center.X + rMods[j].X, topS + rMods[j].Y), CenteredProfile(0.7f).Alter(TextProfile.OrangeMiddle));
        }
        bool heartLit = (base.core.ProfileData.DailyBestDate == DailyRun.TodayKey() && base.core.ProfileData.DailyBestDistance >= 20);
        float heartPulse = (heartLit ? (1f + 0.12f * Component._sin((float)base.ticks * 0.1f)) : 1f);
        base.core.Renderer["fg", 9000, false].DrawTextS("dailies played: " + base.core.ProfileData.DailyTotalPlayed, new Vector2(menuRect.Center.X + rCounter.X, topS + rCounter.Y), CenteredProfile(0.75f).Alter(heartLit ? TextProfile.OrangeMiddle : default(Color).FromRgb(9462096)));
        string todayText = (heartLit ? "played today!" : "not played yet");
        base.core.Renderer["fg", 9000, false].DrawTextS(todayText, new Vector2(menuRect.Center.X + rToday.X, topS + rToday.Y), CenteredProfile(0.75f).Alter(heartLit ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328)));
        base.core.Renderer["fg", 9000, false].DrawSpriteS(_(SpriteName.bat_heart), new Vector2(menuRect.Center.X + rHeart.X, topS + rHeart.Y), (heartLit ? Color.White : default(Color).FromRgb(6910328)) * (heartLit ? 1f : 0.7f), Vector2.One * heartPulse, 0f, SpriteFlip.None, SpriteOrigin.Center);
        bool showGhost = base.core.ProfileData.DailyBestDistance > 0 && (edit || base.core.ProfileData.DailyBestDate != DailyRun.TodayKey());
        if (showGhost)
        {
            float bob = (edit ? 0f : Component._sin((float)base.ticks * 0.05f) * 2f);
            float ghostX = menuRect.Left + rGhost.X;
            base.core.Renderer["fg", 9000, false].DrawSpriteS(_(CharDescription.Get[(Character)base.core.ProfileData.DailyBestCharacter].SkullSprite), new Vector2(ghostX, topS + rGhost.Y + bob), Color.White * 0.45f, Vector2.One * 0.7f, 0f, SpriteFlip.None, SpriteOrigin.Center);
            base.core.Renderer["fg", 9000, false].DrawTextS("yesterday", new Vector2(ghostX, topS + rGhost.Y + 16f + bob), CenteredProfile(0.5f).Alter(default(Color).FromRgb(6910328)));
            base.core.Renderer["fg", 9000, false].DrawTextS(base.core.ProfileData.DailyBestDistance + "m", new Vector2(ghostX, topS + rGhost.Y + 26f + bob), CenteredProfile(0.5f).Alter(TextProfile.OrangeMiddle));
        }
        touchMenu.Draw();
        if (edit)
        {
            DrawEditorChrome();
        }
        if (swingT >= 0)
        {
            base.core.Renderer["fg", 10500, false].FillScreen(Color.Black * Component._M(1f, (float)swingT / 30f));
        }
        base.Draw();
    }

    private void DrawEditorChrome()
    {
        base.core.Renderer["fg", 9000, false].DrawTextS("LAYOUT EDIT", new Vector2(menuRect.Center.X, 12f), CenteredProfile(0.8f, bold: true).Alter(TextProfile.OrangeMiddle));
        Row s = ((sel >= 0) ? rows[sel] : null);
        string info = ((s == null) ? "tap a row on the panel" : ("sel: " + s.Name + "  Y=" + s.Y + " X=" + s.X));
        base.core.Renderer["fg", 9000, false].DrawTextS(info, new Vector2(menuRect.Center.X, 26f), CenteredProfile(0.55f).Alter(default(Color).FromRgb(11216961)));
        base.core.Renderer["fg", 9000, false].DrawTextS("drag to move - DUMP prints consts", new Vector2(menuRect.Center.X, 38f), CenteredProfile(0.45f).Alter(default(Color).FromRgb(9462096)));
        if (s != null && Component._sin((float)base.ticks * 0.25f) > -0.3f)
        {
            base.core.Renderer["fg", 9000, false].DrawTextS("+", MarkerPos(s), CenteredProfile(0.7f).Alter(TextProfile.OrangeMiddle));
        }
    }

    private Vector2 MarkerPos(Row r)
    {
        if (r == rPanel)
        {
            return new Vector2(menuRect.Center.X, menuRect.Top - 8f);
        }
        if (r.IsButton)
        {
            return new Vector2(menuRect.Center.X + r.X, menuRect.Bottom + r.Y - 7f);
        }
        if (r == rHeart)
        {
            return new Vector2(menuRect.Center.X + r.X, menuRect.Top + r.Y - 10f);
        }
        if (r == rGhost)
        {
            return new Vector2(menuRect.Left + r.X, menuRect.Top + r.Y - 14f);
        }
        return new Vector2(menuRect.Center.X + r.X, menuRect.Top + r.Y - 9f);
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