using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
// System.IO と Random は FormationDataHandler に移管したので、ここでは不要な場合があります。
// もしこのファイル内で直接ファイル操作やランダム値生成を再度行う場合は using を戻してください。

// CharacterDataクラスの定義は CharacterData.cs にある想定
// FormationSaveDataEntry, FormationSlotSaveData, AllFormationsSaveData は FormationSaveDataStructures.cs にある想定

public class FormationScreenController
{
    private VisualElement rootElement; // この編成画面全体のUIツリーのルート
    private List<Button> slotButtons;  // 上部の編成スロットボタン (1,2,3)

    // --- サブコントローラーとデータハンドラ ---
    private FormationDataHandler dataHandler;                 // 編成データの永続化とロジックを担当
    private CharacterListUIController characterListController;    // 右側のキャラクターリストUIを担当
    private PlacementAreaUIController placementAreaController;  // 左側の配置エリアUIとタップを担当
    private FormationDragDropController dragDropController;     // ドラッグ＆ドロップ操作を担当

    // --- 状態 ---
    private int currentFormationSlot = 1;                        // 現在編集中の編成スロット番号
    private CharacterData selectedCharacterForTapPlacement = null; // タップ配置用にリストで選択されたキャラ

    // --- 定数 ---
    private const int GRID_COLUMNS = 45; // 仮想グリッドの列数
    private const int GRID_ROWS = 54;    // 仮想グリッドの行数

    // イベントコールバックフィールド (Unregister用)
    private EventCallback<ClickEvent> slotButton1Callback, slotButton2Callback, slotButton3Callback;

    // コンストラクタ (MainScreenControllerから呼ばれる際に、必要な依存性を注入する形も検討可能)
    public FormationScreenController() { }

    public void Initialize(VisualElement formationScreenElement)
    {
        rootElement = formationScreenElement;
        if (rootElement == null) { Debug.LogError("FormationScreenController: rootElement is null in Initialize! Cannot proceed."); return; }

        // --- ダミーの所持キャラクターリスト作成 (本来は外部から渡されるか、データクラスで管理) ---
        List<CharacterData> ownedCharacters = CreateDummyOwnedCharacters();

        // --- 各サブコントローラーとデータハンドラの初期化 ---
        dataHandler = new FormationDataHandler(ownedCharacters); // ownedCharactersリストを渡して初期化

        VisualElement characterListContainerVE = rootElement.Q<ScrollView>("character-list-scrollview")?.Q<VisualElement>("character-list-container");
        VisualTreeAsset characterListItemAsset = Resources.Load<VisualTreeAsset>("UI_Toolkit/FormationScreen/CharacterListItem"); // パスを確認
        if (characterListItemAsset == null) { Debug.LogError("CharacterListItem.uxml (VisualTreeAsset) not found! Check path."); }
        characterListController = new CharacterListUIController(characterListContainerVE, characterListItemAsset, ownedCharacters);
        characterListController.PopulateList(); // リストUIを生成

        VisualElement placementAreaVE = rootElement.Q<VisualElement>("placement-area");
        if (placementAreaVE == null) { Debug.LogError("Placement Area ('placement-area') not found in UXML!"); }
        placementAreaController = new PlacementAreaUIController(placementAreaVE, ownedCharacters, GRID_COLUMNS, GRID_ROWS);

        VisualElement ghostIconVE = rootElement.Q<Image>("drag-ghost-icon");
        if (ghostIconVE == null) { Debug.LogWarning("Drag ghost icon ('drag-ghost-icon') not found in UXML. Dragging will not show a ghost."); }
        dragDropController = new FormationDragDropController(rootElement, ghostIconVE, placementAreaController);


        // --- イベントの購読 ---
        if (characterListController != null) {
            characterListController.OnCharacterListItemClickedEvent += HandleCharacterSelectedForTap;
            characterListController.OnListItemPointerDownEvent += dragDropController.StartDrag;
        }
        if (placementAreaController != null) {
            placementAreaController.OnGridCellTappedEvent += HandleGridCellTappedForPlacement;
        }
        if (dragDropController != null) {
            dragDropController.OnCharacterDroppedOnGridEvent += HandleCharacterDroppedOnGrid;
        }


        // --- 編成スロットボタンの初期化 ---
        slotButtons = new List<Button> {
            rootElement.Q<Button>("slot-button-1"),
            rootElement.Q<Button>("slot-button-2"),
            rootElement.Q<Button>("slot-button-3")
        };
        slotButton1Callback = evt => SelectFormationSlot(1);
        slotButton2Callback = evt => SelectFormationSlot(2);
        slotButton3Callback = evt => SelectFormationSlot(3);
        if (slotButtons[0] != null) slotButtons[0].RegisterCallback(slotButton1Callback);
        if (slotButtons[1] != null) slotButtons[1].RegisterCallback(slotButton2Callback);
        if (slotButtons[2] != null) slotButtons[2].RegisterCallback(slotButton3Callback);

        // --- 初期表示 ---
        SelectFormationSlot(currentFormationSlot);
    }

    private void SelectFormationSlot(int slotIndex)
    {
        Debug.Log($"FormationScreenController: Slot {slotIndex} selected.");
        currentFormationSlot = slotIndex;
        // UI上のボタンのアクティブ表示更新
        for (int i = 0; i < slotButtons.Count; i++) {
            if (slotButtons[i] != null) {
                slotButtons[i].EnableInClassList("active-slot-button", (i + 1) == slotIndex);
            }
        }
        // データハンドラから該当スロットの編成データを取得し、配置エリアUIを更新
        if (dataHandler != null && placementAreaController != null) {
            List<PlacedCharacterInfoForSave> formation = dataHandler.GetFormationForSlot(slotIndex);
            placementAreaController.RedrawFormation(formation);
        } else {
            Debug.LogError("DataHandler or PlacementAreaController is null in SelectFormationSlot.");
        }
    }

