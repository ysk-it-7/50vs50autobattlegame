using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlacedCharacterInfoForSave
{
    public string characterId;
    public int gridX;
    public int gridY;
    public PlacedCharacterInfoForSave() {}
    public PlacedCharacterInfoForSave(string id, Vector2Int pos) { characterId = id; gridX = pos.x; gridY = pos.y; }
    public Vector2Int GetGridPosition() { return new Vector2Int(gridX, gridY); }
}

[System.Serializable]
public class FormationSlotSaveData
{
    public int slotNumber;
    public List<PlacedCharacterInfoForSave> placedCharacters = new List<PlacedCharacterInfoForSave>();
}

[System.Serializable]
public class AllFormationsSaveData
{
    public List<FormationSlotSaveData> allFormations = new List<FormationSlotSaveData>();
}