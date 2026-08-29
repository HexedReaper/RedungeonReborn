using System;
using System.Collections.Generic;
using System.Linq;
using Knighter.Entities;
using Knighter.Gameplay;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Knighter.Graphics;

public sealed class Renderer : Component
{
	private class Layer
	{
		public int Z;

		public List<DrawDesc> DrawsDesc;

		public BlendState Blend;
	}

	private class DrawDesc
	{
		public Texture2D Texture;

		public Vector2 Position;

		public Rectangle SourceRect;

		public Color Tint;

		public float Rotation;

		public Vector2 Origin;

		public Vector2 Scale;

		public SpriteEffects Flip;

		public int Depth;
	}

	private const string DefaultLayer = "default";

	public static readonly BlendState ScreenBlend;

	public Vector2 World;

	private Camera camera;

	public readonly int ScreenWidth;

	public readonly int ScreenHeight;

	public readonly int BufferWidth;

	public readonly int BufferHeight;

	public readonly int DisplayWidth;

	public readonly int DisplayHeight;

	public readonly int TopSafeArea;

	private readonly SpriteBatch spriteBatch;

	private int currentDepth;

	private string currentLayer;

	private bool currentLighting;

	private Dictionary<string, Layer> layers;

	private int layerAndDepthCounter;

	public readonly DefaultShader DefaultShader;

	public readonly OverlayShader OverlayShader;

	public readonly PostEffectShader PostEffectShader;

	public readonly PostEffectManager PostEffectManager;

	private RenderTargets renderTargets;

	private ObjectPool<DrawDesc> pool;

	public bool CustomDrawEnabled;

	private SpriteFonts spriteFonts;

	public Animation Coin;

	public Animation GreenCoin;

	public const char CoinChar = '^';

	public const char GreenCoinChar = 'º';

	private static readonly List<string> fontNames;

	private float scaleFontModifier = 7f / 12f;

	public Renderer this[string layer, int depth, bool lighting = false]
	{
		get
		{
			SetCurrentLayer(layer);
			SetCurrentDepth(depth);
			currentLighting = lighting;
			return this;
		}
	}

	public Renderer this[int depth, bool lighting]
	{
		get
		{
			SetCurrentLayer("default");
			SetCurrentDepth(depth);
			currentLighting = lighting;
			return this;
		}
	}

	public Renderer this[string layer]
	{
		get
		{
			SetCurrentLayer(layer);
			return this;
		}
	}

	public Renderer this[string layer, bool lighting]
	{
		get
		{
			SetCurrentLayer(layer);
			currentLighting = lighting;
			return this;
		}
	}

	public Renderer this[int depth]
	{
		get
		{
			SetCurrentDepth(depth);
			return this;
		}
	}

	public Renderer this[bool lighting]
	{
		get
		{
			currentLighting = lighting;
			return this;
		}
	}

	public Vector2 ScreenCenter => new Vector2((float)ScreenWidth * 0.5f, (float)ScreenHeight * 0.5f);

	public Rectangle ScreenRectangle => new Rectangle(0, 0, ScreenWidth, ScreenHeight);

	private void InitExtras()
	{
	}

	private void UpdateExtras()
	{
	}

