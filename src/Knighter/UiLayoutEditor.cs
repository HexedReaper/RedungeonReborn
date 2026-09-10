using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Knighter;

public class UiLayoutEditor : Component
{
    public class Item
    {
        public string Name;
        public float X;
        public float Y;
        public Func<Vector2> Anchor;
        public Func<bool> Active;
        public bool XOnly;
        public bool YOnly;
        public bool IsScale;
        public bool HasSize;
        public float W;
        public float H;
        public bool Locked;
        public bool HasScale;
        public float TextScale = 1f;
        public bool HasScaleXY;
        public float ScaleX = 1f;
        public float ScaleY = 1f;
        // align (0=center, 1=left, 2=right)
        public bool HasAlign;
        public float Align;
        // color (packed rgb int stored as float so it is printed)
        public bool HasColor;
        public float ColorRgb;
    }

    private const int Depth = 10500;
    private const int HoldFrames = 40;
    private const float GrabRadius = 20f;
    private const float ScaleMin = 0.6f;
    private const float ScaleMax = 1.3f;

    private readonly string tag;

    private readonly List<Item> items = new List<Item>();

    private static readonly string[] slots = { "Y+1", "Y-1", "Y+5", "Y-5", "X+1", "X-1", "X+5", "X-5", "LOCK", "W+.1", "W-.1", "H+.1", "H-.1", "ALIGN", "COL", "HUE", "sel+", "sel-", "DESEL", "DUMP", "EXIT" };
    // host sets this so drag deltas convert screen px -> panel-space px (1/panelScale)
    public float DragScale = 1f;

    public bool Edit { get; private set; }

    public int Sel { get; private set; } = -1;

    private int cornerTouch = -1;

    private int cornerT;

    private int touch = -1;

    private int tool = -1;

    private int downSlot = -1;

    private bool moved;

    private Vector2 start;

    private float origX;

    private float origY;

    public UiLayoutEditor(string tag)
    {
        this.tag = tag;
    }

    // name = const name used in the dump ("Countdown" -> CountdownX / CountdownY)
    // anchor = where the element currently renders on screen (for tap-select + marker), or null
    // active = false while the element is hidden this session (e.g. inactive mod rows), or null
    // x, y = initial position coordinates
    // xOnly, yOnly = restricts position editing to a single axis
    // isScale = indicates whether the values represent scale multipliers rather than screen position
    // hasSize, w, h = optional explicit width and height dimensions for selection bounds
    // Returns the newly created and added Item instance
    public Item Add(string name, float x, float y, Func<Vector2> anchor = null, Func<bool> active = null, bool xOnly = false, bool yOnly = false, bool isScale = false, bool hasSize = false, float w = 0f, float h = 0f)
    {
        Item item = new Item();
        item.Name = name;
        item.X = x;
        item.Y = y;
        item.Anchor = anchor;
        item.Active = active;
        item.XOnly = xOnly;
        item.YOnly = yOnly;
        item.IsScale = isScale;
        item.HasSize = hasSize;
        item.W = w;
        item.H = h;
        items.Add(item);
        return item;
    }

    // call FIRST in the host state's HandleInput; returns true (and blocks host input) while editing
    public bool HandleInput()
    {
        if (!Edit)
        {
            CornerHold();
            return false;
        }
        EditorInput();
        return true;
    }

    // call LAST in the host state's Draw
    public override void Draw()
    {
        if (Edit)
        {
            DrawChrome();
        }
        base.Draw();
    }

