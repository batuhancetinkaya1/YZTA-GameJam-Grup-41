using System.Collections;
using UnityEngine;
using System;

[RequireComponent(typeof(PlayerCore))]
public class AIInputHandler : MonoBehaviour
{
    private PlayerCore m_core;
    private PlayerMovementController m_movement;
    private PlayerCombatController m_combat;
    private PlayerHealthManager m_health;

    [Header("AI Settings")]
    [Tooltip("How often (in seconds) the AI decides on the next action.")]
    [SerializeField] private float m_decisionInterval = 0.15f; // Biraz daha sýk karar alsýn

    [Tooltip("Maximum distance at which the AI can see/detect targets.")]
    [SerializeField] private float m_detectionRange = 500f; // Biraz artýrdýk

    [Tooltip("Desired range for attacking the player (melee).")]
    [SerializeField] private float m_idealAttackRange = 1.5f;

    [Tooltip("AI's reaction delay for blocking or reacting to attacks.")]
    [SerializeField] private float m_blockReactionTime = 0.15f; // Biraz hýzlandýrdýk

    [Tooltip("Chance (0-1) that the AI might block if the target is attacking.")]
    [Range(0f, 1f)]
    [SerializeField] private float m_blockChance = 0.5f;

    [Tooltip("Chance (0-1) that the AI uses a roll instead of a jump to dodge.")]
    [Range(0f, 1f)]
    [SerializeField] private float m_rollInsteadOfJumpChance = 0.3f;

    [Header("General Behavior Tweaks")]
    [Tooltip("Random factor for deciding if the AI tries to do combos, chase, etc.")]
    [Range(0f, 1f)]
    [SerializeField] private float m_randomAggression = 0.6f;  // Biraz artýrdýk

    // Internal states
    private AIState m_currentState = AIState.Idle;
    private float m_decisionTimer = 0f;
    private bool m_isBlocking = false;

    // Target tracking
    private Transform m_target;
    private PlayerCore m_targetCore;  // Eðer hedef de bir PlayerCore ise

    // A small cooldown for repeated actions
    private float m_actionCooldownTimer = 0f;
    private const float ACTION_COOLDOWN = 0.15f; // Daha sýk saldýrabilsin

    private void Awake()
    {
        m_core = GetComponent<PlayerCore>();
        m_movement = m_core.MovementController;
        m_combat = m_core.CombatController;
        m_health = m_core.HealthManager;
    }

    private void Start()
    {
        // Geçerli bir hedef belirleyelim (örneðin, farklý PlayerType'a sahip bir PlayerCore)
        FindTargetPlayer();

        // Ýlk state ayarý
        TransitionTo(AIState.Idle);
    }

    private void Update()
    {
        // Sadece oyunun aktif olduðu durumlarda AI çalýþsýn
        if (GameManager.Instance.CurrentState == GameStates.GameOn ||
            GameManager.Instance.CurrentState == GameStates.FinalFight)
        {
            // Blok durumunu güncelle
            UpdateBlockLogic();

            // Zamanlayýcýlarý güncelle
            m_decisionTimer += Time.deltaTime;
            m_actionCooldownTimer -= Time.deltaTime;

            if (m_decisionTimer >= m_decisionInterval)
            {
                m_decisionTimer = 0f;
                DecideNextAction();
            }
        }
    }

    private void DecideNextAction()
    {
        // Hedef bulunamazsa Idle state'inde kal
        if (m_target == null)
        {
            TransitionTo(AIState.Idle);
            return;
        }

        switch (m_currentState)
        {
            case AIState.Idle:
                ThinkIdle();
                break;
            case AIState.Chase:
                ThinkChase();
                break;
            case AIState.Attack:
                ThinkAttack();
                break;
            case AIState.Block:
                ThinkBlock();
                break;
            case AIState.Retreat:
                ThinkRetreat();
                break;
        }
    }

    //==================================================
    // State Machine: Internal Logic
    //==================================================

    private void ThinkIdle()
    {
        float distance = DistanceToTarget();

        // Eðer hedef detectionRange içindeyse, AI hedefe dönsün
        if (distance < m_detectionRange)
        {
            FaceTarget();
            if (UnityEngine.Random.value < 0.8f)
            {
                TransitionTo(AIState.Chase);
            }
        }
    }

    private void ThinkChase()
    {
        float distance = DistanceToTarget();

        // Hedefe doðru yönelmeden önce yüzünü hedefe döndür
        FaceTarget();

        // Hedef fazla uzaksa Idle'a dön
        if (distance > m_detectionRange + 2f)
        {
            TransitionTo(AIState.Idle);
            return;
        }

        // Eðer ideal saldýrý aralýðýndaysa saldýrýya geç
        if (distance <= m_idealAttackRange + 0.1f)
        {
            TransitionTo(AIState.Attack);
            return;
        }

        // Hedefe doðru hareket et
        float dir = (m_target.position.x > transform.position.x) ? 1f : -1f;
        m_movement.HandleHorizontalMovement(dir);

        // Belirli bir ihtimalle zýpla veya yuvarlan
        if (ShouldDodgeOrJump())
        {
            AttemptJumpOrRoll();
        }
    }