	public void DrawPortraitExtra(Character currentCharacter, bool charUnlocked, Vector2 charCenter, CharDescription charDesc, int charLevel, int depth = 0, float anim = 1f, float cTrans = 0f)
	{
		float num = 1f - Component._m((0f - cTrans) / 40f, 1f);
		switch (currentCharacter)
		{
		case Character.Knight:
			if (charUnlocked && base.ticks % 120 < 20)
			{
				int num3 = (int)Math.Floor((float)(base.ticks % 120) / 20f * 6f) + 1;
				Sprite sprite3 = _("knight_portrait_gloss_" + num3);
				base.core.Renderer["fg", depth + 2, false].DrawSpriteS(sprite3, charCenter.Shift(1f, cTrans) - sprite3.Link, Color.White * anim * num);
			}
			break;
		case Character.Creep:
			if (charUnlocked)
			{
				int num5 = base.ticks % 190;
				if (num5 > 120)
				{
					int num6 = ((num5 <= 125 || num5 >= 185) ? 1 : 2);
					Sprite sprite5 = _("creep_morph_" + num6);
					base.core.Renderer["fg", depth + 2, false].DrawSpriteS(sprite5, charCenter.Shift(0f, cTrans) - sprite5.Link, Color.White * anim * num);
				}
			}
			break;
		case Character.Mage:
		{
			if (!charUnlocked)
			{
				break;
			}
			Sprite sprite6 = _(SpriteName.mage_portrait_glow);
			base.core.Renderer["fg", depth - 1, false].DrawSpriteS(sprite6, charCenter.Shift(0f, cTrans) - sprite6.Link, Color.White * (Component._sin((float)base.ticks * 0.09f) * 0.1f + 0.9f) * num * anim);
			for (int i = 2; i < 7; i++)
			{
				int num7 = 11 + i * 3;
				int num8 = base.ticks / num7 * 7 % 30 - 15;
				int num9 = base.ticks / num7 * 11 % 30 - 15;
				if (num8 * num8 + num9 * num9 <= 225)
				{
					float num10 = 1f - (float)(base.ticks % num7) / (float)num7;
					int num11 = base.ticks / num7 % 3;
					base.core.Renderer["fg", depth + 1, false].DrawSpriteS(_(SpriteName.pixel), charCenter.Shift(0f, cTrans).Shift(-26 + num8, (float)(-28 + num9) + 1.5f * num10), num11 switch
					{
						1 => default(Color).FromRgb(14020915), 
						0 => Color.White, 
						_ => default(Color).FromRgb(14020915), 
					} * anim, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
				}
			}
			break;
		}
		case Character.Ichitaka:
			if (charUnlocked && base.ticks % 120 < 20)
			{
				int num12 = (int)Math.Floor((float)(base.ticks % 120) / 20f * 8f) + 1;
				Sprite sprite8 = _("ichitaka_gloss_" + num12);
				base.core.Renderer["fg", depth + 2, false].DrawSpriteS(sprite8, charCenter.Shift(1f, cTrans - 1f) - sprite8.Link, Color.White * anim * num);
			}
			break;
		case Character.Vampire:
			if (charUnlocked)
			{
				Sprite sprite7 = _(SpriteName.vampire_portrait_eyes);
				base.core.Renderer["fg", depth + 2, false].DrawSpriteS(sprite7, charCenter.Shift(0f, cTrans) - sprite7.Link, Color.White * (Component._sin((float)base.ticks * 0.09f) * 0.5f + 0.5f) * 0.9f * anim * num);
			}
			break;
		case Character.Rib:
			if (charUnlocked)
			{
				Sprite sprite14 = _(SpriteName.rib_portrait_eyes);
				base.core.Renderer["fg", depth + 2, false].DrawSpriteS(sprite14, charCenter.Shift(0f, cTrans) - sprite14.Link, Color.White * (Component._sin((float)base.ticks * 0.09f) * 0.5f + 0.5f) * 0.9f * anim * num);
			}
			break;
		case Character.Vesna:
			if (charUnlocked)
			{
				int num19 = (int)((double)Component._M(0f, anim - 0.375f) * 1.6 * 4.0);
				Sprite sprite13;
				if (num19 >= 1)
				{
					sprite13 = _("vesna_portrait_grass_" + num19);
					base.core.Renderer["fg", depth + 2, false].DrawSpriteS(sprite13, charCenter.Shift(0f, 0f) - sprite13.Link, Color.White);
				}
				num19 = (int)(Component._M(0f, anim - 0.5f) * 2f * 3f) + 1;
				if (num19 >= 2)
				{
					sprite13 = _("vesna_portrait_bg_grass_" + num19);
					base.core.Renderer["fg", depth - 1, false].DrawSpriteS(sprite13, charCenter.Shift(0f, 0f) - sprite13.Link, Color.White);
				}
				sprite13 = _(SpriteName.vesna_portrait_eyes);
				base.core.Renderer["fg", depth + 2, false].DrawSpriteS(sprite13, charCenter.Shift(0f, cTrans) - sprite13.Link, Color.White * (Component._sin((float)base.ticks * 0.09f) * 0.5f + 0.5f) * 0.6f * anim * num);
				int num20 = base.ticks / 5 % 4 + 1;
				if (num20 == 4)
				{
					num20 = 2;
				}
				sprite13 = base.core.SpriteManager.GetSprite("butterfly_y_" + num20);
				base.core.Renderer["fg", depth, false].DrawSpriteS(sprite13, charCenter.Shift(-28f - 30f * (1f - anim), -37f - 10f * (1f - anim)) + new Vector2(Component._cos((float)base.ticks * 0.02f) * 2f, -3f + Component._sin((float)base.ticks * 0.05f) * 5f), null, rotation: Component._cos((float)base.ticks * 0.02f) * 0.2f, scale: new Vector2(0.9f + 0.1f * Component._sin((float)base.ticks * 0.03f)) * anim, flip: SpriteFlip.None, origin: SpriteOrigin.Center);
				num20 = (base.ticks + 30) / 4 % 4 + 1;
				if (num20 == 4)
				{
					num20 = 2;
				}
				sprite13 = base.core.SpriteManager.GetSprite("butterfly_b_" + num20);
				base.core.Renderer["fg", depth, false].DrawSpriteS(sprite13, charCenter.Shift(27f + 30f * (1f - anim), -26f - 10f * (1f - anim)) + new Vector2(Component._sin((float)base.ticks * 0.025f) * 3f, -5f + Component._cos((float)base.ticks * 0.045f) * 4f), null, rotation: Component._sin((float)base.ticks * 0.02f) * 0.2f, scale: new Vector2(0.8f + 0.1f * Component._cos((float)base.ticks * 0.03f)) * anim, flip: SpriteFlip.None, origin: SpriteOrigin.Center);
			}
			break;
		case Character.Nathan:
		{
			if (charUnlocked && base.ticks % 120 < 20)
			{
				int num13 = (int)Math.Floor((float)(base.ticks % 120) / 20f * 6f) + 1;
				Sprite sprite9 = _("nathan_gloss_" + num13);
				base.core.Renderer["fg", depth + 2, false].DrawSpriteS(sprite9, charCenter.Shift(0f, cTrans) - sprite9.Link, Color.White * anim * num);
			}
			Dictionary<Skill, int> skillLevel2 = charDesc.Levels[charLevel - 1].Abilities.SkillLevel;
			int num14 = base.ticks / 3 % 3 + 1;
			Sprite sprite10 = base.core.SpriteManager.GetSprite("nathan_portrait_drone_" + num14);
			if (skillLevel2[Skill.Drone] > 0 || skillLevel2[Skill.Drones] > 0)
			{
				base.core.Renderer["fg", depth - 1, false].DrawSpriteS(sprite10, charCenter.Shift(-36f * anim, -32f) + new Vector2(Component._cos((float)base.ticks * 0.03f) * 3f, Component._sin((float)base.ticks * 0.01f) * 5f), null, rotation: (0f - Component._cos((float)base.ticks * 0.03f + 5.340708f)) * 0.2f, scale: new Vector2(anim * 0.75f), flip: SpriteFlip.None, origin: SpriteOrigin.Center);
			}
			if (skillLevel2[Skill.Drones] > 0)
			{
				num14 = base.ticks / 3 % 3 + 1;
				sprite10 = base.core.SpriteManager.GetSprite("nathan_portrait_drone_" + num14);
				base.core.Renderer["fg", depth - 1, false].DrawSpriteS(sprite10, charCenter.Shift(32f * anim, -40f) + new Vector2(Component._cos((float)(base.ticks + 40) * 0.03f) * 3f, Component._cos((float)(base.ticks + 40) * 0.01f) * 4f), null, rotation: (0f - Component._cos((float)(base.ticks + 40) * 0.03f + 5.340708f)) * 0.2f, scale: new Vector2(anim * 0.75f), flip: SpriteFlip.None, origin: SpriteOrigin.Center);
			}
			break;
		}
		case Character.PanicBot:
			if (charUnlocked)
			{
				int num4 = (int)Math.Floor((float)base.ticks / 5f % 8f) + 1;
				Sprite sprite4 = _("panicbot_flash_" + num4);
				base.core.Renderer["fg", depth + 2, false].DrawSpriteS(sprite4, charCenter.Shift(-47f, cTrans - 1f - 68f), Color.White * anim * num);
			}
			break;
		case Character.Medusa:
			if (charUnlocked && cTrans.IsEqualTo(0f))
			{
				int num21 = base.ticks % 50 / 10 + 1;
				Sprite sprite15 = _("medusa_portrait_" + num21);
				base.core.Renderer["fg", depth, false].DrawSpriteS(sprite15, charCenter - sprite15.Link);
			}
			break;
		case Character.Golem:
			if (charUnlocked)
			{
				float num15 = 0.5f + 0.5f * Component._sin((float)base.ticks * 0.04f);
				Sprite sprite11 = _(SpriteName.golem_portrait_crack_base);
				base.core.Renderer["fg", depth - 1, false].DrawSpriteS(sprite11, charCenter.Shift(0f, -0.5f) - sprite11.Link, Color.White * anim);
				sprite11 = _(SpriteName.golem_portrait_crack_1);
				base.core.Renderer["fg", depth - 1, false].DrawSpriteS(sprite11, charCenter.Shift(0f, -0.5f) - sprite11.Link, Color.White * num15 * anim);
				sprite11 = _(SpriteName.golem_portrait_crack_2);
				base.core.Renderer["fg", depth - 1, false].DrawSpriteS(sprite11, charCenter.Shift(0f, -0.5f) - sprite11.Link, Color.White * (1f - num15) * anim);
				if (cTrans.IsEqualTo(0f))
				{
					Sprite sprite12 = _(SpriteName.golem_portrait_feet);
					base.core.Renderer["fg", depth, false].DrawSpriteS(sprite12, charCenter - sprite12.Link);
					float num16 = Component._sin((float)base.ticks * 0.03f) * anim;
					float num17 = Component._cos((float)base.ticks * 0.03f) * anim;
					float num18 = Component._cos((float)(base.ticks - 40) * 0.03f) * anim;
					sprite12 = _(SpriteName.golem_portrait_body);
					base.core.Renderer["fg", depth, false].DrawSpriteS(sprite12, charCenter.Shift(-21f, -55f + num18 * 2f));
					sprite12 = _(SpriteName.golem_portrait_hand_left);
					base.core.Renderer["fg", depth, false].DrawSpriteS(sprite12, charCenter.Shift(-33f + num16 * 2f, -27f + num17 * 2f), null, null, num16 * 0.1f);
					sprite12 = _(SpriteName.golem_portrait_hand_right);
					base.core.Renderer["fg", depth, false].DrawSpriteS(sprite12, charCenter.Shift(22f - num16 * 2f, -27f + num17 * 2f), null, null, (0f - num16) * 0.1f);
				}
			}
			break;
		case Character.Bragg:
		{
			Dictionary<Skill, int> skillLevel = charDesc.Levels[charLevel - 1].Abilities.SkillLevel;
			if (charUnlocked)
			{
				if (cTrans.IsEqualTo(0f))
				{
					string text = "111111122222232323222222";
					int index = base.ticks / 6 % text.Length;
					Sprite sprite = _("bragg_portrait_" + text[index]);
					base.core.Renderer["fg", depth, false].DrawSpriteS(sprite, charCenter - sprite.Link);
				}
				if (skillLevel[Skill.Parrot] > 0)
				{
					int num2 = base.ticks / 3 % 6 + 1;
					Sprite sprite2 = base.core.SpriteManager.GetSprite("bragg_portrait_parrot_" + num2);
					base.core.Renderer["fg", depth + ((anim < 0.7f) ? (-1) : 0), false].DrawSpriteS(sprite2, charCenter.Shift(36f * anim, -32f) + new Vector2(Component._cos((float)base.ticks * 0.03f) * 3f, Component._sin((float)base.ticks * 0.01f) * 5f), null, rotation: (0f - Component._cos((float)base.ticks * 0.03f + 5.340708f)) * 0.05f, scale: new Vector2(anim), flip: SpriteFlip.None, origin: SpriteOrigin.Center);
				}
			}
			break;
		}
		}
	}

	public void DrawWoodenPanel(RectangleF rect, float opacity = 1f)
	{
		string layer = currentLayer;
		int depth = currentDepth;
		base.core.Renderer[layer, depth, false].DrawRectangleS(rect, default(Color).FromRgb(7095611) * opacity);
		base.core.Renderer[layer, depth, false].DrawRectangleS(rect.Grow(0f, rect.Height - 2f), default(Color).FromRgb(5452592) * opacity);
		base.core.Renderer[layer, depth, false].DrawRectangleS(rect.Grow(0f, rect.Height - 2f, 0f, -1f), default(Color).FromRgb(8803911) * opacity);
		base.core.Renderer[layer, depth, false].DrawRectangleS(rect.Grow(1f, 1f, -1f, -3f), default(Color).FromRgb(5452592) * opacity);
		base.core.Renderer[layer, depth, false].DrawSpriteS(_(SpriteName.panel_corner_tl), rect.TopLeft.Shift(0f, -0.1f), Color.White * opacity);
		base.core.Renderer[layer, depth, false].DrawSpriteS(_(SpriteName.panel_corner_bl), rect.BottomLeft.Shift(0f, -8f), Color.White * opacity);
		base.core.Renderer[layer, depth, false].DrawSpriteS(_(SpriteName.panel_corner_tl), rect.TopRight.Shift(-6f, -0.1f), Color.White * opacity, null, 0f, SpriteFlip.Horizontal);
		base.core.Renderer[layer, depth, false].DrawSpriteS(_(SpriteName.panel_corner_bl), rect.BottomRight.Shift(-6f, -8f), Color.White * opacity, null, 0f, SpriteFlip.Horizontal);
	}

	static Renderer()
	{
		fontNames = new List<string> { "font_bold", "font_thin" };
		ScreenBlend = new BlendState
		{
			ColorSourceBlend = Blend.InverseDestinationColor,
			ColorDestinationBlend = Blend.One,
			ColorBlendFunction = BlendFunction.Add
		};
	}

	public Renderer(SpriteBatch spriteBatch, int bufferWidth, int bufferHeight)
	{
		this.spriteBatch = spriteBatch;
		layers = new Dictionary<string, Layer>();
		float num = 1f / Settings.PixelScale;
		ScreenWidth = (int)((float)bufferWidth * num);
		ScreenHeight = (int)((float)bufferHeight * num);
		BufferWidth = bufferWidth;
		BufferHeight = bufferHeight;
		DisplayWidth = bufferWidth;
		DisplayHeight = bufferHeight;
		World = Vector2.Zero;
		DefaultShader = new DefaultShader();
		OverlayShader = new OverlayShader();
		PostEffectShader = new PostEffectShader();
		PostEffectManager = new PostEffectManager();
		pool = new ObjectPool<DrawDesc>(5000);
	}

	public override void Load()
	{
		AddLayer("bg", -2, BlendState.AlphaBlend);
		AddLayer("default", 0, BlendState.AlphaBlend);
		AddLayer("fg", 2, BlendState.AlphaBlend);
		SetCurrentLayer("default");
		SetCurrentDepth(0);
		InitText();
		InitExtras();
		LoadText();
		DefaultShader.Load();
		OverlayShader.Load();
		PostEffectShader.Load();
		renderTargets = new RenderTargets();
	}

	public override void Unload()
	{
		UnloadText();
	}

	public void SetCamera(Camera camera)
	{
		this.camera = camera;
		ReadCamera();
	}

	public void ReadCamera()
	{
		World = -camera.Position;
	}

	public override void Update()
	{
		if (camera != null)
		{
			camera.Update();
			ReadCamera();
		}
		UpdateText();
		UpdateExtras();
		PostEffectManager.Update();
		base.Update();
	}

	public void SetCurrentDepth(int depth)
	{
		currentDepth = depth;
	}

	public void AddLayer(string name, int z, BlendState blend)
	{
		layers.Add(name, new Layer
		{
			Z = z,
			DrawsDesc = new List<DrawDesc>(),
			Blend = blend
		});
		layers = layers.OrderBy((KeyValuePair<string, Layer> x) => x.Value.Z).ToDictionary((KeyValuePair<string, Layer> x) => x.Key, (KeyValuePair<string, Layer> x) => x.Value);
	}

	public void RemoveLayer(string name)
	{
		layers.Remove(name);
	}

	public void ClearLayers()
	{
		foreach (KeyValuePair<string, Layer> layer in layers)
		{
			layer.Value.DrawsDesc.Clear();
		}
	}

	public void SetCurrentLayer(string layer)
	{
		currentLayer = layer;
	}

	private void BeginLayerAndDepth()
	{
		layerAndDepthCounter++;
	}

	private void EndLayerAndDepth()
	{
		layerAndDepthCounter--;
		if (layerAndDepthCounter == 0)
		{
			ResetLayerAndDepthAndLighting();
		}
	}

	private void ResetLayerAndDepthAndLighting()
	{
		SetCurrentLayer("default");
		SetCurrentDepth(0);
		currentLighting = false;
	}

	private void DrawLightMap()
	{
		Texture2D lightMapTarget = renderTargets.LightMapTarget;
		base.core.SpriteManager.AddOrReplaceTexture("light-map", lightMapTarget);
		Sprite sprite = new Sprite
		{
			X = 0,
			Y = 0,
			Width = lightMapTarget.Width,
			Height = lightMapTarget.Height,
			SrcWidth = lightMapTarget.Width,
			SrcHeight = lightMapTarget.Height,
			TextureName = "light-map"
		};
		base.core.Renderer["fg", 100500, false].DrawSpriteS(sprite, new Vector2(60f, 50f), null, new Vector2(0.1f));
	}

	private void CopyTextureTo(Texture2D target, Texture2D to)
	{
		SetRenderTarget(to);
		spriteBatch.Begin();
		spriteBatch.Draw(target, Vector2.Zero);
		spriteBatch.End();
	}

	private void DrawLayerWithOverlayShaderTo(string layer, Texture2D target, Texture2D bg)
	{
		SetRenderTarget(target);
		OverlayShader.Destination = bg;
		OverlayShader.UpdateUniforms();
		InternalDraw(OverlayShader.Effect, layer);
	}

	private void CustomDraw()
	{
		Texture2D mainTarget = renderTargets.MainTarget;
		Texture2D auxTarget = renderTargets.AuxTarget;
		SetRenderTarget(renderTargets.MainTarget);
		InternalDraw(null, "bg");
		CopyTextureTo(mainTarget, auxTarget);
		DrawLayerWithOverlayShaderTo("bg_overlay", mainTarget, auxTarget);
		SetRenderTarget(renderTargets.MainTarget);
		InternalDraw(null, "default");
		CopyTextureTo(mainTarget, auxTarget);
		DrawLayerWithOverlayShaderTo("overlay", mainTarget, auxTarget);
		CopyTextureTo(mainTarget, auxTarget);
		DrawLayerWithOverlayShaderTo("fg_overlay", mainTarget, auxTarget);
		CopyTextureTo(mainTarget, null);
	}

	public override void Draw()
	{
		if (!CustomDrawEnabled)
		{
			if (Settings.UseCustomShaders)
			{
				BuildLightMap();
				SetRenderTarget(PostEffectManager.Enabled ? renderTargets.AuxTarget : renderTargets.DefaultRenderTarget);
				DefaultShader.LightMap = renderTargets.LightMapTarget;
				DefaultShader.UpdateUniforms();
				InternalDraw(DefaultShader.Effect, "fg", excludeIt: true);
				if (PostEffectManager.Enabled)
				{
					DrawPostEffects();
				}
				InternalDraw(DefaultShader.Effect, "fg");
			}
			else
			{
				SetRenderTarget(renderTargets.DefaultRenderTarget);
				InternalDraw(DefaultShader.Effect, "fg", excludeIt: true);
				InternalDraw(DefaultShader.Effect, "fg");
			}
		}
		else
		{
			CustomDraw();
		}
		base.core.GraphicsDevice.SetRenderTarget(null);
		ClearLayers();
		pool.Clear();
		base.Draw();
	}

	private void DrawPostEffects()
	{
		foreach (PostEffectType value in Enum.GetValues(typeof(PostEffectType)))
		{
			if (PostEffectManager.EnabledEffects.Contains(value))
			{
				SetRenderTarget(renderTargets.MainTarget);
				base.core.GraphicsDevice.Clear(Color.Black);
				PostEffectShader.SetPostEffect(value);
				PostEffectShader.Image = renderTargets.AuxTarget;
				PostEffectShader.UpdateUniforms();
				InternalDrawScreenPixel(PostEffectShader.Effect);
				CopyTextureTo(renderTargets.MainTarget, renderTargets.AuxTarget);
			}
		}
		CopyTextureTo(renderTargets.MainTarget, renderTargets.DefaultRenderTarget);
	}

	private void BuildLightMap()
	{
		if (base.core.CurrentPlayState == null)
		{
			return;
		}
		List<Light> lights = base.core.CurrentPlayState.LightManager.Lights;
		SetRenderTarget(renderTargets.LightMapTarget);
		Sprite sprite = base.core.SpriteManager.GetSprite(SpriteName.light_glow);
		Texture2D texture = base.core.SpriteManager.GetTexture(sprite.TextureName);
		Matrix value = Matrix.CreateScale(new Vector3(Settings.PixelScale, Settings.PixelScale, 1f));
		base.core.GraphicsDevice.Clear(Color.Black);
		spriteBatch.Begin(SpriteSortMode.Deferred, samplerState: SamplerState.LinearClamp, blendState: ScreenBlend, depthStencilState: null, rasterizerState: null, effect: null, transformMatrix: value);
		Rectangle screenRectangle = ScreenRectangle;
		int num = 0;
		foreach (Light item in lights)
		{
			if (!((double)item.Intencity < 0.01) && item.Active)
			{
				float num2 = 2f * (item.InWorld ? (item.ActualRadius * base.core.CurrentPlayState.Camera.Zoom) : item.ActualRadius) / (float)sprite.Width;
				float num3 = num2 * (float)sprite.Width;
				Vector2 position = (item.InWorld ? base.core.Renderer.ToScreen(item.ActualPosition) : item.ActualPosition) - new Vector2(0.5f * num3);
				if (new Rectangle((int)position.X, (int)position.Y, (int)num3, (int)num3).Intersects(screenRectangle))
				{
					num++;
					spriteBatch.Draw(texture, position, new Rectangle(sprite.X, sprite.Y, sprite.SrcWidth, sprite.SrcHeight), item.Color * item.Intencity * 0.9f, 0f, Vector2.Zero, num2, SpriteEffects.None, 0f);
				}
			}
		}
		spriteBatch.End();
	}

	private void InternalDrawScreenPixel(Effect effect)
	{
		Sprite pixel = base.core.SpriteManager.Pixel;
		spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, effect);
		spriteBatch.Draw(base.core.SpriteManager.GetTexture(pixel.TextureName), sourceRectangle: new Rectangle(pixel.X, pixel.Y, pixel.Width, pixel.Height), position: Vector2.Zero, destinationRectangle: null, origin: null, rotation: 0f, scale: new Vector2(BufferWidth, BufferHeight));
		spriteBatch.End();
	}

