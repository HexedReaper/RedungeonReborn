using Microsoft.Xna.Framework.Graphics;

namespace Knighter.Graphics;

public class RenderTargets : Component
{
	public Texture2D DefaultRenderTarget;

	public Texture2D LightMapTarget { get; private set; }

	public Texture2D MainTarget { get; private set; }

	public Texture2D AuxTarget { get; private set; }

	public RenderTargets()
	{
		LightMapTarget = base.core.Renderer.CreateTexture(base.core.Renderer.BufferWidth, base.core.Renderer.BufferHeight);
		MainTarget = base.core.Renderer.CreateTexture(base.core.Renderer.BufferWidth, base.core.Renderer.BufferHeight, preserve: true);
		AuxTarget = base.core.Renderer.CreateTexture(base.core.Renderer.BufferWidth, base.core.Renderer.BufferHeight);
	}
}