    private void ThinkAttack()
    {
        float distance = DistanceToTarget();

        // Saldýrý öncesi yüzünü hedefe çevir
        FaceTarget();

        // Mesafe açýldýysa tekrar kovalamaya geç
        if (distance > m_idealAttackRange + 0.5f)
        {
            TransitionTo(AIState.Chase);
            return;
        }

        // Cooldown kontrolü ve saldýrý iþlemi
        if (m_actionCooldownTimer <= 0f)
        {
            m_combat.HandleAttack();
            m_actionCooldownTimer = ACTION_COOLDOWN;

            // Saldýrý sonrasý rastgele devam kararý
            float r = UnityEngine.Random.value;
            if (r < m_randomAggression)
            {
                // Ayný Attack state'te kalýp combo yapabilir
            }
            else
            {
                float r2 = UnityEngine.Random.value;
                if (r2 < 0.3f)
                {
                    TransitionTo(AIState.Block);
                }
                else if (r2 < 0.5f)
                {
                    TransitionTo(AIState.Retreat);
                }
                else
                {
                    TransitionTo(AIState.Chase);
                }
            }
        }
    }

    private void ThinkBlock()
    {
        // Blok state'ini coroutine ile yönetiyoruz
        StartCoroutine(BlockBriefly());
    }

    private IEnumerator BlockBriefly()
    {
        if (!m_isBlocking)
        {
            m_isBlocking = true;
            m_combat.HandleBlock(true);  // Kalkaný kaldýr
        }

        float blockTime = UnityEngine.Random.Range(0.4f, 0.8f);
        yield return new WaitForSeconds(blockTime);

        m_isBlocking = false;
        m_combat.HandleBlock(false); // Kalkaný indir

        if (DistanceToTarget() <= m_idealAttackRange)
        {
            TransitionTo(AIState.Attack);
        }
        else
        {
            TransitionTo(AIState.Chase);
        }
    }

    private void ThinkRetreat()
    {
        float distance = DistanceToTarget();

        // Yüzünü hedefe döndür
        FaceTarget();

        float dir = (m_target.position.x > transform.position.x) ? -1f : 1f;
        m_movement.HandleHorizontalMovement(dir);

        if (ShouldDodgeOrJump())
        {
            AttemptJumpOrRoll();
        }

        if (distance > m_detectionRange * 0.6f || UnityEngine.Random.value > 0.7f)
        {
            TransitionTo(AIState.Idle);
        }
    }

    //==================================================
    // Helpers & Utility
    //==================================================

    /// <summary>
    /// AI’nin her zaman hedefe dönmesini saðlar.
    /// SpriteRenderer'ýn flipX ayarýný ve melee sensor yönünü (DetectionControl) günceller.
    /// </summary>
    private void FaceTarget()
    {
        if (m_target == null) return;

        bool shouldFaceRight = m_target.position.x > transform.position.x;
        // SpriteRenderer'ý alýp yönünü deðiþtiriyoruz.
        SpriteRenderer sprite = m_core.GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            if (shouldFaceRight)
            {
                sprite.flipX = false;
                m_core.DetectionControl.UpdateMeleeSensorScale(1);
            }
            else
            {
                sprite.flipX = true;
                m_core.DetectionControl.UpdateMeleeSensorScale(-1);
            }
        }
    }

    private void TransitionTo(AIState newState)
    {
        m_currentState = newState;
        // Ýsteðe baðlý: Debug.Log("AI " + m_core.PlayerType + " => " + m_currentState);
    }

    private float DistanceToTarget()
    {
        if (m_target == null)
            return float.MaxValue;
        return Vector2.Distance(transform.position, m_target.position);
    }

    private bool ShouldDodgeOrJump()
    {
        // Örnek: %25 ihtimalle zýpla/roll denesin
        return (UnityEngine.Random.value < 0.25f);
    }

    private void AttemptJumpOrRoll()
    {
        if (UnityEngine.Random.value < m_rollInsteadOfJumpChance)
        {
            m_movement.HandleRoll();
        }
        else
        {
            m_movement.HandleJump();
        }
    }

    /// <summary>
    /// Yakýn dövüþ saldýrýsý algýlama vb. için block mantýðý
    /// </summary>
    private void UpdateBlockLogic()
    {
        if (m_currentState == AIState.Block || m_isBlocking)
            return;

        float distance = DistanceToTarget();
        if (distance > m_idealAttackRange * 1.5f)
            return;

        bool targetIsAttacking = false;
        if (m_targetCore != null && m_targetCore.CombatController != null)
        {
            // Örnek: Gerçek bir saldýrý kontrolü yerine basit bir random senaryo
            targetIsAttacking = (UnityEngine.Random.value < 0.08f);
        }

        if (targetIsAttacking && UnityEngine.Random.value < m_blockChance)
        {
            StartCoroutine(BlockWithDelay(m_blockReactionTime));
        }
    }

    private IEnumerator BlockWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!m_isBlocking && m_currentState != AIState.Block)
        {
            TransitionTo(AIState.Block);
        }
    }

    /// <summary>
    /// Ýlk bulunan farklý PlayerType'a sahip PlayerCore'u hedef alýr.
    /// </summary>
    private void FindTargetPlayer()
    {
        var allPlayers = FindObjectsOfType<PlayerCore>();
        foreach (var pc in allPlayers)
        {
            if (pc != m_core && pc.PlayerType != m_core.PlayerType)
            {
                m_target = pc.transform;
                m_targetCore = pc;
                break;
            }
        }
    }
}

//==================================================
// AI States
//==================================================
public enum AIState
{
    Idle,
    Chase,
    Attack,
    Block,
    Retreat
}