	private void InternalDraw(Effect effect, string onlyThisLayer, bool excludeIt = false)
	{
		foreach (KeyValuePair<string, Layer> layer in layers)
		{
			string key = layer.Key;
			Layer value = layer.Value;
			if ((onlyThisLayer != string.Empty && ((onlyThisLayer != key && !excludeIt) || ((onlyThisLayer == key) & excludeIt))) || value.DrawsDesc.Count == 0)
			{
				continue;
			}
			Begin(value.Blend, effect);
			foreach (DrawDesc item in value.DrawsDesc.OrderBy((DrawDesc dd) => dd.Depth))
			{
				spriteBatch.Draw(item.Texture, item.Position, item.SourceRect, item.Tint, item.Rotation, item.Origin, item.Scale, item.Flip, 0f);
			}
			End();
		}
	}

	public void Begin(BlendState blendState, Effect effect)
	{
		Matrix value = Matrix.CreateScale(new Vector3(Settings.PixelScale, Settings.PixelScale, 1f));
		spriteBatch.Begin(SpriteSortMode.Deferred, blendState, SamplerState.PointClamp, null, null, effect, value);
	}

	public void End()
	{
		spriteBatch.End();
	}

	public float DrawSimpleTextW(string text, Vector2 position, Color tint, float scale = 1f)
	{
		return DrawSimpleTextS(text, ToScreen(position), tint, scale * camera.Zoom);
	}

