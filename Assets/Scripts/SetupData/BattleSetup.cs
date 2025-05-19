// Scripts/BattleSetup.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewBattleSetup", menuName = "Battle/Battle Setup")]
public class BattleSetup : ScriptableObject
{
    public List<CharacterPlacementData> characterPlacements = new List<CharacterPlacementData>();
}