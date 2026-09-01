using System;
using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class BatEntity : Entity
{
	public static BagOf<SoundName> Squeaks;

	private Animation animation;

	private Vector2 spawn;

	private int delay;

	private int xR;

	private int yR;

	private int xD;

	private int yD;

	private int xS;

	private int yS;

	private int fleeDelay = 70;

	private int fleeTimeout = 200;
	
	private bool avoidPlayer;

    private bool unfriended;
	private float avoid;

	private float avoidTarget;
	private bool hunting;
	private bool scatterFlee;
	private int scatterT;
    private float scatterX;

    private Vector2 home;

    private const float HuntRadiusSq = 9f;   // aggro within 3 tiles
    private const float LeashSq = 16f;       // gives up beyond 4 tiles from home
    private const float HuntSpeed = 0.03f;   // anchor drift per tick

	private ParticleEmitter loveEmitter;

	private int idleSoundDelay;

	public bool Moving;

	public bool Fleeing { get; private set; }

	static BatEntity()
	{
		Squeaks = new BagOf<SoundName>().Put(SoundName.bat_squeak_1).Put(SoundName.bat_squeak_2).Put(SoundName.bat_squeak_3);
	}

	public BatEntity(int x, int y, TileDesc desc)
		: base((float)x + 0.4f, (float)y + 0.4f, 0.2f, 0.2f)
	{
		Moving = desc != null;
		if (Moving)
		{
			Init((float)x + 0.4f, (float)y + 0.4f, desc["delay"], desc["x-r"], desc["y-r"], desc["x-d"], desc["y-d"], desc["x-s"], desc["y-s"], desc.Flipped);
		}
	}

	private void Init(float x, float y, int delay, int xR, int yR, int xD, int yD, int xS, int yS, bool flipped = false)
	{
		this.xR = xR;
		this.yR = yR;
		this.delay = delay;
		this.xD = (flipped ? (-xD) : xD);
		this.yD = yD;
		this.xS = xS;
		this.yS = yS;
	}

	private void RefreshIdleSound()
	{
	}

	public override void Load()
	{
		animation = new Animation(0.15f);
		animation.AddAndPlay("fly", new List<SpriteName>
		{
			SpriteName.bat_1,
			SpriteName.bat_2,
			SpriteName.bat_3,
			SpriteName.bat_4
		});
		animation.SkipToRandomFrame();
		spawn = new Vector2(x, y);
		home = spawn;
		avoid = 0f;
		avoidTarget = 0f;
		Dictionary<Skill, int> skillLevel = CharDescription.Get[base.core.ProfileData.Character].Levels[base.core.ProfileData.CurrentCharLevel - 1].Abilities.SkillLevel;
		avoidPlayer = skillLevel.ContainsKey(Skill.BatFriend) && skillLevel[Skill.BatFriend] > 0;
		if (avoidPlayer)
		{
			loveEmitter = base.core.ParticleManager.AddEmitter(inWorld: true, base.WorldCenter, 4f).AttachTo(this);
			loveEmitter.OnSpawn(delegate(Particle p)
			{
				p.Position.Y -= 10f + avoid * 0.03f;
			}).OnUpdate(delegate(Particle p)
			{
				p.Position.Y -= 0.2f;
				p.Dead = p.Age > 50;
			}).OnDraw(delegate(Particle p)
			{
				base.core.Renderer[base.Z].DrawSpriteW(_(SpriteName.bat_heart), p.Position.Shift(Component._cos((float)base.worldTicks * 0.2f), 0f), Color.White * ((float)(50 - p.Age) / 50f), new Vector2(1f + (float)p.Age / 50f) * 0.5f, 0f, SpriteFlip.None, SpriteOrigin.Center);
			});
			loveEmitter.Start(20);
		}
		idleSoundDelay = Component._rnd(60, 120);
		base.Load();
	}

	public override void Update()
    {
        idleSoundDelay--;
        if (idleSoundDelay == 0)
        {
            SendMessage(new PlayWorldSoundMessage(Squeaks.DrawDifferent(), base.WorldCenter));
            idleSoundDelay = Component._rnd(60, 120);
        }
        animation.Update();
        if (!Fleeing)
        {
            VampireChar vampire = ((base.core.CurrentPlayState != null) ? (base.core.CurrentPlayState.Player as VampireChar) : null);
            unfriended = (base.core.OptionsData.UnfriendBats && vampire != null && !vampire.Dead);
            bool anchorMoved = false;
            if (unfriended)
            {
                if (loveEmitter != null)
                {
                    loveEmitter.Stop();
                    loveEmitter = null;
                }
                Vector2 target = vampire.WorldCoordinates;
                Vector2 offset = target - new Vector2(x, y);
                if (base.core.OptionsData.VampirePredator && vampire.FlightActive && offset.LengthSquared() < HuntRadiusSq)
                {
                    Fleeing = true;
                    scatterFlee = true;
                    scatterT = 240;
                    scatterX = x;
                    hunting = false;
                    animation.Speed = 0.4f;
                    SendMessage(new PlayWorldSoundMessage(Squeaks.DrawDifferent(), base.WorldCenter));
                }
                else if (offset.LengthSquared() < HuntRadiusSq && IsLeadHunter() && FairToHunt(vampire, target))
                {
                    if (!hunting)
                    {
                        hunting = true;
                        SendMessage(new PlayWorldSoundMessage(Squeaks.DrawDifferent(), base.WorldCenter));
                    }
                    offset.Normalize();
                    Vector2 next = spawn + offset * HuntSpeed;
                    var tile = levelMap[next];
                    if (tile != null && tile.IsPassableFor(this) && (next - home).LengthSquared() < LeashSq)
                    {
                        spawn = next;
                        anchorMoved = true;
                    }
                }
                else
                {
                    hunting = false;
                }
            }
            else
            {
                hunting = false;
            }
            if (!hunting && (spawn.X != home.X || spawn.Y != home.Y))
            {
                Vector2 back = home - spawn;
                float dist = back.Length();
                spawn = ((dist <= HuntSpeed) ? home : (spawn + back / dist * HuntSpeed));
                anchorMoved = true;
            }
            if (Moving)
            {
                float num = ((xR == 0) ? 0f : ((float)Math.Sin((float)(base.worldTicks + delay + 10 * xS) / (float)(10 * xR)) * (float)xD));
                float num2 = ((yR == 0) ? 0f : ((float)Math.Sin((float)(base.worldTicks + delay + 10 * yS) / (float)(10 * yR)) * (float)yD));
                x = spawn.X + num;
                y = spawn.Y + num2;
                UpdateTiles();
            }
            else if (anchorMoved)
            {
                x = spawn.X;
                y = spawn.Y;
                UpdateTiles();
            }
        }
        else if (scatterFlee)
        {
            scatterT--;
            y -= 0.03f;
            x = scatterX + Component._sin((float)base.worldTicks * 0.15f) * 0.25f;
            UpdateTiles();
            if (scatterT <= 0)
            {
                Vector2 tilePos = new Vector2(x, y);
                var tileHere = levelMap[tilePos];
                bool safeSpot = tileHere != null && tileHere.IsPassableFor(this);
                Vector2 playerPos = base.core.CurrentPlayState.Player.WorldCoordinates;
                bool clearOfPlayer = (tilePos - playerPos).LengthSquared() > 2.25f;
                if (safeSpot && clearOfPlayer)
                {
                    scatterFlee = false;
                    Fleeing = false;
                    spawn = tilePos;
                    home = spawn;
                    hunting = false;
                    animation.Speed = 0.15f;
                }
                else
                {
                    SendMessage(new RemoveEntityMessage(this));
                }
            }
        }
        else
        {
            fleeDelay--;
            if (fleeDelay == 0)
            {
                FlightStep = 0.45f;
                SendMessage(new SpawnEntityMessage(new EffectEntity(base.CenterCoordinates, "dust_", "1234"), CurrentPlatform));
            }
            fleeTimeout--;
            if (fleeTimeout == 0)
            {
                SendMessage(new RemoveEntityMessage(this));
            }
        }
        avoid += (avoidTarget - avoid) * 0.1f;
        base.Update();
    }

	private bool FairToHunt(VampireChar vampire, Vector2 target)
    {
        if (vampire.CurrentPlatform != null)
        {
            return false;
        }
        int open = 0;
        open += (OpenForPlayer(target.Shift(1f, 0f)) ? 1 : 0);
        open += (OpenForPlayer(target.Shift(-1f, 0f)) ? 1 : 0);
        open += (OpenForPlayer(target.Shift(0f, 1f)) ? 1 : 0);
        open += (OpenForPlayer(target.Shift(0f, -1f)) ? 1 : 0);
        return open >= 3;
    }

    private bool OpenForPlayer(Vector2 t)
    {
        var tile = levelMap[t];
        return tile != null && tile.IsPassableFor(base.core.CurrentPlayState.Player);
    }

	private bool IsLeadHunter()
    {
        Vector2 playerPos = base.core.CurrentPlayState.Player.WorldCoordinates;
        float myDist = (playerPos - new Vector2(x, y)).LengthSquared();
        List<Entity> list = base.core.CurrentPlayState.EntityManager.GetEntitiesInRadius(new Vector2(x, y), 12f).FindAll((Entity e) => e is BatEntity && !e.IsBroken && !(e as BatEntity).Fleeing);
        foreach (Entity item in list)
        {
            BatEntity other = item as BatEntity;
            if (other != null && other != this && (new Vector2(other.x, other.y) - playerPos).LengthSquared() < myDist)
            {
                return false;
            }
        }
        return true;
    }

	public override void Draw()
	{
		avoidTarget = ((avoidPlayer && !unfriended) ? (900f - Component._m((base.core.CurrentPlayState.Player.WorldCenter - base.WorldCenter).LengthSquared(), 900f)) : 0f);
		Sprite currentFrame = animation.GetCurrentFrame();
		Color? tint = (unfriended ? default(Color).FromRgb(16732240) : (Color?)null);
        base.core.Renderer[base.Z + 3].DrawSpriteW(currentFrame, base.WorldCenter.Shift(-10.5f, -12f - avoid * 0.03f), tint, new Vector2((!Fleeing) ? 1f : ((scatterFlee ? 1f : (1f + 0.6f * (float)fleeDelay / 70f)))));
		base.core.Renderer["bg", base.Z + 32, false].DrawSpriteW(currentFrame, base.WorldCenter.Shift(-10.5f, 0f), Color.Black * 0.2f, new Vector2(1f, 0.8f), 0f, SpriteFlip.Vertical);
		base.Draw();
	}

	public override void Break(Entity offender)
	{
		if (offender is CreepChar)
		{
			if (Fleeing)
			{
				return;
			}
			Fleeing = true;
			animation.Speed = 0.4f;
			IsBroken = true;
			Vector2 vector = base.WorldCenter - offender.WorldCenter;
			vector.Normalize();
			vector *= 10f;
			SetFlying(value: false);
			SuspendedStartFlying((int)vector.X, (int)vector.Y, 0.001f, ignoreObstacles: true);
		}
		else
		{
			SendMessage(new PlayWorldSoundMessage(SoundName.bat_death, base.WorldCenter));
			SendMessage(new RemoveEntityMessage(this));
			if (offender is PlayerEntity || offender is ProjectileEntity || offender is GolemMissile)
			{
				_inc(Stat.BatsKilled);
			}
			base.core.ParticleManager.AddEmitter(inWorld: true, base.WorldCenter, 3f).OnSpawn(delegate(Particle p)
			{
				p.Velocity = SciHelper.GetRandomVectorInCircle(0.6f);
				p.Velocity.Y -= 0.5f;
			}).OnUpdate(delegate(Particle p)
			{
				p.Position += p.Velocity;
				p.Velocity += new Vector2(0f, 0.05f);
				p.Dead = p.Age > 50;
			})
				.OnDraw(delegate(Particle p)
				{
					base.core.Renderer[base.Z].DrawDotW(p.Position.X, p.Position.Y - 8f, default(Color).FromRgb(12194836) * ((float)(50 - p.Age) / 50f), 1f);
				})
				.Burst(20);
			IsBroken = true;
		}
		base.Break(offender);
	}

	public override void CollideWith(Entity other)
	{
		if (base.Age >= 10 && !Fleeing && !IsBroken)
		{
			if (other is PlayerEntity playerEntity && (!avoidPlayer || unfriended))
            {
                playerEntity.Hurt(InjuryType.Bat, this);
            }
			base.CollideWith(other);
		}
	}
}