	public void DrawSpriteW(Sprite sprite, Vector2 position, Color? tint = null, Vector2? scale = null, float rotation = 0f, SpriteFlip flip = SpriteFlip.None, SpriteOrigin origin = SpriteOrigin.TopLeft)
	{
		DrawSpriteS(sprite, ToScreen(position), tint, (scale ?? Vector2.One) * camera.Zoom, rotation, flip, origin);
	}

	public void DrawSpriteTiledW(Sprite tile, Rectangle box, bool tileHorizontally = true, bool tileVertically = true, Vector2? tileOffset = null)
	{
		int width = tile.Width;
		int height = tile.Height;
		Vector2 obj = tileOffset ?? Vector2.Zero;
		string layer = currentLayer;
		int depth = currentDepth;
		float num = Math.Abs(obj.X) % (float)width;
		if (obj.X < 0f)
		{
			num = (float)width - num;
		}
		float num2 = Math.Abs(obj.Y) % (float)height;
		if (obj.Y < 0f)
		{
			num2 = (float)height - num2;
		}
		float num3 = (float)box.Left - num;
		do
		{
			float num4 = (float)box.Top - num2;
			do
			{
				Sprite sprite = tile.Reduce((int)Math.Max((float)box.Left - num3, 0f), (int)Math.Max((float)box.Top - num4, 0f), (int)Math.Max(num3 + (float)width - (float)box.Right, 0f), (int)Math.Max(num4 + (float)height - (float)box.Bottom, 0f));
				Vector2 position = new Vector2(Math.Max(num3, box.Left), Math.Max(num4, box.Top));
				base.core.Renderer[layer, depth, false].DrawSpriteW(sprite, position);
				num4 += (float)height;
			}
			while (tileVertically && num4 < (float)box.Bottom);
			num3 += (float)width;
		}
		while (tileHorizontally && num3 < (float)box.Right);
	}

