using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class FormationDataHandler
{
    private Dictionary<int, List<PlacedCharacterInfoForSave>> formationSlotsData = new Dictionary<int, List<PlacedCharacterInfoForSave>>();
    private const int MAX_PLACEMENT_SLOTS = 6; // FormationScreenControllerと合わせる
    private const int GRID_COLUMNS = 45; // FormationScreenControllerと合わせる

    private string saveFilePath;
    private List<CharacterData> ownedCharactersProvider; // 所持キャラクターリストへの参照

    public FormationDataHandler(List<CharacterData> ownedCharacters)
    {
        ownedCharactersProvider = ownedCharacters;
        saveFilePath = Path.Combine(Application.persistentDataPath, "formations_data_v3.json");
        InitializeFormationSlotsDataInMemory();
        LoadAllFormationsFromFile();
    }

    private void InitializeFormationSlotsDataInMemory()
    {
        for (int i = 1; i <= 3; i++) { // 3つの編成スロット
            if (!formationSlotsData.ContainsKey(i)) {
                formationSlotsData[i] = new List<PlacedCharacterInfoForSave>();
            }
        }
    }

    public List<PlacedCharacterInfoForSave> GetFormationForSlot(int slotNumber)
    {
        if (formationSlotsData.TryGetValue(slotNumber, out List<PlacedCharacterInfoForSave> formation)) {
            return formation; // 参照を返す
        }
        Debug.LogWarning($"FormationDataHandler: No data for slot {slotNumber}, returning new empty list.");
        var newList = new List<PlacedCharacterInfoForSave>();
        formationSlotsData[slotNumber] = newList; // メモリにも確保
        return newList;
    }

    public void UpdateCharacterInFormation(int slotNumber, int gridX, int gridY, string characterId)
    {
        if (!formationSlotsData.ContainsKey(slotNumber)) InitializeFormationSlotsDataInMemory(); //念のため

        List<PlacedCharacterInfoForSave> formation = formationSlotsData[slotNumber];
        // まず同じグリッド位置の既存キャラを削除
        formation.RemoveAll(pci => pci.gridX == gridX && pci.gridY == gridY);

        if (!string.IsNullOrEmpty(characterId)) // characterIdがnullや空でなければ追加
        {
            // 同じキャラが他の位置にいたら削除 (1編成に同キャラ1体までの場合)
            formation.RemoveAll(pci => pci.characterId == characterId);
            formation.Add(new PlacedCharacterInfoForSave(characterId, new Vector2Int(gridX, gridY)));
        }
        SaveFormationToFileBySlot(slotNumber);
    }

    public void RemoveCharacterFromFormation(int slotNumber, string characterId, int gridX, int gridY)
    {
        if (!formationSlotsData.ContainsKey(slotNumber)) return;
        formationSlotsData[slotNumber].RemoveAll(pci => pci.characterId == characterId && pci.gridX == gridX && pci.gridY == gridY);
        SaveFormationToFileBySlot(slotNumber);
    }

    public void ClearFormationSlot(int slotNumber)
    {
        if (formationSlotsData.ContainsKey(slotNumber)) {
            formationSlotsData[slotNumber].Clear();
            SaveFormationToFileBySlot(slotNumber);
        }
    }


    private void SaveFormationToFileBySlot(int slotNumber)
    {
        if (!formationSlotsData.ContainsKey(slotNumber)) return;
        AllFormationsSaveData allData = LoadAllFormationsDataFromFileObject();
        FormationSlotSaveData slotDataToSave = allData.allFormations.FirstOrDefault(f => f.slotNumber == slotNumber);
        if (slotDataToSave == null) {
            slotDataToSave = new FormationSlotSaveData { slotNumber = slotNumber, placedCharacters = new List<PlacedCharacterInfoForSave>() };
            allData.allFormations.Add(slotDataToSave);
        }
        slotDataToSave.placedCharacters.Clear();
        slotDataToSave.placedCharacters.AddRange(formationSlotsData[slotNumber]); // リストをそのままコピー

        try {
            string json = JsonUtility.ToJson(allData, true);
            File.WriteAllText(saveFilePath, json);
            Debug.Log($"FormationDataHandler: Slot {slotNumber} saved to {saveFilePath}");
        } catch (System.Exception e) { Debug.LogError($"Save error: {e.Message}"); }
    }

    private void LoadAllFormationsFromFile()
    {
        AllFormationsSaveData allData = LoadAllFormationsDataFromFileObject();
        for (int i = 1; i <= 3; i++) { // 全スロットを初期化またはロード
            if (!formationSlotsData.ContainsKey(i)) {
                 formationSlotsData[i] = new List<PlacedCharacterInfoForSave>();
            }
            formationSlotsData[i].Clear(); // メモリ上の既存データをクリア

            FormationSlotSaveData loadedSlotData = allData.allFormations.FirstOrDefault(sd => sd.slotNumber == i);
            if (loadedSlotData != null && loadedSlotData.placedCharacters != null) {
                foreach (var savedEntry in loadedSlotData.placedCharacters) {
                    // ownedCharactersProvider を使って、CharacterDataインスタンスの有効性を確認できるが、
                    // ここでは保存されたIDと座標をそのまま復元する。
                    // 表示時に ownedCharactersProvider から CharacterData を引く。
                    formationSlotsData[i].Add(new PlacedCharacterInfoForSave(savedEntry.characterId, savedEntry.GetGridPosition()));
                }
            }
        }
        Debug.Log($"FormationDataHandler: All formations loaded from {saveFilePath}");
    }

    private AllFormationsSaveData LoadAllFormationsDataFromFileObject()
    {
        if (File.Exists(saveFilePath)) {
            try {
                string json = File.ReadAllText(saveFilePath);
                if (!string.IsNullOrEmpty(json)) {
                    AllFormationsSaveData data = JsonUtility.FromJson<AllFormationsSaveData>(json);
                    if (data != null) {
                        if (data.allFormations == null) data.allFormations = new List<FormationSlotSaveData>();
                        return data;
                    }
                }
            } catch (System.Exception e) { Debug.LogError($"Error loading formation data: {e.Message}"); }
        }
        var newData = new AllFormationsSaveData();
        if (newData.allFormations == null) newData.allFormations = new List<FormationSlotSaveData>();
        return newData;
    }

    // BattleManager が呼び出す用 (CharacterDataのリストを返す)
    public List<CharacterData> GetCharacterDataListForBattle(int slotNumber)
    {
        List<CharacterData> battleFormation = new List<CharacterData>();
        if (formationSlotsData.TryGetValue(slotNumber, out List<PlacedCharacterInfoForSave> placedInfos)) {
            if (placedInfos != null) {
                foreach (var pInfo in placedInfos) {
                    CharacterData charMasterData = ownedCharactersProvider.FirstOrDefault(cd => cd.id == pInfo.characterId);
                    if (charMasterData != null) {
                        // アイコンがロードされていなければロード
                        if (charMasterData.icon == null && !string.IsNullOrEmpty(charMasterData.iconResourcePath)) {
                             charMasterData.icon = Resources.Load<Texture2D>(charMasterData.iconResourcePath);
                        }
                        battleFormation.Add(charMasterData);
                    }
                }
            }
        }
        return battleFormation;
    }
}