using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.States;

public class CharacterModsState : State
{
    private enum Button
    {
        HeaderGylbard,
        ToggleThrust,
        HeaderBragg,
        ToggleBraggAmmo,
        HeaderVampire,
        TogglePredator,
        ToggleUnfriendBats,
        ToggleFastWings,
        HeaderOther,
        ToggleHardcoreWebs,
        ToggleAchievementToasts,
        Back
    }

    // ---- layout tuned on device via UiLayoutEditor (dump 2025-09-01) ----
    private const float PanelDY = 0f;        // (hand-edit only)
    private const float TitleX = 6f;         // rel panel left
    private const float TitleY = 34f;        // rel panel top
    private const float SectionTopY = 67f;   // first header top, rel panel top
    private const float SectionPitch = 17f;  // gap between stacked rows
    private const float TogglePitch = 16f;   // gap between toggle rows inside a section (raise to 20f if taps double-flip)
    private const float HeaderDX = 8f;       // header left/right inset
    private const float ToggleDX = 12f;      // toggle left inset
    private const float LabelDX = 32f;       // toggle label offset from toggle top-left
    private const float LabelDY = -7f;
    private const float BackBtnX = 0f;       // rel panel center
    private const float BackBtnY = 8f;       // below panel bottom

    private TouchMenu<Button> touchMenu;

    private RectangleF menuRect;

    private Sprite block;

    private Sprite chain;

    private int openSection;

    public CharacterModsState()
    {
        base.TransDuration = 30;
        ShowCoins = false;
        IsOverlay = true;
        menuRect = new RectangleF((float)(base.core.Renderer.ScreenWidth - 148) * 0.5f, (float)(base.core.Renderer.ScreenHeight - 233) * 0.5f + PanelDY, 148f, 233f);
        touchMenu = new TouchMenu<Button>(null, OnButtonRelease, "fg", 10000);
        touchMenu.OnToggle = OnToggle;
        float left = menuRect.Left + ToggleDX;
        touchMenu.SetupButton(Button.HeaderGylbard, new RectangleF(menuRect.Left + HeaderDX, menuRect.Top, menuRect.Width - HeaderDX * 2f, 18f), null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Orange, "Gylbard +", null, icon: false, iconIsPicture: false);
        touchMenu.SetupToggle(Button.ToggleThrust, new Vector2(left, menuRect.Top), base.core.OptionsData.DirectionalThrust, 120);
        touchMenu.SetupButton(Button.HeaderBragg, new RectangleF(menuRect.Left + HeaderDX, menuRect.Top, menuRect.Width - HeaderDX * 2f, 18f), null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Orange, "Bragg +", null, icon: false, iconIsPicture: false);
        touchMenu.SetupToggle(Button.ToggleBraggAmmo, new Vector2(left, menuRect.Top), base.core.OptionsData.BraggAmmo, 120);
        touchMenu.SetupButton(Button.HeaderVampire, new RectangleF(menuRect.Left + HeaderDX, menuRect.Top, menuRect.Width - HeaderDX * 2f, 18f), null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Orange, "Vampire +", null, icon: false, iconIsPicture: false);
        touchMenu.SetupToggle(Button.TogglePredator, new Vector2(left, menuRect.Top), base.core.OptionsData.VampirePredator, 120);
        touchMenu.SetupToggle(Button.ToggleUnfriendBats, new Vector2(left, menuRect.Top), base.core.OptionsData.UnfriendBats, 120);
        touchMenu.SetupToggle(Button.ToggleFastWings, new Vector2(left, menuRect.Top), base.core.OptionsData.FastWings, 120);
        touchMenu.SetupButton(Button.HeaderOther, new RectangleF(menuRect.Left + HeaderDX, menuRect.Top, menuRect.Width - HeaderDX * 2f, 18f), null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Orange, "Other +", null, icon: false, iconIsPicture: false);
        touchMenu.SetupToggle(Button.ToggleHardcoreWebs, new Vector2(left, menuRect.Top), base.core.OptionsData.HardcoreWebs, 120);
        touchMenu.SetupToggle(Button.ToggleAchievementToasts, new Vector2(left, menuRect.Top), base.core.OptionsData.AchievementToasts, 120);
        if (base.core.OptionsData.DailyRunEnabled)
        {
            touchMenu[Button.ToggleThrust].Disabled = true;
            touchMenu[Button.ToggleBraggAmmo].Disabled = true;
            touchMenu[Button.TogglePredator].Disabled = true;
            touchMenu[Button.ToggleUnfriendBats].Disabled = true;
            touchMenu[Button.ToggleFastWings].Disabled = true;
            touchMenu[Button.ToggleHardcoreWebs].Disabled = true;
            touchMenu[Button.ToggleAchievementToasts].Disabled = true;
        }
        touchMenu.SetupButton(Button.Back, new RectangleF(menuRect.Center.X + BackBtnX - 35f, menuRect.Bottom + BackBtnY, 70f, 30f), _(SpriteName.button_back), _(SpriteName.button_back_down));
        block = _(SpriteName.options_block);
        chain = _(SpriteName.gui_chain);
        LayoutMenu();
        SendMessage(new PlaySoundMessage(SoundName.trans_2));
    }

