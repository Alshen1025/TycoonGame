using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class WorkerInteraction : MonoBehaviour
{
	//WorkerController가 트리거(BoxCollider)에 들어 올 때 호출
	public Action<WorkerController> OnTriggerStart;
	//WorkerController가 트리거(BoxCollider)안에 머무를 때 호출
	public Action<WorkerController> OnInteraction;
	//WorkerController가 트리거(BoxCollider)에서 나갈 때 호출
	public Action<WorkerController> OnTriggerEnd;

	//상호작용 반복 간격
	public float InteractInterval = 0.5f;
	//현재 상호 작용중인 일꾼
	public WorkerController CurrentWorker;
	//반복 실행중인 코루틴
	private Coroutine _coWorkerInteraction;

	//오브젝트가 씬에 등장(활성화 될 때)
	private void OnEnable()
	{
		//기존 코루틴 정지
		if (_coWorkerInteraction != null)
			StopCoroutine(_coWorkerInteraction);
		//코루틴 시작
		_coWorkerInteraction = StartCoroutine(CoPlayerInteraction());
	}
	//비활성화 시
	private void OnDisable()
	{
		//코루틴 정지
		if (_coWorkerInteraction != null)
			StopCoroutine(_coWorkerInteraction);
		//실행중인 코루틴 null 로 변경
		_coWorkerInteraction = null;
	}

	//트리거에 일꾼이 머무는 동안 실행되는 코루틴
	IEnumerator CoPlayerInteraction()
	{
		while (true)
		{
			//0.5초마다
			yield return new WaitForSeconds(InteractInterval);

			//일꾼이 있고 OnInteraction 에 연결된 델리게이트에 있으면 현재 일꾼이 실행
			if (CurrentWorker != null)
				OnInteraction?.Invoke(CurrentWorker);
		}
	}

	//다른 Collider가 부착된 오브젝트가 안에 들어왔을 때
	private void OnTriggerEnter(Collider other)
	{
		//들어온 오브젝트가 일꾼인지 확인
		WorkerController wc = other.GetComponent<WorkerController>();
		if (wc == null)
			return;

		//일꾼으로 등록하고 이벤트 호출
		CurrentWorker = wc;
		OnTriggerStart?.Invoke(wc);
	}

	//나갈 때는 반대로
	void OnTriggerExit(Collider other)
	{
		WorkerController wc = other.GetComponent<WorkerController>();
		if (wc == null)
			return;

		CurrentWorker = null;
		OnTriggerEnd?.Invoke(wc);
	}
}
