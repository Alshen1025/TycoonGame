using UnityEngine;

//오브젝트 상태
public enum EUnlockedState
{
	Hidden,
	ProcessingConstruction,
	Unlocked
}

public class UnlockableBase : MonoBehaviour
{
	//건설 UI
	public UI_ConstructionArea ConstructionArea;
	//세이브 파일을 만들 때 필요한 데이터 저장
	private UnlockableStateData _data;

	//세이브 데이터에 따라서 세팅
	public void SetInfo(UnlockableStateData data)
	{
		_data = data;
		SetUnlockedState(data.State);
		ConstructionArea.RefreshUI();
	}

	//Get, Set 프로퍼티
	EUnlockedState State
	{
		get { return _data.State; }
		set { _data.State = value; }
	}

	//현재 잠금 상태 여부
	public bool IsUnlocked => State == EUnlockedState.Unlocked;
	//지금까지 건설된 금액
	public long SpentMoney
	{
		get { return _data != null ? _data.SpentMoney : 0;}
		set { if (_data != null) { _data.SpentMoney = value; } }
	}

	//상태를 설정, 각 상태에 따라 오브젝트를 보여줄지 건설 UI보여줄지 결정
	public void SetUnlockedState(EUnlockedState state)
	{
		State = state;

		if (state == EUnlockedState.Hidden)
		{
			gameObject.SetActive(false);
			ConstructionArea.gameObject.SetActive(false);
		}
		else if (state == EUnlockedState.ProcessingConstruction)
		{
			gameObject.SetActive(false);
			ConstructionArea.gameObject.SetActive(true);
		}
		else
		{
			gameObject.SetActive(true);
			ConstructionArea.gameObject.SetActive(false);
		}
	}
}
