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
        ToggleThrust,
        ToggleBraggAmmo,
        TogglePredator,
        ToggleUnfriendBats,
        ToggleHardcoreWebs,
        ToggleAchievementToasts,
        Back
    }

    private TouchMenu<Button> touchMenu;

    private RectangleF menuRect;

    private Sprite block;

    private Sprite chain;

    public CharacterModsState()
    {
        base.TransDuration = 30;
        ShowCoins = false;
        IsOverlay = true;
        menuRect = new RectangleF((float)(base.core.Renderer.ScreenWidth - 148) * 0.5f, (float)(base.core.Renderer.ScreenHeight - 233) * 0.5f, 148f, 233f);
        touchMenu = new TouchMenu<Button>(null, OnButtonRelease, "fg", 10000);
        touchMenu.OnToggle = OnToggle;
        float left = menuRect.Left + 12f;
        touchMenu.SetupToggle(Button.ToggleThrust, new Vector2(left, menuRect.Top + 86f), base.core.OptionsData.DirectionalThrust, 120);
        touchMenu.SetupToggle(Button.ToggleBraggAmmo, new Vector2(left, menuRect.Top + 108f), base.core.OptionsData.BraggAmmo, 120);
        touchMenu.SetupToggle(Button.TogglePredator, new Vector2(left, menuRect.Top + 130f), base.core.OptionsData.VampirePredator, 120);
        touchMenu.SetupToggle(Button.ToggleUnfriendBats, new Vector2(left, menuRect.Top + 150f), base.core.OptionsData.UnfriendBats, 120);
        touchMenu.SetupToggle(Button.ToggleHardcoreWebs, new Vector2(left, menuRect.Top + 172f), base.core.OptionsData.HardcoreWebs, 120);
        touchMenu.SetupToggle(Button.ToggleAchievementToasts, new Vector2(left, menuRect.Top + 192f), base.core.OptionsData.AchievementToasts, 120);
        touchMenu.SetupButton(Button.Back, new RectangleF(menuRect.Center.X - 35f, menuRect.Bottom + 10f, 70f, 30f), _(SpriteName.button_back), _(SpriteName.button_back_down));
        block = _(SpriteName.options_block);
        chain = _(SpriteName.gui_chain);
        SendMessage(new PlaySoundMessage(SoundName.trans_2));
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
        touchMenu[Button.ToggleThrust].Rectangle.Shift(0f, y);
        touchMenu[Button.ToggleBraggAmmo].Rectangle.Shift(0f, y);
        touchMenu[Button.TogglePredator].Rectangle.Shift(0f, y);
        touchMenu[Button.ToggleUnfriendBats].Rectangle.Shift(0f, y);
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
            Width = 110,
            Height = 30,
            BoxAlignment = Alignment2D.Left,
            TextAlignment = Alignment2D.LeftMiddle,
            Decoration = TextDecoration.None,
            Font = Font.Thin,
            Scale = 0.75f
        };
        base.core.Renderer["fg", 9000, false].DrawTextS("gylbard: directional thrust", touchMenu[Button.ToggleThrust].Rectangle.TopLeft.Shift(32f, -7f), textProfile.Alter(touchMenu[Button.ToggleThrust].ToggleValue ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328)));
        base.core.Renderer["fg", 9000, false].DrawTextS("bragg: ammo (3 / 10m)", touchMenu[Button.ToggleBraggAmmo].Rectangle.TopLeft.Shift(32f, -7f), textProfile.Alter(touchMenu[Button.ToggleBraggAmmo].ToggleValue ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328)));
        base.core.Renderer["fg", 9000, false].DrawTextS("vamp: predator dives", touchMenu[Button.TogglePredator].Rectangle.TopLeft.Shift(32f, -7f), textProfile.Alter(touchMenu[Button.TogglePredator].ToggleValue ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328)));
        base.core.Renderer["fg", 9000, false].DrawTextS("vamp: unfriend bats", touchMenu[Button.ToggleUnfriendBats].Rectangle.TopLeft.Shift(32f, -7f), textProfile.Alter(touchMenu[Button.ToggleUnfriendBats].ToggleValue ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328)));
        base.core.Renderer["fg", 9000, false].DrawTextS("other: hardcore webs", touchMenu[Button.ToggleHardcoreWebs].Rectangle.TopLeft.Shift(32f, -7f), textProfile.Alter(touchMenu[Button.ToggleHardcoreWebs].ToggleValue ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328)));
        base.core.Renderer["fg", 9000, false].DrawTextS("other: ach. toasts", touchMenu[Button.ToggleAchievementToasts].Rectangle.TopLeft.Shift(32f, -7f), textProfile.Alter(touchMenu[Button.ToggleAchievementToasts].ToggleValue ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328)));
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
        if (button == Button.Back)
        {
            OnBackButtonPressed();
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