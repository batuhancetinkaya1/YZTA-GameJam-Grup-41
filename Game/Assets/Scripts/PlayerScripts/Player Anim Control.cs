using UnityEngine;

public class PlayerAnimControl : MonoBehaviour
{
    private Animator m_animator;

    private void Start()
    {
        m_animator = GetComponent<Animator>();
    }

    public void SetGrounded(bool isGrounded)
    {
        m_animator.SetBool("Grounded", isGrounded);
    }

    public void SetAirSpeedY(float speedY)
    {
        m_animator.SetFloat("AirSpeedY", speedY);
    }

    public void SetRunningState(float inputX)
    {
        // 1 => koþma, 0 => idle
        m_animator.SetInteger("AnimState", Mathf.Abs(inputX) > Mathf.Epsilon ? 1 : 0);
    }

    public void SetTriggerJump()
    {
        m_animator.SetTrigger("Jump");
    }

    public void SetTriggerRoll()
    {
        m_animator.SetTrigger("Roll");
    }

    public void SetTriggerAttack(int attackNumber)
    {
        m_animator.SetTrigger("Attack" + attackNumber);
    }

    public void SetTriggerBlock()
    {
        m_animator.SetTrigger("Block");
    }

    public void SetTriggerBlocked()
    {
        m_animator.SetTrigger("Blocked");
    }

    public void SetTriggerRespawn()
    {
        m_animator.SetTrigger("Respawn");
    }

    public void SetIdleBlock(bool isBlocking)
    {
        m_animator.SetBool("IdleBlock", isBlocking);
    }

    public void HandleDeath()
    {
        m_animator.SetTrigger("Death");
    }

    public void HandleHurt()
    {
        m_animator.SetTrigger("Hurt");
    }

    public void SetWallSliding(bool isWallSliding)
    {
        m_animator.SetBool("WallSlide", isWallSliding);
    }
}
