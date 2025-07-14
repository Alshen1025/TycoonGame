using UnityEngine;
using UnityEngine.UI;

public class UI_Menu : MonoBehaviour
{
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _quitButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _saveButton.onClick.AddListener(OnSaveClicked);
        _quitButton.onClick.AddListener(OnQuitClicked);
    }

     // 저장 버튼 클릭 시
    private void OnSaveClicked()
    {
        SaveManager.Instance.SaveGame();
        Debug.Log("게임이 저장되었습니다.");
    }

    // 종료 버튼 클릭 시
    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

