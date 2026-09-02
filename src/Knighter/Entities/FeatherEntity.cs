using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class FeatherEntity : Entity
{
    private Animation anim;

    public FeatherEntity(float x, float y)
        : base(x, y, 0.3f, 0.3f)
    {
    }

    public override void Load()
    {
        anim = new Animation(0.2f);
        anim.Add("fall", "parrot_feather_", "11123332");
        anim.Play("fall");
        base.Load();
    }

    public override void Update()
    {
        anim.Update();
        y += 0.08f;
        x += Component._sin((float)base.worldTicks * 0.1f) * 0.02f;
        UpdateTiles();
        if (base.Age > 420)
        {
            SendMessage(new RemoveEntityMessage(this));
        }
        base.Update();
    }

    public override void Draw()
    {
        base.core.Renderer[base.Z + 2].DrawSpriteW(anim.GetCurrentFrame(), base.WorldCenter, Color.White * 0.9f, Vector2.One, 0f, (Component._sin((float)base.worldTicks * 0.07f) > 0f) ? SpriteFlip.Horizontal : SpriteFlip.None, SpriteOrigin.Center);
        base.Draw();
    }

    public override void CollideWith(Entity other)
    {
        if (base.Age >= 5 && other is BraggChar braggChar && !braggChar.Dead)
        {
            braggChar.AddProgress(PointsPerCoin());
            SendMessage(new PlayWorldSoundMessage(SoundName.coin, base.WorldCenter));
            SendMessage(new RemoveEntityMessage(this));
        }
        base.CollideWith(other);
    }

    private static int PointsPerCoin()
    {
        return 2;
    }
}