	public void DrawRectangleW(float x, float y, float width, float height, Color color)
	{
		DrawRectangleS(ToScreen(new Vector2(x, y)), width * camera.Zoom, height * camera.Zoom, color);
	}

	public void DrawRectangleW(Vector2 position, float width, float height, Color color)
	{
		DrawRectangleS(ToScreen(position), width * camera.Zoom, height * camera.Zoom, color);
	}

	public void DrawRectangleW(RectangleF rect, Color color)
	{
		DrawRectangleS(ToScreen(new Vector2(rect.X, rect.Y)), rect.Width * camera.Zoom, rect.Height * camera.Zoom, color);
	}

	public void DrawDotW(float x, float y, Color color, float radius = 2f)
	{
		DrawRectangleW(new RectangleF(x - radius, y - radius, radius * 2f, radius * 2f), color);
	}

	public void DrawDotW(Vector2 center, Color color, float radius = 2f)
	{
		DrawRectangleW(new RectangleF(center.X - radius, center.Y - radius, radius * 2f, radius * 2f), color);
	}

	public void DrawLineW(Vector2 from, Vector2 to, Color color, float thickness = 1f)
	{
		DrawLineS(ToScreen(from), ToScreen(to), color, thickness * camera.Zoom);
	}

	public void DrawSpriteW(string spriteName, Vector2 position, Color? tint = null, Vector2? scale = null, float rotation = 0f, SpriteFlip flip = SpriteFlip.None, SpriteOrigin origin = SpriteOrigin.TopLeft)
	{
		DrawSpriteS(base.core.SpriteManager.GetSprite(spriteName), ToScreen(position), tint, (scale ?? Vector2.One) * camera.Zoom, rotation, flip, origin);
	}

