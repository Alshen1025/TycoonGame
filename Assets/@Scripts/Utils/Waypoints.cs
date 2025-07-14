using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Waypoints : MonoBehaviour
{
	//이 스크립트가 붙어있는 오브젝트의 자식(Waypoints)의 수
	public int GetPointCount()
	{
		return transform.childCount;
	}

	//특정 인덱스의 웨이 포인트
	public Transform GetPoint(int index)
	{
		return transform.GetChild(index);
	}

	//모든 웨이 포인트 반환
	public List<Transform> GetPoints()
	{
		List<Transform> points = new List<Transform>();

		foreach (Transform child in transform)
			points.Add(child);

		return points;
	}

	//에디터에서만 실행
	//웨이 포인트 자리에 구를 그리고 선으로 이어서 에디터에서 보기 편하게
	#region Editor
#if UNITY_EDITOR
	[SerializeField]
	private Color _color = Color.yellow;

	[SerializeField]
	private float _pointSize = 0.2f;

	void OnDrawGizmos()
	{
		Gizmos.color = _color;

		foreach (Transform child in transform)
			Gizmos.DrawSphere(child.position, _pointSize);

		for (int i = 0; i < transform.childCount - 1; i++)
		{
			Transform start = transform.GetChild(i);
			Transform end = transform.GetChild(i + 1);
			Gizmos.DrawLine(start.position, end.position);
		}
	}
#endif
	#endregion
}
