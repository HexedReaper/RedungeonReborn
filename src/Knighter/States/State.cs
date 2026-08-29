using System;
using Knighter.Messages;

namespace Knighter.States;

public abstract class State : Component
{
	public enum TransType
	{
		None,
		In,
		Out
	}

	public bool IsOverlay;

	public bool IsOpaque;

	public bool Loaded;

	public bool ShowCoins = true;

	public TransType Transition;

	public int TransT;

	public CoreEvent SuspendedCoreEvent { get; private set; }

	public int TicksInState { get; protected set; }

	public int TransDuration { get; protected set; }

	public bool IsTopState => base.core.GetCurrentState() == this;

	public int Trans
	{
		get
		{
			if (Transition != TransType.Out)
			{
				return TransDuration - TransT;
			}
			return TransT;
		}
	}

	public int TransReverse
	{
		get
		{
			if (Transition != TransType.In)
			{
				return TransDuration - TransT;
			}
			return TransT;
		}
	}

	public int TransD(int dt, int dd)
	{
		return Math.Min(Math.Max(Trans - dt - 1, 0), TransDuration - dd);
	}

	public int TransReverseD(int dt, int dd)
	{
		return Math.Min(Math.Max(TransReverse + dt, 0), TransDuration - dd);
	}

	public override void Load()
	{
		Loaded = true;
		base.Load();
	}

	public override void Unload()
	{
		base.core.MessageManager.UnsubscribeFromAll(this);
		base.Unload();
	}

	public virtual void HandleInput()
	{
	}

	public virtual void OnLeaveBehind()
	{
	}

	public virtual void OnReturn()
	{
	}

	public override void Update()
	{
		TicksInState++;
		base.Update();
	}

	public void TransitionIn()
	{
		Transition = TransType.In;
		TransT = TransDuration;
		UpdateTransition();
	}

	public virtual void UpdateTransition()
	{
	}

	public void TransitionOut(CoreEvent coreEvent)
	{
		Transition = TransType.Out;
		TransT = TransDuration;
		SuspendedCoreEvent = coreEvent;
		if (TransT == 0)
		{
			OnOutTransitionDone();
		}
	}

	public virtual void OnOutTransitionDone()
	{
		SendMessage(new CoreEventMessage(SuspendedCoreEvent));
	}

	public virtual void OnBackButtonPressed()
	{
	}
}
