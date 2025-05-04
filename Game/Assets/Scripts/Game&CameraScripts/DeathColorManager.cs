// DeathColorManager.cs
using UnityEngine;
using UnityEngine.Tilemaps;

public class DeathColorManager : MonoBehaviour
{
    [Header("Background Tilemap’ler")]
    [Tooltip("Renkini değiştirmek istediğiniz tüm Tilemap’leri sırasıyla ekleyin.")]
    [SerializeField] private Tilemap[] m_backgroundMaps;

    [Header("Ayarlamalar")]
    [Tooltip("Kaç ölümde tam kırmızıya ulaşsın?")]
    [SerializeField] private int m_deathsToMax = 5;

    private int m_deathCount;

    private void OnEnable() => PlayerCore.OnAnyPlayerDeath += HandleDeath;
    private void OnDisable() => PlayerCore.OnAnyPlayerDeath -= HandleDeath;

    private void HandleDeath()
    {
        m_deathCount = Mathf.Min(m_deathCount + 1, m_deathsToMax);

        float t = m_deathCount / (float)m_deathsToMax;            // 0 → 1
        Color target = Color.Lerp(Color.white, Color.red, t);           // Beyaz → Kırmızı

        foreach (var tm in m_backgroundMaps)
            if (tm != null) tm.color = target;
    }
}
