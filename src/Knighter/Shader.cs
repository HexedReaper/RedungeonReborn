using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Knighter;

public class Shader : Component
{
	public Effect Effect { get; protected set; }

	public bool Loaded { get; protected set; }

	public void FetchFromInternet(string url)
	{
	}

	public virtual void InitializeUniforms()
	{
	}

	public virtual void UpdateUniforms()
	{
	}

	protected Matrix BuildDefaultWorldViewProjMatrix()
	{
		Matrix matrix = Matrix.CreateOrthographicOffCenter(0f, base.core.Renderer.ScreenWidth, base.core.Renderer.ScreenHeight, 0f, 0f, 1f);
		matrix = Matrix.CreateTranslation(-0.5f, -0.5f, 0f) * matrix;
		Matrix matrix2 = Matrix.Identity;
		Matrix.Multiply(ref matrix2, ref matrix, out var result);
		return result;
	}

	protected void LoadFromFile(string filePath)
	{
		using Stream stream = Game.Activity.Assets.Open(filePath);
		using MemoryStream memoryStream = new MemoryStream();
		stream.CopyTo(memoryStream);
		Effect = new Effect(base.core.GraphicsDevice, memoryStream.ToArray());
		InitializeUniforms();
		Loaded = true;
	}

	protected void UpdateFlipTexCoordUniform(bool value)
	{
		Effect.Parameters["FlipTexCoord"].SetValue(value ? new Vector2(1f, -1f) : new Vector2(0f, 1f));
	}
}
