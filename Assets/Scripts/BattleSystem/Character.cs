// Scripts/Character.cs
using UnityEngine;
using TMPro; // TextMeshProUGUI を使う場合 (削除またはHPバーに統合)

public class Character : MonoBehaviour
{
    public CharacterStats stats;
    public Team team;
    public Character currentTarget;

    // 新しいフィールド
    public GameObject projectilePrefab;
    public Transform muzzlePoint; // Projectileの発射位置 (キャラクターの子オブジェクトとして設定)
    public GameObject healthBarPrefab; // HPバーのプレハブをアサイン

    private float attackTimer;
    private SpriteRenderer spriteRenderer;
    private HealthBar healthBarInstance;

     private enum State
    {
        Idle,
        MovingToTarget,
        AttackingTarget
    }
    private State currentState = State.Idle;

    // public TextMeshProUGUI hpText; // HPバーに置き換えるためコメントアウトまたは削除

    void Awake()
    {
        stats.Initialize();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning($"{gameObject.name} に SpriteRenderer がありません。");
        }
    }

    void Start()
    {
        attackTimer = Random.Range(0, stats.attackCooldown * 0.5f); // 開始タイミングを少しずらす
        InstantiateHealthBar();
        UpdateHealthBarDisplay(); // 初期HPを表示
    }

    void InstantiateHealthBar()
    {
        if (healthBarPrefab != null)
        {
            GameObject hbGO = Instantiate(healthBarPrefab, transform.position, Quaternion.identity);
            // World Space Canvas の場合、キャラクターの子にして位置を調整
            // hbGO.transform.SetParent(transform); // HealthBar.csのInitializeで行う
            // hbGO.transform.localPosition = new Vector3(0, 1f, 0); // HealthBar.csのOffsetで調整

            healthBarInstance = hbGO.GetComponent<HealthBar>();
            if (healthBarInstance != null)
            {
                healthBarInstance.Initialize(transform); // キャラクターのTransformを渡す
            }
            else
            {
                Debug.LogError("HealthBar PrefabにHealthBarスクリプトがアタッチされていません！");
            }
        }
    }

     void Update()
    {
        // 生死、BattleManagerの存在、戦闘継続状態をチェック
        if (!IsAlive()) return; // 死んでいたら何もしない

        if (BattleManager.Instance == null) return; // BattleManagerがいなければ何もしない

        // バトルが終了していたら、現在のステートに応じた待機処理のみ行うか、何もしない
        if (!BattleManager.Instance.IsBattleOngoing())
        {
            // バトル終了時はIdle状態に戻し、それ以上行動させない
            if (currentState != State.Idle)
            {
                ChangeState(State.Idle);
            }
            // アイドルアニメーションなどがあればここで再生
            return; // Update処理をここで抜ける
        }


        // 状態に応じた処理
        switch (currentState)
        {
            case State.Idle:
                HandleIdleState();
                break;
            case State.MovingToTarget:
                HandleMovingToTargetState();
                break;
            case State.AttackingTarget:
                HandleAttackingTargetState();
                break;
        }
    }

    void ChangeState(State newState)
    {
        if (currentState == newState) return;
        Debug.Log($"{stats.characterName} ({team}) changing state from {currentState} to {newState}"); // この行を追加
        currentState = newState;
        // Debug.Log($"{stats.characterName} changed state to {newState}");
    }

    void HandleIdleState()
    {
        // BattleManager.Instance と IsBattleOngoing のチェックはUpdateの最初で行うのでここでは不要

        // 索敵範囲内の敵を探す
        FindTargetInRange();

        if (currentTarget != null && currentTarget.IsAlive())
        {
            ChangeState(State.MovingToTarget);
        }
        else
        {
            // ターゲットが見つからない場合、前進する
            // ただし、バトルが進行中でなければ前進しない (Updateのガードで担保されているはずだが念のため)
            if (BattleManager.Instance != null && BattleManager.Instance.IsBattleOngoing())
            {
                MoveForward(stats.patrolSpeed);
            }
        }
    }
    void MoveForward(float speed)
    {
        Vector3 forwardDirection;
        // チームに基づいて前進方向を決定
        if (team == Team.Player)
        {
            forwardDirection = Vector3.right; // 味方チームは右へ進む
        }
        else // team == Team.Enemy
        {
            forwardDirection = Vector3.left;  // 敵チームは左へ進む
        }

        // 前進
        transform.position += forwardDirection * speed * Time.deltaTime;

        // 進行方向を向く (スプライトが右向き基準の場合)
        if (forwardDirection.x > 0.01f) // 右を向く
        {
            // スプライトが反転していない状態（右向き）にする
            if (transform.localScale.x < 0) // 現在左を向いていたら
            {
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }
        }
        else if (forwardDirection.x < -0.01f) // 左を向く
        {
            // スプライトを反転させて左向きにする
            if (transform.localScale.x > 0) // 現在右を向いていたら
            {
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }
        }
    }


    void HandleMovingToTargetState()
    {
        if (currentTarget == null || !currentTarget.IsAlive())
        {
            ChangeState(State.Idle);
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.transform.position);

        if (distanceToTarget <= stats.attackRange)
        {
            ChangeState(State.AttackingTarget);
        }
        else
        {
            Vector2 direction = (currentTarget.transform.position - transform.position).normalized;
            transform.position += (Vector3)direction * stats.moveSpeed * Time.deltaTime;

            if (direction.x > 0.01f)
            {
                if (transform.localScale.x < 0)
                    transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }
            else if (direction.x < -0.01f)
            {
                if (transform.localScale.x > 0)
                    transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }
        }
    }

    void HandleAttackingTargetState()
    {
        if (currentTarget == null || !currentTarget.IsAlive())
        {
            ChangeState(State.Idle);
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.transform.position);
        if (distanceToTarget > stats.attackRange * 1.1f)
        {
            ChangeState(State.MovingToTarget);
            return;
        }

        Vector2 directionToTarget = (currentTarget.transform.position - transform.position).normalized;
        if (directionToTarget.x > 0.01f)
        {
            if (transform.localScale.x < 0)
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
        else if (directionToTarget.x < -0.01f)
        {
            if (transform.localScale.x > 0)
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0)
        {
            PerformAttack();
            attackTimer = stats.attackCooldown;
        }
    }
    void FindTargetInRange()
    {
        currentTarget = BattleManager.Instance.FindNearestTargetInDetectionRange(this, stats.detectionRange);
    }

    void PerformAttack()
    {
        if (!IsAlive() || currentTarget == null || !currentTarget.IsAlive()) return;

        Debug.Log($"{stats.characterName} ({team}) attacks {currentTarget.stats.characterName} ({currentTarget.team}).");

        if (projectilePrefab != null && muzzlePoint != null)
        {
            GameObject projGO = Instantiate(projectilePrefab, muzzlePoint.position, muzzlePoint.rotation);
            Projectile projectile = projGO.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Initialize(currentTarget, stats.attackPower, team);
            }
            else
            {
                Debug.LogError("Projectile PrefabにProjectileスクリプトがアタッチされていません！");
                Destroy(projGO); // 不正なProjectileは削除
            }
        }
        else
        {
            // Projectileがない場合のフォールバック (直接ダメージ)
            Debug.LogWarning($"{stats.characterName} is missing ProjectilePrefab or MuzzlePoint. Performing direct damage.");
            currentTarget.TakeDamage(stats.attackPower);
        }

        // 簡単な攻撃エフェクト（例：一瞬点滅）
        if (spriteRenderer != null)
        {
            // ここで攻撃アニメーションやエフェクトを再生する処理を追加できます
        }
    }

    public void TakeDamage(int damage)
    {
        if (!IsAlive()) return; // すでに死んでいる場合は何もしない

        stats.currentHp -= damage;
        Debug.Log($"{stats.characterName} took {damage} damage. Current HP: {stats.currentHp}");

        UpdateHealthBarDisplay();

        if (stats.currentHp <= 0)
        {
            stats.currentHp = 0;
            Die();
        }


        if (spriteRenderer != null)
        {
            StartCoroutine(FlashColor(Color.red, 0.2f));
        }
    }

    System.Collections.IEnumerator FlashColor(Color color, float duration)
    {
        if(spriteRenderer == null) yield break;
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = color;
        yield return new WaitForSeconds(duration / 2);
        spriteRenderer.color = originalColor;
        // yield return new WaitForSeconds(duration / 2); // 点滅を1回にする
    }

    void Die()
    {
        Debug.Log($"{stats.characterName} has died.");
        ChangeState(State.Idle); // 死亡時は行動を停止
        if (spriteRenderer != null) spriteRenderer.color = Color.gray;
        BattleManager.Instance.OnCharacterDied(this);

        // HPバーを非表示または破壊
        if (healthBarInstance != null && healthBarInstance.gameObject != null)
        {
            // Destroy(healthBarInstance.gameObject); // 即時破壊
            healthBarInstance.gameObject.SetActive(false); // 非表示
        }
    }

    public bool IsAlive()
    {
        return stats.currentHp > 0;
    }

    void UpdateHealthBarDisplay()
    {
        if (healthBarInstance != null)
        {
            healthBarInstance.UpdateHealth(stats.currentHp, stats.maxHp);
        }
        /* // 古いHPテキスト表示
        if (hpText != null)
        {
            hpText.text = $"HP: {stats.currentHp}/{stats.maxHp}";
        }
        */
    }

    // Gizmosで範囲を視覚化 (デバッグ用)
    void OnDrawGizmosSelected()
    {
        if (stats == null) return;

        // 索敵範囲
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stats.detectionRange);

        // 攻撃範囲
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stats.attackRange);

        // ターゲットへの線
        if (currentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentTarget.transform.position);
        }
    }
}