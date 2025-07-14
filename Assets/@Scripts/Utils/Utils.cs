using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;


public static class Utils
{
	//Component가 있는지 확인해서 있으면 가져오고 없으면 추가
	public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
	{
		T component = go.GetComponent<T>();
		if (component == null)
			component = go.AddComponent<T>();

		return component;
	}

	//게임 오브젝트만 반환하는 FindChild
	public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
	{
		// 내부적으로 FindChild<T>를 호출해 Transform을 찾기
		Transform transform = FindChild<Transform>(go, name, recursive);
		if (transform == null)
			return null;

		return transform.gameObject;
	}

	public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Component
	{
		//찾을 
		if (go == null)
			return null;

		//재귀 참색이 아니면
		if (recursive == false)
		{
			//자식들만 순회
			for (int i = 0; i < go.transform.childCount; i++)
			{
				Transform transform = go.transform.GetChild(i);

				//name이 없거나 일치하면
				if (string.IsNullOrEmpty(name) || transform.name == name)
				{
					//해당 자식에게서 찾으려는 타입의 컴포넌트를 찾아서 반환
					T component = transform.GetComponent<T>();
					if (component != null)
						return component;
				}
			}
		}
		//재귀 탐색(자손 오브젝트 까지 검사하는 경우)
		else
		{
			//자식의 하위 오브젝트에서 찾음
			foreach (T component in go.GetComponentsInChildren<T>())
			{
				if (string.IsNullOrEmpty(name) || component.gameObject.name == name)
					return component;
			}
		}
		//못찾으면 null반환
		return null;
	}
	//문자열을 열거형으로 변환
	public static T ParseEnum<T>(string value)
	{
		return (T)Enum.Parse(typeof(T), value, true);
	}

	public static EObjectType GetTrayObjectType(Transform t)
	{
		switch (t.gameObject.tag)
		{
			case "Trash":
				return EObjectType.Trash;
			case "Burger":
				return EObjectType.Burger;
			case "Money":
				return EObjectType.Money;
		}

		return EObjectType.None;
	}

	//가지고 있는 돈을 String으로 반환
	//0은 반드시 출력해야 하는 숫자 자리
	//##있으면 출력하고, 없으면 생략하는 자리
	public static string GetMoneyText(long money)
	{
		if (money < 1000) return money.ToString();
		if (money < 1000000) return (money / 1000f).ToString("0.##") + "k"; // (k)
		if (money < 1000000000) return (money / 1000000f).ToString("0.##") + "m"; // (m)
		if (money < 1000000000000) return (money / 1000000000f).ToString("0.##") + "b"; // (b)
		return (money / 1000000000000f).ToString("0.##") + "t"; // (t)
	}
}