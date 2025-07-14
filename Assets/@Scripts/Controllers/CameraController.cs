using UnityEngine;

public class CameraController : MonoBehaviour
{
	//위치를 추적할 오브젝트(플레이어)
    [SerializeField]
    private Transform _target;

	//카메라와 플레이어간의 거리
    private Vector3 _offset;

    void Start()
    {
		//카메라와 타깃의 초기 거리를 저장
		_offset = transform.position - _target.position;
    }

    void LateUpdate()
    {
		//부드럽게 따라가고 싶으면 Lerp사용도 가능
        transform.position = _offset + _target.position;
    }
}