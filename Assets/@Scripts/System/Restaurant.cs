using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class Restaurant : MonoBehaviour
{
	public List<SystemBase> RestaurantSystems = new List<SystemBase>();

	public int StageNum = 0;
	public List<UnlockableBase> Props = new List<UnlockableBase>();
	public List<WorkerController> Workers = new List<WorkerController>();

	private RestaurantData _data;

	private void OnEnable()
	{
		GameManager.Instance.AddEventListener(EEventType.HireWorker, OnHireWorker);
		GameManager.Instance.AddEventListener(EEventType.WorkerSpeedUpgrade, UpgradeWorkerSpeed);
		GameManager.Instance.AddEventListener(EEventType.WorkerCapacityUpgrade, UpgradeWorkerCapacity);
		StartCoroutine(CoDistributeWorkerAI());
	}

	private void OnDisable()
	{
		GameManager.Instance.RemoveEventListener(EEventType.HireWorker, OnHireWorker);
		GameManager.Instance.RemoveEventListener(EEventType.WorkerSpeedUpgrade, UpgradeWorkerSpeed);
		GameManager.Instance.RemoveEventListener(EEventType.WorkerCapacityUpgrade, UpgradeWorkerCapacity);
	}

	public void SetInfo(RestaurantData data)
	{
		_data = data;

		RestaurantSystems = GetComponentsInChildren<SystemBase>().ToList();
		Props = GetComponentsInChildren<UnlockableBase>().ToList();

		for (int i = 0; i < Props.Count; i++)
		{
			UnlockableStateData stateData = data.UnlockableStates[i];
			Props[i].SetInfo(stateData);
		}

		Tutorial tutorial = GetComponent<Tutorial>();
		if (tutorial != null)
			tutorial.SetInfo(data);

		//업그레이드 적용해서 직원 생성
		for (int i = 0; i < data.WorkerCount; i++)
			HireWorkerAndSetDataToWorker(_data.SpeedUpgrade, _data.CapacityUpgrade);
		
	}

	void OnHireWorker()
	{
		GameObject go = GameManager.Instance.SpawnWorker();
		WorkerController wc = go.GetComponent<WorkerController>();
		go.transform.position = Define.WORKER_SPAWN_POS;

		Workers.Add(wc);

		// 세이브 파일 갱신.
		_data.WorkerCount = Mathf.Max(_data.WorkerCount, Workers.Count);
	}
	void HireWorkerAndSetDataToWorker(float speed, int capacity)
	{
		GameObject go = GameManager.Instance.SpawnWorker();
		WorkerController wc = go.GetComponent<WorkerController>();
		go.transform.position = Define.WORKER_SPAWN_POS;
		wc.MoveSpeed += speed;
		wc.MaxPileCount += capacity;
		Workers.Add(wc);

		GameManager.Instance.Player.MoveSpeed += speed;
		GameManager.Instance.Player.MaxPileCount += capacity; ;
		// 필요하면 세이브 파일 갱신.
		_data.WorkerCount = Mathf.Max(_data.WorkerCount, Workers.Count);
	}

	void UpgradeWorkerSpeed()
	{
		foreach (WorkerController worker in Workers)
		{
			if (worker.MoveSpeed < 7 && _data.SpeedUpgrade<4)
			{
				worker.MoveSpeed += 1;
				GameManager.Instance.Player.MoveSpeed += 1;
			}
			else
			{
				StartCoroutine(ShowToastMessageCoroutine("Speed Is Max", 2f));
			}

		}
		_data.SpeedUpgrade++;
	}

	void UpgradeWorkerCapacity()
	{
		foreach (WorkerController worker in Workers)
		{
			if (worker.MaxPileCount < 10 && _data.CapacityUpgrade < 5)
			{
				worker.MaxPileCount += 1;
				GameManager.Instance.Player.MaxPileCount += 1;
			}
			else
			{
				StartCoroutine(ShowToastMessageCoroutine("No further upgrades possible", 2f));
			}
		}
		_data.CapacityUpgrade++;
	}

	void ChangeMessage(string text)
	{
		GameManager.Instance.GameSceneUI.SetToastMessage(text);
	}

	IEnumerator ShowToastMessageCoroutine(string message, float duration)
	{
		ChangeMessage(message);
		yield return new WaitForSeconds(duration);
		ChangeMessage("");
	}

	IEnumerator CoDistributeWorkerAI()
	{
		while (true)
		{
			yield return new WaitForSeconds(1);

			yield return new WaitUntil(() => Workers.Count > 0);

			foreach (WorkerController worker in Workers)
			{				
				// 어딘가 소속되어 있으면 스킵.
				if (worker.CurrentSystem != null)
					continue;

				// 어떤 시스템에 일감이 남아 있으면, 해당 시스템으로 배정.
				foreach (SystemBase system in RestaurantSystems)
				{	
					if (system.HasJob)
					{
						system.AddWorker(worker);
					}
				}
			}
		}
	}
}
