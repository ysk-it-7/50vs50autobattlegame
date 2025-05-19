// Scripts/FormationSelectionManager.cs
using UnityEngine;
using UnityEngine.UI; // Buttonを使うため
using System.Collections.Generic;

public class FormationSelectionManager : MonoBehaviour
{
    public static FormationSelectionManager Instance { get; private set; }

    public List<PlayerFormation> availableFormations; // Inspectorで3つのPlayerFormationアセットをアサイン
    public GameObject formationSelectionUI;           // 配置選択UIの親GameObject
    public Button[] formationButtons;                 // Inspectorで3つのボタンをアサイン

    public PlayerFormation SelectedPlayerFormation { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // シーンをまたいで選択を保持する場合
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (availableFormations.Count != formationButtons.Length)
        {
            Debug.LogError("Available formations count does not match formation buttons count!");
            return;
        }

        for (int i = 0; i < formationButtons.Length; i++)
        {
            int index = i; // クロージャのためのローカルコピー
            formationButtons[i].onClick.AddListener(() => SelectFormation(index));
            // ボタンのテキストやアイコンをformationデータから設定 (オプション)
            Text buttonText = formationButtons[i].GetComponentInChildren<Text>(); // TextMeshProUGUIの場合はそちらを取得
            if (buttonText != null && index < availableFormations.Count && availableFormations[index] != null)
            {
                buttonText.text = availableFormations[index].formationName;
            }
        }

        // 初期状態ではUIを非表示にするか、またはゲーム開始時に表示する
        // formationSelectionUI.SetActive(false);
        // デフォルトフォーメーションを選択しておく (最初のものなど)
        if (availableFormations.Count > 0)
        {
            SelectedPlayerFormation = availableFormations[0]; // デフォルト
        }
    }

    public void ShowFormationSelection() // UIManagerから呼ばれる
    {
        if (formationSelectionUI != null)
        {
            formationSelectionUI.SetActive(true);
            Debug.Log("Formation Selection UI Shown");
        }
        else
        {
            Debug.LogError("FormationSelectionUI is not assigned in FormationSelectionManager!");
        }
        // 必要であれば、ここで他のUIを非表示にするなどの処理も追加
    }

    public void SelectFormation(int formationIndex)
    {
        if (formationIndex < 0 || formationIndex >= availableFormations.Count)
        {
            Debug.LogError("Invalid formation index: " + formationIndex);
            return;
        }

        SelectedPlayerFormation = availableFormations[formationIndex];
        Debug.Log("Player formation selected: " + SelectedPlayerFormation.formationName);

        formationSelectionUI.SetActive(false); // 選択したらUIを閉じる

        // BattleManagerに戦闘開始を通知、またはBattleManagerがこの情報を参照する
        // 例: BattleManager.Instance.StartBattleWithFormation(SelectedPlayerFormation);
        // または、BattleManagerがStart時にこのSelectedPlayerFormationを参照する
        // 今回は後者のアプローチ（BattleManagerが参照）で進めます。

        if (formationSelectionUI != null)
            formationSelectionUI.SetActive(false); // 選択したらUIを閉じる
    }

    // ゲーム開始時やステージ選択後などにこのメソッドを呼んでUIを表示する
    // public void PrepareForBattle() { ShowFormationSelection(); }
}