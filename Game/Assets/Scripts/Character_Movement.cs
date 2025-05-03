using UnityEngine;


/// <summary>
/// Basit 2D platformer hareket scripti:
/// • Yürüyüş / koşu
/// • Tekli + (opsiyonel) çift zıplama
/// • Sprite yönü çevirme
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    /* =====================
       === Ayarlanabilir ===
       ===================== */
    [Header("Movement")]
    [Tooltip("Yatay hareket hızı (birim/s)")]
    public float moveSpeed = 8f;

    [Tooltip("Hızlanma miktarı (0 = anında yön değiştirir)")]
    public float acceleration = 50f;

    [Header("Jump")]
    [Tooltip("Zıplama kuvveti (Impulse)")]
    public float jumpForce = 16f;

    [Tooltip("Çift zıplama aktif olsun mu?")]
    public bool enableDoubleJump = true;

    [Header("Ground Check")]
    [Tooltip("Zemin kontrolü için merkez noktası")]
    public Transform groundCheck;

    [Tooltip("Zemin kontrol yarıçapı")]
    public float groundCheckRadius = 0.1f;

    [Tooltip("Zemin olarak kabul edilen LayerMask")]
    public LayerMask groundLayer;

    /* =====================
       ===  Private  ======
       ===================== */
    private Rigidbody2D rb;
    private bool isGrounded;
    private int jumpPhase;          // 0 = yerde, 1 = havada (ilk zıplama), 2 = double jump kullanıldı
    private float moveInput;

    /* =====================
       ===   Mono    ======
       ===================== */
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        /* --- Girdi Toplama --- */
        moveInput = Input.GetAxisRaw("Horizontal");

        /* --- Zıplama --- */
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                Jump();
                jumpPhase = 1;
            }
            else if (enableDoubleJump && jumpPhase < 2)
            {
                Jump(isDouble: true);
                jumpPhase = 2;
            }
        }

        /* --- Sprite yönü --- */
        if (moveInput != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveInput), 1, 1);
        }
    }

    private void FixedUpdate()
    {
        /* --- Yatay Hareket --- */
        float targetVelX = moveInput * moveSpeed;
        float smoothedVelX = Mathf.Lerp(rb.linearVelocity.x, targetVelX, acceleration * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(smoothedVelX, rb.linearVelocity.y);

        /* --- Zemin Kontrolü --- */
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (isGrounded) jumpPhase = 0;
    }

    /* =====================
       ===   Helpers  =====
       ===================== */
    private void Jump(bool isDouble = false)
    {
        // Çift zıplamada tutarlı yükseklik için mevcut yukarı hızı sıfırla
        if (!isGrounded && isDouble)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }

        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void OnDrawGizmosSelected()
    {
        // Editör'de zemin kontrol alanını gösterir
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}