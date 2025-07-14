using DG.Tweening;
using NUnit.Framework.Internal;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using static Define;

public class Counter : UnlockableBase
{
	//카운터에 쌓을 오브젝트 - 버거와 돈
	private BurgerPile _burgerPile;
	private MoneyPile _moneyPile;

	//
	public MainCounterSystem Owner;

	int _spawnMoneyRemaining = 0;

	// 주문하는 햄버거 수.
	int _nextOrderBurgerCount = 0;

	//손님들이 서있는 위치와 손님들의 컨트롤러 관리
	private List<Transform> _queuePoints = new List<Transform>();
	List<GuestController> _queueGuests = new List<GuestController>();

	//카운터에서 Table과 일하고 있는 직원
	public List<WorkerController> Workers = new List<WorkerController>();
	public List<Table> Tables => Owner?.Tables;

	//직원이 하는 작업
	private WorkerInteraction _burgerInteraction;
	//현재 일 하고있는 직원과 위치
	public WorkerController CurrentBurgerWorker => _burgerInteraction.CurrentWorker;
	public Transform BurgerWorkerPos;

	//쌓여있는 버거 수
	public int BurgerCount => _burgerPile.ObjectCount;
	public bool NeedMoreBurgers => (_nextOrderBurgerCount > 0 && BurgerCount < _nextOrderBurgerCount);

	private WorkerInteraction _cashierInteraction;
	public WorkerController CurrentCashierWorker => _cashierInteraction.CurrentWorker;
	public Transform CashierWorkerPos;
	public bool NeedCashier => (CurrentCashierWorker == null);

	[SerializeField]
	public Transform GuestSpawnPos;

	void Start()
    {
		//시작하면 상호작용들을 다 세팅
		_burgerPile = Utils.FindChild<BurgerPile>(gameObject);
		_moneyPile = Utils.FindChild<MoneyPile>(gameObject);
		_queuePoints = Utils.FindChild<Waypoints>(gameObject).GetPoints();

		// 햄버거 인터랙션.
		_burgerInteraction = _burgerPile.GetComponent<WorkerInteraction>();
		_burgerInteraction.InteractInterval = 0.1f;
		_burgerInteraction.OnInteraction = OnBurgerInteraction;
		
		// 돈 인터랙션.
		_moneyPile.GetComponent<WorkerInteraction>().InteractInterval = 0.02f;
		_moneyPile.GetComponent<WorkerInteraction>().OnInteraction = OnMoneyInteraction;

		// 손님 인터랙션.
		GameObject machine = Utils.FindChild(gameObject, "Machine");
		_cashierInteraction = machine.GetComponent<WorkerInteraction>();
		_cashierInteraction.InteractInterval = 1;
		_cashierInteraction.OnInteraction = OnGuestInteraction;
	}

	private void OnEnable()
	{
		// 손님 스폰.		
		StartCoroutine(CoSpawnGuest());
		// 돈 스폰.
		StartCoroutine(CoSpawnMoney());
	}

	//카운터가 비활성화 되면 모든 코루틴 정지
	private void OnDisable()
	{
		StopAllCoroutines();
	}

	private void Update()
	{
		// 손님 AI
		UpdateGuestQueueAI();
		UpdateGuestOrderAI();
	}

	//손님 스폰 코루틴
	IEnumerator CoSpawnGuest()
	{
		while (true)
		{
			yield return new WaitForSeconds(Define.GUEST_SPAWN_INTERVAL);

			//자리가 있는지 확인
			if (_queueGuests.Count == _queuePoints.Count)
				continue;

			//손님 생성
			GameObject go = GameManager.Instance.SpawnGuest();
			go.transform.position = GuestSpawnPos.position;

			//손님 줄의 가장 뒤가 생성된 손님이 이동해야할 장소
			Transform dest = _queuePoints.Last();

			GuestController guest = go.GetComponent<GuestController>();
			guest.CurrentDestQueueIndex = _queuePoints.Count - 1;
			guest.GuestState = Define.EGuestState.Queuing;

			//()=>는 매개 변수가 없는 함수
			//SetDestination에서 델리게이트를 매개 변수로 받음
			guest.SetDestination(dest.position, () => 
			{ 
				guest.transform.rotation = dest.rotation;
			}); 			

			_queueGuests.Add(guest);
		}
	}