	public void DrawSpriteS(string spriteName, Vector2 position, Color? tint = null, Vector2? scale = null, float rotation = 0f, SpriteFlip flip = SpriteFlip.None, SpriteOrigin origin = SpriteOrigin.TopLeft)
	{
		Sprite sprite = base.core.SpriteManager.GetSprite(spriteName);
		DrawSpriteS(sprite, position, tint ?? Color.White, scale, rotation, flip, origin);
	}

	public void DrawSpriteS(Sprite sprite, Vector2 position, Color? tint = null, Vector2? scale = null, float rotation = 0f, SpriteFlip flip = SpriteFlip.None, SpriteOrigin origin = SpriteOrigin.TopLeft)
	{
		Vector2 scale2 = scale ?? new Vector2(1f);
		Vector2 offset = sprite.GetOffset(flip);
		offset.X *= scale2.X;
		offset.Y *= scale2.Y;
		Vector2 zero = Vector2.Zero;
		if ((origin & (SpriteOrigin.TopCenter | SpriteOrigin.Center | SpriteOrigin.BottomCenter)) != 0)
		{
			zero.X = (float)sprite.Width * 0.5f - (float)sprite.OffXL;
			offset.X = 0f;
		}
		if ((origin & (SpriteOrigin.CenterLeft | SpriteOrigin.Center | SpriteOrigin.CenterRight)) != 0)
		{
			zero.Y = (float)sprite.Height * 0.5f - (float)sprite.OffYT;
			offset.Y = 0f;
		}
		if ((origin & (SpriteOrigin.TopRight | SpriteOrigin.CenterRight | SpriteOrigin.BottomRight)) != 0)
		{
			zero.X = sprite.Width - sprite.OffXL + sprite.OffXR;
		}
		if ((origin & (SpriteOrigin.BottomLeft | SpriteOrigin.BottomCenter | SpriteOrigin.BottomRight)) != 0)
		{
			zero.Y = sprite.Height - sprite.OffYT + sprite.OffYB;
		}
		InternalDrawSprite(base.core.SpriteManager.GetTexture(sprite.TextureName), position + offset, new Rectangle(sprite.X, sprite.Y, sprite.SrcWidth, sprite.SrcHeight), tint ?? Color.White, rotation, zero, scale2, (SpriteEffects)flip);
	}

	public void DrawLineS(Vector2 from, Vector2 to, Color color, float thickness = 1f)
	{
		if (from.X > to.X)
		{
			Vector2 vector = from;
			from = to;
			to = vector;
		}
		Vector2 vector2 = to - from;
		float x = vector2.Length();
		Sprite pixel = base.core.SpriteManager.Pixel;
		InternalDrawSprite(base.core.SpriteManager.GetTexture(pixel.TextureName), from, new Rectangle(pixel.X, pixel.Y, pixel.Width, pixel.Height), color, (float)Math.Atan(vector2.Y / vector2.X), new Vector2(0f, thickness * 0.5f), new Vector2(x, thickness), SpriteEffects.None);
	}

	public void DrawRectangleS(Vector2 position, float width, float height, Color color)
	{
		Sprite pixel = base.core.SpriteManager.Pixel;
		InternalDrawSprite(base.core.SpriteManager.GetTexture(pixel.TextureName), position, new Rectangle(pixel.X, pixel.Y, pixel.Width, pixel.Height), color, 0f, Vector2.Zero, new Vector2(width, height), SpriteEffects.None);
	}

	public void DrawRectangleS(RectangleF rect, Color color)
	{
		Sprite pixel = base.core.SpriteManager.Pixel;
		InternalDrawSprite(base.core.SpriteManager.GetTexture(pixel.TextureName), new Vector2(rect.X, rect.Y), new Rectangle(pixel.X, pixel.Y, pixel.Width, pixel.Height), color, 0f, Vector2.Zero, new Vector2(rect.Width, rect.Height), SpriteEffects.None);
	}

	public void DrawDotS(float x, float y, Color color, float radius = 2f)
	{
		DrawRectangleS(new RectangleF(x - radius, y - radius, radius * 2f, radius * 2f), color);
	}

	public void DrawDotS(Vector2 center, Color color, float radius = 2f)
	{
		DrawRectangleS(new RectangleF(center.X - radius, center.Y - radius, radius * 2f, radius * 2f), color);
	}

	public float SimpleTextWidth(string text, float scale = 1f)
	{
		return (float)(text.Length * 6) * scale;
	}

	public float DrawSimpleTextS(string text, Vector2 position, Color tint, float scale = 1f)
	{
		string text2 = currentLayer;
		int num = currentDepth;
		for (int i = 0; i < text.Length; i++)
		{
			SetCurrentLayer(text2);
			SetCurrentDepth(num);
			DrawSpriteS(base.core.SpriteManager.MakeCharSprite(text[i]), new Vector2(position.X + (float)i * scale * 6f, position.Y), tint, new Vector2(scale));
		}
		return (float)(text.Length * 6) * scale;
	}

	public void FillScreen(Color color)
	{
		DrawRectangleS(new Vector2(-1f), ScreenWidth + 2, ScreenHeight + 2, color);
	}

	private Vector2 LightHack()
	{
		if (!Settings.UseCustomShaders || !currentLighting)
		{
			return Vector2.Zero;
		}
		return new Vector2(100500f, 0f);
	}

	private void InternalDrawSprite(Texture2D texture, Vector2 position, Rectangle sourceRect, Color tint, float rotation, Vector2 origin, Vector2 scale, SpriteEffects flip)
	{
		float num = (rotation.IsZero() ? 1f : Component._M((float)sourceRect.Width * scale.X, (float)sourceRect.Height * scale.Y));
		float num2 = position.X - origin.X * scale.X;
		float num3 = position.Y - origin.Y * scale.Y;
		if (num2 < (float)ScreenWidth + num && num3 < (float)ScreenHeight + num && num2 + (float)sourceRect.Width * scale.X >= 0f - num && num3 + (float)sourceRect.Height * scale.Y >= 0f - num)
		{
			DrawDesc drawDesc = pool.Get();
			drawDesc.Texture = texture;
			drawDesc.Position = position + LightHack();
			drawDesc.SourceRect = sourceRect;
			drawDesc.Tint = tint;
			drawDesc.Rotation = rotation;
			drawDesc.Origin = origin;
			drawDesc.Scale = scale;
			drawDesc.Flip = flip;
			drawDesc.Depth = currentDepth;
			layers[currentLayer].DrawsDesc.Add(drawDesc);
		}
		if (layerAndDepthCounter == 0)
		{
			ResetLayerAndDepthAndLighting();
		}
	}

