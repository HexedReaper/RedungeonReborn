using KnighterAndroid;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace Knighter;

public sealed class Game1 : Game
{
	private GraphicsDeviceManager graphics;

	private SpriteBatch spriteBatch;

	public GooglePlayHelper GooglePlayHelper;

	public Game1()
	{
		graphics = new GraphicsDeviceManager(this);
		base.Content.RootDirectory = "Content";
		graphics.SupportedOrientations = DisplayOrientation.Portrait;
		graphics.IsFullScreen = true;
	}

	protected override void Initialize()
    {
        // guarantee BOTH minimum virtual dims on every device:
        // width >= 177 (original rule), height >= 340 (panel 233 + button stack)
        float scaleW = (float)graphics.GraphicsDevice.Viewport.Width / 177f;
        float scaleH = Settings.GuiScale * (float)graphics.GraphicsDevice.Viewport.Height / 340f;
        Settings.PixelScale = (float)System.Math.Min(scaleW, scaleH);
        base.Initialize();
    }

	protected override void LoadContent()
	{
		spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
		Core.Initialize(this, graphics.GraphicsDevice, spriteBatch, base.Content, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
		Core.Instance.Load();
	}

	protected override void UnloadContent()
	{
		Core.Instance.Unload();
	}

	protected override void Update(GameTime gameTime)
	{
		if (base.IsActive)
		{
			TouchPanel.DisplayWidth = (int)((float)graphics.GraphicsDevice.Viewport.Width / Settings.PixelScale);
			TouchPanel.DisplayHeight = (int)((float)graphics.GraphicsDevice.Viewport.Height / Settings.PixelScale);
			Core.Instance.Update();
			base.Update(gameTime);
		}
	}

	protected override void Draw(GameTime gameTime)
	{
		if (base.IsActive)
		{
			base.GraphicsDevice.Clear(new Color(15, 15, 15));
			Core.Instance.GameTime = gameTime;
			Core.Instance.Draw();
			base.Draw(gameTime);
		}
	}

	public void OnEnteringBackground()
	{
		if (Core.Instance != null)
		{
			Core.Instance.OnEnteringBackground();
		}
	}
}
