using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float m_speed = 4.0f;
    [SerializeField] private float m_jumpForce = 7.5f;
    [SerializeField] private float m_rollForce = 8.0f;
    [SerializeField] private float m_wallSlideSpeed = 2f;
    [SerializeField] private float m_wallStickTime = 1f;
    [SerializeField] private float m_wallJumpTime = 0.2f;
    [SerializeField] private GameObject dustPrefab;


    private PlayerCore m_playerCore;
    private Rigidbody2D m_body2d;
    private SpriteRenderer m_spriteRenderer;

    // Sens�rler
    private SensorPlayer m_groundSensor;
    private SensorPlayer m_wallSensorR1, m_wallSensorR2;
    private SensorPlayer m_wallSensorL1, m_wallSensorL2;

    public bool m_grounded = false;
    private bool m_isWallSliding = false;
    private bool m_wallJumping = false;
    public bool m_isStuck = false;
    private float m_wallJumpingCurrentTime = 0f;
    private float m_wallStickCounter = 0f;
    private int m_wallSlidingSide = 0;
    private int m_facingDirection = 1;

    // Roll
    public bool m_rolling = false;
    private float m_rollDuration = 8.0f / 14.0f;
    private float m_rollCurrentTime;

    public bool IsRolling => m_rolling;
    public bool IsGrounded => m_grounded;
    public bool IsWallSliding => m_isWallSliding;
    public int FacingDirection => m_facingDirection;

    public bool IsStuck
    {
        get => m_isStuck;
        set
        {
            if (m_isStuck != value)
            {
                m_isStuck = value;
            }
        }
    }

    public void Initialize(PlayerCore core)
    {
        m_playerCore = core;
    }

    private void Awake()
    {
        m_body2d = GetComponent<Rigidbody2D>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();

        // Sens�rleri bul
        m_groundSensor = transform.Find("GroundSensor").GetComponent<SensorPlayer>();
        m_wallSensorR1 = transform.Find("WallSensor_R1").GetComponent<SensorPlayer>();
        m_wallSensorR2 = transform.Find("WallSensor_R2").GetComponent<SensorPlayer>();
        m_wallSensorL1 = transform.Find("WallSensor_L1").GetComponent<SensorPlayer>();
        m_wallSensorL2 = transform.Find("WallSensor_L2").GetComponent<SensorPlayer>();
    }

    private void Update()
    {
        CheckRollState();
        CheckWallJumpState();
        CheckGrounded();
        HandleWallSliding();

        if (m_playerCore && m_playerCore.AnimControl)
        {
            m_playerCore.AnimControl.SetAirSpeedY(m_body2d.linearVelocity.y);
        }
    }

    #region Horizontal Movement
    public void HandleHorizontalMovement(float inputX)
    {
        bool isStuck = m_isStuck;
        //Duvar sens�rleri
        if ((m_wallSensorL1.State() && !m_wallSensorL2.State()) && !m_grounded||
            (m_wallSensorR1.State() && !m_wallSensorR2.State()) && !m_grounded)
        {
            return;
        }

        if (!m_wallSensorL1.State() && !m_wallSensorL2.State() &&
            !m_wallSensorR1.State() && !m_wallSensorR2.State() &&
            !m_grounded && !m_isWallSliding && isStuck)
        {
            return;
        }

        if ((!m_wallSensorL1.State() && m_wallSensorL2.State() && !m_grounded && !m_isWallSliding && isStuck) ||
            (!m_wallSensorR1.State() && m_wallSensorR2.State() && !m_grounded && !m_isWallSliding && isStuck))
        {
            return;
        }

        // Duvarda kayma
        if (m_isWallSliding)
        {
            // Z�t y�ne hamle
            if ((m_wallSlidingSide > 0 && inputX < 0) ||
                (m_wallSlidingSide < 0 && inputX > 0))
            {
                m_isWallSliding = false;
                m_playerCore.AnimControl.SetWallSliding(false);
                m_playerCore.AnimControl.SetTriggerJump();
            }

            // Yava� yatay kayma
            m_body2d.linearVelocity = new Vector2(inputX * (m_speed * 0.5f), m_body2d.linearVelocity.y);
            return;
        }

        // Y�n belirleme
        if (inputX > 0)
        {
            m_spriteRenderer.flipX = false;
            m_playerCore.DetectionControl.UpdateMeleeSensorScale(1);
            m_facingDirection = 1;
        }
        else if (inputX < 0)
        {
            m_spriteRenderer.flipX = true;
            m_playerCore.DetectionControl.UpdateMeleeSensorScale(-1);
            m_facingDirection = -1;
        }

        // Roll veya wallJump yap�lm�yorsa normal hareket
        if (!m_rolling && !m_wallJumping)
        {
            m_body2d.linearVelocity = new Vector2(inputX * m_speed, m_body2d.linearVelocity.y);
        }

        // Animasyon
        if (m_playerCore && m_playerCore.AnimControl)
            m_playerCore.AnimControl.SetRunningState(inputX);

        //if (m_grounded && inputX != 0)
        //{
        //    AudioManager.Instance.PlaySFXWithNewSource("Step", transform.position);
        //}
    }
    #endregion

    #region Jump
    public void HandleJump()
    {
        if (m_grounded && !m_rolling)
        {
            PerformJump(m_jumpForce);
        }
        else if (m_isWallSliding)
        {
            PerformWallJump();
        }
    }

    private void PerformJump(float jumpForce)
    {
        m_playerCore.AnimControl.SetTriggerJump();
        m_grounded = false;
        m_playerCore.AnimControl.SetGrounded(m_grounded);

        m_body2d.linearVelocity = new Vector2(m_body2d.linearVelocity.x, jumpForce);
        m_groundSensor.Disable(0.2f);
        //AudioManager.Instance.PlaySFXWithNewSource("Jump", transform.position);
    }

    private void PerformWallJump()
    {
        m_playerCore.AnimControl.SetTriggerJump();
        m_isWallSliding = false;
        m_playerCore.AnimControl.SetWallSliding(false);

        m_body2d.linearVelocity = new Vector2(-m_wallSlidingSide * m_jumpForce * 0.75f, m_jumpForce * 0.75f);
        m_wallJumping = true;
        m_wallJumpingCurrentTime = 0f;

        if (m_wallSlidingSide > 0)
        {
            m_wallSensorR1.Disable(0.3f);
            m_wallSensorR2.Disable(0.3f);
        }
        else
        {
            m_wallSensorL1.Disable(0.3f);
            m_wallSensorL2.Disable(0.3f);
        }

        m_spriteRenderer.flipX = (-m_wallSlidingSide < 0);
        m_facingDirection = (-m_wallSlidingSide < 0) ? -1 : 1;
        //AudioManager.Instance.PlaySFXWithNewSource("Jump", transform.position);
    }
    #endregion

    #region Roll
    public void HandleRoll()
    {
        if (!m_rolling)
        {
            m_rolling = true;
            m_rollCurrentTime = 0f;
            m_playerCore.AnimControl.SetTriggerRoll();

            if (m_isWallSliding)
            {
                m_isWallSliding = false;
                m_playerCore.AnimControl.SetWallSliding(false);
                m_body2d.linearVelocity = new Vector2(-m_wallSlidingSide * m_rollForce, m_body2d.linearVelocity.y);

                if (m_wallSlidingSide > 0)
                {
                    m_wallSensorR1.Disable(0.3f);
                    m_wallSensorR2.Disable(0.3f);
                }
                else
                {
                    m_wallSensorL1.Disable(0.3f);
                    m_wallSensorL2.Disable(0.3f);
                }

                m_spriteRenderer.flipX = (-m_wallSlidingSide < 0);
                m_facingDirection = (-m_wallSlidingSide < 0) ? -1 : 1;
            }
            else
            {
                m_body2d.linearVelocity = new Vector2(m_facingDirection * m_rollForce, m_body2d.linearVelocity.y);
            }

            // Melee sensor y�n�n� g�ncelle
            m_playerCore.DetectionControl.UpdateMeleeSensorScale(m_facingDirection);

            // Collider boyutu k�saltma (iste�e ba�l�)
            BoxCollider2D colliderToChange = m_body2d.GetComponent<BoxCollider2D>();
            colliderToChange.offset = new Vector2(0, 0.2f);
            colliderToChange.size = new Vector2(colliderToChange.size.x, 0.3f);
        }
    }

    #endregion

    #region Private Helpers
    private void CheckRollState()
    {
        if (m_rolling)
        {
            m_rollCurrentTime += Time.deltaTime;

            // Raycast check for grid above player
            Vector2 raycastStartPosition = new Vector2(transform.position.x, transform.position.y + 0.5f);

            RaycastHit2D hit = Physics2D.Raycast(raycastStartPosition, Vector2.up, 1f, LayerMask.GetMask("Grid"));
            bool hasBlockAbove = hit.collider != null;

            // Raycast'i g�rselle�tir
            Debug.DrawRay(raycastStartPosition, Vector2.up * 1f, hasBlockAbove ? Color.red : Color.green);

            if (m_rollCurrentTime > m_rollDuration)
            {
                if (!hasBlockAbove)
                {
                    // If not under grid, end roll normally
                    BoxCollider2D colliderToChange = m_body2d.GetComponent<BoxCollider2D>();
                    colliderToChange.offset = new Vector2(0, 0.670486f);
                    colliderToChange.size = new Vector2(colliderToChange.size.x, 1.183028f);
                    m_rolling = false;
                }
                else
                {
                    m_body2d.linearVelocity = new Vector2(m_facingDirection * m_rollForce / 2, m_body2d.linearVelocity.y);
                    //m_playerCore.AnimControl.SetTriggerRoll();
                    m_rollCurrentTime = m_rollDuration - 0.2f;
                }
            }
        }
    }

    private void CheckWallJumpState()
    {
        if (m_wallJumping)
        {
            m_wallJumpingCurrentTime += Time.deltaTime;
            if (m_wallJumpingCurrentTime >= m_wallJumpTime)
            {
                m_wallJumping = false;
            }
        }
    }

    private void CheckGrounded()
    {
        if (!m_grounded && m_groundSensor.State())
        {
            Debug.Log(m_groundSensor.State());
            m_groundSensor.Stater();

            m_grounded = true;
            m_playerCore.AnimControl.SetGrounded(m_grounded);
            //AudioManager.Instance.PlaySFXWithNewSource("Land", transform.position);
        }
        else if (m_grounded && !m_groundSensor.State())
        {
            m_grounded = false;
            m_playerCore.AnimControl.SetGrounded(m_grounded);
        }
    }

    private void HandleWallSliding()
    {
        bool onRightWall = m_wallSensorR1.State() && m_wallSensorR2.State();
        bool onLeftWall = m_wallSensorL1.State() && m_wallSensorL2.State();

        if ((onRightWall || onLeftWall) && !m_grounded)
        {
            m_isWallSliding = true;
            m_wallSlidingSide = onRightWall ? 1 : -1;

            if (m_wallStickCounter > 0)
            {
                m_wallStickCounter -= Time.deltaTime;
                m_body2d.linearVelocity = new Vector2(m_body2d.linearVelocity.x, 0);
            }
            else
            {
                m_body2d.linearVelocity = new Vector2(m_body2d.linearVelocity.x, -m_wallSlideSpeed);
            }

            m_spriteRenderer.flipX = (m_wallSlidingSide < 0);
        }
        else
        {
            if (m_isWallSliding)
            {
                m_isWallSliding = false;
                m_playerCore.AnimControl.SetWallSliding(false);
            }
            m_wallStickCounter = m_wallStickTime;
        }

        m_playerCore.AnimControl.SetWallSliding(m_isWallSliding);
    }

    public void AE_SlideDust()
    {
        Vector3 spawnPosition;

        if (m_facingDirection == 1)
            spawnPosition = m_wallSensorR2.transform.position;
        else
            spawnPosition = m_wallSensorL2.transform.position;

        if (dustPrefab != null)
        {
            GameObject dust = Instantiate(dustPrefab, spawnPosition, transform.localRotation);
            dust.transform.localScale = new Vector3(m_facingDirection, 1, 1);
        }
    }
    #endregion
}