	public Vector2 ToScreen(Vector2 position)
	{
		return (position + World) * camera.Zoom;
	}

	public Vector2 ToWorld(Vector2 position)
	{
		return position / camera.Zoom - World;
	}

	public Texture2D DrawToTexture()
	{
		Texture2D texture2D = CreateTexture(DisplayWidth, DisplayHeight);
		renderTargets.DefaultRenderTarget = texture2D;
		Draw();
		renderTargets.DefaultRenderTarget = null;
		return texture2D;
	}

	public void DrawToTexture(Texture2D texture)
	{
		renderTargets.DefaultRenderTarget = texture;
		Draw();
		renderTargets.DefaultRenderTarget = null;
	}

	public Texture2D CreateTexture(int width, int height, bool preserve = false)
	{
		if (preserve)
		{
			return new RenderTarget2D(base.core.GraphicsDevice, width, height, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		}
		return new RenderTarget2D(base.core.GraphicsDevice, width, height);
	}

	private void SetRenderTarget(Texture2D renderTarget)
	{
		base.core.GraphicsDevice.SetRenderTarget((RenderTarget2D)renderTarget);
	}

	public float DrawPathForMedusaIntoTexture(Texture2D renderTarget, List<Vector2> points, int startIndex, Vector2 offset, bool clearTarget, float startR)
	{
		SetRenderTarget(renderTarget);
		if (clearTarget)
		{
			base.core.GraphicsDevice.Clear(Color.Transparent);
		}
		if (startIndex >= points.Count)
		{
			SetRenderTarget(null);
			return startR;
		}
		Sprite sprite = _(SpriteName.circle_15);
		Rectangle value = new Rectangle(sprite.X, sprite.Y, sprite.SrcWidth, sprite.SrcHeight);
		List<Vector2> list = new List<Vector2>();
		float num = startR;
		spriteBatch.Begin(SpriteSortMode.Deferred, samplerState: SamplerState.LinearClamp, blendState: BlendState.AlphaBlend, depthStencilState: null, rasterizerState: null, effect: null, transformMatrix: Matrix.Identity);
		for (int i = startIndex; i < points.Count - 1; i++)
		{
			Vector2 vector = points[i];
			Vector2 vector2 = points[i + 1];
			Vector2 vector3 = vector;
			Vector2 vector4 = vector2 - vector;
			list.Clear();
			float num2 = Math.Abs(vector2.X - vector.X) + Math.Abs(vector2.Y - vector.Y);
			float num3 = 1f / num2;
			for (float num4 = 0f; num4 < 1f - num3; num4 += num3)
			{
				Vector2 item = vector3 + num4 * vector4;
				list.Add(item);
			}
			float num5 = ((num2 < 5f) ? 0f : ((!(num2 > 20f)) ? ((num2 - 5f) / 15f) : 1f));
			float num6 = 0.5f + 0.5f * (1f - num5);
			foreach (Vector2 item2 in list)
			{
				num += (num6 - num) * 0.1f;
				spriteBatch.Draw(base.core.SpriteManager.GetTexture(sprite.TextureName), item2 - offset - new Vector2(sprite.SrcWidth, sprite.SrcHeight) * 0.5f * num, value, Color.White, 0f, Vector2.Zero, new Vector2(num), SpriteEffects.None, 0f);
			}
		}
		spriteBatch.End();
		SetRenderTarget(null);
		return num;
	}

	private string FontToFontName(Font font)
	{
		if (Locale.UsesExternalFont(base.core.LocaleManager.CurrentLocale))
		{
			return $"font_{base.core.LocaleManager.CurrentLocale.ToString()}";
		}
		return fontNames[(int)font];
	}

	private void InitText()
	{
		spriteFonts = new SpriteFonts();
	}

	private void LoadText()
	{
		spriteFonts.Load();
		Coin = new Animation(0.25f);
		Coin.Add("spin", "coin_gold_", "123456");
		Coin.Play("spin");
		GreenCoin = new Animation(0.25f);
		GreenCoin.Add("spin", "coin_green_", "123456");
		GreenCoin.Play("spin");
	}

	private void UnloadText()
	{
		spriteFonts.Unload();
	}

	private void UpdateText()
	{
		Coin.Update();
		GreenCoin.Update();
	}

	private List<string> SplitTextByLines(string text, TextProfile profile)
	{
		List<string> list = new List<string>();
		SpriteFont spriteFont = spriteFonts[FontToFontName(profile.Font)];
		string[] array = text.Split('\n');
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Replace("-", "-¬").Split(' ', '¬');
			string text2 = string.Empty;
			for (int j = 0; j < array2.Length; j++)
			{
				string text3 = text2 + array2[j] + ((array2[j].Length > 0 && array2[j][array2[j].Length - 1] == '-') ? "" : " ");
				if (ComputeTextWidth(text3, spriteFont, profile.Scale) > (float)profile.Width && j > 0)
				{
					list.Add(text2);
					text2 = array2[j] + ((array2[j].Length > 0 && array2[j][array2[j].Length - 1] == '-') ? "" : " ");
				}
				else
				{
					text2 = text3;
				}
			}
			list.Add(text2);
		}
		return list;
	}

	public RectangleF DrawTextW(string text, Vector2 position, TextProfile profile)
	{
		return DrawTextS(text, ToScreen(position), profile);
	}

