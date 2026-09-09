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
    }

    private const int Depth = 10500;
    private const int HoldFrames = 40;
    private const float GrabRadius = 20f;
    private const float ScaleMin = 0.6f;
    private const float ScaleMax = 1.3f;

    private readonly string tag;

    private readonly List<Item> items = new List<Item>();

    private static readonly string[] slots = { "Y+1", "Y-1", "Y+5", "Y-5", "X+1", "X-1", "W+.1", "W-.1", "H+.1", "H-.1", "sel+", "sel-", "DUMP", "EXIT" };

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
        return new RectangleF(base.core.Renderer.ScreenWidth - 29f, 6f + (float)i * 17f, 27f, 15f);
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
                else
                {
                    SelectAt(tl.Position);
                    if (Sel >= 0)
                    {
                        touch = tl.Id;
                        origX = items[Sel].X;
                        origY = items[Sel].Y;
                    }
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
                    if (tool < 0 && Sel >= 0)
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
            Adjust(true, 0.1f);
            break;
        case 7:
            Adjust(true, -0.1f);
            break;
        case 8:
            Adjust(false, 0.1f);
            break;
        case 9:
            Adjust(false, -0.1f);
            break;
        case 10:
            CycleSel(1);
            break;
        case 11:
            CycleSel(-1);
            break;
        case 12:
            Dump();
            break;
        case 13:
            SetEdit(false);
            break;
        }
    }

    private void Nudge(float dx, float dy)
    {
        if (Sel < 0)
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
        if (Sel < 0)
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
    }

    private static float ClampScale(float v)
    {
        return Math.Max(ScaleMin, Math.Min(ScaleMax, v));
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
            Width = 27,
            Height = 15,
            BoxAlignment = Alignment2D.Middle,
            TextAlignment = Alignment2D.Middle,
            Decoration = TextDecoration.None,
            Font = Font.Thin,
            Scale = 0.45f
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
            base.core.Renderer["fg", Depth, false].DrawSpriteS(btn, new Vector2(r.Left, r.Top), (down ? default(Color).FromRgb(11216961) : Color.White) * 0.9f);
            base.core.Renderer["fg", Depth, false].DrawTextS(slots[i], new Vector2(r.Center.X, r.Center.Y), SlotProfile().Alter(down ? TextProfile.OrangeMiddle : default(Color).FromRgb(16777215)));
        }
        base.core.Renderer["fg", Depth, false].DrawTextS("LAYOUT EDIT: " + tag, new Vector2(sw, 12f), HeadProfile(0.7f).Alter(TextProfile.OrangeMiddle));
        string info;
        if (Sel < 0)
        {
            info = "tap a row on the menu";
        }
        else
        {
            Item it = items[Sel];
            info = "sel: " + it.Name + "  X=" + Fmt(it.X) + " Y=" + Fmt(it.Y);
            if (it.IsScale)
            {
                info += " scale=" + FmtScale(it.Y);
            }
            if (it.HasSize)
            {
                info += " W=" + Fmt(it.W) + " H=" + Fmt(it.H);
            }
        }
        base.core.Renderer["fg", Depth, false].DrawTextS(info, new Vector2(sw, 26f), HeadProfile(0.55f).Alter(default(Color).FromRgb(11216961)));
        base.core.Renderer["fg", Depth, false].DrawTextS("drag to move - DUMP prints consts - EXIT done", new Vector2(sw, 38f), HeadProfile(0.45f).Alter(default(Color).FromRgb(9462096)));
        if (Sel >= 0 && items[Sel].Anchor != null && Component._sin((float)base.ticks * 0.25f) > -0.3f)
        {
            base.core.Renderer["fg", Depth, false].DrawTextS("+", items[Sel].Anchor(), HeadProfile(0.7f).Alter(TextProfile.OrangeMiddle));
        }
    }
}