using UnityEngine;

[System.Serializable]
public class CharacterStats
{
    public string characterName = "Character";
    public int maxHp = 100;
    public int currentHp;
    public int attackPower = 10;
    public float attackCooldown = 2.0f; // 次の攻撃までの時間

    // 新しいステータス
    public float moveSpeed = 3f;
    public float patrolSpeed;
    public float detectionRange = 10f; // 敵を認識する範囲
    public float attackRange = 2f;     // 攻撃を開始する距離

    public void Initialize()
    {
        currentHp = maxHp;
        patrolSpeed = moveSpeed/2;
    }
}