    private void Place(Button button, float y)
    {
        touchMenu[button].Rectangle.Y = y;
    }

    private void LayoutMenu()
    {
        touchMenu[Button.HeaderGylbard].Label = "Gylbard " + ((openSection == 0) ? "-" : "+");
        touchMenu[Button.HeaderBragg].Label = "Bragg " + ((openSection == 1) ? "-" : "+");
        touchMenu[Button.HeaderVampire].Label = "Vampire " + ((openSection == 2) ? "-" : "+");
        touchMenu[Button.HeaderOther].Label = "Other " + ((openSection == 3) ? "-" : "+");
        touchMenu[Button.ToggleThrust].Hidden = openSection != 0;
        touchMenu[Button.ToggleBraggAmmo].Hidden = openSection != 1;
        touchMenu[Button.TogglePredator].Hidden = openSection != 2;
        touchMenu[Button.ToggleUnfriendBats].Hidden = openSection != 2;
        touchMenu[Button.ToggleFastWings].Hidden = openSection != 2;
        touchMenu[Button.ToggleHardcoreWebs].Hidden = openSection != 3;
        touchMenu[Button.ToggleAchievementToasts].Hidden = openSection != 3;
        float y = menuRect.Top + SectionTopY;
        Place(Button.HeaderGylbard, y);
        y += SectionPitch;
        if (openSection == 0)
        {
            Place(Button.ToggleThrust, y);
            y += TogglePitch;
        }
        Place(Button.HeaderBragg, y);
        y += SectionPitch;
        if (openSection == 1)
        {
            Place(Button.ToggleBraggAmmo, y);
            y += TogglePitch;
        }
        Place(Button.HeaderVampire, y);
        y += SectionPitch;
        if (openSection == 2)
        {
            Place(Button.TogglePredator, y);
            y += TogglePitch;
            Place(Button.ToggleUnfriendBats, y);
            y += TogglePitch;
            Place(Button.ToggleFastWings, y);
            y += TogglePitch;
        }
        Place(Button.HeaderOther, y);
        y += SectionPitch;
        if (openSection == 3)
        {
            Place(Button.ToggleHardcoreWebs, y);
            y += TogglePitch;
            Place(Button.ToggleAchievementToasts, y);
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
        touchMenu[Button.HeaderGylbard].Rectangle.Shift(0f, y);
        touchMenu[Button.ToggleThrust].Rectangle.Shift(0f, y);
        touchMenu[Button.HeaderBragg].Rectangle.Shift(0f, y);
        touchMenu[Button.ToggleBraggAmmo].Rectangle.Shift(0f, y);
        touchMenu[Button.HeaderVampire].Rectangle.Shift(0f, y);
        touchMenu[Button.TogglePredator].Rectangle.Shift(0f, y);
        touchMenu[Button.ToggleUnfriendBats].Rectangle.Shift(0f, y);
        touchMenu[Button.ToggleFastWings].Rectangle.Shift(0f, y);
        touchMenu[Button.HeaderOther].Rectangle.Shift(0f, y);
        touchMenu[Button.ToggleHardcoreWebs].Rectangle.Shift(0f, y);
        touchMenu[Button.ToggleAchievementToasts].Rectangle.Shift(0f, y);
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
        if (openSection == 0)
        {
            base.core.Renderer["fg", 9000, false].DrawTextS("directional thrust", touchMenu[Button.ToggleThrust].Rectangle.TopLeft.Shift(LabelDX, LabelDY), textProfile.Alter(touchMenu[Button.ToggleThrust].ToggleValue ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328)));
        }
        if (openSection == 1)
        {
            base.core.Renderer["fg", 9000, false].DrawTextS("ammo (3 / 10m)", touchMenu[Button.ToggleBraggAmmo].Rectangle.TopLeft.Shift(LabelDX, LabelDY), textProfile.Alter(touchMenu[Button.ToggleBraggAmmo].ToggleValue ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328)));
        }
        if (openSection == 2)
        {
            base.core.Renderer["fg", 9000, false].DrawTextS("predator dives", touchMenu[Button.TogglePredator].Rectangle.TopLeft.Shift(LabelDX, LabelDY), textProfile.Alter(touchMenu[Button.TogglePredator].ToggleValue ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328)));
            base.core.Renderer["fg", 9000, false].DrawTextS("unfriend bats", touchMenu[Button.ToggleUnfriendBats].Rectangle.TopLeft.Shift(LabelDX, LabelDY), textProfile.Alter(touchMenu[Button.ToggleUnfriendBats].ToggleValue ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328)));
            base.core.Renderer["fg", 9000, false].DrawTextS("fast wings x1.5", touchMenu[Button.ToggleFastWings].Rectangle.TopLeft.Shift(LabelDX, LabelDY), textProfile.Alter(touchMenu[Button.ToggleFastWings].ToggleValue ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328)));
        }
        if (openSection == 3)
        {
            base.core.Renderer["fg", 9000, false].DrawTextS("hardcore webs", touchMenu[Button.ToggleHardcoreWebs].Rectangle.TopLeft.Shift(LabelDX, LabelDY), textProfile.Alter(touchMenu[Button.ToggleHardcoreWebs].ToggleValue ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328)));
            base.core.Renderer["fg", 9000, false].DrawTextS("achievement toasts", touchMenu[Button.ToggleAchievementToasts].Rectangle.TopLeft.Shift(LabelDX, LabelDY), textProfile.Alter(touchMenu[Button.ToggleAchievementToasts].ToggleValue ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328)));
        }
        touchMenu.Draw();
        base.Draw();
    }

    private void OnToggle(Button button, bool newValue)
    {
        if (button == Button.ToggleThrust)
        {
            base.core.OptionsData.DirectionalThrust = newValue;
            base.core.SaveOptions();
            SendMessage(new PlaySoundMessage(SoundName.gylbard_sword));
        }
        if (button == Button.ToggleBraggAmmo)
        {
            base.core.OptionsData.BraggAmmo = newValue;
            base.core.SaveOptions();
            SendMessage(new PlaySoundMessage(SoundName.bragg_gun_cock));
        }
        if (button == Button.TogglePredator)
        {
            base.core.OptionsData.VampirePredator = newValue;
            base.core.SaveOptions();
            SendMessage(new PlaySoundMessage(SoundName.kazhan_turn));
        }
        if (button == Button.ToggleUnfriendBats)
        {
            base.core.OptionsData.UnfriendBats = newValue;
            base.core.SaveOptions();
            SendMessage(new PlaySoundMessage(SoundName.kazhan_flap_1));
        }
        if (button == Button.ToggleFastWings)
        {
            base.core.OptionsData.FastWings = newValue;
            base.core.SaveOptions();
            SendMessage(new PlaySoundMessage(SoundName.kazhan_flap_2));
        }
        if (button == Button.ToggleHardcoreWebs)
        {
            base.core.OptionsData.HardcoreWebs = newValue;
            base.core.SaveOptions();
            SendMessage(new PlaySoundMessage(SoundName.web_1));
        }
        if (button == Button.ToggleAchievementToasts)
        {
            base.core.OptionsData.AchievementToasts = newValue;
            base.core.SaveOptions();
            SendMessage(new PlaySoundMessage(SoundName.coin));
        }
    }

    private void OnButtonRelease(Button button)
    {
        switch (button)
        {
        case Button.HeaderGylbard:
            openSection = ((openSection == 0) ? (-1) : 0);
            LayoutMenu();
            SendMessage(new PlaySoundMessage(SoundName.piston_retract));
            break;
        case Button.HeaderBragg:
            openSection = ((openSection == 1) ? (-1) : 1);
            LayoutMenu();
            SendMessage(new PlaySoundMessage(SoundName.piston_retract));
            break;
        case Button.HeaderVampire:
            openSection = ((openSection == 2) ? (-1) : 2);
            LayoutMenu();
            SendMessage(new PlaySoundMessage(SoundName.piston_retract));
            break;
        case Button.HeaderOther:
            openSection = ((openSection == 3) ? (-1) : 3);
            LayoutMenu();
            SendMessage(new PlaySoundMessage(SoundName.piston_retract));
            break;
        case Button.Back:
            OnBackButtonPressed();
            break;
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