	IEnumerator CoSpawnMoney()
	{
		while (true)
		{
			yield return new WaitForSeconds(Define.MONEY_SPAWN_INTERVAL);

			if (_spawnMoneyRemaining <= 0)
				continue;

			_spawnMoneyRemaining--;

			_moneyPile.SpawnObject();
		}
	}

	#region GuestAI
	private void UpdateGuestQueueAI()
	{
		// 줄서기 관리.
		for (int i = 0; i < _queueGuests.Count; i++)
		{
			int guestIndex = i;
			GuestController guest = _queueGuests[guestIndex];
			if (guest.HasArrivedAtDestination == false)
				continue;

			// 다음 지점으로 이동.
			if (guest.CurrentDestQueueIndex > guestIndex)
			{
				guest.CurrentDestQueueIndex--;

				Transform dest = _queuePoints[guest.CurrentDestQueueIndex];
				guest.SetDestination(dest.position, () =>
				{
					guest.transform.rotation = dest.rotation;
				});
			}
		}
	}

	private void UpdateGuestOrderAI()
	{
		// 이미 주문이 진행중이라면 리턴.
		if (_nextOrderBurgerCount > 0)
			return;

		// 손님이 없다면 리턴.
		int maxOrderCount = Mathf.Min(Define.GUEST_MAX_ORDER_BURGER_COUNT, _queueGuests.Count);
		if (maxOrderCount == 0)
			return;

		// 이동중인지 확인.
		GuestController guest = _queueGuests[0];
		if (guest.HasArrivedAtDestination == false)
			return;

		// 맨 앞 자리 도착.
		if (guest.CurrentDestQueueIndex != 0)
			return;

		// 주문 진행.
		int orderCount = Random.Range(1, maxOrderCount + 1);
		_nextOrderBurgerCount = orderCount;
		guest.OrderCount = orderCount;
	}
	#endregion

	#region Interaction
	void OnBurgerInteraction(WorkerController wc)
	{
		_burgerPile.TrayToPile(wc.Tray);
	}

	void OnMoneyInteraction(WorkerController wc)
	{
		_moneyPile.DespawnObjectWithJump(wc.transform.position, () =>
		{
			// TODO : ADD MONEY
			GameManager.Instance.Money += 5;
		});
	}

	void OnGuestInteraction(WorkerController wc)
	{
		Table destTable = FindTableToServeGuests();
		if (destTable == null)
			return;

		//버거 개수에 따라 줄에 있는 손님들을 자리로 이동
		//손님 트레이에 버거 쌓기
		for (int i = 0; i < _nextOrderBurgerCount; i++)
		{
			GuestController guest = _queueGuests[i];
			guest.SetDestination(destTable.Chairs[i].position); // 의자로 향해 이동.
			guest.GuestState = Define.EGuestState.Serving;
			guest.OrderCount = 0;

			_burgerPile.PileToTray(guest.Tray);
		}
		//버거 개당 10
		_spawnMoneyRemaining = _nextOrderBurgerCount * 3;

		// 점유한다.
		destTable.Guests = _queueGuests.GetRange(0, _nextOrderBurgerCount);
		destTable.TableState = Define.ETableState.Reserved;

		// 줄에서 제거.
		_queueGuests.RemoveRange(0, _nextOrderBurgerCount);

		// 주문 처리 끝났으므로 0으로 리셋.
		_nextOrderBurgerCount = 0;
	}

	public Table FindTableToServeGuests()
	{
		// 손님 있어야 함.
		if (_nextOrderBurgerCount == 0)
			return null;

		// 햄버거 있어야 함.
		if (_burgerPile.ObjectCount < _nextOrderBurgerCount)
			return null;

		// 자리 수가 맞는 테이블이 있어야 함.
		foreach (Table table in Tables)
		{
			if (table.IsUnlocked == false)
				continue;
			if (table.IsOccupied)
				continue;

			if (_nextOrderBurgerCount > table.Chairs.Count)
				continue;

			return table;
		}

		return null;
	}
	#endregion
}
