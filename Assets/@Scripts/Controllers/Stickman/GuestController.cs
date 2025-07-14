using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.XR;
using static Define;

public class GuestController : StickmanController
{
	//State는 Stickman의 AnimState 
	private EGuestState _guestState = EGuestState.None;
	public EGuestState GuestState
	{
		get { return _guestState; }
		set 
		{ 
			_guestState = value;
			
			if (value == EGuestState.Eating)
				State = EAnimState.Eating;

			UpdateAnimation(); 
		}
	}

	public int CurrentDestQueueIndex;

	protected override void Awake()
	{
		base.Awake();		
	}

	protected override void Update()
	{
		base.Update();

		//손님이 현재 식사중이지 X -> 줄 서는 중이거나 줄로 이동하는 중
		if (GuestState != EGuestState.Eating)
		{
			//이동이 끝났으면
			if (HasArrivedAtDestination)
			{
				//_navMeshAgent.isStopped = true;
				State = EAnimState.Idle;
			}
			//이동 중이면
			else
			{
				State = EAnimState.Move;
				LookAtDestination();
			}
		}
		else
		{
			//_navMeshAgent.isStopped = true;
		}
	}
}