    // キャラクターリストでキャラクターが選択された (タップ配置用)
    private void HandleCharacterSelectedForTap(CharacterData charData, VisualElement listItem)
    {
        selectedCharacterForTapPlacement = charData;
        // 選択ハイライトはCharacterListUIControllerが行う
    }

    // 配置エリアのグリッドセルがタップされた
    private void HandleGridCellTappedForPlacement(Vector2Int gridPos)
    {
        if (dataHandler == null || placementAreaController == null) return;

        if (selectedCharacterForTapPlacement != null) { // キャラが選択中なら配置
            dataHandler.UpdateCharacterInFormation(currentFormationSlot, gridPos.x, gridPos.y, selectedCharacterForTapPlacement.id);
            placementAreaController.RedrawFormation(dataHandler.GetFormationForSlot(currentFormationSlot));
            if (characterListController != null) characterListController.DeselectCurrentListItem();
            selectedCharacterForTapPlacement = null;
        } else { // キャラ未選択なら、そのグリッドのキャラを削除
            List<PlacedCharacterInfoForSave> currentFormation = dataHandler.GetFormationForSlot(currentFormationSlot);
            PlacedCharacterInfoForSave existingChar = currentFormation.FirstOrDefault(pci => pci.GetGridPosition() == gridPos);
            if (existingChar != null) {
                dataHandler.RemoveCharacterFromFormation(currentFormationSlot, existingChar.characterId, gridPos.x, gridPos.y);
                placementAreaController.RedrawFormation(dataHandler.GetFormationForSlot(currentFormationSlot));
            }
        }
    }

    // ドラッグ＆ドロップでキャラクターが配置エリアにドロップされた
    private void HandleCharacterDroppedOnGrid(CharacterData charData, Vector2Int gridPos)
    {
        Debug.Log($"FormationScreenController: '{charData.displayName}' dropped on grid {gridPos}.");
        if (dataHandler == null || placementAreaController == null) return;

        dataHandler.UpdateCharacterInFormation(currentFormationSlot, gridPos.x, gridPos.y, charData.id);
        placementAreaController.RedrawFormation(dataHandler.GetFormationForSlot(currentFormationSlot));
    }


    // ダミーの所持キャラクターリスト作成
    private List<CharacterData> CreateDummyOwnedCharacters()
    {
        List<CharacterData> characters = new List<CharacterData>();
        string dummyIconResourcePath = "UI_Toolkit_Icons/sample_character_icon";
        Texture2D dummyIconTexture = Resources.Load<Texture2D>(dummyIconResourcePath);
        if (dummyIconTexture == null) { Debug.LogWarning($"Dummy icon texture not found at Resources/{dummyIconResourcePath}. Please create this file and import as Texture2D."); }

        for (int i = 0; i < 10; i++) {
            characters.Add(new CharacterData {
                id = "char_owned_" + i, displayName = "所持キャラ " + (i + 1),
                icon = dummyIconTexture, iconResourcePath = dummyIconResourcePath,
                hp = Random.Range(200, 500), atk = Random.Range(50, 150),
                spd = Random.Range(20, 40), attackSpeed = Mathf.Round(Random.Range(0.5f, 1.5f) * 10f) / 10f
            });
        }
        return characters;
    }


    // BattleManagerから現在の編成データを取得するために呼ばれる
    public List<CharacterData> GetCurrentActiveFormationForBattle() // メソッド名を変更して統一
    {
        if (dataHandler != null) {
            return dataHandler.GetCharacterDataListForBattle(currentFormationSlot);
        }
        Debug.LogWarning("FormationScreenController: dataHandler is null. Returning empty list for battle formation.");
        return new List<CharacterData>();
    }

    public void UnregisterCallbacks()
    {
        Debug.Log("FormationScreenController.UnregisterCallbacks() called.");
        // スロットボタン
        if (slotButtons[0] != null && slotButton1Callback != null) slotButtons[0].UnregisterCallback<ClickEvent>(slotButton1Callback);
        if (slotButtons[1] != null && slotButton2Callback != null) slotButtons[1].UnregisterCallback<ClickEvent>(slotButton2Callback);
        if (slotButtons[2] != null && slotButton3Callback != null) slotButtons[2].UnregisterCallback<ClickEvent>(slotButton3Callback);

        // サブコントローラーのイベント購読解除
        if (characterListController != null) {
            characterListController.OnCharacterListItemClickedEvent -= HandleCharacterSelectedForTap;
            if (dragDropController != null) { // dragDropControllerがnullでないことを確認
                characterListController.OnListItemPointerDownEvent -= dragDropController.StartDrag;
            }
        }
        if (placementAreaController != null) {
            placementAreaController.OnGridCellTappedEvent -= HandleGridCellTappedForPlacement;
            placementAreaController.UnregisterCallbacks(); // PlacementAreaUIController内部のイベントも解除
        }
        if (dragDropController != null) {
            dragDropController.OnCharacterDroppedOnGridEvent -= HandleCharacterDroppedOnGrid;
            dragDropController.UnregisterGlobalCallbacks();
        }
    }
    public List<CharacterData> GetCurrentActiveFormationForBattle()
{
    if (dataHandler != null)
    {
        return dataHandler.GetCharacterDataListForBattle(currentFormationSlot);
    }
    Debug.LogWarning("FormationScreenController: dataHandler is null. Returning empty list for battle formation.");
    return new List<CharacterData>();
}
}