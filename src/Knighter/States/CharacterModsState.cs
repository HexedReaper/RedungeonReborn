using System;
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
        DirectionalThrust,
        Back
    }

    private readonly TouchMenu<Button> touchMenu;

    private RectangleF menuRect;

    private Sprite block;

    private Sprite chain;

    private int hintTimer;

    private string hint;

    private Button hintButton;

    public CharacterModsState()
    {
        base.TransDuration = 25;
        IsOverlay = true;
        ShowCoins = false;
        touchMenu = new TouchMenu<Button>(null, OnButtonRelease, "fg", 3000);
        menuRect = new RectangleF((float)(base.core.Renderer.ScreenWidth - 148) * 0.5f, (float)(base.core.Renderer.ScreenHeight - 262) * 0.5f, 148f, 233f);
        block = _(SpriteName.options_block);
        chain = _(SpriteName.gui_chain);
        int num = 35;
        int num2 = 30;
        float num3 = menuRect.Left + 17f;
        float num4 = menuRect.Bottom - 40f;
        touchMenu.SetupButton(Button.DirectionalThrust, new RectangleF(num3, num4 - 70f, num, num2), base.core.SpriteManager.GetSprite(SpriteName.button), base.core.SpriteManager.GetSprite(SpriteName.button_pressed), null, stretch: true);
        touchMenu.SetupButton(Button.Back, new RectangleF(menuRect.Center.X - 35f, menuRect.Bottom, 70f, 30f), _(SpriteName.button_back), _(SpriteName.button_back_down));
    }

    public override void UpdateTransition()
    {
        float y = (float)Tween.BackEaseOut(TransD(0, 4), -250.0, 250.0, base.TransDuration - 4);
        touchMenu[Button.DirectionalThrust].Rectangle.Shift(0f, y);
        touchMenu[Button.Back].Rectangle.Shift(0f, y);
        base.UpdateTransition();
    }

    public override void HandleInput()
    {
        if (Transition == TransType.None)
        {
            touchMenu.HandleInput();
            base.HandleInput();
        }
    }

    public override void Update()
    {
        IsOpaque = Transition == TransType.None;
        if (hintTimer > 0)
        {
            hintTimer--;
        }
        base.Update();
    }

    public override void Draw()
    {
        float num = 1f - (float)base.Trans / (float)base.TransDuration;
        base.core.Renderer["fg", 2000, false].FillScreen(Color.Black * (1f - num * num * num));
        float num2 = (float)Tween.BackEaseOut(TransD(0, 4), -250.0, 250.0, base.TransDuration - 4);
        for (int i = chain.Height; menuRect.Top + 21f + num2 - (float)i > (float)(-chain.Height); i += chain.Height)
        {
            base.core.Renderer["fg", 2000, false].DrawSpriteS(chain, new Vector2(menuRect.Left + 20f, menuRect.Top + 21f + num2 - (float)i));
            base.core.Renderer["fg", 2000, false].DrawSpriteS(chain, new Vector2(menuRect.Right - 19f - (float)chain.Width, menuRect.Top + 21f + num2 - (float)i));
        }
        base.core.Renderer["fg", 2002, false].DrawSpriteS(block, menuRect.TopLeft.Shift(0f, num2));
        TextProfile obj = new TextProfile
        {
            Decoration = TextDecoration.None,
            Color = default(Color).FromRgb(9462096),
            Scale = 0.6f,
            Width = (int)menuRect.Width,
            BoxAlignment = Alignment2D.Left,
            TextAlignment = Alignment2D.Center
        };
        TextProfile textProfile2 = obj.Alter(boxAlignment: Alignment2D.Left, textAlignment: Alignment2D.Center, scale: 0.5f, color: default(Color).FromRgb(6844288), secondColor: default(Color).FromRgb(855827), decoration: TextDecoration.None, width: (int)menuRect.Width);
        base.core.Renderer["fg", 2002, false].DrawTextS("CHARACTER MODS", menuRect.TopLeft.Shift(5f, 40f + num2 + 17), obj.Alter(null, null, null, font: Font.Bold, textAlignment: Alignment2D.Middle, width: (int)menuRect.Width - 10, height: 44, boxAlignment: null, scale: 1f));
        base.core.Renderer["fg", 2002, false].DrawTextS("directional thrust: " + (base.core.OptionsData.DirectionalThrust ? "on" : "off"), touchMenu[Button.DirectionalThrust].Rectangle.TopRight.Shift(8f, 8f), textProfile2.Alter(null, null, null, font: Font.Bold, textAlignment: Alignment2D.Left, width: (int)menuRect.Width - 55, height: null, boxAlignment: null, scale: 0.8f));
        if (hintTimer > 0)
        {
            float num4 = 1f - (float)hintTimer / 70f;
            RectangleF rectangleF2 = touchMenu[hintButton].Rectangle.Clone();
            rectangleF2.X -= 30f;
            rectangleF2.Width += 60f;
            rectangleF2.Y -= 20f + 20f * num4;
            base.core.Renderer["fg", 3010, false].DrawTextS(hint, rectangleF2.CenterTop, TextProfile.OrangeBoldText.Alter(TextProfile.OrangeLight * (1f - num4 * num4 * num4), Color.Black * (1f - num4 * num4 * num4), TextDecoration.Contour));
        }
        touchMenu.Draw();
        base.Draw();
    }

    private void OnButtonRelease(Button button)
    {
        switch (button)
        {
        case Button.DirectionalThrust:
            base.core.OptionsData.DirectionalThrust = !base.core.OptionsData.DirectionalThrust;
            UpdateLabels();
            base.core.SaveOptions();
            hint = ("directional thrust: " + (base.core.OptionsData.DirectionalThrust ? "on" : "off"));
            hintButton = Button.DirectionalThrust;
            hintTimer = 70;
            SendMessage(new PlaySoundMessage(SoundName.gylbard_sword));
            break;
        case Button.Back:
            OnBackButtonPressed();
            break;
        }
    }

    public override void Load()
    {
        UpdateLabels();
        SendMessage(new PlaySoundMessage(SoundName.trans_2));
    }

    private void UpdateLabels()
    {
        touchMenu[Button.DirectionalThrust].LabelSprite = (base.core.OptionsData.DirectionalThrust ? _(SpriteName.knight_sword_big) : _(SpriteName.knight_sword));
    }

    public override void OnBackButtonPressed()
    {
        SendMessage(new PlaySoundMessage(SoundName.trans_1), 7);
        TransitionOut(CoreEvent.PopState);
        base.OnBackButtonPressed();
    }
}