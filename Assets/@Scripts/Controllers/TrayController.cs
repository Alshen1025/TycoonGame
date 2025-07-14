using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;
using static UnityEngine.UI.GridLayoutGroup;

public class TrayController : MonoBehaviour
{
	//Tray의 오브젝트들이 흔들리거나 기우는 값
	[SerializeField]
	private Vector2 _shakeRange = new Vector2(0.8f, 0.4f);
	[SerializeField]
	private float _bendFactor = 0.1f;
	//아이템 높이
	[SerializeField]
	private float _itemHeight = 0.5f;

	//쟁반에 있는 아이템 종류
	private EObjectType _objectType = EObjectType.None;
	public EObjectType CurrentTrayObjectType
	{
		get { return _objectType; }
		set 
		{ 
			_objectType = value;
			//아이템 종류에 따라 아이템 높이를 다르게 설정
			switch (value)
			{
				case EObjectType.Trash:
					_itemHeight = 0.2f;
					break;
				case EObjectType.Burger:
					_itemHeight = 0.5f;
					break;
			}
		}
	}

	public int ItemCount => _items.Count; // 쟁반 위에 들고 있는 아이템 개수.
	public int ReservedCount => _reserved.Count; // 쟁반 위로 이동중.
	public int TotalItemCount => _reserved.Count + _items.Count; // 쟁반 위로 이동중인 아이템을 포함한 전체 개수.

	//쟁반으로 이동 중인 물체
	private HashSet<Transform> _reserved = new HashSet<Transform>();
	//쟁반에 있는 아이템
	private List<Transform> _items = new List<Transform>();

	private MeshRenderer _meshRenderer;

	//Tray의 주인
	private StickmanController _owner;
	public bool IsPlayer = false;

	public bool Visible
	{
		set { if (_meshRenderer != null ) _meshRenderer.enabled = value; _owner?.UpdateAnimation(); }
		get { return (_meshRenderer != null) ? _meshRenderer.enabled : false; }
	}

	private void Start()
	{
		_meshRenderer = GetComponent<MeshRenderer>();
		_owner = transform.parent.GetComponent<StickmanController>();
		Visible = false;
	}

	// 휘는거 조정.
	private void Update()
	{
		//아이템이 있는 경우에만 보이도록
		Visible = (_items.Count > 0);

		//아이템이 없으면 쟁반위 오브젝트를 수정할 필요 X
		if (_items.Count == 0)
			return;
		
		//기본은 정지상태
		Vector3 moveDir = Vector3.zero;

		if (IsPlayer)
		{
			//플레이어 이동에 쓰는 조이스틱의 방향 가져와서 Vector3로 변환
			//카메라가 45도 기울어져 있으므로 회전 보정 후 단위 벡터로 변환
			Vector3 dir = GameManager.Instance.JoystickDir;
			moveDir = new Vector3(dir.x, 0, dir.y);
			moveDir = (Quaternion.Euler(0, 45, 0) * moveDir).normalized;
		}
		//제일 아래에 있는 오브젝트는 고정
		_items[0].position = transform.position;
		_items[0].rotation = transform.rotation;

		//두번째 아이템 부터 이전 아이템 위에 쌓이도록 처리
		
		for (int i = 1; i < _items.Count; i++)
		{
			//아이템 개수에 따라 rate계산해서 위치와 회전보간
			float rate = Mathf.Lerp(_shakeRange.x, _shakeRange.y, i / (float)_items.Count);
			_items[i].position =  Vector3.Lerp(_items[i].position, _items[i - 1].position + (_items[i - 1].up * _itemHeight), rate);
			_items[i].rotation = Quaternion.Lerp(_items[i].rotation, _items[i - 1].rotation, rate);

			//플레이어가 움직이고 있으면 살짝 휘도록
			if (moveDir != Vector3.zero)
				_items[i].rotation *= Quaternion.Euler(-i * _bendFactor * rate, 0, 0);
		}
	}

	public void AddToTray(Transform child)
	{
		// 운반하는 물체 종류 추적
		EObjectType objectType = Utils.GetTrayObjectType(child);
		if (objectType == EObjectType.None)
			return;

		// 다른 종류의 아이템이 있으면 수집 불가.
		if (CurrentTrayObjectType != EObjectType.None && CurrentTrayObjectType != objectType)
			return;

		//최대 개수 설정
		if (TotalItemCount >= _owner.MaxPileCount)
			return;

		//아이템 종류를 저장
		CurrentTrayObjectType = objectType;

		//수집한 아이템을 '이동 중'상태로 변경
		_reserved.Add(child);

		//위치 계산 쟁반위치에 아이템 개수 x 높이만큼이동
 		Vector3 dest = transform.position + Vector3.up * TotalItemCount * _itemHeight;

		//애니메이션
		child.DOJump(dest, 5, 1, 0.3f)
			.OnComplete(() =>
			{
				//끝나면 이동중상태를 트레이에 있는 상태로 변경
				_reserved.Remove(child);
				_items.Add(child);
			});
	}

	public Transform RemoveFromTray()
	{
		//쟁반에 아이템이 없거나, 아직 이동 중인 아이템이 있으면 제거 불가
		if (ItemCount == 0 || ReservedCount > 0)
			return null;

		//가장 마지막 아이템을 꺼내야함 
		Transform item = _items.Last();

		//null일 경우 예외 처리
		if (item == null)
			return null;

		//아이템 리스트에서 제거
		_items.RemoveAt(_items.Count - 1);

		// 아이템이 없으면 상태를 초기화
		if (TotalItemCount == 0)
			CurrentTrayObjectType = EObjectType.None;

		//꺼낸 맨 마지막 아이템을 반환
		return item;
	}
}
