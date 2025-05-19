// Scripts/PlayerFormation.cs
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PlayerCharacterPlacement
{
    public GameObject playerCharacterPrefab; // 使用するプレイヤーキャラクターのプレハブ
    public Vector2 relativePosition;       // 陣形の基準点からの相対位置
    public string characterNameOverride = ""; // (オプション)
    // 必要であれば、どのスロットのキャラクターかを示すIDなども追加可能
}

[CreateAssetMenu(fileName = "NewPlayerFormation", menuName = "Battle/Player Formation")]
public class PlayerFormation : ScriptableObject
{
    public string formationName = "Default Formation"; // UI表示用
    public Sprite formationIcon; // (オプション) UIで表示するアイコン
    public List<PlayerCharacterPlacement> playerPlacements = new List<PlayerCharacterPlacement>();
    // プレイヤーキャラクターの最大数なども定義できる
    // public int maxPlayerCharacters = 3;
}