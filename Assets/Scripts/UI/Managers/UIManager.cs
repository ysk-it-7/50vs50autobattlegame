// Scripts/UIManager.cs
using UnityEngine;
using UnityEngine.UI; // Buttonを使うため

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Main Menu UI")]
    public GameObject mainMenuPanel; // InspectorでMainMenuPanelをアサイン
    public Button mainMenuButton;    // InspectorでMainMenuButtonをアサイン

    [Header("Formation Selection UI")]
    public Button openFormationSelectionButton; // InspectorでOpenFormationSelectionButtonをアサイン
    // FormationSelectionManagerへの参照を持つか、直接FormationSelectionUIを制御する
    // ここではFormationSelectionManagerのメソッドを呼ぶ形にする
    // public GameObject formationSelectionPanel; // FormationSelectionManagerが制御するので不要な場合も

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 初期状態ではメインメニューパネルを非表示
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        // メインメニューボタンのクリックイベント設定
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ToggleMainMenu);

        // メインメニュー内の「フォーメーション」ボタンのクリックイベント設定
        if (openFormationSelectionButton != null)
            openFormationSelectionButton.onClick.AddListener(ShowFormationSelectionUI);
    }

    public void ToggleMainMenu()
    {
        Debug.Log("MainMenuButton Clicked - Toggling MainMenuPanel");
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(!mainMenuPanel.activeSelf);
            // メインメニュー表示時は他の操作を無効にする (Time.timeScale = 0 など) かもしれない
            // if (mainMenuPanel.activeSelf) Time.timeScale = 0f;
            // else Time.timeScale = 1f;
        }
    }

    public void ShowFormationSelectionUI()
    {
        Debug.Log("OpenFormationSelectionButton Clicked - Showing Formation Selection");
        // まずメインメニューを閉じる（オプション）
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        // FormationSelectionManagerにUI表示を依頼
        if (FormationSelectionManager.Instance != null)
        {
            FormationSelectionManager.Instance.ShowFormationSelection();
        }
        else
        {
            Debug.LogError("FormationSelectionManager instance not found!");
        }
    }

    // 他のUIパネルの表示/非表示メソッドもここに追加できる
}