	public RectangleF DrawTextS(string text, Vector2 position, TextProfile profile)
	{
		float num = profile.Scale;
		if (Locale.UsesExternalFont(base.core.LocaleManager.CurrentLocale))
		{
			num *= 7f / 12f;
		}
		SpriteFont spriteFont = spriteFonts[FontToFontName(profile.Font)];
		List<string> list = SplitTextByLines(text, profile);
		float num2 = (float)spriteFont.LineHeight * num;
		float num3 = num2 * (float)list.Count;
		RectangleF rectangleF = new RectangleF(position.X, position.Y, profile.Width, profile.Height);
		switch (profile.BoxAlignment.Horizontal)
		{
		case Alignment.Center:
			rectangleF.X -= rectangleF.Width * 0.5f;
			break;
		case Alignment.Max:
			rectangleF.X -= rectangleF.Width;
			break;
		}
		switch (profile.BoxAlignment.Vertical)
		{
		case Alignment.Center:
			rectangleF.Y -= rectangleF.Height * 0.5f;
			break;
		case Alignment.Max:
			rectangleF.Y -= rectangleF.Height;
			break;
		}
		float num4 = 0f;
		switch (profile.TextAlignment.Vertical)
		{
		case Alignment.Center:
			num4 = rectangleF.Center.Y - num3 * 0.5f;
			break;
		case Alignment.Max:
			num4 = rectangleF.Bottom - num3;
			break;
		case Alignment.Min:
			num4 = rectangleF.Top;
			break;
		}
		float num5 = 0f;
		float y = num4;
		float num6 = SciHelper.BigFloat;
		BeginLayerAndDepth();
		foreach (string item in list)
		{
			float num7 = ComputeTextWidth(item, spriteFont, num);
			num5 = Math.Max(num5, num7);
			float num8 = 0f;
			switch (profile.TextAlignment.Horizontal)
			{
			case Alignment.Center:
				num8 = rectangleF.Center.X - num7 * 0.5f + 4f * num;
				break;
			case Alignment.Max:
				num8 = rectangleF.Right - num7;
				break;
			case Alignment.Min:
				num8 = rectangleF.Left;
				break;
			}
			num6 = Math.Min(num6, num8);
			InternalDrawTextS(item, new Vector2(num8, num4), spriteFont, profile.Color, profile.SecondColor, profile.Decoration, num);
			num4 += num2;
		}
		EndLayerAndDepth();
		return new RectangleF(num6, y, num5, num3);
	}

	public float DrawTextS(string text, Vector2 position, Color tint, bool drawShadow = true, float scale = 1f)
	{
		SpriteFont spriteFont = spriteFonts[FontToFontName(Font.Bold)];
		TextDecoration decoration = (drawShadow ? TextDecoration.Contour : TextDecoration.None);
		return InternalDrawTextS(text, position, spriteFont, tint, Color.Black, decoration, scale);
	}

	private float InternalDrawTextS(string text, Vector2 position, SpriteFont spriteFont, Color color, Color? secondColor, TextDecoration decoration, float scale)
	{
		int num = currentDepth;
		float x = position.X;
		float num2 = position.Y + (float)spriteFont.LineHeight * scale;
		float num3 = x;
		float num4 = num2;
		if (!secondColor.HasValue)
		{
			secondColor = color;
		}
		BeginLayerAndDepth();
		for (int i = 0; i < text.Length; i++)
		{
			switch (text[i])
			{
			case ' ':
			case '\u00a0':
				num3 += (float)spriteFont.SpaceWidth * scale;
				continue;
			case '\u2009':
				num3 += (float)spriteFont.SpaceWidth * 0.5f * scale;
				continue;
			case '^':
				base.core.Renderer.DrawSpriteS(Coin.GetCurrentFrame(), new Vector2(num3 + 1f, num4 + ((float)(-spriteFont.LineHeight - Coin.GetCurrentFrame().Height) * 0.5f + 2.5f) * scale) - new Vector2(scale), Color.White * ((float)(int)color.A / 255f), new Vector2(scale));
				num3 += (float)(Coin.GetCurrentFrame().Width + 2) * scale;
				continue;
			case 'º':
				base.core.Renderer.DrawSpriteS(GreenCoin.GetCurrentFrame(), new Vector2(num3 + 1f, num4 + ((float)(-spriteFont.LineHeight - GreenCoin.GetCurrentFrame().Height) * 0.5f + 2.5f) * scale) - new Vector2(scale), Color.White * ((float)(int)color.A / 255f), new Vector2(scale));
				num3 += (float)(GreenCoin.GetCurrentFrame().Width + 2) * scale;
				continue;
			}
			SpriteGlyph spriteGlyph = spriteFont[text[i]];
			if (i > 0)
			{
				num3 += scale * (float)spriteGlyph.GetKerningFor(text[i - 1]);
			}
			float x2 = num3 + (float)spriteGlyph.Offset * scale;
			float y = num4 - (float)spriteGlyph.Base * scale;
			SetCurrentDepth(num);
			if (decoration == TextDecoration.Extrude1 || decoration == TextDecoration.Extrude2 || decoration == TextDecoration.ExtrudeUp1 || decoration == TextDecoration.ExtrudeUp2)
			{
				for (int j = 0; j < ((decoration == TextDecoration.Extrude1 || decoration == TextDecoration.ExtrudeUp1) ? 1 : 2); j++)
				{
					base.core.Renderer.DrawSpriteS(spriteGlyph.Sprite, new Vector2(x2, y) + new Vector2(0f, (float)((decoration != TextDecoration.ExtrudeUp1 && decoration != TextDecoration.ExtrudeUp2) ? 1 : (-1)) * scale * (float)(j + 1)), secondColor.Value, new Vector2(scale));
				}
			}
			SetCurrentDepth(num + 1);
			base.core.Renderer.DrawSpriteS(spriteGlyph.Sprite, new Vector2(x2, y), color, new Vector2(scale));
			num3 += scale * (float)(spriteGlyph.Sprite.Width + 1);
		}
		SetCurrentDepth(num);
		EndLayerAndDepth();
		return num3 - position.X;
	}

	private float ComputeTextWidth(string text, SpriteFont spriteFont, float scale = 1f)
	{
		float num = 0f;
		for (int i = 0; i < text.Length; i++)
		{
			switch (text[i])
			{
			case ' ':
				num += (float)spriteFont.SpaceWidth * scale;
				continue;
			case '\u00a0':
				num += (float)spriteFont.SpaceWidth * scale;
				continue;
			case '\u2009':
				num += (float)spriteFont.SpaceWidth * 0.5f * scale;
				continue;
			case '^':
				num += (float)(Coin.GetCurrentFrame().Width + 2) * scale;
				continue;
			}
			SpriteGlyph spriteGlyph = spriteFont[text[i]];
			if (i > 0)
			{
				num += scale * (float)spriteGlyph.GetKerningFor(text[i - 1]);
			}
			num += scale * (float)(spriteGlyph.Sprite.Width + 1);
		}
		return num;
	}

	public bool CanDrawText(string text)
	{
		SpriteFont spriteFont = spriteFonts[FontToFontName(Font.Bold)];
		foreach (char c in text)
		{
			if (c != ' ' && c != '\u00a0' && c != '\u2009' && !spriteFont.HasSpriteGlyphFor(c))
			{
				return false;
			}
		}
		return true;
	}
}
