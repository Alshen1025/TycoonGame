using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static Define;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(NavMeshAgent))]
public class StickmanController : MonoBehaviour
{
	[SerializeField, Range(1, 5)]
	protected float _moveSpeed = 3;

	public float MoveSpeed
	{
		get { return _moveSpeed; }
		set { _moveSpeed = value; }
	}

	[SerializeField]
	protected int _maxPileCount = 5;
	public int MaxPileCount
	{
		get { return _maxPileCount; }
		set { _maxPileCount = value; }
	}

	[SerializeField]
	protected float _rotateSpeed = 360;

	protected Animator _animator;
	protected AudioSource _audioSource;
	protected NavMeshAgent _navMeshAgent;
	protected UI_OrderBubble _orderBubble;

	public TrayController Tray { get; protected set; }

	//Animation
	#region Animator
	//State를 저장하는 변수, get과 set구현
	//set으로 상태를 변경하면 UpdateAnimation함수를 호출해서 상태 변경
	private EAnimState _state = EAnimState.None;
	public EAnimState State
	{
		get { return _state; }
		set
		{
			if (_state == value)
				return;

			_state = value;

			UpdateAnimation();
		}
	}

	int _lastAnim = -1;

	public virtual void UpdateAnimation()
	{
		int nextAnim = -1;

		//Idle과 Move의 경우 Serving상태에 따라 애니메이션이 달라지므로 조건 확인해서 nextAnim설정
		switch (State)
		{
			case EAnimState.Idle:
				nextAnim = IsServing ? Define.SERVING_IDLE : Define.IDLE;
				break;
			case EAnimState.Move:
				nextAnim = IsServing ? Define.SERVING_MOVE : Define.MOVE;
				break;
			case EAnimState.Eating:
				nextAnim = Define.EATING;
				break;
		}
		//애니메이션이 같으면 Return
		if (_lastAnim == nextAnim)
			return;

		//애니메이션을 부드럽게 변경
		_animator.CrossFade(nextAnim, 0.01f);
		_lastAnim = nextAnim;
	}
	#endregion


	//길찾기
	#region NavMeshAgent
	public Vector3 Destination
	{
		get { return _navMeshAgent.destination; }
		protected set
		{
			//목적지 설정 후 이동, 회전
			_navMeshAgent.SetDestination(value);
			_navMeshAgent.isStopped = false;
			LookAtDestination();
		}
	}

	//목적지에 도착했는지 확인
	//약간의 오차 허용
	public bool HasArrivedAtDestination
	{
		get
		{
			Vector3 dir = Destination - transform.position;
			return dir.sqrMagnitude < 0.2f;
		}
	}

	//목적지를 향해 회전
	protected void LookAtDestination()
	{
		Vector3 moveDir = (Destination - transform.position).normalized;
		if (moveDir != Vector3.zero && moveDir.sqrMagnitude > 0.01f)
		{
			Quaternion lookRotation = Quaternion.LookRotation(moveDir);
			transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, Time.deltaTime * _rotateSpeed);
		}
	}

	//콜백 함수 저장을 위한 변수
	private Action OnArrivedAtDestCallback;

	public void SetDestination(Vector3 dest, Action onArrivedAtDest = null)
	{
		//public Vector3 Destination의 Set이 실행
		Destination = dest;
		//도착시 실행할 콜백 함수 저장
		OnArrivedAtDestCallback = onArrivedAtDest;
	}
	#endregion

	#region OrderBubble
	public int OrderCount
	{
		set
		{
			//주문할 물품의 개수
			//개수가 0보다 크면 주문을 하고있는 상태이므로 UI활성화
			_orderBubble.Count = value;

			if (value > 0)
				_orderBubble.gameObject.SetActive(true);
			else
				_orderBubble.gameObject.SetActive(false);
		}
	}
	#endregion

	#region Tray
	//Tray.Visible의 값을 반환
	//Serving을 실행할 때만 Tray가 보이기 때문
	public bool IsServing => Tray.Visible;
	//public bool IsServing
	//{
	//	get { return Tray.Visible; }
	//}
	#endregion

	protected virtual void Awake()
	{
		_animator = GetComponent<Animator>();
		_audioSource = GetComponent<AudioSource>();
		_navMeshAgent = GetComponent<NavMeshAgent>();
		_orderBubble = Utils.FindChild<UI_OrderBubble>(gameObject);
		Tray = Utils.FindChild<TrayController>(gameObject);

		_navMeshAgent.speed = _moveSpeed;
		_navMeshAgent.stoppingDistance = 0.1f;
		_navMeshAgent.radius = 0.01f;
		_navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

		Destination = transform.position;

		OrderCount = 0;
	}

	protected virtual void Update()
	{
		// 중력 작용.
		transform.position = new Vector3(transform.position.x, 0, transform.position.z);

		//콜백함수가 존재하고 목적지에 도착했으면

		if (OnArrivedAtDestCallback != null)
		{
			if (HasArrivedAtDestination)
			{
				//도착했을 때의 콜백을 한 번만 실행하기 위해서 실행 후 null로 초기화
				OnArrivedAtDestCallback?.Invoke();
				OnArrivedAtDestCallback = null;
			}
		}
	}
}
