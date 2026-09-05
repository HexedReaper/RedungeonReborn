using System;
using System.Collections.Generic;
using System.Reflection;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.States;

public class CharacterModsState : State
{
    private class ModDef
    {
        public string Section;
        public string Label;
        public string FieldName;
        public SoundName ToggleSound;
        public FieldInfo Field;

        public ModDef(string section, string label, string fieldName, SoundName toggleSound)
        {
            Section = section;
            Label = label;
            FieldName = fieldName;
            ToggleSound = toggleSound;
            Field = typeof(OptionsData).GetField(fieldName) ?? throw new Exception("OptionsData field missing: " + fieldName);
        }

        public bool GetValue(OptionsData data) => (bool)Field.GetValue(data);
        public void SetValue(OptionsData data, bool value) => Field.SetValue(data, value);
    }

    private static readonly ModDef[] AllMods = new ModDef[]
    {
        new ModDef("Gylbard", "directional thrust", "DirectionalThrust", SoundName.gylbard_sword),
        new ModDef("Bragg", "scavenger ammo", "BraggAmmo", SoundName.bragg_gun_cock),
        new ModDef("Bragg", "feather trail", "BraggFeathers", SoundName.bragg_parrot_voice_1),
        new ModDef("Bragg", "gun jam", "BraggJam", SoundName.bragg_gun_cock),
        new ModDef("Vampire", "predator dives", "VampirePredator", SoundName.kazhan_turn),
        new ModDef("Vampire", "unfriend bats", "UnfriendBats", SoundName.kazhan_flap_1),
        new ModDef("Vampire", "fast wings x1.5", "FastWings", SoundName.kazhan_flap_2),
        new ModDef("Creep", "escape jump", "CreepEscapeJump", SoundName.creep_scare),
        new ModDef("Creep", "moving platform glide", "CreepGlideMovingPlatforms", SoundName.swoosh_1),
        new ModDef("Other", "hardcore webs", "HardcoreWebs", SoundName.web_1),
        new ModDef("Other", "achievement toasts", "AchievementToasts", SoundName.coin),
    };

    private class SectionDef
    {
        public string Name;
        public List<ModDef> Mods = new List<ModDef>();
    }

    private List<SectionDef> sections;

    private TouchMenu<int> touchMenu;

    private RectangleF menuRect;

    private Sprite block;

    private Sprite chain;

    private int openSection;

    private int backButtonId;

    private const float PanelDY = 0f;
    private const float TitleX = 6f;
    private const float TitleY = 34f;
    private const float SectionTopY = 67f;
    private const float SectionPitch = 17f;
    private const float TogglePitch = 16f;
    private const float HeaderDX = 8f;
    private const float ToggleDX = 12f;
    private const float LabelDX = 32f;
    private const float LabelDY = -7f;
    private const float BackBtnX = 0f;
    private const float BackBtnY = 8f;

    public CharacterModsState()
    {
        base.TransDuration = 30;
        ShowCoins = false;
        IsOverlay = true;
        menuRect = new RectangleF((float)(base.core.Renderer.ScreenWidth - 148) * 0.5f, (float)(base.core.Renderer.ScreenHeight - 233) * 0.5f + PanelDY, 148f, 233f);

        sections = new List<SectionDef>();
        foreach (ModDef mod in AllMods)
        {
            SectionDef section = sections.Find(s => s.Name == mod.Section);
            if (section == null)
            {
                section = new SectionDef { Name = mod.Section };
                sections.Add(section);
            }
            section.Mods.Add(mod);
        }

        touchMenu = new TouchMenu<int>(null, OnButtonRelease, "fg", 10000);
        touchMenu.OnToggle = OnToggle;
        float left = menuRect.Left + ToggleDX;

        int id = 0;
        for (int s = 0; s < sections.Count; s++)
        {
            touchMenu.SetupButton(id, new RectangleF(menuRect.Left + HeaderDX, menuRect.Top, menuRect.Width - HeaderDX * 2f, 18f), null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Orange, sections[s].Name + " +", null, icon: false, iconIsPicture: false);
            id++;
            for (int m = 0; m < sections[s].Mods.Count; m++)
            {
                touchMenu.SetupToggle(id, new Vector2(left, menuRect.Top), sections[s].Mods[m].GetValue(base.core.OptionsData), 120);
                id++;
            }
        }

        if (base.core.OptionsData.DailyRunEnabled)
        {
            for (int s = 0; s < sections.Count; s++)
            {
                for (int m = 0; m < sections[s].Mods.Count; m++)
                {
                    touchMenu[GetToggleId(s, m)].Disabled = true;
                }
            }
        }

        backButtonId = id;
        touchMenu.SetupButton(backButtonId, new RectangleF(menuRect.Center.X + BackBtnX - 35f, menuRect.Bottom + BackBtnY, 70f, 30f), _(SpriteName.button_back), _(SpriteName.button_back_down));
        block = _(SpriteName.options_block);
        chain = _(SpriteName.gui_chain);
        LayoutMenu();
        SendMessage(new PlaySoundMessage(SoundName.trans_2));
    }

