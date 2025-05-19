using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    public BattleSetup currentBattleSetup;

    [HideInInspector] public List<Character> playerTeamObjects = new List<Character>();
    [HideInInspector] public List<Character> enemyTeamObjects = new List<Character>();

    private enum BattleState { NotStarted, Ongoing, PlayerWin, EnemyWin }
    private BattleState currentState = BattleState.NotStarted;

    private MainScreenController mainScreenController;

    void Awake()
    {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        mainScreenController = Object.FindFirstObjectByType<MainScreenController>();
        if (mainScreenController == null) {
            Debug.LogError("BattleManager: MainScreenController not found in the scene!");
        }

        if (currentBattleSetup == null) {
            LogToBattleConsole("Error: BattleSetup is not assigned in BattleManager Inspector!");
            return;
        }

        SetupBattlefield();
    }

    void SetupBattlefield()
    {
        LogToBattleConsole("Setting up battlefield...");
        currentState = BattleState.NotStarted;
        ClearExistingCharacters();
        SetupEnemyTeam();
        SetupPlayerTeamFromFormation();

        if (playerTeamObjects.Count > 0 && enemyTeamObjects.Count > 0) {
            StartBattle();
        } else if (playerTeamObjects.Count == 0 && enemyTeamObjects.Count > 0) {
            LogToBattleConsole("Player team is empty. Battle cannot start. Enemy wins by default.");
            currentState = BattleState.EnemyWin;
        } else if (enemyTeamObjects.Count == 0 && playerTeamObjects.Count > 0) {
            LogToBattleConsole("Enemy team is empty. Battle cannot start. Player wins by default.");
            currentState = BattleState.PlayerWin;
        } else {
             LogToBattleConsole("Both teams are empty. Battle cannot start.");
        }
    }

    void ClearExistingCharacters()
    {
        foreach (Character character in playerTeamObjects.Where(c => c != null)) Destroy(character.gameObject);
        playerTeamObjects.Clear();
        foreach (Character character in enemyTeamObjects.Where(c => c != null)) Destroy(character.gameObject);
        enemyTeamObjects.Clear();
    }

    void SetupEnemyTeam()
    {
        if (currentBattleSetup == null || currentBattleSetup.characterPlacements == null) {
            LogToBattleConsole("Cannot setup enemy team: BattleSetup or characterPlacements is null.");
            return;
        }
        foreach (CharacterPlacementData placementData in currentBattleSetup.characterPlacements) {
            if (placementData.team == Team.Enemy) {
                if (placementData.characterPrefab == null) {
                    Debug.LogWarning("Enemy characterPrefab in placement data is null. Skipping.");
                    continue;
                }
                GameObject charGO = Instantiate(placementData.characterPrefab, placementData.initialPosition, Quaternion.identity);
                Character character = charGO.GetComponent<Character>();
                if (character != null) {
                    character.team = placementData.team;
                    enemyTeamObjects.Add(character);
                    if (!string.IsNullOrEmpty(placementData.characterNameOverride)) charGO.name = placementData.characterNameOverride;
                } else {
                    Debug.LogError($"Prefab {placementData.characterPrefab.name} for enemy does not have a Character component!");
                    Destroy(charGO);
                }
            }
        }
        LogToBattleConsole($"Enemy team setup with {enemyTeamObjects.Count} members.");
    }

    void SetupPlayerTeamFromFormation()
    {
        if (mainScreenController == null) {
            LogToBattleConsole("Error: MainScreenController not available to get formation data for player team.");
            return;
        }

        List<CharacterData> playerFormationData = mainScreenController.GetCurrentFormationDataForBattle();
        LogToBattleConsole($"Setting up player team from formation. Found {playerFormationData.Count} characters in formation data.");

        if (playerFormationData != null) {
            int playerSpawnIndex = 0;
            foreach (CharacterData charDataFromUI in playerFormationData) {
                if (charDataFromUI == null) continue;

                GameObject characterPrefab = GetBattleCharacterPrefabFromData(charDataFromUI);
                if (characterPrefab != null) {
                    Vector3 spawnPosition = CalculatePlayerSpawnPosition(playerSpawnIndex);
                    GameObject charGO = Instantiate(characterPrefab, spawnPosition, Quaternion.identity);
                    Character battleCharacter = charGO.GetComponent<Character>();
                    if (battleCharacter != null) {
                        battleCharacter.stats.characterName = charDataFromUI.displayName;
                        battleCharacter.stats.maxHp = charDataFromUI.hp;
                        battleCharacter.stats.attackPower = charDataFromUI.atk;
                        // ここで他のステータスも設定
                        // battleCharacter.stats.moveSpeed = ...; // CharacterDataにあれば
                        // battleCharacter.stats.detectionRange = ...; // CharacterDataにあれば
                        // battleCharacter.stats.attackRange = ...; // CharacterDataにあれば
                        battleCharacter.stats.Initialize(); // HPなどを初期化
                        battleCharacter.team = Team.Player;
                        playerTeamObjects.Add(battleCharacter);
                        playerSpawnIndex++;
                    } else {
                        Debug.LogError($"Instantiated player prefab {characterPrefab.name} does not have a Character component!");
                        Destroy(charGO);
                    }
                } else {
                    LogToBattleConsole($"Warning: Could not find or load battle prefab for player character data ID: {charDataFromUI.id}");
                }
            }
        }
        LogToBattleConsole($"Player team setup with {playerTeamObjects.Count} actual members.");
    }

    private GameObject GetBattleCharacterPrefabFromData(CharacterData uiCharData)
    {
        if (uiCharData == null || string.IsNullOrEmpty(uiCharData.id)) return null;
        string prefabPath = "BattleCharacters/" + uiCharData.id;
        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        if (prefab == null) {
            Debug.LogWarning($"Battle character prefab not found at Resources/{prefabPath}. Ensure prefab exists and path is correct.");
        }
        return prefab;
    }

    private Vector3 CalculatePlayerSpawnPosition(int index)
    {
        return new Vector3(-5f + (index * 2.0f), -2f, 0); // 仮の配置ロジック
    }

    public void StartBattle()
    {
        if (currentState == BattleState.Ongoing) {
            LogToBattleConsole("Battle is already ongoing.");
            return;
        }
        if (playerTeamObjects.Count == 0 || enemyTeamObjects.Count == 0) {
            LogToBattleConsole("Cannot start battle: One or both teams are empty.");
            return;
        }
        LogToBattleConsole("Battle Started!");
        currentState = BattleState.Ongoing;
    }

    public Character FindNearestTargetInDetectionRange(Character attacker, float detectionRange)
    {
        List<Character> potentialTargets;
        if (attacker.team == Team.Player) {
            potentialTargets = enemyTeamObjects.Where(e => e != null && e.IsAlive()).ToList();
        } else {
            potentialTargets = playerTeamObjects.Where(p => p != null && p.IsAlive()).ToList();
        }
        Character nearestTarget = null;
        float minDistanceSqr = detectionRange * detectionRange + 1f;
        foreach (Character target in potentialTargets) {
            if (target == null || !target.gameObject.activeInHierarchy) continue; // 念のためアクティブチェック
            float distanceSqr = (attacker.transform.position - target.transform.position).sqrMagnitude;
            if (distanceSqr <= (detectionRange * detectionRange) && distanceSqr < minDistanceSqr) {
                minDistanceSqr = distanceSqr;
                nearestTarget = target;
            }
        }
        return nearestTarget;
    }

    public void OnCharacterDied(Character deadCharacter)
    {
        if (currentState != BattleState.Ongoing) return;
        LogToBattleConsole($"{deadCharacter.stats.characterName} ({deadCharacter.team}) has been defeated!");
        CheckBattleEnd();
    }

    void CheckBattleEnd()
    {
        if (currentState != BattleState.Ongoing) return;
        bool allPlayerTeamDead = playerTeamObjects.All(p => p == null || !p.IsAlive());
        bool allEnemyTeamDead = enemyTeamObjects.All(e => e == null || !e.IsAlive());
        if (allEnemyTeamDead && playerTeamObjects.Any(p => p != null && p.IsAlive())) {
            currentState = BattleState.PlayerWin;
            LogToBattleConsole("Player Team Wins!");
        } else if (allPlayerTeamDead && enemyTeamObjects.Any(e => e != null && e.IsAlive())) {
            currentState = BattleState.EnemyWin;
            LogToBattleConsole("Enemy Team Wins! Game Over.");
        } else if (allPlayerTeamDead && allEnemyTeamDead) {
            LogToBattleConsole("All characters defeated! It's a draw?");
        }
    }

    public bool IsBattleOngoing()
    {
        return currentState == BattleState.Ongoing;
    }

    void LogToBattleConsole(string message)
    {
        Debug.Log($"[BattleManagerLog] {message}");
        if (mainScreenController != null) {
            // mainScreenController.UpdateBattleLogOnUI(message); // UI ToolkitのLabelに表示する場合
        }
    }
}