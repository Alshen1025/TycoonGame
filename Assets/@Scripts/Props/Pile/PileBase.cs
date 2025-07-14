using DG.Tweening;
using NUnit.Framework.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static Define;
using static UnityEditor.PlayerSettings;

[RequireComponent(typeof(BoxCollider))]
public class PileBase : MonoBehaviour
{
	#region Fields
	//가로로 몇 칸 쌓을지
	//열
	[SerializeField]
	protected int _row = 2;

	//세로로 몇 칸 쌓을지
	//행
	[SerializeField]
	protected int _column = 2;

	//각 오브젝트의 크기(가로 새로 높이)
	[SerializeField]
	protected Vector3 _size = new Vector3(0.5f, 0.1f, 0.5f);

	//오브젝트 떨어트릴 때 인터벌
	[SerializeField]
	protected float _dropInterval = 0.05f;
	#endregion

	#region Contents
	//현재 쌓고있는 오브텍트의 타입
	protected EObjectType _objectType = EObjectType.None;

	//현재 쌓고있는 오브젝트 타입에 따라 생성
	public void SpawnObject()
	{
		switch (_objectType)
		{
			case EObjectType.Burger:
				{
					GameObject go = GameManager.Instance.SpawnBurger();
					AddToPile(go, false);
				}				
				break;
			case EObjectType.Money:
				{
					GameObject go = GameManager.Instance.SpawnMoney();
					AddToPile(go, false);
				}
				break;
			case EObjectType.Trash:
				{
					GameObject go = GameManager.Instance.SpawnTrash();
					AddToPile(go, false);
				}				
				break;
		}	
	}

	//생성, 애니메이션 동시에
	public void SpawnObjectWithJump(Vector3 spawnPos)
	{
		switch (_objectType)
		{
			case EObjectType.Burger:
				{
					GameObject go = GameManager.Instance.SpawnBurger();
					go.transform.position = spawnPos;
					AddToPile(go, true);
				}
				break;
			case EObjectType.Money:
				{
					GameObject go = GameManager.Instance.SpawnMoney();
					go.transform.position = spawnPos;
					AddToPile(go, true);
				}
				break;
			case EObjectType.Trash:
				{
					GameObject go = GameManager.Instance.SpawnTrash();
					go.transform.position = spawnPos;
					AddToPile(go, true);
				}
				break;
		}
	}

	//오브젝트 제거
	//제거한 오브젝트 Despawn
	//오브젝트 풀링을 위해 삭제되는게 아니라 비활성화 됨
	public void DespawnObject()
	{
		if (ObjectCount == 0)
			return;

		switch (_objectType)
		{
			case EObjectType.Burger:
				{
					GameObject go = RemoveFromPile();
					GameManager.Instance.DespawnBurger(go);
				}
				break;
			case EObjectType.Money:
				{
					GameObject go = RemoveFromPile();
					GameManager.Instance.DespawnMoney(go);
				}
				break;
			case EObjectType.Trash:
				{
					GameObject go = RemoveFromPile();
					GameManager.Instance.DespawnTrash(go);
				}
				break;
		}
	}

	//제거, 애니메이션 재생 동시 실행
	public void DespawnObjectWithJump(Vector3 destPos, Action onDespawnCallback = null)
	{
		if (ObjectCount == 0)
			return;

		switch (_objectType)
		{
			case EObjectType.Burger:
				{
					GameObject go = RemoveFromPile();
					go.transform
						.DOJump(destPos, 3, 1, 0.3f)
						.OnComplete(() =>
						{
							GameManager.Instance.DespawnBurger(go);
							onDespawnCallback?.Invoke();
						});
				}
				break;
			case EObjectType.Money:
				{
					GameObject go = RemoveFromPile();
					go.transform
						.DOJump(destPos, 3, 1, 0.3f)
						.OnComplete(() =>
						{
							GameManager.Instance.DespawnMoney(go);
							onDespawnCallback?.Invoke();
						});
				}
				break;
			case EObjectType.Trash:
				{
					GameObject go = RemoveFromPile();
					go.transform
						.DOJump(destPos, 3, 1, 0.3f)
						.OnComplete(() =>
						{
							GameManager.Instance.DespawnTrash(go);
							onDespawnCallback?.Invoke();
						});
				}
				break;
		}
	}

	// Tray -> Pile
	public void TrayToPile(TrayController tray)
	{
		//트레이에 오브젝트가 없으면 리턴
		if (tray.CurrentTrayObjectType == EObjectType.None)
			return;
		//트레이에 오브젝트가 있는데 현재 쌓여있는 오브젝트와 쌓으려는 오브젝트가 다르면 리턴
		if (tray.CurrentTrayObjectType != EObjectType.None && _objectType != tray.CurrentTrayObjectType)
			return;
		//트레이로부터 제거
		Transform t = tray.RemoveFromTray();
		if (t == null)
			return;

		t.rotation = Quaternion.identity;
		//제거한 오브젝트 쌓기
		AddToPile(t.gameObject, jump: true);
	}

	// Pile -> Tray
	public void PileToTray(TrayController tray)
	{
		//같은 조건 검사
		if (_objectType == EObjectType.None)
			return;
		if (tray.CurrentTrayObjectType != EObjectType.None && _objectType != tray.CurrentTrayObjectType)
			return;

		//파일에서 제거
		GameObject go = RemoveFromPile();
		if (go == null)
			return;
		//트레이에 추가
		tray.AddToTray(go.transform);
	}
	#endregion

	#region Pile
	protected Stack<GameObject> _objects = new Stack<GameObject>();

	public int ObjectCount => _objects.Count;
	private void AddToPile(GameObject go, bool jump = false)
	{
		// 스택에 추가
		_objects.Push(go);

		// 위치를 조정
		Vector3 pos = GetPositionAt(_objects.Count - 1);

		if (jump)
			go.transform.DOJump(pos, 3, 1, 0.3f);
		else
			go.transform.position = pos;
	}

	//가장 위에 쌓인 오브젝트 제거 후 반환
	private GameObject RemoveFromPile()
	{
		if (_objects.Count == 0)
			return null;

		// 스택에서 제거한다.
		return _objects.Pop();
	}

	//오브젝트가 쌓일 위치를 파악
	private Vector3 GetPositionAt(int pileIndex)
	{
		//전체 배치에서 가운데로 위치보정
		Vector3 offset = new Vector3((_row - 1) * _size.x / 2, 0, (_column - 1) * _size.z / 2);
		//시작 위치
		Vector3 startPos = transform.position - offset;

		//행 계산
		int row = (pileIndex / _row) % _column;
		//열 계산
		int column = pileIndex % _row;
		//높이 계산
		int height = pileIndex / (_row * _column);

		//오브젝트를 놓을 위치 계산
		float x = startPos.x + column * _size.x;
		float y = startPos.y + height * _size.y;
		float z = startPos.z + row * _size.z;

		return new Vector3(x, y, z);
	}
	#endregion


	//에디터에서 위치를 보여주도록
	#region Editor
#if UNITY_EDITOR
	void OnDrawGizmosSelected()
	{
		Vector3 offset = new Vector3((_row - 1) * _size.x / 2, 0, (_column - 1) * _size.z / 2);
		Vector3 startPos = transform.position - offset; // 0번 칸의 위치.

		Gizmos.color = Color.yellow;

		for (int r = 0; r < _row; r++)
		{
			for (int c = 0; c < _column; c++)
			{
				Vector3 center = startPos + new Vector3(r * _size.x, _size.y / 2, c * _size.z);
				Gizmos.DrawWireCube(center, _size);
			}
		}
	}
#endif
	#endregion
}
