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
        SectionGylbard,
        ToggleThrust,
        SectionBragg,
        ToggleBraggAmmo,
        SectionVampire,
        TogglePredator,
        ToggleUnfriendBats,
        SectionOther,
        ToggleHardcoreWebs,
        ToggleAchievementToasts,
        Back
    }

    private TouchMenu<Button> touchMenu;

    private RectangleF menuRect;

    private Sprite block;

    private Sprite chain;

    private Dictionary<Button, float> rowY;

    private bool gylbardOpen;

    private bool braggOpen;

    private bool vampireOpen;

    private bool otherOpen;

    public CharacterModsState()
    {
        base.TransDuration = 30;
        ShowCoins = false;
        IsOverlay = true;
        menuRect = new RectangleF((float)(base.core.Renderer.ScreenWidth - 148) * 0.5f, (float)(base.core.Renderer.ScreenHeight - 262) * 0.5f, 148f, 233f);
        touchMenu = new TouchMenu<Button>(null, OnButtonRelease, "fg", 10000);
        touchMenu.OnToggle = OnToggle;
        rowY = new Dictionary<Button, float>();
        SetupHeader(Button.SectionGylbard, "Gylbard");
        SetupToggleRow(Button.ToggleThrust, base.core.OptionsData.DirectionalThrust);
        SetupHeader(Button.SectionBragg, "Bragg");
        SetupToggleRow(Button.ToggleBraggAmmo, base.core.OptionsData.BraggAmmo);
        SetupHeader(Button.SectionVampire, "Vampire");
        SetupToggleRow(Button.TogglePredator, base.core.OptionsData.VampirePredator);
        SetupToggleRow(Button.ToggleUnfriendBats, base.core.OptionsData.UnfriendBats);
        SetupHeader(Button.SectionOther, "Other");
        SetupToggleRow(Button.ToggleHardcoreWebs, base.core.OptionsData.HardcoreWebs);
        SetupToggleRow(Button.ToggleAchievementToasts, base.core.OptionsData.AchievementToasts);
        touchMenu.SetupButton(Button.Back, new RectangleF(menuRect.Center.X - 35f, menuRect.Bottom + 10f, 70f, 30f), _(SpriteName.button_back), _(SpriteName.button_back_down));
        block = _(SpriteName.options_block);
        chain = _(SpriteName.gui_chain);
        LayoutMenu();
        SendMessage(new PlaySoundMessage(SoundName.trans_2));
    }

    private void SetupHeader(Button button, string label)
    {
        touchMenu.SetupButton(button, new RectangleF(menuRect.Left + 8f, menuRect.Top, menuRect.Width - 16f, 18f), null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Orange, label, null, icon: true, iconIsPicture: false, blink: false, null, null, 2);
        rowY[button] = menuRect.Top;
    }

    private void SetupToggleRow(Button button, bool value)
    {
        touchMenu.SetupToggle(button, menuRect.TopLeft.Shift(12f, 0f), value, 120);
        rowY[button] = menuRect.Top;
    }

    private void PlaceRow(Button button, float y)
    {
        float dy = y - rowY[button];
        touchMenu[button].Rectangle.Shift(0f, dy);
        rowY[button] = y;
    }

    private void LayoutMenu()
    {
        float y = menuRect.Top + 86f;
        touchMenu[Button.SectionGylbard].Label = (gylbardOpen ? "Gylbard -" : "Gylbard +");
        PlaceRow(Button.SectionGylbard, y);
        y += 20f;
        touchMenu[Button.ToggleThrust].Hidden = !gylbardOpen;
        if (gylbardOpen)
        {
            PlaceRow(Button.ToggleThrust, y);
            y += 20f;
        }
        touchMenu[Button.SectionBragg].Label = (braggOpen ? "Bragg -" : "Bragg +");
        PlaceRow(Button.SectionBragg, y);
        y += 20f;
        touchMenu[Button.ToggleBraggAmmo].Hidden = !braggOpen;
        if (braggOpen)
        {
            PlaceRow(Button.ToggleBraggAmmo, y);
            y += 20f;
        }
        touchMenu[Button.SectionVampire].Label = (vampireOpen ? "Vampire -" : "Vampire +");
        PlaceRow(Button.SectionVampire, y);
        y += 20f;
        touchMenu[Button.TogglePredator].Hidden = !vampireOpen;
        touchMenu[Button.ToggleUnfriendBats].Hidden = !vampireOpen;
        if (vampireOpen)
        {
            PlaceRow(Button.TogglePredator, y);
            y += 20f;
            PlaceRow(Button.ToggleUnfriendBats, y);
            y += 20f;
        }
        touchMenu[Button.SectionOther].Label = (otherOpen ? "Other -" : "Other +");
        PlaceRow(Button.SectionOther, y);
        y += 20f;
        touchMenu[Button.ToggleHardcoreWebs].Hidden = !otherOpen;
        touchMenu[Button.ToggleAchievementToasts].Hidden = !otherOpen;
        if (otherOpen)
        {
            PlaceRow(Button.ToggleHardcoreWebs, y);
            y += 20f;
            PlaceRow(Button.ToggleAchievementToasts, y);
            y += 20f;
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
        touchMenu[Button.SectionGylbard].Rectangle.Shift(0f, y);
        touchMenu[Button.ToggleThrust].Rectangle.Shift(0f, y);
        touchMenu[Button.SectionBragg].Rectangle.Shift(0f, y);
        touchMenu[Button.ToggleBraggAmmo].Rectangle.Shift(0f, y);
        touchMenu[Button.SectionVampire].Rectangle.Shift(0f, y);
        touchMenu[Button.TogglePredator].Rectangle.Shift(0f, y);
        touchMenu[Button.ToggleUnfriendBats].Rectangle.Shift(0f, y);
        touchMenu[Button.SectionOther].Rectangle.Shift(0f, y);
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
        base.core.Renderer["fg", 9000, false].DrawTextS("MODIFICATIONS", menuRect.TopLeft.Shift(5f, 57f + num2), new TextProfile
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
            Width = 87,
            Height = 30,
            BoxAlignment = Alignment2D.Left,
            TextAlignment = Alignment2D.LeftMiddle,
            Decoration = TextDecoration.None,
            Font = Font.Thin,
            Scale = 0.75f
        };
        if (gylbardOpen)
        {
            base.core.Renderer["fg", 9000, false].DrawTextS("directional thrust", touchMenu[Button.ToggleThrust].Rectangle.TopLeft.Shift(32f, -7f), textProfile.Alter(touchMenu[Button.ToggleThrust].ToggleValue ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328)));
        }
        if (braggOpen)
        {
            base.core.Renderer["fg", 9000, false].DrawTextS("bragg ammo (3 / 10m)", touchMenu[Button.ToggleBraggAmmo].Rectangle.TopLeft.Shift(32f, -7f), textProfile.Alter(touchMenu[Button.ToggleBraggAmmo].ToggleValue ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328)));
        }
        if (vampireOpen)
        {
            base.core.Renderer["fg", 9000, false].DrawTextS("predator dives", touchMenu[Button.TogglePredator].Rectangle.TopLeft.Shift(32f, -7f), textProfile.Alter(touchMenu[Button.TogglePredator].ToggleValue ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328)));
            base.core.Renderer["fg", 9000, false].DrawTextS("unfriend bats", touchMenu[Button.ToggleUnfriendBats].Rectangle.TopLeft.Shift(32f, -7f), textProfile.Alter(touchMenu[Button.ToggleUnfriendBats].ToggleValue ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328)));
        }
        if (otherOpen)
        {
            base.core.Renderer["fg", 9000, false].DrawTextS("hardcore webs", touchMenu[Button.ToggleHardcoreWebs].Rectangle.TopLeft.Shift(32f, -7f), textProfile.Alter(touchMenu[Button.ToggleHardcoreWebs].ToggleValue ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328)));
            base.core.Renderer["fg", 9000, false].DrawTextS("achievement toasts", touchMenu[Button.ToggleAchievementToasts].Rectangle.TopLeft.Shift(32f, -7f), textProfile.Alter(touchMenu[Button.ToggleAchievementToasts].ToggleValue ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328)));
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
        case Button.SectionGylbard:
            gylbardOpen = !gylbardOpen;
            LayoutMenu();
            SendMessage(new PlaySoundMessage(SoundName.piston_retract));
            break;
        case Button.SectionBragg:
            braggOpen = !braggOpen;
            LayoutMenu();
            SendMessage(new PlaySoundMessage(SoundName.piston_retract));
            break;
        case Button.SectionVampire:
            vampireOpen = !vampireOpen;
            LayoutMenu();
            SendMessage(new PlaySoundMessage(SoundName.piston_retract));
            break;
        case Button.SectionOther:
            otherOpen = !otherOpen;
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