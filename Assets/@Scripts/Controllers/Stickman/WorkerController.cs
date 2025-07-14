using System.Collections;
using UnityEngine;
using static Define;


[RequireComponent(typeof(CharacterController))]
public class WorkerController : StickmanController
{
	protected CharacterController _controller;
	public SystemBase CurrentSystem;

	//작업을 관리하는 코루틴
	public Coroutine WorkerJob;

	public void DoJob(IEnumerator job)
	{
		//이미 실행중인 작업이 있으면 종료하고 새로운 작업으로 이동
		if (WorkerJob != null)
			StopCoroutine(WorkerJob);
		
		WorkerJob = StartCoroutine(job);
	}

	protected override void Awake()
	{
		//부모 클래스의 Awake도 같이 실행
		base.Awake();
		//Controller
		_controller = GetComponent<CharacterController>();
	}

	private void Start()
	{
		//스폰 되자마자 가게로 이동
		State = Define.EAnimState.Move;
	}

	protected override void Update()
    {
		//부모 클래스의 Update도 같이 실행
		base.Update();

		if (HasArrivedAtDestination)
		{
			_navMeshAgent.isStopped = true;
			State = EAnimState.Idle;
		}
		else
		{
			State = EAnimState.Move;
			LookAtDestination();
		}
	}
}
