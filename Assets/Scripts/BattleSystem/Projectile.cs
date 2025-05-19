// Scripts/Projectile.cs
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public int damage;
    private Character target;
    private Team attackerTeam; // どのチームが撃ったか

    public GameObject impactEffectPrefab; // (オプション) 着弾エフェクト

    public void Initialize(Character targetCharacter, int projectileDamage, Team teamOfAttacker)
    {
        target = targetCharacter;
        damage = projectileDamage;
        attackerTeam = teamOfAttacker;
    }

    void Update()
    {
        if (target == null || !target.IsAlive())
        {
            // ターゲットがいない、または死んでいる場合は消滅
            Destroy(gameObject);
            return;
        }

        Vector3 direction = (target.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        transform.right = direction; // プロジェクタイルをターゲットの方向に向ける（スプライトによる）

        // ターゲットとの距離をチェックして、近すぎたら衝突とみなす
        if (Vector3.Distance(transform.position, target.transform.position) < 0.5f) // 0.5f は適宜調整
        {
            HitTarget();
        }
    }

    void HitTarget()
    {
        if (target != null && target.IsAlive())
        {
            // 敵チームのProjectileのみダメージを与える
            if (target.team != attackerTeam)
            {
                Debug.Log($"{gameObject.name} hit {target.stats.characterName} for {damage} damage.");
                target.TakeDamage(damage);
            }
            else
            {
                // 味方に当たった場合は何もしないか、別の処理 (例: 回復弾なら回復)
                Debug.Log($"{gameObject.name} hit friendly target {target.stats.characterName}, no damage dealt.");
            }
        }

        if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    // (オプション) 衝突判定を使う場合 (Collider2DとRigidbody2Dが必要)
    /*
    void OnTriggerEnter2D(Collider2D other)
    {
        if (target == null) return; // 既に処理済みかターゲットがいない

        Character hitCharacter = other.GetComponent<Character>();
        if (hitCharacter != null && hitCharacter == target)
        {
            HitTarget();
        }
        // 壁などに当たった場合も消滅させるなら
        // else if (other.gameObject.CompareTag("Wall")) { Destroy(gameObject); }
    }
    */
}