    private void CornerHold()
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
                    if (cornerT >= HoldFrames)
                    {
                        cornerTouch = -1;
                        SetEdit(true);
                    }
                }
            }
        }
    }

    private void SetEdit(bool on)
    {
        Edit = on;
        touch = -1;
        tool = -1;
        downSlot = -1;
        SendMessage(new PlaySoundMessage(on ? SoundName.piston_extend : SoundName.piston_retract));
    }

    private RectangleF SlotRect(int i)
    {
        if (i == 18)
        {
            return new RectangleF(1f, 179f, 40f, 18f);
        }
        if (i == 19)
        {
            return new RectangleF(base.core.Renderer.ScreenWidth - 41f, 179f, 40f, 18f);
        }
        if (i == 20)
        {
            return new RectangleF(base.core.Renderer.ScreenWidth - 41f, 201f, 40f, 18f);
        }
        int col = i / 9;
        int row = i % 9;
        float x = ((col == 0) ? 1f : (base.core.Renderer.ScreenWidth - 25f));
        return new RectangleF(x, 6f + (float)row * 19f, 24f, 16f);
    }

    private int SlotAt(Vector2 p)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (SlotRect(i).Contains(p))
            {
                return i;
            }
        }
        return -1;
    }

    private void EditorInput()
    {
        foreach (TouchLocation tl in base.core.TouchState)
        {
            if (tl.State == TouchLocationState.Pressed)
            {
                tool = SlotAt(tl.Position);
                moved = false;
                start = tl.Position;
                if (tool >= 0)
                {
                    downSlot = tool;
                    touch = tl.Id;
                }
                else if (Sel < 0)
                {
                    SelectAt(tl.Position);
                    if (Sel >= 0)
                    {
                        touch = tl.Id;
                        origX = items[Sel].X;
                        origY = items[Sel].Y;
                    }
                }
                else if (!items[Sel].Locked)
                {
                    touch = tl.Id;
                    origX = items[Sel].X;
                    origY = items[Sel].Y;
                }
            }
            else if (tl.Id == touch)
            {
                if (tl.State == TouchLocationState.Moved)
                {
                    Vector2 d = tl.Position - start;
                    if (Math.Abs(d.X) > 2f || Math.Abs(d.Y) > 2f)
                    {
                        moved = true;
                    }
                    if (tool < 0 && Sel >= 0 && !items[Sel].Locked)
                    {
                        if (items[Sel].IsScale)
                        {
                            items[Sel].Y = ClampScale(origY + d.Y * 0.005f);
                        }
                        else
                        {
                            items[Sel].X = (float)Math.Round(origX + d.X * DragScale);
                            items[Sel].Y = (float)Math.Round(origY + d.Y * DragScale);
                        }
                    }
                }
                else if (tl.State == TouchLocationState.Released)
                {
                    if (tool >= 0 && !moved)
                    {
                        SlotAction(tool);
                    }
                    downSlot = -1;
                    tool = -1;
                    touch = -1;
                }
            }
        }
    }

    private void SelectAt(Vector2 p)
    {
        int found = -1;
        float best = GrabRadius;
        for (int i = 0; i < items.Count; i++)
        {
            Item it = items[i];
            if (it.Anchor == null || (it.Active != null && !it.Active()))
            {
                continue;
            }
            float d = Vector2.Distance(p, it.Anchor());
            if (d < best)
            {
                best = d;
                found = i;
            }
        }
        if (found >= 0 && found != Sel)
        {
            Sel = found;
            SendMessage(new PlaySoundMessage(SoundName.paper_touch));
        }
    }

    private void CycleSel(int dir)
    {
        if (items.Count == 0)
        {
            return;
        }
        int i = Sel;
        for (int tries = 0; tries < items.Count; tries++)
        {
            i = (i + dir + items.Count) % items.Count;
            if (items[i].Active == null || items[i].Active())
            {
                break;
            }
        }
        Sel = i;
        SendMessage(new PlaySoundMessage(SoundName.paper_touch));
    }

    private void SlotAction(int slot)
    {
        switch (slot)
        {
        case 0:
            Nudge(0f, 1f);
            break;
        case 1:
            Nudge(0f, -1f);
            break;
        case 2:
            Nudge(0f, 5f);
            break;
        case 3:
            Nudge(0f, -5f);
            break;
        case 4:
            Nudge(1f, 0f);
            break;
        case 5:
            Nudge(-1f, 0f);
            break;
        case 6:
            Nudge(5f, 0f);
            break;
        case 7:
            Nudge(-5f, 0f);
            break;
        case 8:
            if (Sel >= 0)
            {
                items[Sel].Locked = !items[Sel].Locked;
                SendMessage(new PlaySoundMessage(items[Sel].Locked ? SoundName.knight_step_2 : SoundName.unlock_lock));
            }
            break;
        case 9:
            Adjust(true, 0.1f);
            break;
        case 10:
            Adjust(true, -0.1f);
            break;
        case 11:
            Adjust(false, 0.1f);
            break;
        case 12:
            Adjust(false, -0.1f);
            break;
        case 13:
            if (Sel >= 0 && items[Sel].HasAlign)
            {
                items[Sel].Align = (items[Sel].Align + 1f) % 3f;
                SendMessage(new PlaySoundMessage(SoundName.paper_touch));
            }
            break;
        case 14:
            if (Sel >= 0 && items[Sel].HasColor)
            {
                int idx = Array.IndexOf(Palette, (int)items[Sel].ColorRgb);
                items[Sel].ColorRgb = Palette[(idx + 1) % Palette.Length];
                SendMessage(new PlaySoundMessage(SoundName.coin));
            }
            break;
        case 15:
            if (Sel >= 0 && items[Sel].HasColor)
            {
                Color c = Col(items[Sel].ColorRgb);
                RgbToHsv(c.R / 255f, c.G / 255f, c.B / 255f, out float h, out float s, out float v);
                Color rotated = HsvToColor(h + 36f, s, v);
                items[Sel].ColorRgb = (rotated.R << 16) | (rotated.G << 8) | rotated.B;
                SendMessage(new PlaySoundMessage(SoundName.coin));
            }
            break;
        case 16:
            CycleSel(1);
            break;
        case 17:
            CycleSel(-1);
            break;
        case 18:
            Sel = -1;
            SendMessage(new PlaySoundMessage(SoundName.paper_touch));
            break;
        case 19:
            Dump();
            break;
        case 20:
            SetEdit(false);
            break;
        }
    }

    private void Nudge(float dx, float dy)
    {
        if (Sel < 0 || items[Sel].Locked)
        {
            return;
        }
        Item it = items[Sel];
        if (it.IsScale)
        {
            if (dy != 0f)
            {
                it.Y = ClampScale(it.Y + dy * 0.01f);
            }
            return;
        }
        it.X += dx;
        it.Y += dy;
    }

    private void Adjust(bool width, float amount)
    {
        if (Sel < 0 || items[Sel].Locked)
        {
            return;
        }
        Item it = items[Sel];
        if (it.IsScale)
        {
            it.Y = ClampScale(it.Y + amount);
        }
        else if (it.HasSize)
        {
            if (width)
            {
                it.W = Math.Max(8f, it.W + amount * 20f);
            }
            else
            {
                it.H = Math.Max(8f, it.H + amount * 20f);
            }
        }
        else if (it.HasScaleXY)
        {
            if (width)
            {
                it.ScaleX = Math.Max(0.3f, Math.Min(2f, it.ScaleX + amount * 0.5f));
            }
            else
            {
                it.ScaleY = Math.Max(0.3f, Math.Min(2f, it.ScaleY + amount * 0.5f));
            }
        }
        else if (it.HasScale)
        {
            it.TextScale = Math.Max(0.3f, Math.Min(2f, it.TextScale + amount * 0.5f));
        }
        else if (it.YOnly)
        {
            it.Y += amount * 20f;
        }
    }

    private static float ClampScale(float v)
    {
        return Math.Max(ScaleMin, Math.Min(ScaleMax, v));
    }


    private static readonly int[] Palette = { 16777215, 16732240, 15967806, 11216961, 14040624, 5481258, 5463138, 9810914, 5199247, 4076369, 6910328, 9462096 };

    private Color Col(float rgb)
    {
        return default(Color).FromRgb((int)rgb);
    }

    private static void RgbToHsv(float r, float g, float b, out float h, out float s, out float v)
    {
        float max = Math.Max(r, Math.Max(g, b));
        float min = Math.Min(r, Math.Min(g, b));
        v = max;
        float d = max - min;
        s = ((max <= 0f) ? 0f : (d / max));
        if (d <= 0f)
        {
            h = 0f;
            return;
        }
        if (max == r)
        {
            h = (g - b) / d % 6f;
        }
        else if (max == g)
        {
            h = (b - r) / d + 2f;
        }
        else
        {
            h = (r - g) / d + 4f;
        }
        h *= 60f;
        if (h < 0f)
        {
            h += 360f;
        }
    }

    private static Color HsvToColor(float h, float s, float v)
    {
        h = ((h % 360f + 360f) % 360f) / 60f;
        int i = (int)h;
        float f = h - (float)i;
        float p = v * (1f - s);
        float q = v * (1f - s * f);
        float t = v * (1f - s * (1f - f));
        float r = 0f;
        float g = 0f;
        float b = 0f;
        switch (i % 6)
        {
        case 0: r = v; g = t; b = p; break;
        case 1: r = q; g = v; b = p; break;
        case 2: r = p; g = v; b = t; break;
        case 3: r = p; g = q; b = v; break;
        case 4: r = t; g = p; b = v; break;
        default: r = v; g = p; b = q; break;
        }
        return new Color(r, g, b);
    }

    private void Dump()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("// ---- UiLayoutEditor dump: ").Append(tag).Append(" ----\n");
        for (int i = 0; i < items.Count; i++)
        {
            Item it = items[i];
            if (it.IsScale)
            {
                sb.Append("private const float ").Append(it.Name).Append(" = ").Append(FmtScale(it.Y)).Append('\n');
            }
            else if (it.XOnly)
            {
                sb.Append("private const float ").Append(it.Name).Append(" = ").Append(Fmt(it.X)).Append('\n');
            }
            else if (it.YOnly)
            {
                sb.Append("private const float ").Append(it.Name).Append(" = ").Append(Fmt(it.Y)).Append('\n');
            }
            else
            {
                sb.Append("private const float ").Append(it.Name).Append("X = ").Append(Fmt(it.X)).Append('\n');
                sb.Append("private const float ").Append(it.Name).Append("Y = ").Append(Fmt(it.Y)).Append('\n');
            }
            if (it.HasSize)
            {
                sb.Append("private const float ").Append(it.Name).Append("W = ").Append(Fmt(it.W)).Append('\n');
                sb.Append("private const float ").Append(it.Name).Append("H = ").Append(Fmt(it.H)).Append('\n');
            }
            if (it.HasScaleXY)
            {
                sb.Append("private const float ").Append(it.Name).Append("ScaleX = ").Append(FmtScale(it.ScaleX)).Append('\n');
                sb.Append("private const float ").Append(it.Name).Append("ScaleY = ").Append(FmtScale(it.ScaleY)).Append('\n');
            }
            if (it.HasAlign)
            {
                sb.Append("private const float ").Append(it.Name).Append("Align = ").Append(FmtScale(it.Align)).Append('\n');
            }
            if (it.HasColor)
            {
                sb.Append("private const float ").Append(it.Name).Append("Color = ").Append(FmtScale(it.ColorRgb)).Append('\n');
            }
        }
        Console.WriteLine(sb.ToString());
        SendMessage(new PlaySoundMessage(SoundName.paper_touch));
    }

    private static string Fmt(float v)
    {
        return Math.Round(v).ToString("0", CultureInfo.InvariantCulture) + "f;";
    }

    private static string FmtScale(float v)
    {
        return v.ToString("0.00", CultureInfo.InvariantCulture) + "f;";
    }

    private TextProfile SlotProfile()
    {
        return new TextProfile
        {
            Width = 24,
            Height = 15,
            BoxAlignment = Alignment2D.Middle,
            TextAlignment = Alignment2D.Middle,
            Decoration = TextDecoration.None,
            Font = Font.Thin,
            Scale = 0.4f
        };
    }

    private TextProfile HeadProfile(float scale)
    {
        return new TextProfile
        {
            Width = 220,
            Height = 12,
            BoxAlignment = Alignment2D.Middle,
            TextAlignment = Alignment2D.Middle,
            Decoration = TextDecoration.None,
            Font = Font.Thin,
            Scale = scale
        };
    }

    private void DrawChrome()
    {
        Sprite btn = _(SpriteName.button);
        float sw = base.core.Renderer.ScreenWidth * 0.5f;
        for (int i = 0; i < slots.Length; i++)
        {
            RectangleF r = SlotRect(i);
            bool down = downSlot == i;
            Vector2 fit = new Vector2((r.Width - 2f) / (float)btn.Width, (r.Height - 2f) / (float)btn.Height);
            base.core.Renderer["fg", Depth, false].DrawSpriteS(btn, new Vector2(r.Left + 1f, r.Top + 1f), (down ? default(Color).FromRgb(11216961) : Color.White) * 0.9f, fit);
            base.core.Renderer["fg", Depth, false].DrawTextS(slots[i], new Vector2(r.Center.X, r.Center.Y), SlotProfile().Alter(down ? TextProfile.OrangeMiddle : default(Color).FromRgb(16777215)));
        }
        base.core.Renderer["fg", Depth, false].DrawTextS("LAYOUT EDIT: " + tag, new Vector2(sw, 12f), HeadProfile(0.7f).Alter(TextProfile.OrangeMiddle));
        string info;
        if (Sel < 0)
        {
            info = "tap a row to select it";
        }
        else
        {
            Item it = items[Sel];
            info = "sel: " + it.Name + "  X=" + Fmt(it.X) + " Y=" + Fmt(it.Y) + (it.Locked ? "  [LOCKED]" : "");
            if (it.IsScale)
            {
                info += " scale=" + FmtScale(it.Y);
            }
            if (it.HasSize)
            {
                info += " W=" + Fmt(it.W) + " H=" + Fmt(it.H);
            }
            if (it.HasScale)
            {
                info += " text=" + FmtScale(it.TextScale);
            }
            if (it.HasAlign)
            {
                info += " align=" + ((it.Align < 0.5f) ? "C" : ((it.Align < 1.5f) ? "L" : "R"));
            }
            if (it.HasColor)
            {
                info += " rgb=" + (int)it.ColorRgb;
            }
        }
        base.core.Renderer["fg", Depth, false].DrawTextS(info, new Vector2(sw, 26f), HeadProfile(0.55f).Alter(default(Color).FromRgb(11216961)));
        base.core.Renderer["fg", Depth, false].DrawTextS((Sel < 0) ? "tap a row - DUMP prints consts - EXIT done" : "DESEL to pick another - LOCK stops accidents", new Vector2(sw, 38f), HeadProfile(0.45f).Alter(default(Color).FromRgb(9462096)));
        if (Sel >= 0 && items[Sel].Anchor != null && !items[Sel].Locked && Component._sin((float)base.ticks * 0.25f) > -0.3f)
        {
            base.core.Renderer["fg", Depth, false].DrawTextS("+", items[Sel].Anchor(), HeadProfile(0.7f).Alter(TextProfile.OrangeMiddle));
        }
    }
}