using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerCore m_playerCore;

    [Header("Key Code Keyboard")]
    public KeyCode rightMoveButton = KeyCode.D;
    public KeyCode leftMoveButton = KeyCode.A;
    public KeyCode jumpButton = KeyCode.W;
    public KeyCode blockButton = KeyCode.LeftShift;
    public KeyCode attackButton = KeyCode.J;
    public KeyCode rollButton = KeyCode.K;

    [Header("Key Code Joystick")]
    public KeyCode JoyStick_rightMoveButton;
    public KeyCode JoyStick_leftMoveButton;
    public KeyCode JoyStick_jumpButton;
    public KeyCode JoyStick_blockButton;
    public KeyCode JoyStick_attackButton;
    public KeyCode JoyStick_rollButton;

    private void Awake()
    {
        m_playerCore = GetComponent<PlayerCore>();
    }

    private void Start()
    {
        if(m_playerCore != null) 
            m_playerCore = GetComponent<PlayerCore>();
    }

    private void Update()
    {
        if (m_playerCore == null) return;

        // Eðer bu player AI ise InputHandler’ý kullanma
        if (m_playerCore.PlayerType == PlayerType.AI1 || m_playerCore.PlayerType == PlayerType.AI2)
            return;

        if (GameManager.Instance.CurrentState == GameStates.GameOn || GameManager.Instance.CurrentState == GameStates.FinalFight)
        {
            GetInput();
        }
    }

    private void GetInput()
    {
        // Horizontal
        float horizontalInput = 0f;
        bool isRightPressed = Input.GetKey(rightMoveButton) || Input.GetKey(JoyStick_rightMoveButton);
        bool isLeftPressed = Input.GetKey(leftMoveButton) || Input.GetKey(JoyStick_leftMoveButton);

        if (isRightPressed)
            horizontalInput = 1f;
        else if (isLeftPressed)
            horizontalInput = -1f;

        m_playerCore.MovementController.HandleHorizontalMovement(horizontalInput);

        // Jump
        if (Input.GetKeyDown(jumpButton) || Input.GetKeyDown(JoyStick_jumpButton))
        {
            m_playerCore.MovementController.HandleJump();
        }

        // Roll
        if (Input.GetKeyDown(rollButton) || Input.GetKeyDown(JoyStick_rollButton))
        {
            m_playerCore.MovementController.HandleRoll();
        }

        // Attack
        if (Input.GetKeyDown(attackButton) || Input.GetKeyDown(JoyStick_attackButton))
        {
            m_playerCore.CombatController.HandleAttack();
        }

        // Block
        if (Input.GetKeyDown(blockButton) || Input.GetKeyDown(JoyStick_blockButton))
        {
            m_playerCore.CombatController.HandleBlock(true);
        }
        else if (Input.GetKeyUp(blockButton) || Input.GetKeyUp(JoyStick_blockButton))
        {
            m_playerCore.CombatController.HandleBlock(false);
        }
    }
}
