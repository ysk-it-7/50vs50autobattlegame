// Scripts/CharacterPlacementData.cs
using UnityEngine;

[System.Serializable] // Inspectorで表示・編集可能にする
public class CharacterPlacementData
{
    public GameObject characterPrefab; // 配置するキャラクターのプレハブ
    public Vector2 initialPosition;    // 初期配置位置
    public Team team;                  // キャラクターのチーム
    public string characterNameOverride = ""; // (オプション) 名前を上書きする場合

    // (オプション) 初期向きなどの他のパラメータもここに追加可能
    // public Vector3 initialRotation;
}