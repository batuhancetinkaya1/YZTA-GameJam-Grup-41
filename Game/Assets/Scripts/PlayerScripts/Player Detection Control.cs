using UnityEngine;

public class PlayerDetectionControl : MonoBehaviour
{
    private PlayerCore m_playerCore;

    private void Awake()
    {
        m_playerCore = GetComponent<PlayerCore>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Grid"))
        {
            m_playerCore.MovementController.IsStuck = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Grid"))
        {
            m_playerCore.MovementController.IsStuck = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("EndDoor"))
        {
            if (m_playerCore.RespawnController.HasKey())
            {
                AudioManager.Instance.PlaySFX("Victory");
                PlayerPrefs.SetString("ArenaMode", "PVB");
                UIManager.Instance.LoadFightScene();
            }
        }

        if (collision.CompareTag("DiePoint"))
        {
            m_playerCore.HealthManager.ReceiveDamage(m_playerCore.HealthManager.CurrentHealth);
        }
    }

    // Melee sensor boyutunu güncelleyebiliriz
    public void UpdateMeleeSensorScale(int facingDir)
    {
        Transform meleeRange = transform.Find("MeleeRange");
        if (meleeRange != null)
        {
            meleeRange.localScale = new Vector2(facingDir, 1);
        }
    }
}
