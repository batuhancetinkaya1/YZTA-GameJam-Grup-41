using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private float m_baseDamage = 10f;
    [SerializeField] private float m_thirdAttackDamage = 15f;

    private PlayerCore m_playerCore;
    private SensorPlayer m_meleeRangeSensor;

    // Attack Combo
    private int m_currentAttack = 0;
    private float m_timeSinceAttack = 0f;

    // Referans
    private PlayerMovementController m_movementController;

    public void Initialize(PlayerCore core)
    {
        m_playerCore = core;
        m_movementController = GetComponent<PlayerMovementController>();
    }

    private void Awake()
    {
        m_meleeRangeSensor = transform.Find("MeleeRange").GetComponent<SensorPlayer>();
    }

    private void Update()
    {
        m_timeSinceAttack += Time.deltaTime;
    }

    public void HandleAttack()
    {
        if (m_movementController.IsWallSliding) return;

        if (m_timeSinceAttack > 0.25f && !m_movementController.IsRolling)
        {
            m_currentAttack++;

            if (m_currentAttack > 3)
                m_currentAttack = 1;

            if (m_timeSinceAttack > 1.0f)
                m_currentAttack = 1;

            m_playerCore.AnimControl.SetTriggerAttack(m_currentAttack);
            //AudioManager.Instance.PlaySFXWithNewSource("SwordBasic", transform.position);

            if (m_meleeRangeSensor.State())
            {
                ApplyDamage(m_currentAttack);
            }
            m_timeSinceAttack = 0.0f;
        }
    }

    private void ApplyDamage(int currentAttackState)
    {
        float damage = currentAttackState < 3 ? m_baseDamage : m_thirdAttackDamage;
        //List<EnemyBase> enemiesToHit = new List<EnemyBase>();

        foreach (var col in m_meleeRangeSensor.Colliders)
        {
            // Diðer Player ise
            if (col.CompareTag("Player"))
            {
                var targetHealth = col.GetComponent<PlayerHealthManager>();
                var targetCore = col.GetComponent<PlayerCore>();

                // Ayný tipte player (Örn. PlayerOne vs PlayerOne) ise vurmak istemiyor olabilirsiniz
                // ama eðer PvP varsa:
                if (targetHealth != null && targetCore.PlayerType != m_playerCore.PlayerType)
                {
                    targetHealth.ReceiveDamage(damage, transform);
                }
            }
            // Enemy
            //else if (col.CompareTag("Enemy"))
            //{
            //    var enemy = col.GetComponent<EnemyBase>();
            //    if (enemy != null)
            //    {
            //        enemiesToHit.Add(enemy);
            //    }
            //}
        }

        // Toplanan düþmanlara hasar
        //foreach (var enemy in enemiesToHit)
        //{
        //    enemy.TakeDamage(damage);
        //}
    }

    public void HandleBlock(bool isBlocking)
    {
        if (m_movementController.IsWallSliding) return;

        if (!m_movementController.IsRolling)
        {
            if (isBlocking)
            {
                m_playerCore.HealthManager.IsBlocking = true;
                m_playerCore.AnimControl.SetTriggerBlock();
                m_playerCore.AnimControl.SetIdleBlock(true);
            }
            else
            {
                m_playerCore.HealthManager.IsBlocking = false;
                m_playerCore.AnimControl.SetIdleBlock(false);
            }
        }
    }
}
