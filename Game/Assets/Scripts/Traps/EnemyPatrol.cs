using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    /* ─────────────  Inspector Alanları ───────────── */
    [Header("Patrol Points")]
    [SerializeField] private Vector2 startPoint;
    [SerializeField] private Vector2 endPoint;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("Path Settings")]
    [Tooltip("true = yatay devriye | false = dikey devriye")]
    [SerializeField] private bool m_isHorizontal = true;

    /* ─────────────  Özel Alanlar ───────────── */
    private Vector2 _targetPoint;
    private Vector3 _initialScale;

    /* ─────────────  Unity Metotları ───────────── */
    private void Awake()
    {
        _initialScale = transform.localScale;
        _targetPoint = endPoint;       // başlangıçta hedef
    }

    private void Update()
    {
        Patrol();
    }

    /* ─────────────  Devriye Mantığı ───────────── */
    private void Patrol()
    {
        // 1. Hedefe doğru ilerle
        transform.position = Vector2.MoveTowards(
            transform.position, _targetPoint, moveSpeed * Time.deltaTime);

        // 2. Yön vektörünü al
        Vector2 dir = _targetPoint - (Vector2)transform.position;

        // 3. Yatay / dikey durumuna göre görünümü ayarla
        if (m_isHorizontal)
        {
            // Sprite sola‑sağa dönsün (ölçek flip)
            if (Mathf.Abs(dir.x) > 0.001f)
            {
                float signX = Mathf.Sign(dir.x);          // +1 sağ, ‑1 sol
                transform.localScale = new Vector3(
                    _initialScale.x * signX,
                    _initialScale.y,
                    _initialScale.z);

                // Yatayda rotasyon 0° kalsın
                transform.localRotation = Quaternion.identity;
            }
        }
        else // dikey devriye
        {
            if (Mathf.Abs(dir.y) > 0.001f)
            {
                // Yukarı → +90°,  Aşağı → ‑90°
                float angle = dir.y > 0 ? 90f : -90f;
                transform.localRotation = Quaternion.Euler(0f, 0f, angle);

                // Ölçek orijinal kalsın
                transform.localScale = _initialScale;
            }
        }

        // 4. Hedef nokta değiştir
        if (Vector2.Distance(transform.position, _targetPoint) < 0.1f)
            _targetPoint = (_targetPoint == startPoint) ? endPoint : startPoint;
    }

    /* ─────────────  Debug Görselleştirme ───────────── */
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(startPoint, endPoint);
        Gizmos.DrawSphere(startPoint, 0.1f);
        Gizmos.DrawSphere(endPoint, 0.1f);
    }
}