    private int GetHeaderId(int sectionIndex)
    {
        int id = 0;
        for (int s = 0; s < sectionIndex; s++)
        {
            id += 1 + sections[s].Mods.Count;
        }
        return id;
    }

    private int GetToggleId(int sectionIndex, int modIndex)
    {
        return GetHeaderId(sectionIndex) + 1 + modIndex;
    }

    private void Place(int id, float y)
    {
        touchMenu[id].Rectangle.Y = y;
    }

    private void LayoutMenu()
    {
        for (int s = 0; s < sections.Count; s++)
        {
            touchMenu[GetHeaderId(s)].Label = sections[s].Name + " " + ((openSection == s) ? "-" : "+");
            for (int m = 0; m < sections[s].Mods.Count; m++)
            {
                touchMenu[GetToggleId(s, m)].Hidden = openSection != s;
            }
        }
        float y = menuRect.Top + SectionTopY;
        for (int s = 0; s < sections.Count; s++)
        {
            Place(GetHeaderId(s), y);
            y += SectionPitch;
            if (openSection == s)
            {
                for (int m = 0; m < sections[s].Mods.Count; m++)
                {
                    Place(GetToggleId(s, m), y);
                    y += TogglePitch;
                }
            }
        }
    }

    public override void Update()
    {
        touchMenu.Update();
        IsOpaque = Transition == TransType.None;
        base.core.AudioManager.MusicVolumeBox.Set("character-mods", 0.3f, inWorld: false);
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
        int id = 0;
        for (int s = 0; s < sections.Count; s++)
        {
            touchMenu[id].Rectangle.Shift(0f, y);
            id++;
            for (int m = 0; m < sections[s].Mods.Count; m++)
            {
                touchMenu[id].Rectangle.Shift(0f, y);
                id++;
            }
        }
        touchMenu[backButtonId].Rectangle.Shift(0f, y);
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
        base.core.Renderer["fg", 9000, false].DrawTextS("MODIFICATIONS", menuRect.TopLeft.Shift(TitleX, TitleY + num2), new TextProfile
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
        if (openSection >= 0 && openSection < sections.Count)
        {
            SectionDef openSec = sections[openSection];
            for (int m = 0; m < openSec.Mods.Count; m++)
            {
                ModDef mod = openSec.Mods[m];
                base.core.Renderer["fg", 9000, false].DrawTextS(mod.Label, touchMenu[GetToggleId(openSection, m)].Rectangle.TopLeft.Shift(LabelDX, LabelDY), textProfile.Alter(touchMenu[GetToggleId(openSection, m)].ToggleValue ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328)));
            }
        }
        touchMenu.Draw();
        base.Draw();
    }

    private void OnToggle(int id, bool newValue)
    {
        for (int s = 0; s < sections.Count; s++)
        {
            for (int m = 0; m < sections[s].Mods.Count; m++)
            {
                if (GetToggleId(s, m) == id)
                {
                    ModDef mod = sections[s].Mods[m];
                    mod.SetValue(base.core.OptionsData, newValue);
                    base.core.SaveOptions();
                    SendMessage(new PlaySoundMessage(mod.ToggleSound));
                    return;
                }
            }
        }
    }

    private void OnButtonRelease(int id)
    {
        if (id == backButtonId)
        {
            OnBackButtonPressed();
            return;
        }
        for (int s = 0; s < sections.Count; s++)
        {
            if (GetHeaderId(s) == id)
            {
                openSection = ((openSection == s) ? (-1) : s);
                LayoutMenu();
                SendMessage(new PlaySoundMessage(SoundName.piston_retract));
                return;
            }
        }
    }

    public override void OnBackButtonPressed()
    {
        base.core.SaveOptions();
        TransitionOut(CoreEvent.PopState);
        SendMessage(new PlaySoundMessage(SoundName.trans_1), 7);
        base.OnBackButtonPressed();
    }
}