using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

#region Pool
//내부적으로 오브젝트 풀을 관리하는 클래스
internal class Pool
{
	private GameObject _prefab;
	private IObjectPool<GameObject> _pool;

	private Transform _root;
	private Transform Root
	{
		get
		{
			if (_root == null)
			{
				//오브젝트들을 정리할 부모 오브젝트생성
				GameObject go = new GameObject() { name = $"@{_prefab.name}Pool" };
				_root = go.transform;
			}

			return _root;
		}
	}

	//생성자
	public Pool(GameObject prefab)
	{
		_prefab = prefab;
		//풀 생성하면서 콜백 함수 등록
		_pool = new ObjectPool<GameObject>(OnCreate, OnGet, OnRelease, OnDestroy);
	}

	//오브젝트 반남
	public void Push(GameObject go)
	{
		if (go.activeSelf)
			_pool.Release(go);
	}
	//오브젝트 가져오기
	public GameObject Pop()
	{
		return _pool.Get();
	}

	#region Funcs
	//새 오브젝트 생성시 호출
	private GameObject OnCreate()
	{
		//복제하고 정리용 오브젝트의 자식으로 설정하고 이름은 프리팹 이름으로(숫자 붙는거 방지)
		GameObject go = GameObject.Instantiate(_prefab);
		go.transform.SetParent(Root);
		go.name = _prefab.name;
		return go;
	}

	//풀에서 꺼낼 때
	private void OnGet(GameObject go)
	{
		go.SetActive(true);
	}
	//풀에 반납할 때
	private void OnRelease(GameObject go)
	{
		go.SetActive(false);
	}
	//오브젝트 파괴시
	private void OnDestroy(GameObject go)
	{
		GameObject.Destroy(go);
	}
	#endregion
}
#endregion

//poolmanager는 1개만 있으면 되므로 싱글톤으로 인스턴스 생성
public class PoolManager : Singleton<PoolManager>
{
	//풀은 이름 기준으로 관리
	private Dictionary<string, Pool> _pools = new Dictionary<string, Pool>();

	//외부에서 오브젝트를 요청 시, 풀에 없으면 생성하고 있으면 풀에서 꺼내기
	public GameObject Pop(GameObject prefab)
	{
		if (_pools.ContainsKey(prefab.name) == false)
			CreatePool(prefab);

		return _pools[prefab.name].Pop();
	}
	//오브젝트를 풀에 반남
	public bool Push(GameObject go)
	{
		if (_pools.ContainsKey(go.name) == false)
			return false;

		_pools[go.name].Push(go);
		return true;
	}
	//풀 초기화
	//풀은 사라지지만 오브젝트는 파괴되지 않음
	public void Clear()
	{
		_pools.Clear();
	}
	//풀 만들기
	private void CreatePool(GameObject original)
	{
		Pool pool = new Pool(original);
		_pools.Add(original.name, pool);
	}
}
