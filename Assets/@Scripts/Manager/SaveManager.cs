using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static Define;

#region DataModel
[Serializable]
public class GameSaveData
{
	// 소지금.
	public long Money = 0;

	// 플레이어 상태
	public int RestaurantIndex;
	public Vector3 PlayerPosition;

	// 스테이지 별 상태.
	public List<RestaurantData> Restaurants;
}

[Serializable]
public class RestaurantData
{
	// 직원 수.
	public int WorkerCount;

	// 업그레이드.
	public float SpeedUpgrade;
	public int CapacityUpgrade; 

	// 프랍들.
	public ETutorialState TutorialState = ETutorialState.None;
	public List<UnlockableStateData> UnlockableStates;
}

[Serializable]
public class UnlockableStateData
{
	//오브젝트들의 현재 잠금 상태와 해금을 위한 금액
	public EUnlockedState State = EUnlockedState.Hidden;
	public long SpentMoney = 0;
}
#endregion

public class SaveManager : Singleton<SaveManager>
{
	//실제 저장되는 객체
	private GameSaveData _saveData = new GameSaveData();

	//외부에서 읽을 수 있도록 프로퍼티 제공
	public GameSaveData SaveData => _saveData;

	//게임 데이터 파일이 저장될 경로
	//LocalLow->Companyname -> Projectname
	public string Path { get { return Application.persistentDataPath + "/SaveData.json"; } }

	private void Awake()
	{
		//저장 파일이 존재하면 오드, 없으면 초기화 후 바로 저장
		if (LoadGame() == false)
		{
			InitGame();
			SaveGame();
		}

		//씬이 바뀌어도 파괴X
		DontDestroyOnLoad(gameObject);
	}

	//초기화 함수
	public void InitGame()
	{
		//세이브 파일 없는지 검사
		if (File.Exists(Path))
			return;

		// 소지금.
		_saveData.Money = 5000;

		// 각종 업그레이드.

		// 스테이지 별 상태.
		const int MAX_STAGE = 10;
		const int MAX_PROPS = 20;

		_saveData.Restaurants = new List<RestaurantData>();
		for (int i = 0; i < MAX_STAGE; i++)
		{
			RestaurantData restaurantData = new RestaurantData();

			//UnlockableStateData를 프랍 개수만큼 생성해서 리스트에 추가
			restaurantData.UnlockableStates = new List<UnlockableStateData>();
			for (int j = 0; j < MAX_PROPS; j++)
				restaurantData.UnlockableStates.Add(new UnlockableStateData());

			//세이브 데이터 리스트에 추가
			_saveData.Restaurants.Add(restaurantData);
		}
	}

	//현재 세이브 데이터를 저장
	public void SaveGame()
	{
		string jsonStr = JsonUtility.ToJson(_saveData);
		File.WriteAllText(Path, jsonStr);
		Debug.Log($"Save Game Completed : {Path}");
	}

	//파일 불러오기
	public bool LoadGame()
	{
		if (File.Exists(Path) == false)
			return false;

		string fileStr = File.ReadAllText(Path);
		GameSaveData data = JsonUtility.FromJson<GameSaveData>(fileStr);

		//불러온 데이터로 교체
		if (data != null)
			_saveData = data;

		Debug.Log($"Save Game Loaded : {Path}");
		return true;
	}
}
